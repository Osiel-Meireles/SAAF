using System.Text;
using Sakrus.Core.Entities;
using Sakrus.Services.Documentos;

namespace Sakrus.Tests.Documentos;

public class TermoSepultamentoGeradorTests
{
    private static async Task<(TestDbContextFactory factory, int atendimentoId, ConfiguracaoInstitucional instituicao)> CriarBaseAsync(bool comEmpresa = true, bool vinculaGaveta = true)
    {
        var factory = new TestDbContextFactory("termo-sepultamento-" + Guid.NewGuid());
        using var db = factory.CreateDbContext();

        var cemiterio = new Cemiterio { Nome = "Bela Vista" };
        var classificacao = new ClassificacaoEspaco { Nome = "Tipo A", Natureza = NaturezaEspaco.Gaveta };
        var responsavel = new Responsavel
        {
            Nome = "João da Silva",
            CPF = "12345678901",
            RG = "1234567",
            OrgaoEmissor = "SSP/BA",
            Telefone = "77999999999",
            Endereco = "Rua A, 10",
            Profissao = "Comerciante",
            EstadoCivil = "Casado(a)"
        };
        var falecido = new Falecido
        {
            Nome = "Maria da Silva",
            DataFalecimento = new DateTime(2026, 4, 5),
            EstadoCivil = "Casada",
            Sexo = "Feminino",
            LocalCorpo = "Hospital Municipal",
            CausaMorte = CausaMorte.Natural
        };
        var funeraria = new Funeraria { Nome = "Funerária X" };
        var jazigo = new Jazigo { CodigoIdentificador = "J1", Quadra = "3", Ala = "A", NumeroLote = "12", Cemiterio = cemiterio };
        var gaveta = new Gaveta { Jazigo = jazigo, Numero = "02", ClassificacaoEspaco = classificacao };
        if (vinculaGaveta)
        {
            gaveta.Falecido = falecido;
            gaveta.DataSepultamento = new DateTime(2026, 4, 8);
        }
        var atendimento = new Atendimento
        {
            Responsavel = responsavel,
            Falecido = falecido,
            Funeraria = funeraria,
            EmpresaExecutora = comEmpresa ? funeraria : null,
            DataSepultamento = new DateTime(2026, 4, 8),
            HorarioSepultamento = new TimeSpan(10, 30, 0),
            LocalFalecimento = "Hospital Municipal",
            NumeroDeclaracaoObito = "DO-12345",
            GrauParentesco = "Filho(a)",
            LiberacaoMunicipal = true,
            TipoAtendimento = TipoAtendimentoFunerario.Particular
        };
        db.AddRange(atendimento, gaveta);
        await db.SaveChangesAsync();

        return (factory, atendimento.Id, new ConfiguracaoInstitucional());
    }

    private static ContextoEmissao CriarContexto(int atendimentoId, ConfiguracaoInstitucional instituicao)
        => new(atendimentoId, new CamposValores(), instituicao, null, new DateTime(2026, 4, 8, 10, 0, 0));

    [Fact]
    public async Task GeraPdf_Valido()
    {
        var (factory, atendimentoId, instituicao) = await CriarBaseAsync();
        var gerador = new TermoSepultamentoGerador(factory);

        var pdf = await gerador.GerarAsync(CriarContexto(atendimentoId, instituicao));

        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));
    }

    [Fact]
    public async Task SemEmpresaExecutora_Bloqueia()
    {
        var (factory, atendimentoId, instituicao) = await CriarBaseAsync(comEmpresa: false);
        var gerador = new TermoSepultamentoGerador(factory);

        var ex = await Assert.ThrowsAsync<DocumentoBloqueadoException>(
            () => gerador.GerarAsync(CriarContexto(atendimentoId, instituicao)));

        Assert.Contains("empresa executora", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SemLocalSepultamento_Bloqueia()
    {
        var (factory, atendimentoId, instituicao) = await CriarBaseAsync(vinculaGaveta: false);
        var gerador = new TermoSepultamentoGerador(factory);

        var ex = await Assert.ThrowsAsync<DocumentoBloqueadoException>(
            () => gerador.GerarAsync(CriarContexto(atendimentoId, instituicao)));

        Assert.Contains("local de sepultamento", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AtendimentoInexistente_Bloqueia()
    {
        var factory = new TestDbContextFactory("termo-sepultamento-" + Guid.NewGuid());
        var gerador = new TermoSepultamentoGerador(factory);

        var ex = await Assert.ThrowsAsync<DocumentoBloqueadoException>(
            () => gerador.GerarAsync(CriarContexto(999, new ConfiguracaoInstitucional())));

        Assert.Contains("Atendimento", ex.Message);
    }
}
