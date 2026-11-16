using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using Sakrus.Core.Entities;
using Sakrus.Core.Helpers;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

/// <summary>
/// Documento 19 — Folha Financeira (histórico permanente de lançamentos e pagamentos do lote particular).
/// Uma linha por exercício, com valor do m² cadastrado, área, valor devido, pago, saldo,
/// documento/comprovante e data do pagamento. Sem cálculo automático de juros ou multa.
/// </summary>
public class FolhaFinanceiraGerador : GeradorBase
{
    public FolhaFinanceiraGerador(IDbContextFactory<ApplicationDbContext> dbFactory)
        : base(dbFactory)
    {
    }

    public override string Codigo => "19";

    public override IReadOnlyList<CampoDefinicao> Campos => new[]
    {
        new CampoDefinicao("gavetaRelacionada", "Gaveta relacionada", TipoCampo.Selecao, FonteOpcoes: "gavetasJazigo",
            Ajuda: "Opcional. Gavetas cadastradas neste jazigo."),
        new CampoDefinicao("dataBase", "Data-base (sepultamento ou compra do jazigo)", TipoCampo.Data,
            Ajuda: "O mês desta data é o mês de referência do vencimento anual. Em branco, usa a aquisição do titular.")
    };

    public override async Task<string?> ValidarPreCondicoesAsync(int entidadeId)
    {
        using var db = await DbFactory.CreateDbContextAsync();
        return await FinanceiroJazigoComum.ValidarLoteParticularAsync(db, entidadeId);
    }

