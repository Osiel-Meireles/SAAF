using System.Text;
using Sakrus.Core.Entities;
using Sakrus.Services.Documentos;

namespace Sakrus.Tests.Documentos;

public class FolhaRostoGeradorTests
{
    [Fact]
    public void MontarLinhas_Sepultamento_CalculaExumacao()
    {
        var jazigo = new Jazigo { CodigoIdentificador = "J1" };
        jazigo.Gavetas.Add(new Gaveta { Numero = "01", Falecido = new Falecido { Nome = "João da Silva", DataSepultamento = new DateTime(2026, 4, 8) } });
        jazigo.Gavetas.Add(new Gaveta { Numero = "02" });

        var linhas = FolhaRostoGerador.MontarLinhas(jazigo);

        Assert.Equal(2, linhas.Count);
        Assert.Equal("João da Silva", linhas[0].Falecido);
        Assert.Equal(new DateTime(2026, 4, 8), linhas[0].DataSepultamento);
        Assert.Equal(new DateTime(2031, 4, 8), linhas[0].DataExumacao);
        Assert.Equal("Livre", linhas[1].Falecido);
        Assert.Null(linhas[1].DataSepultamento);
        Assert.Null(linhas[1].DataExumacao);
    }

    [Fact]
    public void MontarLinhas_GavetaOcupadaSemDataSepultamento_MostraTracos()
    {
        var jazigo = new Jazigo { CodigoIdentificador = "J1" };
        jazigo.Gavetas.Add(new Gaveta { Numero = "01", Falecido = new Falecido { Nome = "Maria" } });

        var linhas = FolhaRostoGerador.MontarLinhas(jazigo);

        Assert.Single(linhas);
        Assert.Equal("Maria", linhas[0].Falecido);
        Assert.Null(linhas[0].DataSepultamento);
        Assert.Null(linhas[0].DataExumacao);
    }

    [Fact]
    public void MontarLinhas_SemGavetas_UsaFalecidosSepultados()
    {
        var jazigo = new Jazigo { CodigoIdentificador = "J1" };
        jazigo.Falecidos.Add(new Falecido { Nome = "Maria", Status = StatusFalecido.Sepultado, DataSepultamento = new DateTime(2026, 5, 1) });
        jazigo.Falecidos.Add(new Falecido { Nome = "José", Status = StatusFalecido.NaoSepultado });

        var linhas = FolhaRostoGerador.MontarLinhas(jazigo);

        Assert.Single(linhas);
        Assert.Equal("s/n", linhas[0].Numero);
        Assert.Equal("Maria", linhas[0].Falecido);
        Assert.Equal(new DateTime(2031, 5, 1), linhas[0].DataExumacao);
    }

    [Fact]
    public void MontarLinhas_Nada_RetornaLinhaNenhumaGaveta()
    {
        var jazigo = new Jazigo { CodigoIdentificador = "J1" };

        var linhas = FolhaRostoGerador.MontarLinhas(jazigo);

        Assert.Single(linhas);
        Assert.Equal("Nenhuma gaveta cadastrada", linhas[0].Falecido);
    }

    [Fact]
    public async Task GeraPdf_Valido()
    {
        var factory = new TestDbContextFactory("folha-rosto-" + Guid.NewGuid());
        using var db = factory.CreateDbContext();

        var cemiterio = new Cemiterio { Nome = "Bela Vista" };
        var classificacao = new ClassificacaoEspaco { Nome = "Jazigo Tipo A", Natureza = NaturezaEspaco.Jazigo };
        var responsavel = new Responsavel { Nome = "João da Silva", CPF = "12345678901" };
        var jazigo = new Jazigo
        {
            CodigoIdentificador = "J1",
            Quadra = "3",
            Ala = "A",
            NumeroLote = "12",
            Cemiterio = cemiterio,
            ClassificacaoEspaco = classificacao
        };
        jazigo.Gavetas.Add(new Gaveta { Numero = "01", Falecido = new Falecido { Nome = "Maria", DataSepultamento = new DateTime(2026, 4, 8) } });
        jazigo.Gavetas.Add(new Gaveta { Numero = "02" });
        db.JazigoProprietarios.Add(new JazigoProprietario { Jazigo = jazigo, Responsavel = responsavel, Ativo = true, TipoVinculo = TipoVinculoJazigo.Titular });
        db.Jazigos.Add(jazigo);
        await db.SaveChangesAsync();

        var gerador = new FolhaRostoGerador(factory);
        var ctx = new ContextoEmissao(jazigo.Id, new CamposValores(), new ConfiguracaoInstitucional(), null, new DateTime(2026, 9, 30, 10, 0, 0));

        var pdf = await gerador.GerarAsync(ctx);

        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));
    }

    [Fact]
    public async Task JazigoInexistente_Bloqueia()
    {
        var factory = new TestDbContextFactory("folha-rosto-" + Guid.NewGuid());
        var gerador = new FolhaRostoGerador(factory);

        var ex = await Assert.ThrowsAsync<DocumentoBloqueadoException>(
            () => gerador.GerarAsync(new ContextoEmissao(999, new CamposValores(), new ConfiguracaoInstitucional(), null, new DateTime(2026, 9, 30, 10, 0, 0))));

        Assert.Contains("Jazigo", ex.Message);
    }
}
