using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Core.Helpers;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Tests;

public class RegrasDocumentosTests
{
    // ── Exumação ───────────────────────────────────────────────────────────

    [Fact]
    public void DataLiberacaoExumacao_UsaDataSepultamento()
    {
        Assert.Equal(new DateTime(2031, 4, 8), RegrasDocumentos.DataLiberacaoExumacao(new DateTime(2026, 4, 8)));
    }

    [Fact]
    public void DataLiberacaoExumacao_AnoBissexto_29Fev2024_Retorna28Fev2029()
    {
        Assert.Equal(new DateTime(2029, 2, 28), RegrasDocumentos.DataLiberacaoExumacao(new DateTime(2024, 2, 29)));
    }

    [Fact]
    public void DataLiberacaoExumacao_NaoUsaDataObito()
    {
        var sepultamento = new DateTime(2026, 4, 8);
        var obito = new DateTime(2026, 4, 5);

        Assert.Equal(sepultamento.AddYears(5), RegrasDocumentos.DataLiberacaoExumacao(sepultamento));
        Assert.NotEqual(obito.AddYears(5), RegrasDocumentos.DataLiberacaoExumacao(sepultamento));
    }

    // ── Área ───────────────────────────────────────────────────────────────

    [Fact]
    public void CalcularAreaM2_RetornaAreaArredondada()
    {
        Assert.Equal(7.70m, RegrasDocumentos.CalcularAreaM2(3.5m, 2.2m));
    }

    [Fact]
    public void CalcularAreaM2_ValoresNulosOuNaoPositivos_RetornaNull()
    {
        Assert.Null(RegrasDocumentos.CalcularAreaM2(null, 2.2m));
        Assert.Null(RegrasDocumentos.CalcularAreaM2(3.5m, null));
        Assert.Null(RegrasDocumentos.CalcularAreaM2(0m, 2.2m));
        Assert.Null(RegrasDocumentos.CalcularAreaM2(3.5m, 0m));
        Assert.Null(RegrasDocumentos.CalcularAreaM2(-1m, 2.2m));
    }

    [Fact]
    public void CalcularAreaM2_ArredondaAwayFromZero()
    {
        Assert.Equal(2.01m, RegrasDocumentos.CalcularAreaM2(1.0m, 2.005m));
    }

    // ── Valor ──────────────────────────────────────────────────────────────

    [Fact]
    public void CalcularValor_ArredondaAwayFromZero()
    {
        Assert.Equal(770.00m, RegrasDocumentos.CalcularValor(7.70m, 100m));
        Assert.Equal(2.01m, RegrasDocumentos.CalcularValor(1.0m, 2.005m));
    }

    // ── Parcelamento ───────────────────────────────────────────────────────

    [Theory]
    [InlineData(0, 0)]
    [InlineData(99.99, 1)]
    [InlineData(100, 1)]
    [InlineData(250, 2)]
    [InlineData(1199.99, 11)]
    [InlineData(1200, 12)]
    [InlineData(5000, 12)]
    public void MaxParcelasPermitidas_RetornaEsperado(decimal total, int esperado)
    {
        Assert.Equal(esperado, RegrasDocumentos.MaxParcelasPermitidas(total));
    }

    [Fact]
    public void SimularParcelamento_SomaSempreIgualTotal()
    {
        var totais = new[] { 1000m, 100m, 99.99m, 1200m, 333.33m, 250m, 1199.99m, 5000m, 0.01m };

        foreach (var total in totais)
        {
            var max = RegrasDocumentos.MaxParcelasPermitidas(total);
            for (var n = 1; n <= max; n++)
            {
                var parcelas = RegrasDocumentos.SimularParcelamento(total, n);
                Assert.Equal(total, parcelas.Sum(p => p.Valor));
            }
        }
    }

    [Fact]
    public void SimularParcelamento_1000Em3Parcelas_33333_33333_33334()
    {
        var parcelas = RegrasDocumentos.SimularParcelamento(1000m, 3);

        Assert.Equal(3, parcelas.Count);
        Assert.Equal(333.33m, parcelas[0].Valor);
        Assert.Equal(333.33m, parcelas[1].Valor);
        Assert.Equal(333.34m, parcelas[2].Valor);
    }

