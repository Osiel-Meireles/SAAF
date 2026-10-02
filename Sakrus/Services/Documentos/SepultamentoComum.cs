using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

/// <summary>
/// Lógica compartilhada de resolução do local de sepultamento, empresa executora e data/hora
/// usada pelos termos de sepultamento (documentos 1 e 2).
/// O local, a data e a hora são obtidos do que foi preenchido no modal de sepultamento,
/// evitando informações divergentes entre o documento e o sepultamento realizado.
/// </summary>
public static class SepultamentoComum
{
    public static async Task<(LocalCemiterial Local, Funeraria Empresa, string DataHora)> ResolverDadosSepultamentoAsync(
        IDbContextFactory<ApplicationDbContext> dbFactory,
        ApplicationDbContext db,
        ContextoEmissao ctx,
        Atendimento atendimento)
    {
        var local = await ResolverLocalSepultamentoAsync(db, atendimento);
        if (local == null)
            throw new DocumentoBloqueadoException(
                "Não foi possível identificar o local de sepultamento. Realize o sepultamento pelo modal de sepultamento.");

        var empresaExecutoraId = ctx.Campos.GetInt("empresaExecutoraId") ?? atendimento.EmpresaExecutoraId;
        if (empresaExecutoraId == null)
            throw new DocumentoBloqueadoException("Sepultamento exige empresa executora");

        var empresa = await db.Funerarias.FindAsync(empresaExecutoraId.Value);
        if (empresa == null)
            throw new DocumentoBloqueadoException("Empresa executora não encontrada");

        var dataSepultamento = atendimento.DataSepultamento;
        var horaSepultamento = atendimento.HorarioSepultamento;
        if (dataSepultamento == null && horaSepultamento == null)
            throw new DocumentoBloqueadoException("Informe a data e o horário do sepultamento");

        var pt = new System.Globalization.CultureInfo("pt-BR");
        var dataHora = dataSepultamento.HasValue
            ? dataSepultamento.Value.ToString("dd 'DE' MMMM 'DE' yyyy", pt).ToUpper(pt)
            : string.Empty;
        if (horaSepultamento.HasValue)
            dataHora += (dataHora.Length > 0 ? " - " : "") + $"{horaSepultamento.Value.Hours:00}:{horaSepultamento.Value.Minutes:00} HORAS";

        return (local, empresa, dataHora);
    }

    private static async Task<LocalCemiterial?> ResolverLocalSepultamentoAsync(ApplicationDbContext db, Atendimento atendimento)
    {
        // 1. Gaveta pública vinculada ao falecido no sepultamento
        var gavetaPublica = await db.GavetasPublicas
            .Include(g => g.Cemiterio)
            .Include(g => g.ClassificacaoEspaco)
            .FirstOrDefaultAsync(g => g.FalecidoId == atendimento.FalecidoId && g.Ocupada);
        if (gavetaPublica != null)
        {
            return new LocalCemiterial(
                "Publica",
                gavetaPublica.Id,
                gavetaPublica.Cemiterio?.Nome ?? gavetaPublica.Setor,
                gavetaPublica.Cemiterio?.Municipio,
                gavetaPublica.Quadra,
                gavetaPublica.Ala,
                gavetaPublica.Lote,
                gavetaPublica.NumeroGaveta,
                gavetaPublica.ClassificacaoEspaco?.Nome,
                gavetaPublica.DataOcupacao);
        }

        // 2. Gaveta particular vinculada ao falecido no sepultamento
        var gaveta = await db.Gavetas
            .Include(g => g.Jazigo).ThenInclude(j => j.Cemiterio)
            .Include(g => g.Jazigo).ThenInclude(j => j.ClassificacaoEspaco)
            .Include(g => g.ClassificacaoEspaco)
            .FirstOrDefaultAsync(g => g.FalecidoId == atendimento.FalecidoId);
        if (gaveta != null)
        {
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

        // 3. Jazigo vinculado ao falecido (sepultamento em jazigo sem gaveta específica)
        if (atendimento.Falecido.JazigoId.HasValue)
        {
            var jazigo = await db.Jazigos
                .Include(j => j.Cemiterio)
                .Include(j => j.ClassificacaoEspaco)
                .FirstOrDefaultAsync(j => j.Id == atendimento.Falecido.JazigoId.Value);
            if (jazigo != null)
            {
                return new LocalCemiterial(
                    "Particular",
                    jazigo.Id,
                    jazigo.Cemiterio?.Nome ?? "—",
                    jazigo.Cemiterio?.Municipio,
                    jazigo.Quadra,
                    jazigo.Ala,
                    jazigo.NumeroLote,
                    null,
                    jazigo.ClassificacaoEspaco?.Nome,
                    atendimento.Falecido.DataSepultamento);
            }
        }

        return null;
    }
}
