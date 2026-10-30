using System.Text;
using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;
using Sakrus.Services.Documentos;
using Xunit;

namespace Sakrus.Tests.Documentos;

public class NovosGeradoresDocumentosTests
{
    private static async Task<(TestDbContextFactory factory, int falecidoId, int atendimentoId, int jazigoId, int titularId, int empresaId)> CriarDadosTesteAsync(string sufixo)
    {
        var factory = new TestDbContextFactory("novos-geradores-" + sufixo + "-" + Guid.NewGuid());
        using var db = factory.CreateDbContext();

        db.ConfiguracoesInstitucionais.Add(new ConfiguracaoInstitucional
        {
            NomeCoordenador = "Coordenador Teste",
            CargoCoordenador = "Coordenador(a) da CAAFE",
            NomePresidenteConselho = "Presidente Teste",
            MunicipioEmissao = "Luís Eduardo Magalhães",
            UfEmissao = "BA"
        });

        var cemiterio = new Cemiterio { Nome = "Cemitério Municipal" };
        var classificacao = new ClassificacaoEspaco { Nome = "Jazigo Especial", Natureza = NaturezaEspaco.Jazigo };
        var modelo = new ModeloJazigo { Nome = "Padrão" };

        var titular = new Responsavel
        {
            Nome = "Responsável Geral",
            CPF = "12345678901",
            RG = "998877",
            OrgaoEmissor = "SSP/BA",
            Telefone = "77 98888-1111",
            Endereco = "Rua Central, 123",
            Bairro = "Centro",
            Municipio = "Luís Eduardo Magalhães",
            Uf = "BA"
        };
        db.Responsaveis.Add(titular);

        var empresa = new Funeraria
        {
            Nome = "HPF SERVICE LTDA",
            CNPJ = "12.345.678/0001-90",
            Ativo = true,
            EhExecutora = true
        };
        db.Funerarias.Add(empresa);

        var falecido = new Falecido
        {
            Nome = "Falecido Teste",
            DataFalecimento = new DateTime(2026, 3, 10),
            DataSepultamento = new DateTime(2026, 3, 11),
            CausaMorte = CausaMorte.Acidente,
            CausaMorteDescricao = "Acidente automobilístico na BR-020",
            CartorioRegistro = "1º Ofício",
            MunicipioCartorio = "LEM",
            MatriculaObito = "12345601552026400001000000123",
            Sexo = "Masculino"
        };
        db.Falecidos.Add(falecido);

        var jazigo = new Jazigo
        {
            CodigoIdentificador = "J-101",
            Quadra = "4",
            Ala = "B",
            NumeroLote = "15",
            Largura = 2.5m,
            Comprimento = 3.0m,
            Cemiterio = cemiterio,
            ClassificacaoEspaco = classificacao,
            ModeloJazigo = modelo
        };
        db.Jazigos.Add(jazigo);

        await db.SaveChangesAsync();

        var vinculo = new JazigoProprietario
        {
            JazigoId = jazigo.Id,
            ResponsavelId = titular.Id,
            TipoVinculo = TipoVinculoJazigo.Titular,
            Ativo = true,
            DataAquisicao = new DateTime(2026, 1, 1)
        };
        db.JazigoProprietarios.Add(vinculo);

        var gaveta = new Gaveta
        {
            JazigoId = jazigo.Id,
            Numero = "01",
            ClassificacaoEspaco = classificacao,
            FalecidoId = falecido.Id,
            DataSepultamento = new DateTime(2026, 3, 11)
        };
        db.Gavetas.Add(gaveta);
        falecido.JazigoId = jazigo.Id;

        var atendimento = new Atendimento
        {
            FalecidoId = falecido.Id,
            ResponsavelId = titular.Id,
            FunerariaId = empresa.Id,
            EmpresaExecutoraId = empresa.Id,
            TipoAtendimento = TipoAtendimentoFunerario.Particular,
            DataSepultamento = new DateTime(2026, 3, 11),
            HorarioSepultamento = new TimeSpan(16, 0, 0),
            NumeroDeclaracaoObito = "DO-998877"
        };
        db.Atendimentos.Add(atendimento);

        await db.SaveChangesAsync();

        return (factory, falecido.Id, atendimento.Id, jazigo.Id, titular.Id, empresa.Id);
    }

