using Microsoft.EntityFrameworkCore;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

public record OpcaoLocal(string Chave, string Descricao, LocalCemiterial Local);

public interface IGavetaSelecaoService
{
    Task<IReadOnlyList<OpcaoLocal>> ListarOpcoesAsync(int? responsavelId, bool incluirPublicas = true, bool somenteLivres = true);
    Task<LocalCemiterial?> ResolverAsync(string chave);
}

public class GavetaSelecaoService : IGavetaSelecaoService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public GavetaSelecaoService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IReadOnlyList<OpcaoLocal>> ListarOpcoesAsync(int? responsavelId, bool incluirPublicas = true, bool somenteLivres = true)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var opcoes = new List<OpcaoLocal>();

        if (responsavelId.HasValue)
        {
            var jazigoIds = await db.JazigoProprietarios
                .Where(jp => jp.ResponsavelId == responsavelId.Value && jp.Ativo)
                .Select(jp => jp.JazigoId)
                .Distinct()
                .ToListAsync();

            var gavetas = await db.Gavetas
                .Include(g => g.Jazigo).ThenInclude(j => j.Cemiterio)
                .Include(g => g.Jazigo).ThenInclude(j => j.ClassificacaoEspaco)
                .Include(g => g.ClassificacaoEspaco)
                .Where(g => jazigoIds.Contains(g.JazigoId))
                .ToListAsync();

            foreach (var gaveta in gavetas)
            {
                if (somenteLivres && gaveta.FalecidoId != null)
                    continue;

                var local = new LocalCemiterial(
                    "Particular",
                    gaveta.Id,
                    gaveta.Jazigo.Cemiterio?.Nome ?? "—",
                    gaveta.Jazigo.Cemiterio?.Municipio,
                    gaveta.Jazigo.Quadra,
                    gaveta.Jazigo.Ala,
                    gaveta.Jazigo.NumeroLote,
                    gaveta.Numero,
                    gaveta.ClassificacaoEspaco?.Nome ?? gaveta.Jazigo.ClassificacaoEspaco?.Nome,
                    gaveta.DataSepultamento);

                opcoes.Add(new OpcaoLocal($"P:{gaveta.Id}", DescricaoLocal(local), local));
            }
        }

        if (incluirPublicas)
        {
            var publicas = await db.GavetasPublicas
                .Include(g => g.Cemiterio)
                .Include(g => g.ClassificacaoEspaco)
                .ToListAsync();

            foreach (var gaveta in publicas)
            {
                if (somenteLivres && gaveta.Ocupada)
                    continue;

                var local = new LocalCemiterial(
                    "Publica",
                    gaveta.Id,
                    gaveta.Cemiterio?.Nome ?? gaveta.Setor,
                    gaveta.Cemiterio?.Municipio,
                    gaveta.Quadra,
                    gaveta.Ala,
                    gaveta.Lote,
                    gaveta.NumeroGaveta,
                    gaveta.ClassificacaoEspaco?.Nome,
                    gaveta.DataOcupacao);

                opcoes.Add(new OpcaoLocal($"U:{gaveta.Id}", DescricaoLocal(local), local));
            }
        }

        return opcoes;
    }

    public async Task<LocalCemiterial?> ResolverAsync(string chave)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await ResolverLocalAsync(db, chave);
    }

    public static async Task<LocalCemiterial?> ResolverLocalAsync(ApplicationDbContext db, string chave)
    {
        if (string.IsNullOrWhiteSpace(chave))
            return null;

        if (chave.StartsWith("P:", StringComparison.OrdinalIgnoreCase) && int.TryParse(chave.AsSpan(2), out var gavetaId))
        {
            var gaveta = await db.Gavetas
                .Include(g => g.Jazigo).ThenInclude(j => j.Cemiterio)
                .Include(g => g.Jazigo).ThenInclude(j => j.ClassificacaoEspaco)
                .Include(g => g.ClassificacaoEspaco)
                .FirstOrDefaultAsync(g => g.Id == gavetaId);

            if (gaveta == null)
                return null;

            return new LocalCemiterial(
                "Particular",
                gaveta.Id,
                gaveta.Jazigo.Cemiterio?.Nome ?? "—",
                gaveta.Jazigo.Cemiterio?.Municipio,
                gaveta.Jazigo.Quadra,
                gaveta.Jazigo.Ala,
                gaveta.Jazigo.NumeroLote,
                gaveta.Numero,
                gaveta.ClassificacaoEspaco?.Nome ?? gaveta.Jazigo.ClassificacaoEspaco?.Nome,
                gaveta.DataSepultamento);
        }

        if (chave.StartsWith("U:", StringComparison.OrdinalIgnoreCase) && int.TryParse(chave.AsSpan(2), out var publicaId))
        {
            var gaveta = await db.GavetasPublicas
                .Include(g => g.Cemiterio)
                .Include(g => g.ClassificacaoEspaco)
                .FirstOrDefaultAsync(g => g.Id == publicaId);

            if (gaveta == null)
                return null;

            return new LocalCemiterial(
                "Publica",
                gaveta.Id,
                gaveta.Cemiterio?.Nome ?? gaveta.Setor,
                gaveta.Cemiterio?.Municipio,
                gaveta.Quadra,
                gaveta.Ala,
                gaveta.Lote,
                gaveta.NumeroGaveta,
                gaveta.ClassificacaoEspaco?.Nome,
                gaveta.DataOcupacao);
        }

        return null;
    }

    public static string DescricaoLocal(LocalCemiterial local)
    {
        var partes = new List<string>();
        if (!string.IsNullOrWhiteSpace(local.Cemiterio))
            partes.Add(local.Cemiterio);
        if (!string.IsNullOrWhiteSpace(local.Quadra))
            partes.Add($"Quadra {local.Quadra}");
        if (!string.IsNullOrWhiteSpace(local.Ala))
            partes.Add($"Ala {local.Ala}");
        if (!string.IsNullOrWhiteSpace(local.Lote))
            partes.Add($"Lote {local.Lote}");
        if (!string.IsNullOrWhiteSpace(local.Gaveta))
            partes.Add($"Gaveta {local.Gaveta}");
        if (!string.IsNullOrWhiteSpace(local.Classificacao))
            partes.Add($"({local.Classificacao})");

        var tipo = local.Tipo == "Particular" ? "Particular" : "Pública";
        return $"{tipo} — {string.Join(" / ", partes)}";
    }
}
