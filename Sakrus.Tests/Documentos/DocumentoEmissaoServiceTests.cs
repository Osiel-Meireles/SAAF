using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;
using Sakrus.Services;
using Sakrus.Services.Documentos;

namespace Sakrus.Tests.Documentos;

public class GeradorStubT1 : GeradorBase
{
    public static byte[] PdfBytes { get; } = { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34, 0x0A };

    public GeradorStubT1(IDbContextFactory<ApplicationDbContext> dbFactory)
        : base(dbFactory)
    {
    }

    public override string Codigo => "T1";

    public override IReadOnlyList<CampoDefinicao> Campos => new[]
    {
        new CampoDefinicao("campoObrigatorio", "Campo obrigatório", TipoCampo.Texto, Obrigatorio: true)
    };

    public override Task<byte[]> GerarAsync(ContextoEmissao ctx) => Task.FromResult(PdfBytes);
}

public class GeradorStubT2 : GeradorBase
{
    public GeradorStubT2(IDbContextFactory<ApplicationDbContext> dbFactory)
        : base(dbFactory)
    {
    }

    public override string Codigo => "T2";

    public override Task<byte[]> GerarAsync(ContextoEmissao ctx) => Task.FromResult(GeradorStubT1.PdfBytes);
}

public class GeradorStubPreCondicao : GeradorBase
{
    public GeradorStubPreCondicao(IDbContextFactory<ApplicationDbContext> dbFactory)
        : base(dbFactory)
    {
    }

    public override string Codigo => "T3";

    public override Task<string?> ValidarPreCondicoesAsync(int entidadeId)
        => Task.FromResult<string?>("Bloqueado por pré-condição");

    public override Task<byte[]> GerarAsync(ContextoEmissao ctx) => Task.FromResult(GeradorStubT1.PdfBytes);
}

public class DocumentoEmissaoServiceTests
{
    private static readonly DateTimeOffset DataBase = new(2026, 9, 30, 12, 0, 0, TimeSpan.Zero);

    private static DocumentoEmissaoService CriarServico(TestDbContextFactory factory, params IDocumentoGerador[] geradores)
    {
        var relogio = new FakeTimeProvider(DataBase);
        var numeracao = new NumeracaoService(factory, relogio);
        return new DocumentoEmissaoService(factory, geradores, numeracao, new FakeCurrentUser(), relogio, CriarFileStorage());
    }

    private static FileStorageService CriarFileStorage()
    {
        var env = new Mock<IWebHostEnvironment>();
        env.Setup(e => e.ContentRootPath).Returns(Path.Combine(Path.GetTempPath(), "sakrus-tests-" + Guid.NewGuid().ToString("N")));
        env.Setup(e => e.EnvironmentName).Returns("Development");
        return new FileStorageService(env.Object, NullLogger<FileStorageService>.Instance);
    }

