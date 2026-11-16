using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sakrus.Services.Documentos;

namespace Sakrus.Tests.Documentos;

public class PdfMergerTests
{
    static PdfMergerTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private static byte[] GerarPdf(string titulo)
        => Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.Content().Text(titulo);
            });
        }).GeneratePdf();

    [Fact]
    public void DoisPdfs_ResultamEmUmArquivoComDuasPaginas()
    {
        var pdf = PdfMerger.Mesclar(new[] { GerarPdf("Documento A"), GerarPdf("Documento B") });

        Assert.NotEmpty(pdf);
        Assert.Equal(2, ContarPaginas(pdf));
    }

    [Fact]
    public void UmPdf_RetornaOMesmoConteudo()
    {
        var original = GerarPdf("Documento único");

        var pdf = PdfMerger.Mesclar(new[] { original });

        Assert.Equal(original, pdf);
    }

    [Fact]
    public void ListaVazia_LancaExcecao()
        => Assert.Throws<ArgumentException>(() => PdfMerger.Mesclar(Array.Empty<byte[]>()));

    private static int ContarPaginas(byte[] pdf)
    {
        var texto = System.Text.Encoding.Latin1.GetString(pdf);
        return System.Text.RegularExpressions.Regex.Matches(texto, @"/Type\s*/Page[^s]").Count;
    }
}
