using Sakrus.Services.Documentos;

namespace Sakrus.Tests.Documentos;

public class TextoTemplateTests
{
    [Fact]
    public void Preencher_SubstituiChaves()
    {
        var resultado = TextoTemplate.Preencher("Olá {{nome}}!", new Dictionary<string, string?> { ["nome"] = "João" });

        Assert.Equal("Olá João!", resultado);
    }

    [Fact]
    public void Preencher_CaseInsensitive()
    {
        var resultado = TextoTemplate.Preencher("{{EMPRESA.Nome}}", new Dictionary<string, string?> { ["empresa.nome"] = "HPF" });

        Assert.Equal("HPF", resultado);
    }

    [Fact]
    public void Preencher_ToleraEspacosInternos()
    {
        var resultado = TextoTemplate.Preencher("{{ empresa.nome }}", new Dictionary<string, string?> { ["empresa.nome"] = "HPF" });

        Assert.Equal("HPF", resultado);
    }

    [Fact]
    public void Preencher_ChaveAusente_ViraVazio()
    {
        var resultado = TextoTemplate.Preencher("A {{x}} B", new Dictionary<string, string?>());

        Assert.Equal("A  B", resultado);
    }

    [Fact]
    public void Preencher_ChaveVazia_ViraVazio()
    {
        var resultado = TextoTemplate.Preencher("A {{x}} B", new Dictionary<string, string?> { ["x"] = "" });

        Assert.Equal("A  B", resultado);
    }

    [Fact]
    public void PreencherObrigatorio_ChaveAusente_Lanca()
    {
        var ex = Assert.Throws<DocumentoBloqueadoException>(
            () => TextoTemplate.PreencherObrigatorio("{{x}}", new Dictionary<string, string?>()));

        Assert.Contains("x", ex.Message);
    }

    [Fact]
    public void PreencherObrigatorio_ChaveVazia_Lanca()
    {
        var ex = Assert.Throws<DocumentoBloqueadoException>(
            () => TextoTemplate.PreencherObrigatorio("{{a}} e {{b}}", new Dictionary<string, string?> { ["a"] = "ok", ["b"] = "" }));

        Assert.Contains("b", ex.Message);
    }

    [Fact]
    public void ChavesUsadas_RetornaChavesDistintas()
    {
        var chaves = TextoTemplate.ChavesUsadas("{{a}} e {{b}} e {{a}}");

        Assert.Equal(new[] { "a", "b" }, chaves);
    }
}