    public override async Task<byte[]> GerarAsync(ContextoEmissao ctx)
    {
        using var db = await DbFactory.CreateDbContextAsync();

        var jazigo = await DadosComuns.JazigoCompletoAsync(db, ctx.EntidadeId);
        var concessionario = DadosComuns.Qualificacao(DadosComuns.TitularAtivo(jazigo));

        var lancamentosRaw = await db.LancamentosFinanceiros
            .Where(l => l.JazigoId == jazigo.Id)
            .ToListAsync();

        var dataBase = FinanceiroJazigoComum.ResolverDataBase(ctx, jazigo, lancamentosRaw);
        var linhas = await FinanceiroJazigoComum.MontarLancamentosAsync(db, jazigo, dataBase, ctx.DataEmissao);

        var concessoes = FinanceiroJazigoComum.FiltrarTipo(linhas, TipoLancamento.Concessao);
        var manutencoes = FinanceiroJazigoComum.FiltrarTipo(linhas, TipoLancamento.Manutencao);

        var falecidos = jazigo.Gavetas
            .Where(g => g.Falecido != null)
            .OrderBy(g => g.Numero)
            .ToList();

        var cultura = new System.Globalization.CultureInfo("pt-BR");
        var mesBase = cultura.TextInfo.ToUpper(dataBase.ToString("MMMM", cultura));

        return Pdf(ctx, "FOLHA DE ROSTO FINANCEIRA", col =>
        {
            col.Secao("Nome do Concessionário(a)");
            col.Item().PaddingTop(3).Text(Tr(concessionario.Nome) ?? "—").Bold();
            col.Campos(
                ("CPF", concessionario.Cpf),
                ("RG", concessionario.Rg),
                ("Órgão emissor", concessionario.OrgaoEmissor),
                ("Telefone", concessionario.Telefone),
                ("Endereço", concessionario.Endereco));

            col.Secao("IDENTIFICAÇÃO E LOCALIZAÇÃO DO LOTE CEMITERIAL");
            col.Campos(
                ("Quadra", jazigo.Quadra),
                ("Ala", jazigo.Ala),
                ("Lote", jazigo.NumeroLote),
                ("Gaveta", Tr(ctx.Campos.Get("gavetaRelacionada"))),
                ("Tipo de sepultura", jazigo.ModeloJazigo?.Nome),
                ("Nº gavetas", jazigo.Gavetas.Count > 0 ? jazigo.Gavetas.Count.ToString("00") : null));

            var tipoRevestimento = string.Join(" - ", new[] { jazigo.ClassificacaoEspaco?.Nome, jazigo.Revestimento }
                .Where(v => !string.IsNullOrWhiteSpace(v)));
            col.Tabela(
                new[] { "Largura", "Comprimento", "Total m²", "Tipo jazigo / revestimento" },
                new[]
                {
                    (IReadOnlyList<string>)new[]
                    {
                        jazigo.Largura?.ToString("0.000", cultura) ?? "—",
                        jazigo.Comprimento?.ToString("0.000", cultura) ?? "—",
                        jazigo.AreaM2?.ToString("0.000", cultura) ?? "—",
                        tipoRevestimento.Length > 0 ? tipoRevestimento : "—"
                    }
                });

            col.Secao("TAXA DE CONCESSÃO");
            col.Tabela(
                new[] { "Valor m²", "Valor da concessão", "Valor pago", "Nº documento", "Data pagamento" },
                concessoes.Select(l => (IReadOnlyList<string>)new[]
                {
                    DadosComuns.FormatarMoeda(l.ValorM2),
                    DadosComuns.FormatarMoeda(l.ValorDevido),
                    DadosComuns.FormatarMoeda(l.ValorPago),
                    l.Documento ?? "",
                    l.DataPagamento.HasValue ? DadosComuns.FormatarData(l.DataPagamento) : ""
                }));

            col.Item().PaddingTop(4).Text($"DATA BASE PARA VENCIMENTO: {mesBase}").Bold();

            col.Secao("TAXA ANUAL MANUTENÇÃO");
            col.Tabela(
                new[] { "Ano", "Valor m²", "Valor manutenção", "Valor pago", "Nº documento", "Data pagamento" },
                manutencoes.Select(l => (IReadOnlyList<string>)new[]
                {
                    l.Exercicio.ToString(),
                    DadosComuns.FormatarMoeda(l.ValorM2),
                    DadosComuns.FormatarMoeda(l.ValorDevido),
                    l.ValorPago > 0m ? DadosComuns.FormatarMoeda(l.ValorPago) : "",
                    l.Documento ?? "",
                    l.DataPagamento.HasValue ? DadosComuns.FormatarData(l.DataPagamento) : ""
                }));

            var totalDevido = linhas.Sum(l => l.ValorDevido);
            var totalPago = linhas.Sum(l => l.ValorPago);
            var totalSaldo = linhas.Sum(l => l.Saldo);

            col.Secao("Resumo financeiro");
            col.Campos(
                ("Total devido", DadosComuns.FormatarMoeda(totalDevido)),
                ("Total pago", DadosComuns.FormatarMoeda(totalPago)),
                ("Saldo em aberto", DadosComuns.FormatarMoeda(totalSaldo)),
                ("Exercícios em aberto", manutencoes.Where(l => l.EmAberto).Select(l => l.Exercicio.ToString()).DefaultIfEmpty("—").Aggregate((a, b) => $"{a}, {b}")));

            col.Secao("Falecido(s)");
            col.Tabela(
                new[] { "Falecido(a)", "QD", "Ala", "LT", "G", "Óbito", "Exumação" },
                falecidos.Count == 0
                    ? new[] { (IReadOnlyList<string>)new[] { "—", "", "", "", "", "", "" } }
                    : falecidos.Select(g => (IReadOnlyList<string>)new[]
                    {
                        g.Falecido!.Nome,
                        jazigo.Quadra ?? "",
                        jazigo.Ala ?? "",
                        jazigo.NumeroLote ?? "",
                        g.Numero.ToString().PadLeft(2, '0'),
                        DadosComuns.FormatarData(g.Falecido.DataFalecimento),
                        DadosComuns.FormatarData(RegrasDocumentos.DataLiberacaoExumacao(g.DataSepultamento ?? g.Falecido.DataFalecimento))
                    }));

            col.LocalEData(ctx);
            col.AssinaturaCoordenacao(ctx);
        });
    }

    private static string? Tr(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor;
}
