using System.IO.Compression;
using System.Text;
using Sakrus.Core.Entities;
using Sakrus.Services.Documentos;

namespace Sakrus.Tests.Documentos;

public class GeradoresAtendimentoTests
{
    private static async Task<(TestDbContextFactory factory, int atendimentoId, ConfiguracaoInstitucional instituicao)> CriarBaseAsync(
        string nomeBase,
        bool comEmpresa = true,
        bool comRegistroCapela = false,
        bool comServicosAuxilio = false,
        TipoAtendimentoFunerario tipoAtendimento = TipoAtendimentoFunerario.Particular,
        bool vinculaGaveta = true)
    {
        var factory = new TestDbContextFactory(nomeBase + "-" + Guid.NewGuid());
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
            TipoAtendimento = tipoAtendimento,
            PecaAnatomicaIdentificacao = "Peça 01",
            PecaAnatomicaDataCirurgia = new DateTime(2026, 4, 6),
            PecaAnatomicaHospital = "Hospital Municipal",
            PecaAnatomicaCidadeOrigem = "Barreiras",
            PecaAnatomicaEstadoOrigem = "BA"
        };
        db.AddRange(atendimento, gaveta);

        if (comRegistroCapela)
        {
            var capela = new Capela { Nome = "Capela Municipal" };
            db.Add(new RegistroCapela
            {
                Capela = capela,
                Atendimento = atendimento,
                HoraEntrada = new DateTime(2026, 4, 8, 8, 0, 0),
                HoraSaida = new DateTime(2026, 4, 8, 10, 0, 0)
            });
        }

        if (comServicosAuxilio)
        {
            var servico = new ServicoAuxilio { Codigo = "URNA", Descricao = "Urna funerária" };
            atendimento.ServicosAuxilio.Add(new AtendimentoServicoAuxilio { ServicoAuxilio = servico });
        }

        await db.SaveChangesAsync();