    [Fact]
    public async Task Documento07_ExumacaoTransferencia_DeveGerarPdfValido()
    {
        var (factory, falecidoId, _, _, _, empresaId) = await CriarDadosTesteAsync("doc7");
        var gerador = new ExumacaoTransferenciaGerador(factory);

        var campos = new CamposValores();
        campos["empresaExecutoraId"] = empresaId.ToString();
        campos["destinoTipo"] = "Ossuário";
        campos["destinoDescricao"] = "Ossuário Municipal Geral - Nicho 12";

        using var db = factory.CreateDbContext();
        var instituicao = await db.ConfiguracoesInstitucionais.FirstAsync();
        var ctx = new ContextoEmissao(falecidoId, campos, instituicao, "007/2026", DateTime.Today);

        var pdf = await gerador.GerarAsync(ctx);

        Assert.NotNull(pdf);
        Assert.True(pdf.Length > 1000);
        var cabecalhoPdf = Encoding.ASCII.GetString(pdf.Take(10).ToArray());
        Assert.StartsWith("%PDF-", cabecalhoPdf);
    }

    [Fact]
    public async Task Documento08_ExumacaoOutroCemiterio_DeveGerarPdfValido()
    {
        var (factory, falecidoId, _, _, _, empresaId) = await CriarDadosTesteAsync("doc8");
        var gerador = new ExumacaoOutroCemiterioGerador(factory);

        var campos = new CamposValores();
        campos["empresaExecutoraId"] = empresaId.ToString();
        campos["cemiterioDestino"] = "Cemitério da Saudade";
        campos["municipioDestino"] = "Barreiras";
        campos["ufDestino"] = "BA";

        using var db = factory.CreateDbContext();
        var instituicao = await db.ConfiguracoesInstitucionais.FirstAsync();
        var ctx = new ContextoEmissao(falecidoId, campos, instituicao, "008/2026", DateTime.Today);

        var pdf = await gerador.GerarAsync(ctx);

        Assert.NotNull(pdf);
        Assert.True(pdf.Length > 1000);
        var cabecalhoPdf = Encoding.ASCII.GetString(pdf.Take(10).ToArray());
        Assert.StartsWith("%PDF-", cabecalhoPdf);
    }

    [Fact]
    public async Task Documento09_ExumacaoJudicial_DeveGerarPdfValido()
    {
        var (factory, falecidoId, _, _, _, empresaId) = await CriarDadosTesteAsync("doc9");
        var gerador = new ExumacaoJudicialGerador(factory);

        var campos = new CamposValores();
        campos["numeroProcesso"] = "0001234-56.2026.8.05.0154";
        campos["autoridadeJuizo"] = "Vara Crime de Luís Eduardo Magalhães";
        campos["tipoDocumentoJudicial"] = "Mandado de Exumação";
        campos["destinoDeterminado"] = "Departamento de Polícia Técnica (DPT)";
        campos["empresaExecutoraId"] = empresaId.ToString();

        using var db = factory.CreateDbContext();
        var instituicao = await db.ConfiguracoesInstitucionais.FirstAsync();
        var ctx = new ContextoEmissao(falecidoId, campos, instituicao, "009/2026", DateTime.Today);

        var pdf = await gerador.GerarAsync(ctx);

        Assert.NotNull(pdf);
        Assert.True(pdf.Length > 1000);
        var cabecalhoPdf = Encoding.ASCII.GetString(pdf.Take(10).ToArray());
        Assert.StartsWith("%PDF-", cabecalhoPdf);
    }

    [Fact]
    public async Task Documento10_GuiaSepultamentoDespojos_DeveGerarPdfValido()
    {
        var (factory, falecidoId, _, _, _, empresaId) = await CriarDadosTesteAsync("doc10");
        var gerador = new GuiaSepultamentoDespojosGerador(factory);

        var campos = new CamposValores();
        campos["empresaExecutoraId"] = empresaId.ToString();
        campos["cemiterioOrigem"] = "Cemitério de Origem";
        campos["municipioOrigem"] = "Brasília - DF";
        campos["cemiterioDestino"] = "Cemitério Municipal LEM";
        campos["quadra"] = "02";
        campos["lote"] = "10";

        using var db = factory.CreateDbContext();
        var instituicao = await db.ConfiguracoesInstitucionais.FirstAsync();
        var ctx = new ContextoEmissao(falecidoId, campos, instituicao, "010/2026", DateTime.Today);

        var pdf = await gerador.GerarAsync(ctx);

        Assert.NotNull(pdf);
        Assert.True(pdf.Length > 1000);
        var cabecalhoPdf = Encoding.ASCII.GetString(pdf.Take(10).ToArray());
        Assert.StartsWith("%PDF-", cabecalhoPdf);
    }

