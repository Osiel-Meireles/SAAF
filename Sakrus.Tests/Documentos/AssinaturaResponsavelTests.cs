using System.Text;
using System.Text.RegularExpressions;
using Sakrus.Core.Entities;
using Sakrus.Services;
using Sakrus.Services.Documentos;

namespace Sakrus.Tests.Documentos;

/// <summary>
/// Garante que a imagem da assinatura SAAF (Assets/assinatura-saaf.png)
/// seja embutida exclusivamente nos campos institucionais (Presidente do Conselho / Coordenação CAAFE)
/// e NÃO no campo de assinatura do responsável/familiar.
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

    [Fact]
    public void AssetAssinatura_EstaDisponivel()
    {
        Assert.NotNull(DocumentoLayout.AssinaturaSAAF);
    }

    [Fact]
    public async Task AutorizacaoConstrucao_ContemAssinaturaSAAF()
    {
        var factory = new TestDbContextFactory("assinatura-construcao-" + Guid.NewGuid());
        int jazigoId;
        int empresaId;
        using (var db = factory.CreateDbContext())
        {
            var cemiterio = new Cemiterio { Nome = "Bela Vista" };
            var classificacao = new ClassificacaoEspaco { Nome = "Jazigo Tipo A", Natureza = NaturezaEspaco.Jazigo };
            var modelo = new ModeloJazigo { Nome = "Tradicional" };
            var titular = new Responsavel { Nome = "João Titular", CPF = "11122233344" };
            var empresa = new Funeraria { Nome = "Funerária São Lucas", Ativo = true, EhExecutora = true };
            var jazigo = new Jazigo
            {
                CodigoIdentificador = "J-01",
                Quadra = "3",
                Ala = "A",
                NumeroLote = "12",
                Cemiterio = cemiterio,
                ClassificacaoEspaco = classificacao,
                ModeloJazigo = modelo
            };
            db.Cemiterios.Add(cemiterio);
            db.ClassificacoesEspaco.Add(classificacao);
            db.ModelosJazigos.Add(modelo);
            db.Responsaveis.Add(titular);
            db.Funerarias.Add(empresa);
            db.Jazigos.Add(jazigo);
            db.JazigoProprietarios.Add(new JazigoProprietario
            {
                Jazigo = jazigo,
                Responsavel = titular,
                TipoVinculo = TipoVinculoJazigo.Titular,
                Ativo = true
            });
            await db.SaveChangesAsync();
            jazigoId = jazigo.Id;
            empresaId = empresa.Id;
        }

        var campos = new CamposValores(new Dictionary<string, string>
        {
            ["finalidade"] = AutorizacaoConstrucaoGerador.Finalidades[0],
            ["empresaExecutoraId"] = empresaId.ToString(),
            ["revestimento"] = "Mármore"
        });
        var ctx = new ContextoEmissao(jazigoId, campos, new ConfiguracaoInstitucional(), null, DateTime.Today);
        var pdf = await new AutorizacaoConstrucaoGerador(factory).GerarAsync(ctx);

        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));
        Assert.True(ContemImagemAssinatura(pdf), "Autorização de Construção deve conter a assinatura SAAF.");
    }

    [Fact]
    public void Relatorios_NaoContemAssinaturaSAAFNoResponsavel()
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

        Assert.False(ContemImagemAssinatura(servico.GerarPdfOrdemServico(atendimento)),
            "Ordem de Serviço: assinatura SAAF não deve aparecer no campo do responsável.");
        Assert.False(ContemImagemAssinatura(servico.GerarPdfGuiaSepultamento(atendimento.Falecido, atendimento)),
            "Guia de Sepultamento: assinatura SAAF não deve aparecer no campo do responsável.");
    }
}
