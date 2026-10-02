using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

public enum TipoCampo { Texto, TextoLongo, Data, Hora, Numero, SimNao, Selecao }

public record CampoDefinicao(
    string Chave,
    string Rotulo,
    TipoCampo Tipo,
    bool Obrigatorio = false,
    string? FonteOpcoes = null,
    IReadOnlyList<string>? Opcoes = null,
    string? Ajuda = null);

public class CamposValores
{
    private readonly Dictionary<string, string> _valores;

    public CamposValores()
        => _valores = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public CamposValores(IDictionary<string, string> valores)
        => _valores = new Dictionary<string, string>(valores, StringComparer.OrdinalIgnoreCase);

    public string? this[string chave]
    {
        get => Get(chave);
        set
        {
            if (value == null)
                _valores.Remove(chave);
            else
                _valores[chave] = value;
        }
    }

    public string? Get(string chave)
        => _valores.TryGetValue(chave, out var valor) ? valor : null;

    public DateTime? GetData(string chave)
    {
        var valor = Get(chave);
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        if (DateTime.TryParseExact(valor, new[] { "yyyy-MM-dd", "dd/MM/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
            return data;

        return null;
    }

    public TimeSpan? GetHora(string chave)
    {
        var valor = Get(chave);
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        if (TimeSpan.TryParseExact(valor, "hh\\:mm", CultureInfo.InvariantCulture, out var hora))
            return hora;

        return null;
    }

    public int? GetInt(string chave)
    {
        var valor = Get(chave);
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        return int.TryParse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture, out var inteiro) ? inteiro : null;
    }

    public bool GetBool(string chave)
    {
        var valor = Get(chave);
        if (string.IsNullOrWhiteSpace(valor))
            return false;

        if (bool.TryParse(valor, out var booleano))
            return booleano;

        return valor is "1" or "sim" or "Sim" or "SIM" or "s" or "S" or "yes" or "Yes" or "YES";
    }

    public IReadOnlyDictionary<string, string> ToDictionary() => _valores;

    public string ToJson() => JsonSerializer.Serialize(_valores);

    public static CamposValores FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new CamposValores();

        var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
        return new CamposValores(dict);
    }
}

public record ContextoEmissao(
    int EntidadeId,
    CamposValores Campos,
    ConfiguracaoInstitucional Instituicao,
    string? Numero,
    DateTime DataEmissao);

public class DocumentoBloqueadoException : Exception
{
    public IReadOnlyList<string> Motivos { get; }

    public DocumentoBloqueadoException(IReadOnlyList<string> motivos)
        : base(string.Join("; ", motivos))
    {
        Motivos = motivos;
    }

    public DocumentoBloqueadoException(string motivo)
        : this(new[] { motivo })
    {
    }
}

public interface IDocumentoGerador
{
    string Codigo { get; }
    IReadOnlyList<CampoDefinicao> Campos { get; }
    Task<string?> ValidarPreCondicoesAsync(int entidadeId);
    Task<byte[]> GerarAsync(ContextoEmissao ctx);
}

public abstract class GeradorBase : IDocumentoGerador
{
    protected GeradorBase(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        DbFactory = dbFactory;
    }

    protected IDbContextFactory<ApplicationDbContext> DbFactory { get; }

    public abstract string Codigo { get; }

    public virtual IReadOnlyList<CampoDefinicao> Campos => Array.Empty<CampoDefinicao>();

    public virtual Task<string?> ValidarPreCondicoesAsync(int entidadeId)
        => Task.FromResult<string?>(null);

    public abstract Task<byte[]> GerarAsync(ContextoEmissao ctx);

    protected static void Exigir(params (string rotulo, string? valor)[] itens)
    {
        var vazios = itens
            .Where(i => string.IsNullOrWhiteSpace(i.valor))
            .Select(i => $"Campo obrigatório não informado: {i.rotulo}")
            .ToList();

        if (vazios.Count > 0)
            throw new DocumentoBloqueadoException(vazios);
    }

    protected static byte[] Pdf(ContextoEmissao ctx, string titulo, Action<ColumnDescriptor> conteudo)
        => DocumentoLayout.Criar(ctx, titulo, conteudo);
}
