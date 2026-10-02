using Sakrus.Services.Documentos;

namespace Sakrus.Tests.Documentos;

public class NumeracaoServiceTests
{
    private static readonly DateTimeOffset DataBase = new(2026, 9, 30, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task MesmaEntidade_RetornaMesmoNumeroEmTresChamadas()
    {
        var factory = new TestDbContextFactory("numeracao-" + Guid.NewGuid());
        var servico = new NumeracaoService(factory, new FakeTimeProvider(DataBase));

        var n1 = await servico.ObterOuGerarAsync("X", "Atendimento", 1);
        var n2 = await servico.ObterOuGerarAsync("X", "Atendimento", 1);
        var n3 = await servico.ObterOuGerarAsync("X", "Atendimento", 1);

        Assert.Equal(n1.Id, n2.Id);
        Assert.Equal(n1.Id, n3.Id);
        Assert.Equal("001/2026", n1.Numero);
    }

    [Fact]
    public async Task EntidadesDistintas_Sequenciais()
    {
        var factory = new TestDbContextFactory("numeracao-" + Guid.NewGuid());
        var servico = new NumeracaoService(factory, new FakeTimeProvider(DataBase));

        var n1 = await servico.ObterOuGerarAsync("X", "Atendimento", 1);
        var n2 = await servico.ObterOuGerarAsync("X", "Atendimento", 2);

        Assert.Equal("001/2026", n1.Numero);
        Assert.Equal("002/2026", n2.Numero);
    }

    [Fact]
    public async Task ChavesDistintas_Independentes()
    {
        var factory = new TestDbContextFactory("numeracao-" + Guid.NewGuid());
        var servico = new NumeracaoService(factory, new FakeTimeProvider(DataBase));

        var n1 = await servico.ObterOuGerarAsync("A", "Atendimento", 1);
        var n2 = await servico.ObterOuGerarAsync("B", "Atendimento", 1);

        Assert.Equal("001/2026", n1.Numero);
        Assert.Equal("001/2026", n2.Numero);
    }

    [Fact]
    public async Task ViradaDeAno_ReiniciaEm001()
    {
        var factory = new TestDbContextFactory("numeracao-" + Guid.NewGuid());
        var relogio = new FakeTimeProvider(DataBase);
        var servico = new NumeracaoService(factory, relogio);

        var n1 = await servico.ObterOuGerarAsync("X", "Atendimento", 1);
        relogio.SetUtcNow(new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var n2 = await servico.ObterOuGerarAsync("X", "Atendimento", 2);

        Assert.Equal("001/2026", n1.Numero);
        Assert.Equal("001/2027", n2.Numero);
    }

    [Fact]
    public async Task MesmaChaveCompartilhada_EntreDuasEmissoes_ReusaNumero()
    {
        var factory = new TestDbContextFactory("numeracao-" + Guid.NewGuid());
        var servico = new NumeracaoService(factory, new FakeTimeProvider(DataBase));

        var n1 = await servico.ObterOuGerarAsync("AUTORIZACAO_TRANSLADACAO", "Falecido", 5);
        var n2 = await servico.ObterOuGerarAsync("AUTORIZACAO_TRANSLADACAO", "Falecido", 5);

        Assert.Equal(n1.Id, n2.Id);
        Assert.Equal(n1.Numero, n2.Numero);
        Assert.Equal("001/2026", n2.Numero);
    }

    [Fact]
    public async Task ObterAsync_SemRegistro_RetornaNull()
    {
        var factory = new TestDbContextFactory("numeracao-" + Guid.NewGuid());
        var servico = new NumeracaoService(factory, new FakeTimeProvider(DataBase));

        var registro = await servico.ObterAsync("X", "Atendimento", 1);

        Assert.Null(registro);
    }
}
