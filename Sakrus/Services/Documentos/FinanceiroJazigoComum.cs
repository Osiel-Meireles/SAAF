using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using Sakrus.Core.Entities;
using Sakrus.Core.Helpers;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

public record LinhaFinanceira(
    int Exercicio,
    TipoLancamento Tipo,
    decimal ValorM2,
    decimal? AreaM2,
    decimal ValorDevido,
    decimal ValorPago,
    string? Documento,
    DateTime? DataPagamento)
{
    public decimal Saldo => Math.Max(0m, ValorDevido - ValorPago);
    public bool EmAberto => Saldo > 0m;
}

/// <summary>
/// Apuração financeira do lote particular (documentos 19 e 20).
/// O valor por m² é o cadastrado para cada exercício (ValoresMetroQuadrado); na ausência dele,
/// usa-se o valor vigente da configuração financeira. Lançamentos já registrados prevalecem
/// sobre o cálculo por área, preservando o histórico de cada ano.
/// </summary>
public static class FinanceiroJazigoComum
{
    public static async Task<decimal> ObterValorM2Async(ApplicationDbContext db, int exercicio)
    {
        var cadastrado = await db.ValoresMetroQuadrado
            .Where(v => v.Exercicio == exercicio)
            .Select(v => v.Valor)
            .FirstOrDefaultAsync();

        if (cadastrado > 0m)
            return cadastrado;

        var config = await db.ConfiguracoesFinanceiras.FirstOrDefaultAsync();
        return config?.ValorMetroQuadrado ?? 0m;
    }

    public static DateTime ResolverDataBase(ContextoEmissao ctx, Jazigo jazigo, IReadOnlyList<LancamentoFinanceiro> lancamentos)
    {
        var informada = ctx.Campos.GetData("dataBase");
        if (informada.HasValue)
            return informada.Value;

        var titular = DadosComuns.TitularAtivo(jazigo);
        if (titular != null && titular.DataAquisicao != default)
            return titular.DataAquisicao;

        var sepultamento = jazigo.Gavetas
            .Where(g => g.DataSepultamento.HasValue)
            .Select(g => g.DataSepultamento!.Value)
            .DefaultIfEmpty()
            .Min();
        if (sepultamento != default)
            return sepultamento;

        var primeiro = lancamentos.OrderBy(l => l.Exercicio).FirstOrDefault();
        return primeiro != null
            ? new DateTime(primeiro.Exercicio, 1, 1)
            : ctx.DataEmissao.Date;
    }

    public static async Task<IReadOnlyList<LinhaFinanceira>> MontarLancamentosAsync(
        ApplicationDbContext db,
        Jazigo jazigo,
        DateTime dataBase,
        DateTime referencia)
    {
        var lancamentos = await db.LancamentosFinanceiros
            .Where(l => l.JazigoId == jazigo.Id)
            .OrderBy(l => l.Exercicio)
            .ThenBy(l => l.Tipo)
            .ToListAsync();

        var area = jazigo.AreaM2;
        var exercicioInicial = dataBase.Year;
        var exercicioReferencia = referencia.Year;
        var exercicioMaximo = lancamentos.Count > 0
            ? Math.Max(exercicioReferencia, lancamentos.Max(l => l.Exercicio))
            : exercicioReferencia;

        if (exercicioMaximo < exercicioInicial)
            exercicioMaximo = exercicioInicial;

        var linhas = new List<LinhaFinanceira>();

        for (var exercicio = exercicioInicial; exercicio <= exercicioMaximo; exercicio++)
        {
            var tipos = exercicio == exercicioInicial
                ? new[] { TipoLancamento.Concessao, TipoLancamento.Manutencao }
                : new[] { TipoLancamento.Manutencao };

            foreach (var tipo in tipos)
            {
                var doAno = lancamentos.Where(l => l.Exercicio == exercicio && l.Tipo == tipo).ToList();
                var valorM2 = await ObterValorM2Async(db, exercicio);
                var devido = CalcularDevido(doAno, area, valorM2);
                var pago = Math.Round(doAno.Where(l => l.Pago).Sum(l => l.Valor), 2, MidpointRounding.AwayFromZero);
                var comprovante = doAno
                    .Select(l => l.Observacao)
                    .FirstOrDefault(o => !string.IsNullOrWhiteSpace(o));
                var dataPagamento = doAno
                    .Where(l => l.Pago && l.DataPagamento.HasValue)
                    .Select(l => l.DataPagamento!.Value)
                    .DefaultIfEmpty()
                    .Min();

                linhas.Add(new LinhaFinanceira(
                    exercicio,
                    tipo,
                    valorM2,
                    area,
                    devido,
                    pago,
                    string.IsNullOrWhiteSpace(comprovante) ? null : comprovante,
                    dataPagamento == default ? null : dataPagamento));
            }
        }

        return linhas;
    }

    private static decimal CalcularDevido(IReadOnlyList<LancamentoFinanceiro> doAno, decimal? area, decimal valorM2)
    {
        if (doAno.Count > 0)
            return Math.Round(doAno.Sum(l => l.Valor), 2, MidpointRounding.AwayFromZero);

        if (area == null || valorM2 <= 0m)
            return 0m;

        return RegrasDocumentos.CalcularValor(area.Value, valorM2);
    }

    public static IReadOnlyList<LinhaFinanceira> FiltrarTipo(IEnumerable<LinhaFinanceira> linhas, TipoLancamento tipo)
        => linhas.Where(l => l.Tipo == tipo).ToList();

    public static void RenderIdentificacaoLote(ColumnDescriptor col, Jazigo jazigo, string? gaveta)
    {
        col.Secao("Identificação e características do lote");
        col.Campos(
            ("Cemitério", jazigo.Cemiterio?.Nome),
            ("Código do jazigo", string.IsNullOrWhiteSpace(jazigo.CodigoIdentificador) ? null : jazigo.CodigoIdentificador),
            ("Quadra", string.IsNullOrWhiteSpace(jazigo.Quadra) ? null : jazigo.Quadra),
            ("Ala", string.IsNullOrWhiteSpace(jazigo.Ala) ? null : jazigo.Ala),
            ("Lote", string.IsNullOrWhiteSpace(jazigo.NumeroLote) ? null : jazigo.NumeroLote),
            ("Gaveta relacionada", string.IsNullOrWhiteSpace(gaveta) ? null : gaveta),
            ("Tipo de sepultura", jazigo.ModeloJazigo?.Nome),
            ("Tipo/classificação do jazigo", jazigo.ClassificacaoEspaco?.Nome),
            ("Revestimento", string.IsNullOrWhiteSpace(jazigo.Revestimento) ? null : jazigo.Revestimento),
            ("Largura", jazigo.Largura.HasValue ? $"{jazigo.Largura.Value.ToString("0.##")} m" : null),
            ("Comprimento", jazigo.Comprimento.HasValue ? $"{jazigo.Comprimento.Value.ToString("0.##")} m" : null),
            ("Área total", DadosComuns.FormatarArea(jazigo.AreaM2)));
    }

    /// <summary>
    /// Pré-condição comum dos documentos financeiros do lote particular: existência e área calculável.
    /// </summary>
    public static async Task<string?> ValidarLoteParticularAsync(ApplicationDbContext db, int entidadeId)
    {
        var jazigo = await db.Jazigos
            .Include(j => j.Cemiterio)
            .FirstOrDefaultAsync(j => j.Id == entidadeId);

        if (jazigo == null)
            return "Jazigo não encontrado";

        if (jazigo.AreaM2 == null)
            return "Informe a largura e o comprimento do lote para calcular a área";

        return null;
    }
}
