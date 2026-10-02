using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
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
        new CampoDefinicao("gavetaRelacionada", "Gaveta relacionada", TipoCampo.Texto),
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

        var gavetaFalecido = jazigo.Gavetas
            .Where(g => g.Falecido != null)
            .OrderBy(g => g.DataSepultamento)
            .FirstOrDefault();
        var falecido = gavetaFalecido?.Falecido;

        return Pdf(ctx, "FOLHA FINANCEIRA", col =>
        {
            col.Paragrafo(TextosPadrao.FolhaFinanceiraFinalidade);

            col.Secao("Dados do concessionário");
            col.Campos(
                ("Nome", Tr(concessionario.Nome)),
                ("CPF", concessionario.Cpf),
                ("RG", concessionario.Rg),
                ("Órgão emissor", concessionario.OrgaoEmissor),
                ("Endereço", concessionario.Endereco),
                ("Telefone", concessionario.Telefone));

            FinanceiroJazigoComum.RenderIdentificacaoLote(col, jazigo, Tr(ctx.Campos.Get("gavetaRelacionada")));

            if (falecido != null)
            {
                col.Secao("Falecido relacionado");
                col.Campos(
                    ("Nome", falecido.Nome),
                    ("Local de sepultamento/gaveta", DadosComuns.FormatarLocalJazigo(jazigo, gavetaFalecido!.Numero)),
                    ("Data do óbito", DadosComuns.FormatarData(falecido.DataFalecimento)));
            }

            col.Secao("Cálculo da área");
            col.Campos(
                ("Largura", jazigo.Largura.HasValue ? $"{jazigo.Largura.Value.ToString("0.##")} m" : null),
                ("Comprimento", jazigo.Comprimento.HasValue ? $"{jazigo.Comprimento.Value.ToString("0.##")} m" : null),
                ("Área total (largura × comprimento)", DadosComuns.FormatarArea(jazigo.AreaM2)));

            col.Secao("Data-base e mês de referência");
            col.Campos(
                ("Data-base", DadosComuns.FormatarData(dataBase)),
                ("Mês de referência do vencimento anual", dataBase.ToString("MM/yyyy")));

            col.Secao("Taxa de concessão");
            col.Tabela(
                new[] { "Exercício", "Valor m²", "Área", "Valor da concessão", "Valor pago", "Saldo", "Nº documento", "Data pagamento" },
                concessoes.Select(LinhaDe));

            col.Secao("Taxas anuais de manutenção");
            col.Tabela(
                new[] { "Ano", "Valor m²", "Área", "Valor manutenção", "Valor pago", "Saldo", "Nº documento", "Data pagamento" },
                manutencoes.Select(LinhaDe));

            var totalDevido = linhas.Sum(l => l.ValorDevido);
            var totalPago = linhas.Sum(l => l.ValorPago);
            var totalSaldo = linhas.Sum(l => l.Saldo);

            col.Secao("Resumo financeiro");
            col.Campos(
                ("Total devido", DadosComuns.FormatarMoeda(totalDevido)),
                ("Total pago", DadosComuns.FormatarMoeda(totalPago)),
                ("Saldo em aberto", DadosComuns.FormatarMoeda(totalSaldo)),
                ("Exercícios em aberto", manutencoes.Where(l => l.EmAberto).Select(l => l.Exercicio.ToString()).DefaultIfEmpty("—").Aggregate((a, b) => $"{a}, {b}")));

            col.Paragrafo("Não são calculados juros ou multas automaticamente nesta folha; eventuais acréscimos decorrem de lançamento manual.");

            col.LocalEData(ctx);
            col.BlocoAssinaturas("Assinatura do Responsável (Concessionário)", "Assinatura da CAAFE");
            col.AssinaturaCoordenacao(ctx);
        });
    }

    private static IReadOnlyList<string> LinhaDe(LinhaFinanceira l)
        => new[]
        {
            l.Exercicio.ToString(),
            DadosComuns.FormatarMoeda(l.ValorM2),
            DadosComuns.FormatarArea(l.AreaM2),
            DadosComuns.FormatarMoeda(l.ValorDevido),
            DadosComuns.FormatarMoeda(l.ValorPago),
            DadosComuns.FormatarMoeda(l.Saldo),
            l.Documento ?? "—",
            DadosComuns.FormatarData(l.DataPagamento)
        };

    private static string? Tr(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor;
}
