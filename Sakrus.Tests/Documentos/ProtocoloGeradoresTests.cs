using System.Text;
using Sakrus.Core.Entities;
using Sakrus.Services.Documentos;

namespace Sakrus.Tests.Documentos;

public class ProtocoloGeradoresTests
{
    private static async Task<(TestDbContextFactory factory, int protocoloId, ConfiguracaoInstitucional instituicao)> CriarBaseAsync(
        bool geraRegularizacao = true,
        bool comJazigo = true)
    {
        var factory = new TestDbContextFactory("protocolo-gerador-" + Guid.NewGuid());
        using var db = factory.CreateDbContext();

        var responsavel = new Responsavel
        {
            Nome = "João da Silva",
            CPF = "12345678901",
            RG = "1234567",
            Telefone = "77999999999",
            Endereco = "Rua A, 10"
        };
        var assunto = new AssuntoProtocolo { Nome = "Regularização de lote", Ativo = true, GeraRegularizacao = geraRegularizacao };
        var jazigo = comJazigo
            ? new Jazigo
            {
                CodigoIdentificador = "J1",
                Quadra = "3",
                Ala = "A",
                NumeroLote = "12",
                Cemiterio = new Cemiterio { Nome = "Bela Vista" }
            }
            : null;
        var protocolo = new Protocolo
        {
            Ano = 2026,
            Sequencia = 1,
            Numero = "001/2026",
            AssuntoProtocolo = assunto,
            Responsavel = responsavel,
            Jazigo = jazigo,
            Observacao = "Observação do protocolo",
            DataAbertura = new DateTime(2026, 9, 30)
        };
        db.Protocolos.Add(protocolo);
        await db.SaveChangesAsync();

        return (factory, protocolo.Id, new ConfiguracaoInstitucional());
    }

    private static ContextoEmissao CriarContexto(int protocoloId, ConfiguracaoInstitucional instituicao, string? numero = null)
        => new(protocoloId, new CamposValores(), instituicao, numero, new DateTime(2026, 9, 30, 10, 0, 0));

    [Fact]
    public async Task ProtocoloAdministrativo_GeraPdf_Valido()
    {
        var (factory, protocoloId, instituicao) = await CriarBaseAsync();
        var gerador = new ProtocoloAdministrativoGerador(factory);

        var pdf = await gerador.GerarAsync(CriarContexto(protocoloId, instituicao, "001/2026"));

        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));
    }

    [Fact]
    public async Task ProtocoloAdministrativo_ProtocoloInexistente_Bloqueia()
    {
        var factory = new TestDbContextFactory("protocolo-gerador-" + Guid.NewGuid());
        var gerador = new ProtocoloAdministrativoGerador(factory);

        var ex = await Assert.ThrowsAsync<DocumentoBloqueadoException>(
            () => gerador.GerarAsync(CriarContexto(999, new ConfiguracaoInstitucional())));

        Assert.Contains("Protocolo", ex.Message);
    }

    [Fact]
    public async Task FolhaDespacho_GeraPdf_Valido()
    {
        var (factory, protocoloId, instituicao) = await CriarBaseAsync();
        var gerador = new FolhaDespachoGerador(factory);

        var pdf = await gerador.GerarAsync(CriarContexto(protocoloId, instituicao, "001/2026"));

        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));
    }

    [Fact]
    public async Task DocumentoRegularizacao_GeraPdf_Valido()
    {
        var (factory, protocoloId, instituicao) = await CriarBaseAsync(geraRegularizacao: true, comJazigo: true);
        var gerador = new DocumentoRegularizacaoGerador(factory);

        var preCondicao = await gerador.ValidarPreCondicoesAsync(protocoloId);
        Assert.Null(preCondicao);

        var pdf = await gerador.GerarAsync(CriarContexto(protocoloId, instituicao, "001/2026"));

        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));
    }

    [Fact]
    public async Task DocumentoRegularizacao_AssuntoSemRegularizacao_Bloqueia()
    {
        var (factory, protocoloId, _) = await CriarBaseAsync(geraRegularizacao: false, comJazigo: true);
        var gerador = new DocumentoRegularizacaoGerador(factory);

        var preCondicao = await gerador.ValidarPreCondicoesAsync(protocoloId);

        Assert.NotNull(preCondicao);
        Assert.Contains("regularização", preCondicao, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DocumentoRegularizacao_SemJazigo_Bloqueia()
    {
        var (factory, protocoloId, _) = await CriarBaseAsync(geraRegularizacao: true, comJazigo: false);
        var gerador = new DocumentoRegularizacaoGerador(factory);

        var preCondicao = await gerador.ValidarPreCondicoesAsync(protocoloId);

        Assert.NotNull(preCondicao);
        Assert.Contains("lote", preCondicao, StringComparison.OrdinalIgnoreCase);
    }
}