    private static async Task AdicionarTipoDocumentoAsync(TestDbContextFactory factory, TipoDocumento tipo)
    {
        using var db = factory.CreateDbContext();
        db.TiposDocumento.Add(tipo);
        db.ConfiguracoesInstitucionais.Add(new ConfiguracaoInstitucional());
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task CampoObrigatorioAusente_Bloqueia()
    {
        var factory = new TestDbContextFactory("emissao-" + Guid.NewGuid());
        var servico = CriarServico(factory, new GeradorStubT1(factory));
        await AdicionarTipoDocumentoAsync(factory, new TipoDocumento { Codigo = "T1", Nome = "Teste 1", EntidadeTipo = "Atendimento", Ativo = true, Ordem = 1 });

        var ex = await Assert.ThrowsAsync<DocumentoBloqueadoException>(
            () => servico.EmitirAsync("T1", 1, new CamposValores()));

        Assert.Contains("Campo obrigatório", ex.Message);
    }

    [Fact]
    public async Task Reemissao_MantemMesmoNumero_EIncrementaVersao()
    {
        var factory = new TestDbContextFactory("emissao-" + Guid.NewGuid());
        var servico = CriarServico(factory, new GeradorStubT1(factory));
        await AdicionarTipoDocumentoAsync(factory, new TipoDocumento
        {
            Codigo = "T1",
            Nome = "Teste 1",
            EntidadeTipo = "Atendimento",
            ExigeNumeroUnico = true,
            GeraNumeroNaPrimeiraEmissao = true,
            ChaveNumeracao = "CHAVE_T1",
            Ativo = true,
            Ordem = 1
        });

        var campos = new CamposValores(new Dictionary<string, string> { ["campoObrigatorio"] = "x" });
        var r1 = await servico.EmitirAsync("T1", 1, campos);
        var r2 = await servico.EmitirAsync("T1", 1, campos);

        Assert.Equal("001/2026", r1.Numero);
        Assert.Equal(r1.Numero, r2.Numero);
        Assert.Equal(1, r1.Versao);
        Assert.Equal(2, r2.Versao);
    }

    [Fact]
    public async Task Reimprimir_DevolveBytesIdenticos()
    {
        var factory = new TestDbContextFactory("emissao-" + Guid.NewGuid());
        var servico = CriarServico(factory, new GeradorStubT1(factory));
        await AdicionarTipoDocumentoAsync(factory, new TipoDocumento { Codigo = "T1", Nome = "Teste 1", EntidadeTipo = "Atendimento", Ativo = true, Ordem = 1 });

        var campos = new CamposValores(new Dictionary<string, string> { ["campoObrigatorio"] = "x" });
        var resultado = await servico.EmitirAsync("T1", 1, campos);
        var bytes = await servico.ReimprimirAsync(resultado.DocumentoEmitidoId);

        Assert.NotNull(bytes);
        Assert.Equal(GeradorStubT1.PdfBytes, bytes);
    }

    [Fact]
    public async Task PreCondicaoComMensagem_Bloqueia()
    {
        var factory = new TestDbContextFactory("emissao-" + Guid.NewGuid());
        var servico = CriarServico(factory, new GeradorStubPreCondicao(factory));
        await AdicionarTipoDocumentoAsync(factory, new TipoDocumento { Codigo = "T3", Nome = "Teste 3", EntidadeTipo = "Atendimento", Ativo = true, Ordem = 3 });

        var ex = await Assert.ThrowsAsync<DocumentoBloqueadoException>(
            () => servico.EmitirAsync("T3", 1, new CamposValores()));

        Assert.Contains("Bloqueado por pré-condição", ex.Message);
    }

    [Fact]
    public async Task DocumentoQueExigeNumeroMasNaoGera_BloqueiaAteExistir_DepoisReusa()
    {
        var factory = new TestDbContextFactory("emissao-" + Guid.NewGuid());
        var servico = CriarServico(factory, new GeradorStubT1(factory), new GeradorStubT2(factory));
        using (var db = factory.CreateDbContext())
        {
            db.TiposDocumento.AddRange(
                new TipoDocumento
                {
                    Codigo = "T1",
                    Nome = "Teste 1",
                    EntidadeTipo = "Atendimento",
                    ExigeNumeroUnico = true,
                    GeraNumeroNaPrimeiraEmissao = true,
                    ChaveNumeracao = "CHAVE_T1",
                    Ativo = true,
                    Ordem = 1
                },
                new TipoDocumento
                {
                    Codigo = "T2",
                    Nome = "Teste 2",
                    EntidadeTipo = "Atendimento",
                    ExigeNumeroUnico = true,
                    GeraNumeroNaPrimeiraEmissao = false,
                    ChaveNumeracao = "CHAVE_T1",
                    Ativo = true,
                    Ordem = 2
                });
            db.ConfiguracoesInstitucionais.Add(new ConfiguracaoInstitucional());
            await db.SaveChangesAsync();
        }

        var campos = new CamposValores(new Dictionary<string, string> { ["campoObrigatorio"] = "x" });

        var ex = await Assert.ThrowsAsync<DocumentoBloqueadoException>(
            () => servico.EmitirAsync("T2", 1, campos));
        Assert.Contains("CHAVE_T1", ex.Message);

        var r1 = await servico.EmitirAsync("T1", 1, campos);
        var r2 = await servico.EmitirAsync("T2", 1, campos);

        Assert.Equal("001/2026", r1.Numero);
        Assert.Equal(r1.Numero, r2.Numero);
    }

    [Fact]
    public async Task GeradorNaoImplementado_Bloqueia()
    {
        var factory = new TestDbContextFactory("emissao-" + Guid.NewGuid());
        var servico = CriarServico(factory);
        await AdicionarTipoDocumentoAsync(factory, new TipoDocumento { Codigo = "ZZZ", Nome = "Sem gerador", EntidadeTipo = "Atendimento", Ativo = true, Ordem = 9 });

        var ex = await Assert.ThrowsAsync<DocumentoBloqueadoException>(
            () => servico.EmitirAsync("ZZZ", 1, new CamposValores()));

        Assert.Contains("Gerador não implementado", ex.Message);
    }

    [Fact]
    public async Task Historico_OrdenadoPorEmitidoEmDesc()
    {
        var factory = new TestDbContextFactory("emissao-" + Guid.NewGuid());
        var servico = CriarServico(factory, new GeradorStubT1(factory));
        await AdicionarTipoDocumentoAsync(factory, new TipoDocumento { Codigo = "T1", Nome = "Teste 1", EntidadeTipo = "Atendimento", Ativo = true, Ordem = 1 });

        var campos = new CamposValores(new Dictionary<string, string> { ["campoObrigatorio"] = "x" });
        var r1 = await servico.EmitirAsync("T1", 1, campos);
        var r2 = await servico.EmitirAsync("T1", 1, campos);
        Assert.Equal(1, r1.Versao);
        Assert.Equal(2, r2.Versao);

        var historico = await servico.HistoricoAsync("Atendimento", 1);

        Assert.Equal(2, historico.Count);
        Assert.Equal(2, historico[0].Versao);
        Assert.Equal(1, historico[1].Versao);
        Assert.True(historico[0].EmitidoEm >= historico[1].EmitidoEm);
    }
}