    [Fact]
    public async Task Documento10A_AutorizacaoTransladacao_DeveGerarPdfValido()
    {
        var (factory, falecidoId, _, _, _, _) = await CriarDadosTesteAsync("doc10a");
        var gerador = new AutorizacaoTransladacaoGerador(factory);

        var campos = new CamposValores();
        campos["cemiterioOrigem"] = "Cemitério de São Paulo";
        campos["municipioOrigem"] = "São Paulo - SP";
        campos["cemiterioDestino"] = "Cemitério Municipal LEM";

        using var db = factory.CreateDbContext();
        var instituicao = await db.ConfiguracoesInstitucionais.FirstAsync();
        var ctx = new ContextoEmissao(falecidoId, campos, instituicao, "010-A/2026", DateTime.Today);

        var pdf = await gerador.GerarAsync(ctx);

        Assert.NotNull(pdf);
        Assert.True(pdf.Length > 1000);
        var cabecalhoPdf = Encoding.ASCII.GetString(pdf.Take(10).ToArray());
        Assert.StartsWith("%PDF-", cabecalhoPdf);
    }

    [Fact]
    public async Task Documento12_TermoConcessao_DeveGerarPdfValido()
    {
        var (factory, _, _, jazigoId, _, _) = await CriarDadosTesteAsync("doc12");
        var gerador = new TermoConcessaoGerador(factory);

        var preCond = await gerador.ValidarPreCondicoesAsync(jazigoId);
        Assert.Null(preCond);

        var campos = new CamposValores();
        campos["revestimento"] = "Granito Polido";
        campos["testemunha1"] = "Testemunha Um - 000.111.222-33";
        campos["testemunha2"] = "Testemunha Dois - 444.555.666-77";

        using var db = factory.CreateDbContext();
        var instituicao = await db.ConfiguracoesInstitucionais.FirstAsync();
        var ctx = new ContextoEmissao(jazigoId, campos, instituicao, "CTR-012/2026", DateTime.Today);

        var pdf = await gerador.GerarAsync(ctx);

        Assert.NotNull(pdf);
        Assert.True(pdf.Length > 1000);
        var cabecalhoPdf = Encoding.ASCII.GetString(pdf.Take(10).ToArray());
        Assert.StartsWith("%PDF-", cabecalhoPdf);
    }

    [Fact]
    public async Task Documento13_EncaminhamentoTermoConcessao_DeveGerarPdfValido()
    {
        var (factory, _, _, jazigoId, _, _) = await CriarDadosTesteAsync("doc13");
        var gerador = new EncaminhamentoTermoConcessaoGerador(factory);

        var preCond = await gerador.ValidarPreCondicoesAsync(jazigoId);
        Assert.Null(preCond);

        var campos = new CamposValores();

        using var db = factory.CreateDbContext();
        var instituicao = await db.ConfiguracoesInstitucionais.FirstAsync();
        var ctx = new ContextoEmissao(jazigoId, campos, instituicao, "CTR-012/2026", DateTime.Today);

        var pdf = await gerador.GerarAsync(ctx);

        Assert.NotNull(pdf);
        Assert.True(pdf.Length > 1000);
        var cabecalhoPdf = Encoding.ASCII.GetString(pdf.Take(10).ToArray());
        Assert.StartsWith("%PDF-", cabecalhoPdf);
    }

    [Fact]
    public async Task CausaMorteDescricao_QuandoPreenchida_DeveAparecerEmDadosComuns()
    {
        var (factory, falecidoId, _, _, _, _) = await CriarDadosTesteAsync("causa-desc");
        using var db = factory.CreateDbContext();

        var falecido = await DadosComuns.FalecidoAsync(db, falecidoId);

        Assert.Equal("Acidente automobilístico na BR-020", falecido.CausaObito);
    }
}
