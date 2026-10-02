using Sakrus.Core.Entities;
using Sakrus.Services.Documentos;

namespace Sakrus.Tests.Documentos;

public class ProtocoloServiceTests
{
    private static readonly DateTimeOffset DataBase = new(2026, 9, 30, 12, 0, 0, TimeSpan.Zero);

    private static async Task<(TestDbContextFactory factory, int responsavelId, int assunto1, int assunto2, int assunto3)> CriarBaseAsync()
    {
        var factory = new TestDbContextFactory("protocolo-" + Guid.NewGuid());
        using var db = factory.CreateDbContext();

        var responsavel = new Responsavel { Nome = "João da Silva" };
        var a1 = new AssuntoProtocolo { Nome = "Assunto 1", Ativo = true };
        var a2 = new AssuntoProtocolo { Nome = "Assunto 2", Ativo = true };
        var a3 = new AssuntoProtocolo { Nome = "Assunto 3", Ativo = true };
        db.Responsaveis.Add(responsavel);
        db.AssuntosProtocolo.AddRange(a1, a2, a3);
        await db.SaveChangesAsync();

        return (factory, responsavel.Id, a1.Id, a2.Id, a3.Id);
    }

    [Fact]
    public async Task SequenciaUnica_EntreAssuntosDistintos()
    {
        var (factory, responsavelId, a1, a2, a3) = await CriarBaseAsync();
        var servico = new ProtocoloService(factory, new FakeTimeProvider(DataBase));

        var p1 = await servico.AbrirAsync(responsavelId, a1, null, null);
        var p2 = await servico.AbrirAsync(responsavelId, a2, null, null);
        var p3 = await servico.AbrirAsync(responsavelId, a3, null, null);

        Assert.Equal("001/2026", p1.Numero);
        Assert.Equal("002/2026", p2.Numero);
        Assert.Equal("003/2026", p3.Numero);
    }

    [Fact]
    public async Task ReinicioAnual()
    {
        var (factory, responsavelId, a1, a2, _) = await CriarBaseAsync();
        var relogio = new FakeTimeProvider(DataBase);
        var servico = new ProtocoloService(factory, relogio);

        var p1 = await servico.AbrirAsync(responsavelId, a1, null, null);
        relogio.SetUtcNow(new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var p2 = await servico.AbrirAsync(responsavelId, a2, null, null);

        Assert.Equal("001/2026", p1.Numero);
        Assert.Equal("001/2027", p2.Numero);
    }

    [Fact]
    public async Task DocumentosDoProtocolo_EmOrdem_EAtivos()
    {
        var factory = new TestDbContextFactory("protocolo-docs-" + Guid.NewGuid());
        using var db = factory.CreateDbContext();

        var responsavel = new Responsavel { Nome = "João da Silva" };
        var assunto = new AssuntoProtocolo
        {
            Nome = "Assunto",
            Ativo = true,
            Documentos = new List<AssuntoProtocoloDocumento>
            {
                new() { TipoDocumentoCodigo = "B", Ordem = 2 },
                new() { TipoDocumentoCodigo = "A", Ordem = 1 },
                new() { TipoDocumentoCodigo = "C", Ordem = 3 },
                new() { TipoDocumentoCodigo = "D", Ordem = 4 }
            }
        };
        db.TiposDocumento.AddRange(
            new TipoDocumento { Codigo = "A", Nome = "Doc A", EntidadeTipo = "X", Ativo = true, Ordem = 1 },
            new TipoDocumento { Codigo = "B", Nome = "Doc B", EntidadeTipo = "X", Ativo = true, Ordem = 2 },
            new TipoDocumento { Codigo = "C", Nome = "Doc C", EntidadeTipo = "X", Ativo = true, Ordem = 3 },
            new TipoDocumento { Codigo = "D", Nome = "Doc D", EntidadeTipo = "X", Ativo = false, Ordem = 4 });
        db.Responsaveis.Add(responsavel);
        db.AssuntosProtocolo.Add(assunto);
        await db.SaveChangesAsync();

        var servico = new ProtocoloService(factory, new FakeTimeProvider(DataBase));
        var protocolo = await servico.AbrirAsync(responsavel.Id, assunto.Id, null, null);
        var documentos = await servico.DocumentosDoProtocoloAsync(protocolo.Id);

        Assert.Equal(new[] { "A", "B", "C" }, documentos.Select(d => d.Codigo));
    }

    [Fact]
    public async Task AbrirAsync_AssuntoInativo_Bloqueia()
    {
        var factory = new TestDbContextFactory("protocolo-" + Guid.NewGuid());
        using var db = factory.CreateDbContext();

        var responsavel = new Responsavel { Nome = "João da Silva" };
        var assunto = new AssuntoProtocolo { Nome = "Inativo", Ativo = false };
        db.Responsaveis.Add(responsavel);
        db.AssuntosProtocolo.Add(assunto);
        await db.SaveChangesAsync();

        var servico = new ProtocoloService(factory, new FakeTimeProvider(DataBase));

        await Assert.ThrowsAsync<DocumentoBloqueadoException>(
            () => servico.AbrirAsync(responsavel.Id, assunto.Id, null, null));
    }
}