    [Fact]
    public void SimularParcelamento_AcimaDoMaximoGlobal_LancaExcecao()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RegrasDocumentos.SimularParcelamento(1000m, 13));
    }

    [Fact]
    public void SimularParcelamento_AcimaDoPermitidoParaOTotal_LancaExcecao()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RegrasDocumentos.SimularParcelamento(1000m, 11));
    }

    [Fact]
    public void SimularParcelamento_ZeroParcelas_LancaExcecao()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RegrasDocumentos.SimularParcelamento(1000m, 0));
    }

    [Fact]
    public void OpcoesParcelamento_RetornaUmaOpcaoPorQuantidadePermitida()
    {
        var opcoes = RegrasDocumentos.OpcoesParcelamento(1000m);

        Assert.Equal(10, opcoes.Count);
        Assert.Equal(1, opcoes[0].Parcelas);
        Assert.Equal(1000m, opcoes[0].ValorParcela);
        Assert.Equal(1000m, opcoes[0].ValorUltimaParcela);
        Assert.Equal(10, opcoes[^1].Parcelas);
        Assert.Equal(100m, opcoes[^1].ValorParcela);
        Assert.Equal(100m, opcoes[^1].ValorUltimaParcela);
    }

    [Fact]
    public void OpcoesParcelamento_TotalAbaixoDaParcelaMinima_RetornaApenasAVista()
    {
        var opcoes = RegrasDocumentos.OpcoesParcelamento(99.99m);

        Assert.Single(opcoes);
        Assert.Equal(1, opcoes[0].Parcelas);
        Assert.Equal(99.99m, opcoes[0].ValorParcela);
    }

    // ── Numeração ──────────────────────────────────────────────────────────

    [Fact]
    public void FormatarNumeroAnual_FormataComTresDigitos()
    {
        Assert.Equal("001/2026", RegrasDocumentos.FormatarNumeroAnual(1, 2026));
        Assert.Equal("012/2026", RegrasDocumentos.FormatarNumeroAnual(12, 2026));
        Assert.Equal("120/2026", RegrasDocumentos.FormatarNumeroAnual(120, 2026));
    }

    // ── Idade ──────────────────────────────────────────────────────────────

    [Fact]
    public void CalcularIdade_RetornaAnosCompletos()
    {
        Assert.Equal(30, RegrasDocumentos.CalcularIdade(new DateTime(1990, 5, 10), new DateTime(2020, 5, 10)));
        Assert.Equal(29, RegrasDocumentos.CalcularIdade(new DateTime(1990, 5, 10), new DateTime(2020, 5, 9)));
    }

    [Fact]
    public void CalcularIdade_SemNascimento_RetornaNull()
    {
        Assert.Null(RegrasDocumentos.CalcularIdade(null, new DateTime(2020, 5, 10)));
    }

    [Fact]
    public void Falecido_Idade_CalculadaDeDataNascimentoEDataFalecimento()
    {
        var falecido = new Falecido
        {
            DataNascimento = new DateTime(1990, 5, 10),
            DataFalecimento = new DateTime(2020, 5, 9)
        };

        Assert.Equal(29, falecido.Idade);
    }

    [Fact]
    public void Falecido_Idade_SemDataNascimento_RetornaNull()
    {
        var falecido = new Falecido { DataFalecimento = new DateTime(2020, 5, 9) };

        Assert.Null(falecido.Idade);
    }

    // ── Modelo (índices únicos) ────────────────────────────────────────────

    [Fact]
    public void NumeroRegistro_IndicesUnicos_ConfiguradosNoModelo()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(NumeroRegistro));
        Assert.NotNull(entityType);

        var indices = entityType.GetIndexes().ToList();
        Assert.Contains(indices, i => i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual(new[] { "Chave", "EntidadeTipo", "EntidadeId" }));
        Assert.Contains(indices, i => i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual(new[] { "Chave", "Ano", "Sequencia" }));
    }
}
