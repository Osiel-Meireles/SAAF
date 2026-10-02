using Sakrus.Core.Entities;
using Sakrus.Services.Documentos;

namespace Sakrus.Tests.Documentos;

public class GavetaSelecaoServiceTests
{
    private static async Task<(TestDbContextFactory factory, int responsavelId, int outroResponsavelId, int vinculoId, int gaveta1Id, int gaveta2Id, int gavetaOutroId, int publicaLivreId, int publicaOcupadaId)> CriarBaseAsync()
    {
        var factory = new TestDbContextFactory("gaveta-selecao-" + Guid.NewGuid());
        using var db = factory.CreateDbContext();

        var cemiterio = new Cemiterio { Nome = "Bela Vista" };
        var classificacao = new ClassificacaoEspaco { Nome = "Tipo A", Natureza = NaturezaEspaco.Gaveta };
        var responsavel = new Responsavel { Nome = "Resp" };
        var outro = new Responsavel { Nome = "Outro" };
        var jazigo1 = new Jazigo { CodigoIdentificador = "J1", Quadra = "3", Ala = "A", NumeroLote = "12", Cemiterio = cemiterio };
        var jazigo2 = new Jazigo { CodigoIdentificador = "J2", Quadra = "5", NumeroLote = "1", Cemiterio = cemiterio };
        var gaveta1 = new Gaveta { Jazigo = jazigo1, Numero = "01", ClassificacaoEspaco = classificacao };
        var gaveta2 = new Gaveta { Jazigo = jazigo1, Numero = "02" };
        var gavetaOutro = new Gaveta { Jazigo = jazigo2, Numero = "01" };
        var publicaLivre = new GavetaPublica { Setor = "Setor A", Quadra = "1", Lote = "1", NumeroGaveta = "01", Ocupada = false, Cemiterio = cemiterio };
        var publicaOcupada = new GavetaPublica { Setor = "Setor B", Quadra = "2", Lote = "2", NumeroGaveta = "02", Ocupada = true };
        var vinculo = new JazigoProprietario { Jazigo = jazigo1, Responsavel = responsavel, Ativo = true, TipoVinculo = TipoVinculoJazigo.Titular };
        db.JazigoProprietarios.Add(vinculo);
        db.JazigoProprietarios.Add(new JazigoProprietario { Jazigo = jazigo2, Responsavel = outro, Ativo = true, TipoVinculo = TipoVinculoJazigo.Titular });
        db.AddRange(gaveta1, gaveta2, gavetaOutro, publicaLivre, publicaOcupada);
        await db.SaveChangesAsync();

        return (factory, responsavel.Id, outro.Id, vinculo.Id, gaveta1.Id, gaveta2.Id, gavetaOutro.Id, publicaLivre.Id, publicaOcupada.Id);
    }

    [Fact]
    public async Task ResponsavelComDuasParticulares_RetornaAmbasMaisPublicasLivres()
    {
        var (factory, responsavelId, _, _, g1, g2, gavetaOutro, publicaLivre, publicaOcupada) = await CriarBaseAsync();
        var servico = new GavetaSelecaoService(factory);

        var opcoes = await servico.ListarOpcoesAsync(responsavelId);
        var chaves = opcoes.Select(o => o.Chave).ToList();

        Assert.Contains($"P:{g1}", chaves);
        Assert.Contains($"P:{g2}", chaves);
        Assert.Contains($"U:{publicaLivre}", chaves);
        Assert.DoesNotContain($"P:{gavetaOutro}", chaves);
        Assert.DoesNotContain($"U:{publicaOcupada}", chaves);
    }

    [Fact]
    public async Task ResponsavelComUmaParticularElegivel_RetornaSoEla()
    {
        var (factory, responsavelId, _, _, g1, g2, _, publicaLivre, _) = await CriarBaseAsync();
        using (var db = factory.CreateDbContext())
        {
            var gaveta2 = await db.Gavetas.FindAsync(g2);
            gaveta2!.FalecidoId = 999;
            await db.SaveChangesAsync();
        }

        var servico = new GavetaSelecaoService(factory);
        var opcoes = await servico.ListarOpcoesAsync(responsavelId);
        var chaves = opcoes.Select(o => o.Chave).ToList();

        Assert.Contains($"P:{g1}", chaves);
        Assert.DoesNotContain($"P:{g2}", chaves);
        Assert.Contains($"U:{publicaLivre}", chaves);
    }