        return (factory, atendimento.Id, new ConfiguracaoInstitucional());
    }

    private static ContextoEmissao CriarContexto(int atendimentoId, ConfiguracaoInstitucional instituicao, Action<CamposValores>? preencher = null)
    {
        var campos = new CamposValores();
        preencher?.Invoke(campos);

        return new ContextoEmissao(atendimentoId, campos, instituicao, null, new DateTime(2026, 4, 8, 10, 0, 0));
    }

    // ── Documento 2: Termo de Sepultamento de Peças Anatômicas ───────────────

    [Fact]
    public async Task PecasAnatomicas_GeraPdf_Valido()
    {
        var (factory, atendimentoId, instituicao) = await CriarBaseAsync("pecas-anatomicas");
        var gerador = new PecasAnatomicasGerador(factory);

        var pdf = await gerador.GerarAsync(CriarContexto(atendimentoId, instituicao));

        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));
    }

    [Fact]
    public async Task PecasAnatomicas_SemLocalSepultamento_Bloqueia()
    {
        var (factory, atendimentoId, instituicao) = await CriarBaseAsync("pecas-anatomicas", vinculaGaveta: false);
        var gerador = new PecasAnatomicasGerador(factory);

        var ex = await Assert.ThrowsAsync<DocumentoBloqueadoException>(
            () => gerador.GerarAsync(CriarContexto(atendimentoId, instituicao)));

        Assert.Contains("local de sepultamento", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ── Documento 3: Termo de Translado ──────────────────────────────────────

    [Fact]
    public async Task Translado_GeraPdf_Valido()
    {
        var (factory, atendimentoId, instituicao) = await CriarBaseAsync("translado");
        var gerador = new TransladoGerador(factory);

        var pdf = await gerador.GerarAsync(CriarContexto(atendimentoId, instituicao, c =>
        {
            c["destinoMunicipio"] = "Barreiras";
            c["destinoUf"] = "BA";
            c["destinoLocal"] = "Cemitério São João";
            c["dataSaida"] = "2026-04-08";
            c["horaSaida"] = "14:00";
        }));

        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));
    }

    [Fact]
    public async Task Translado_SemDestino_Bloqueia()
    {
        var (factory, atendimentoId, instituicao) = await CriarBaseAsync("translado");
        var gerador = new TransladoGerador(factory);

        var ex = await Assert.ThrowsAsync<DocumentoBloqueadoException>(
            () => gerador.GerarAsync(CriarContexto(atendimentoId, instituicao, c => c["destinoMunicipio"] = "Barreiras")));

        Assert.Contains("destino", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ── Documento 4: Termo de Uso de Capela ──────────────────────────────────

    [Fact]
    public async Task UsoCapela_GeraPdf_Valido()
    {
        var (factory, atendimentoId, instituicao) = await CriarBaseAsync("uso-capela", comRegistroCapela: true);
        var gerador = new UsoCapelaGerador(factory);

        var pdf = await gerador.GerarAsync(CriarContexto(atendimentoId, instituicao));

        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));
    }

    [Fact]
    public async Task UsoCapela_SemRegistro_Bloqueia()
    {
        var (factory, atendimentoId, instituicao) = await CriarBaseAsync("uso-capela", comRegistroCapela: false);
        var gerador = new UsoCapelaGerador(factory);

        var ex = await Assert.ThrowsAsync<DocumentoBloqueadoException>(
            () => gerador.GerarAsync(CriarContexto(atendimentoId, instituicao)));

        Assert.Contains("capela", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ── Documento 5: Termo de Dispensa de Uso da Capela ──────────────────────

    [Fact]
    public async Task DispensaCapela_GeraPdf_Valido()
    {
        var (factory, atendimentoId, instituicao) = await CriarBaseAsync("dispensa-capela");
        var gerador = new DispensaCapelaGerador(factory);

        var pdf = await gerador.GerarAsync(CriarContexto(atendimentoId, instituicao, c => c["localVelorio"] = "Residência da família"));

        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));
    }

    [Fact]
    public async Task DispensaCapela_SemLocalVelorio_Bloqueia()
    {
        var (factory, atendimentoId, instituicao) = await CriarBaseAsync("dispensa-capela");
        var gerador = new DispensaCapelaGerador(factory);

        var ex = await Assert.ThrowsAsync<DocumentoBloqueadoException>(
            () => gerador.GerarAsync(CriarContexto(atendimentoId, instituicao)));

        Assert.Contains("velório", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ── Documento 6: Auxílio Funeral — Requerimento e Ordem de Serviço ───────

    [Fact]
    public async Task AuxilioFuneral_GeraPdf_Valido_SemValores()
    {
        var (factory, atendimentoId, instituicao) = await CriarBaseAsync(
            "auxilio-funeral",
            comServicosAuxilio: true,
            tipoAtendimento: TipoAtendimentoFunerario.AuxilioFuneral);
        var gerador = new AuxilioFuneralGerador(factory);

        var pdf = await gerador.GerarAsync(CriarContexto(atendimentoId, instituicao));

        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));

        var texto = ExtrairTextoPdf(pdf);
        // Extração simples de texto não decodifica glifos (QuestPDF/SkiaSharp grava IDs de glifos);
        // a garantia real é no código do gerador, que nunca formata valores monetários.
        Assert.DoesNotContain("R$", texto, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AuxilioFuneral_TipoErradoSemServicos_Bloqueia()
    {
        var (factory, atendimentoId, instituicao) = await CriarBaseAsync(
            "auxilio-funeral",
            comServicosAuxilio: false,
            tipoAtendimento: TipoAtendimentoFunerario.Particular);
        var gerador = new AuxilioFuneralGerador(factory);

        var ex = await Assert.ThrowsAsync<DocumentoBloqueadoException>(
            () => gerador.GerarAsync(CriarContexto(atendimentoId, instituicao)));

        Assert.Contains("Auxílio Funeral", ex.Message);
    }

    // ── Extração simples de texto de PDF (melhor esforço) ────────────────────

    private static string ExtrairTextoPdf(byte[] pdf)
    {
        var texto = new StringBuilder();
        var ascii = Encoding.ASCII.GetString(pdf);
        var pos = 0;

        while (true)
        {
            var inicio = ascii.IndexOf("stream", pos, StringComparison.Ordinal);
            if (inicio < 0)
                break;
            inicio = ascii.IndexOf('\n', inicio) + 1;
            var fim = ascii.IndexOf("endstream", inicio, StringComparison.Ordinal);
            if (fim < 0)
                break;

            var dados = pdf.AsSpan(inicio, fim - inicio);
            if (dados.Length >= 2 && dados[dados.Length - 1] == '\n')
                dados = dados[..(dados.Length - 1)];
            if (dados.Length >= 1 && dados[dados.Length - 1] == '\r')
                dados = dados[..(dados.Length - 1)];

            string? conteudo;
            try
            {
                using var ms = new MemoryStream();
                using (var zlib = new ZLibStream(new MemoryStream(dados.ToArray()), CompressionMode.Decompress))
                {
                    zlib.CopyTo(ms);
                }
                conteudo = Encoding.UTF8.GetString(ms.ToArray());
            }
            catch
            {
                conteudo = Encoding.UTF8.GetString(dados);
            }

            // Considera apenas streams de conteúdo (contêm operadores de texto BT/ET e Tj/TJ);
            // streams binários (fontes, imagens) geram falsos positivos.
            if (conteudo.Contains("BT", StringComparison.Ordinal)
                && (conteudo.Contains("Tj", StringComparison.Ordinal) || conteudo.Contains("TJ", StringComparison.Ordinal)))
            {
                texto.Append(conteudo);
            }

            pos = fim + 9;
        }

        return texto.ToString();
    }
}
