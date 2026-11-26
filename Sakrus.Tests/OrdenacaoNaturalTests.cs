using Sakrus.Core.Helpers;

namespace Sakrus.Tests;

public class OrdenacaoNaturalTests
{
    [Fact]
    public void OrdenaNumerosSemZerosAEsquerda()
    {
        var numeros = new[] { "1", "10", "100", "101", "11", "2", "20", "3", "500", "99" };

        var ordenados = numeros.OrdenarPor(n => n).ToList();

        Assert.Equal(new[] { "1", "2", "3", "10", "11", "20", "99", "100", "101", "500" }, ordenados);
    }

    [Fact]
    public void OrdenaGavetasDe1A500NaSequenciaCorreta()
    {
        var gavetas = Enumerable.Range(1, 500)
            .Select(i => i.ToString())
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        var ordenadas = gavetas.OrdenarPor(n => n).ToList();

        Assert.Equal(Enumerable.Range(1, 500).Select(i => i.ToString()), ordenadas);
    }

    [Fact]
    public void MantemSufixoAlfabeticoDepoisDoNumero()
    {
        var lotes = new[] { "10-B", "2", "10-A", "10", "1" };

        var ordenados = lotes.OrdenarPor(l => l).ToList();

        Assert.Equal(new[] { "1", "2", "10", "10-A", "10-B" }, ordenados);
    }

    [Fact]
    public void TrataZerosAEsquerdaComoMesmoNumero()
    {
        Assert.Equal(0, OrdenacaoNatural.Comparar("007", "7"));
        Assert.True(OrdenacaoNatural.Comparar("007", "8") < 0);
    }
}
