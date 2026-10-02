using System.Text.RegularExpressions;

namespace Sakrus.Services.Documentos;

public static class TextoTemplate
{
    private static readonly Regex PlaceholderRegex = new(
        @"\{\{\s*([A-Za-z0-9_.]+)\s*\}\}",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static string Preencher(string modelo, IReadOnlyDictionary<string, string?> valores)
    {
        if (string.IsNullOrEmpty(modelo))
            return modelo;

        var lookup = CriarLookup(valores);

        return PlaceholderRegex.Replace(modelo, m =>
        {
            var chave = m.Groups[1].Value;
            if (lookup.TryGetValue(chave, out var valor) && !string.IsNullOrEmpty(valor))
                return valor;

            return string.Empty;
        });
    }

    public static string PreencherObrigatorio(string modelo, IReadOnlyDictionary<string, string?> valores)
    {
        var lookup = CriarLookup(valores);

        var ausentes = ChavesUsadas(modelo)
            .Where(chave => !lookup.TryGetValue(chave, out var valor) || string.IsNullOrWhiteSpace(valor))
            .Select(chave => $"Chave obrigatória não informada: {chave}")
            .ToList();

        if (ausentes.Count > 0)
            throw new DocumentoBloqueadoException(ausentes);

        return Preencher(modelo, valores);
    }

    public static IReadOnlyList<string> ChavesUsadas(string modelo)
    {
        if (string.IsNullOrEmpty(modelo))
            return Array.Empty<string>();

        return PlaceholderRegex.Matches(modelo)
            .Select(m => m.Groups[1].Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static Dictionary<string, string?> CriarLookup(IReadOnlyDictionary<string, string?> valores)
    {
        var lookup = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in valores)
            lookup[kv.Key] = kv.Value;

        return lookup;
    }
}