    [Fact]
    public async Task GavetaDeOutroResponsavel_NuncaAparece()
    {
        var (factory, responsavelId, outroResponsavelId, _, g1, _, gavetaOutro, _, _) = await CriarBaseAsync();
        var servico = new GavetaSelecaoService(factory);

        var doResponsavel = (await servico.ListarOpcoesAsync(responsavelId)).Select(o => o.Chave).ToList();
        var doOutro = (await servico.ListarOpcoesAsync(outroResponsavelId)).Select(o => o.Chave).ToList();

        Assert.DoesNotContain($"P:{gavetaOutro}", doResponsavel);
        Assert.DoesNotContain($"P:{g1}", doOutro);
    }

    [Fact]
    public async Task VinculoInativo_NaoAparece()
    {
        var (factory, responsavelId, _, vinculoId, g1, _, _, _, _) = await CriarBaseAsync();
        using (var db = factory.CreateDbContext())
        {
            var vinculo = await db.JazigoProprietarios.FindAsync(vinculoId);
            vinculo!.Ativo = false;
            await db.SaveChangesAsync();
        }

        var servico = new GavetaSelecaoService(factory);
        var opcoes = await servico.ListarOpcoesAsync(responsavelId);

        Assert.DoesNotContain(opcoes, o => o.Chave == $"P:{g1}");
    }

    [Fact]
    public async Task SomenteLivres_FiltraOcupadas()
    {
        var (factory, responsavelId, _, _, g1, g2, _, publicaLivre, publicaOcupada) = await CriarBaseAsync();
        using (var db = factory.CreateDbContext())
        {
            var gaveta2 = await db.Gavetas.FindAsync(g2);
            gaveta2!.FalecidoId = 999;
            await db.SaveChangesAsync();
        }

        var servico = new GavetaSelecaoService(factory);

        var livres = await servico.ListarOpcoesAsync(responsavelId, incluirPublicas: true, somenteLivres: true);
        var todas = await servico.ListarOpcoesAsync(responsavelId, incluirPublicas: true, somenteLivres: false);

        Assert.DoesNotContain(livres, o => o.Chave == $"P:{g2}");
        Assert.DoesNotContain(livres, o => o.Chave == $"U:{publicaOcupada}");
        Assert.Contains(todas, o => o.Chave == $"P:{g2}");
        Assert.Contains(todas, o => o.Chave == $"U:{publicaOcupada}");
        Assert.Contains(livres, o => o.Chave == $"P:{g1}");
        Assert.Contains(livres, o => o.Chave == $"U:{publicaLivre}");
    }

    [Fact]
    public async Task SemResponsavel_RetornaSoPublicas()
    {
        var (factory, _, _, _, _, _, _, publicaLivre, _) = await CriarBaseAsync();
        var servico = new GavetaSelecaoService(factory);

        var opcoes = await servico.ListarOpcoesAsync(null);
        var chaves = opcoes.Select(o => o.Chave).ToList();

        Assert.Contains($"U:{publicaLivre}", chaves);
        Assert.All(chaves, c => Assert.StartsWith("U:", c));
    }

    [Fact]
    public async Task ResolverAsync_Particular_CarregaLocal()
    {
        var (factory, _, _, _, g1, _, _, _, _) = await CriarBaseAsync();
        var servico = new GavetaSelecaoService(factory);

        var local = await servico.ResolverAsync($"P:{g1}");

        Assert.NotNull(local);
        Assert.Equal("Particular", local!.Tipo);
        Assert.Equal("Bela Vista", local.Cemiterio);
        Assert.Equal("3", local.Quadra);
        Assert.Equal("A", local.Ala);
        Assert.Equal("12", local.Lote);
        Assert.Equal("01", local.Gaveta);
        Assert.Equal("Tipo A", local.Classificacao);
    }

    [Fact]
    public async Task ResolverAsync_ChaveInvalida_RetornaNull()
    {
        var factory = new TestDbContextFactory("gaveta-selecao-" + Guid.NewGuid());
        var servico = new GavetaSelecaoService(factory);

        Assert.Null(await servico.ResolverAsync("X:999"));
        Assert.Null(await servico.ResolverAsync(""));
    }
}
