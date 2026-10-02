using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using Sakrus.Core.Entities;
using Sakrus.Core.Helpers;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

/// <summary>
/// Documento 20 — Folha de Regularização (demonstrativo do custo para regularizar o lote particular).
/// Reúne a Taxa de Concessão (A), o parcelamento máximo cabível, as Taxas de Manutenção em aberto (B)
/// e o valor total da regularização (C = A + B). Os valores por m² são os cadastrados por exercício.
/// </summary>
public class FolhaRegularizacaoGerador : GeradorBase
{
    public FolhaRegularizacaoGerador(IDbContextFactory<ApplicationDbContext> dbFactory)
        : base(dbFactory)
    {
    }

    public override string Codigo => "20";

    public override IReadOnlyList<CampoDefinicao> Campos => new[]
    {
        new CampoDefinicao("gavetaRelacionada", "Gaveta relacionada", TipoCampo.Texto),
        new CampoDefinicao("dataBase", "Data-base (sepultamento ou compra do jazigo)", TipoCampo.Data,
            Ajuda: "Define o primeiro exercício a ser considerado. Em branco, usa a aquisição do titular."),
        new CampoDefinicao("parcelas", "Número de parcelas da concessão", TipoCampo.Numero,
            Ajuda: "Em branco, o sistema usa a quantidade máxima permitida (até 12 parcelas, mínimo R$ 100,00 por parcela).")
    };

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

        var area = jazigo.AreaM2;
        var valorM2Concessao = await FinanceiroJazigoComum.ObterValorM2Async(db, dataBase.Year);
        var taxaConcessao = area.HasValue
            ? RegrasDocumentos.CalcularValor(area.Value, valorM2Concessao)
            : linhas.Where(l => l.Tipo == TipoLancamento.Concessao).Sum(l => l.ValorDevido);

        var maxParcelas = RegrasDocumentos.MaxParcelasPermitidas(taxaConcessao);
        var parcelasInformadas = ctx.Campos.GetInt("parcelas");
        var parcelas = parcelasInformadas is > 0 and <= RegrasDocumentos.MaxParcelas
            ? Math.Min(parcelasInformadas.Value, maxParcelas)
            : maxParcelas;

        var parcelamento = parcelas > 0
            ? RegrasDocumentos.SimularParcelamento(taxaConcessao, parcelas)
            : Array.Empty<ParcelaSimulada>();

        var manutencoes = FinanceiroJazigoComum.FiltrarTipo(linhas, TipoLancamento.Manutencao)
            .Where(l => l.EmAberto)
            .ToList();

        var totalManutencoes = manutencoes.Sum(l => l.ValorDevido);
        var totalRegularizacao = taxaConcessao + totalManutencoes;

        return Pdf(ctx, "FOLHA DE REGULARIZAÇÃO", col =>
        {
            col.Paragrafo(TextosPadrao.FolhaRegularizacaoFinalidade);

            col.Secao("Dados do concessionário");
            col.Campos(
                ("Nome", Tr(concessionario.Nome)),
                ("CPF", concessionario.Cpf),
                ("RG", concessionario.Rg),
                ("Órgão emissor", concessionario.OrgaoEmissor),
                ("Endereço", concessionario.Endereco),
                ("Telefone", concessionario.Telefone));

            FinanceiroJazigoComum.RenderIdentificacaoLote(col, jazigo, Tr(ctx.Campos.Get("gavetaRelacionada")));

            col.Secao("A — Taxa de Concessão");
            col.Campos(
                ("Área total do lote", DadosComuns.FormatarArea(area)),
                ("Valor do m² da concessão", DadosComuns.FormatarMoeda(valorM2Concessao)),
                ("Taxa de concessão (A)", DadosComuns.FormatarMoeda(taxaConcessao)));

            col.Secao("Parcelamento da Taxa de Concessão");
            col.Campos(
                ("Valor da concessão", DadosComuns.FormatarMoeda(taxaConcessao)),
                ("Máximo de parcelas permitido", maxParcelas > 0 ? maxParcelas.ToString() : "—"),
                ("Parcelas adotadas", parcelas > 0 ? parcelas.ToString() : "—"),
                ("Valor aproximado por parcela", parcelamento.Count > 0 ? DadosComuns.FormatarMoeda(parcelamento[0].Valor) : "—"));

            if (parcelamento.Count > 1)
            {
                col.Tabela(
                    new[] { "Parcela", "Valor" },
                    parcelamento.Select(p => (IReadOnlyList<string>)new[] { p.Numero.ToString(), DadosComuns.FormatarMoeda(p.Valor) }));
            }

            col.Paragrafo($"Regra aplicada: maior número de parcelas possível sem ultrapassar {RegrasDocumentos.MaxParcelas} e sem que qualquer parcela fique abaixo de {DadosComuns.FormatarMoeda(RegrasDocumentos.ParcelaMinima)}.");

            col.Secao("B — Taxas anuais de manutenção em aberto");
            if (manutencoes.Count == 0)
            {
                col.Paragrafo("Não há exercício de manutenção em aberto.");
            }
            else
            {
                col.Tabela(
                    new[] { "Ano", "Área do lote", "Valor do m²", "Valor da manutenção" },
                    manutencoes.Select(l => (IReadOnlyList<string>)new[]
                    {
                        l.Exercicio.ToString(),
                        DadosComuns.FormatarArea(l.AreaM2),
                        DadosComuns.FormatarMoeda(l.ValorM2),
                        DadosComuns.FormatarMoeda(l.ValorDevido)
                    }));
            }

            col.Campos(("Total das taxas de manutenção (B)", DadosComuns.FormatarMoeda(totalManutencoes)));

            col.Secao("C — Valor total da regularização");
            col.Item().PaddingTop(6).Border(0.8f).Padding(6).Column(c =>
            {
                c.Item().Text($"A — Taxa de Concessão: {DadosComuns.FormatarMoeda(taxaConcessao)}");
                c.Item().Text($"B — Total das Taxas de Manutenção: {DadosComuns.FormatarMoeda(totalManutencoes)}");
                c.Item().PaddingTop(4).Text(t =>
                {
                    t.Span("C = A + B — VALOR TOTAL DA REGULARIZAÇÃO: ").Bold();
                    t.Span(DadosComuns.FormatarMoeda(totalRegularizacao)).Bold().FontSize(13);
                });
            });

            col.Paragrafo("Os valores por metro quadrado são aqueles cadastrados para cada exercício; o cadastro de um novo valor não altera os exercícios anteriores.");

            col.LocalEData(ctx);
            col.BlocoAssinaturas("Assinatura do Responsável (Concessionário)", "Assinatura da CAAFE");
            col.AssinaturaCoordenacao(ctx);
        });
    }

    private static string? Tr(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor;
}
