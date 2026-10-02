using System.Text;
using System.Text.RegularExpressions;
using Sakrus.Core.Entities;
using Sakrus.Services;
using Sakrus.Services.Documentos;

namespace Sakrus.Tests.Documentos;

/// <summary>
/// Garante que a imagem da assinatura do responsável (Assets/assinatura-saaf.png)
/// seja embutida acima da linha de "Assinatura do Responsável" em todos os documentos.
/// </summary>
public class AssinaturaResponsavelTests
{
    // Proporção do recorte da assinatura (1000x614); brasão ~0,77 e logo ~1,50.
    private const double ProporcaoAssinatura = 1.629;

    private static bool ContemImagemAssinatura(byte[] pdf)
        => Regex.Matches(Encoding.ASCII.GetString(pdf), @"/Width\s+(\d+)\s+/Height\s+(\d+)")
            .Cast<Match>()
            .Select(m => (Largura: int.Parse(m.Groups[1].Value), Altura: int.Parse(m.Groups[2].Value)))
            .Any(d => Math.Abs((double)d.Largura / d.Altura - ProporcaoAssinatura) < 0.02);

    private static async Task<byte[]> GerarTermoAsync(string documento)
    {
        var factory = new TestDbContextFactory("assinatura-" + documento + "-" + Guid.NewGuid());
        using var db = factory.CreateDbContext();

        var funeraria = new Funeraria { Nome = "Funerária X" };
        var falecido = new Falecido
        {
            Nome = "Maria da Silva",
            DataFalecimento = new DateTime(2026, 4, 5),
            Sexo = "Feminino",
            EstadoCivil = "Casada",
            LocalCorpo = "Hospital Municipal",
            CausaMorte = CausaMorte.Natural
        };
        var jazigo = new Jazigo
        {
            CodigoIdentificador = "J1", Quadra = "3", Ala = "A", NumeroLote = "12",
            Cemiterio = new Cemiterio { Nome = "Bela Vista" }
        };
        var gaveta = new Gaveta
        {
            Jazigo = jazigo, Numero = "02", Falecido = falecido,
            ClassificacaoEspaco = new ClassificacaoEspaco { Nome = "Tipo A", Natureza = NaturezaEspaco.Gaveta },
            DataSepultamento = new DateTime(2026, 4, 8)
        };
        var atendimento = new Atendimento
        {
            Responsavel = new Responsavel { Nome = "João da Silva", CPF = "12345678901" },
            Falecido = falecido,
            Funeraria = funeraria,
            EmpresaExecutora = funeraria,
            DataSepultamento = new DateTime(2026, 4, 8),
            HorarioSepultamento = new TimeSpan(10, 30, 0),
            LocalFalecimento = "Hospital Municipal",
            NumeroDeclaracaoObito = "DO-12345",
            GrauParentesco = "Filho(a)",
            LiberacaoMunicipal = true,
            TipoAtendimento = TipoAtendimentoFunerario.Particular,
            PecaAnatomicaIdentificacao = "Peça 01",
            PecaAnatomicaDataCirurgia = new DateTime(2026, 4, 6),
            PecaAnatomicaHospital = "Hospital Municipal",
            PecaAnatomicaCidadeOrigem = "Barreiras",
            PecaAnatomicaEstadoOrigem = "BA"
        };
        db.AddRange(atendimento, gaveta);

        if (documento == "capela")
        {
            db.Add(new RegistroCapela
            {
                Capela = new Capela { Nome = "Capela Municipal" },
                Atendimento = atendimento,
                HoraEntrada = new DateTime(2026, 4, 8, 8, 0, 0),
                HoraSaida = new DateTime(2026, 4, 8, 10, 0, 0)
            });
        }

        await db.SaveChangesAsync();

        var campos = new CamposValores();
        campos["localVelorio"] = "Residência da família";
        campos["destinoMunicipio"] = "Barreiras";
        campos["destinoUf"] = "BA";
        campos["destinoLocal"] = "Cemitério São João";
        campos["dataSaida"] = "08/04/2026";
        campos["horaSaida"] = "14:00";

        var ctx = new ContextoEmissao(atendimento.Id, campos, new ConfiguracaoInstitucional(), null,
            new DateTime(2026, 4, 8, 10, 0, 0));

        return documento switch
        {
            "termo" => await new TermoSepultamentoGerador(factory).GerarAsync(ctx),
            "pecas" => await new PecasAnatomicasGerador(factory).GerarAsync(ctx),
            "translado" => await new TransladoGerador(factory).GerarAsync(ctx),
            "capela" => await new UsoCapelaGerador(factory).GerarAsync(ctx),
            "dispensa" => await new DispensaCapelaGerador(factory).GerarAsync(ctx),
            _ => throw new ArgumentOutOfRangeException(nameof(documento))
        };
    }

    [Theory]
    [InlineData("termo")]
    [InlineData("pecas")]
    [InlineData("translado")]
    [InlineData("capela")]
    [InlineData("dispensa")]
    public async Task TodosTermos_EmbedemImagemAssinatura(string documento)
    {
        var pdf = await GerarTermoAsync(documento);

        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));
        Assert.True(ContemImagemAssinatura(pdf),
            $"{documento}: imagem 'Assinatura SAAF' não foi embutida no PDF");
    }

    [Fact]
    public void Relatorios_EmbedemImagemAssinatura()
    {
        var atendimento = new Atendimento
        {
            Responsavel = new Responsavel { Nome = "João da Silva", CPF = "12345678901", Telefone = "77999999999" },
            Falecido = new Falecido
            {
                Nome = "Maria da Silva",
                DataFalecimento = new DateTime(2026, 4, 5),
                CausaMorte = CausaMorte.Natural
            },
            ItensFaturados =
            {
                new ItemFaturado { CategoriaItem = "Urna", QuantidadeOuKm = 1, ValorTotalCalculado = 100 }
            }
        };

        var servico = new RelatorioService(new TestDbContextFactory("assinatura-relatorios-" + Guid.NewGuid()));

        Assert.True(ContemImagemAssinatura(servico.GerarPdfOrdemServico(atendimento)),
            "Ordem de Serviço: imagem 'Assinatura SAAF' não foi embutida no PDF");
        Assert.True(ContemImagemAssinatura(servico.GerarPdfGuiaSepultamento(atendimento.Falecido, atendimento)),
            "Guia de Sepultamento: imagem 'Assinatura SAAF' não foi embutida no PDF");
    }

    [Fact]
    public void AssetAssinatura_EstaDisponivel()
    {
        Assert.NotNull(DocumentoLayout.AssinaturaResponsavel);
    }
}
