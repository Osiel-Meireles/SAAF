using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Services.Documentos;

namespace Sakrus.Tests.Documentos;

public class DiagnosticoTests
{
    [Fact]
    public async Task Diagnostico_Historico()
    {
        var factory = new TestDbContextFactory("diag-" + Guid.NewGuid());
        using (var db = factory.CreateDbContext())
        {
            db.TiposDocumento.Add(new TipoDocumento { Codigo = "T1", Nome = "Teste 1", EntidadeTipo = "Atendimento", Ativo = true, Ordem = 1 });
            db.ConfiguracoesInstitucionais.Add(new ConfiguracaoInstitucional());
            await db.SaveChangesAsync();
        }

        var relogio = new FakeTimeProvider(new DateTimeOffset(2026, 9, 30, 12, 0, 0, TimeSpan.Zero));
        var numeracao = new NumeracaoService(factory, relogio);
        var env = new Moq.Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        env.Setup(e => e.ContentRootPath).Returns(Path.Combine(Path.GetTempPath(), "sakrus-diag-" + Guid.NewGuid().ToString("N")));
        env.Setup(e => e.EnvironmentName).Returns("Development");
        var fileStorage = new Sakrus.Services.FileStorageService(env.Object, Microsoft.Extensions.Logging.Abstractions.NullLogger<Sakrus.Services.FileStorageService>.Instance);
        var servico = new DocumentoEmissaoService(factory, new IDocumentoGerador[] { new GeradorStubT1(factory) }, numeracao, new FakeCurrentUser(), relogio, fileStorage);

        var campos = new CamposValores(new Dictionary<string, string> { ["campoObrigatorio"] = "x" });
        var r1 = await servico.EmitirAsync("T1", 1, campos);
        var r2 = await servico.EmitirAsync("T1", 1, campos);
        Assert.Equal(1, r1.Versao);
        Assert.Equal(2, r2.Versao);

        using var db2 = factory.CreateDbContext();
        var todos = await db2.DocumentosEmitidos.ToListAsync();
        Assert.Equal(2, todos.Count);

        var historico = await servico.HistoricoAsync("Atendimento", 1);
        Assert.Equal(2, historico.Count);
    }
}
