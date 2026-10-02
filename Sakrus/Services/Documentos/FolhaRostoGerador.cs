using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Core.Helpers;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

public record LinhaFolhaRosto(string Numero, string Falecido, DateTime? DataSepultamento, DateTime? DataExumacao);

public class FolhaRostoGerador : GeradorBase
{
    public FolhaRostoGerador(IDbContextFactory<ApplicationDbContext> dbFactory)
        : base(dbFactory)
    {
    }

    public override string Codigo => "18";

    public override async Task<byte[]> GerarAsync(ContextoEmissao ctx)
    {
        using var db = await DbFactory.CreateDbContextAsync();

        var jazigo = await db.Jazigos
            .Include(j => j.Cemiterio)
            .Include(j => j.ClassificacaoEspaco)
            .Include(j => j.Gavetas).ThenInclude(g => g.Falecido)
            .Include(j => j.Proprietarios).ThenInclude(p => p.Responsavel)
            .Include(j => j.Falecidos)
            .FirstOrDefaultAsync(j => j.Id == ctx.EntidadeId);

        if (jazigo == null)
            throw new DocumentoBloqueadoException("Jazigo não encontrado");

        var titular = jazigo.Proprietarios.FirstOrDefault(p => p.Ativo && p.TipoVinculo == TipoVinculoJazigo.Titular);
        var nomeTitular = titular?.Responsavel?.Nome;
        var identificacao = DadosComuns.FormatarCpf(titular?.Responsavel?.CPF);

        var linhas = MontarLinhas(jazigo);
        var linhasTabela = linhas
            .Select(l => (IReadOnlyList<string>)new[]
            {
                l.Numero,
                l.Falecido,
                l.DataSepultamento?.ToString("dd/MM/yyyy") ?? "—",
                l.DataExumacao?.ToString("dd/MM/yyyy") ?? "—"
            })
            .ToList();

        return Pdf(ctx, "FOLHA DE ROSTO", col =>
        {
            col.Secao("Concessionário");
            col.Campos(
                ("Nome", string.IsNullOrWhiteSpace(nomeTitular) ? null : nomeTitular),
                ("Identificação conforme cadastro", string.IsNullOrWhiteSpace(identificacao) ? null : identificacao));

            col.Secao("Identificação do espaço");
            col.Campos(
                ("Cemitério", jazigo.Cemiterio?.Nome),
                ("Quadra", string.IsNullOrWhiteSpace(jazigo.Quadra) ? null : jazigo.Quadra),
                ("Ala", string.IsNullOrWhiteSpace(jazigo.Ala) ? null : jazigo.Ala),
                ("Lote", string.IsNullOrWhiteSpace(jazigo.NumeroLote) ? null : jazigo.NumeroLote),
                ("Tipo/classificação", jazigo.ClassificacaoEspaco?.Nome),
                ("Nº de gavetas", jazigo.Gavetas.Count.ToString()));

            col.Secao("Falecidos vinculados");
            col.Tabela(
                new[] { "Gaveta", "Falecido", "Data do sepultamento", "Exumação a partir de" },
                linhasTabela);
        });
    }

    public static IReadOnlyList<LinhaFolhaRosto> MontarLinhas(Jazigo jazigo)
    {
        var linhas = new List<LinhaFolhaRosto>();

        if (jazigo.Gavetas.Count > 0)
        {
            foreach (var gaveta in jazigo.Gavetas.OrderBy(g => g.Numero, StringComparer.OrdinalIgnoreCase))
            {
                var nomeFalecido = gaveta.Falecido?.Nome;
                var dataSepultamento = gaveta.DataSepultamento;

                linhas.Add(new LinhaFolhaRosto(
                    gaveta.Numero,
                    string.IsNullOrWhiteSpace(nomeFalecido) ? "Livre" : nomeFalecido,
                    dataSepultamento,
                    dataSepultamento.HasValue ? RegrasDocumentos.DataLiberacaoExumacao(dataSepultamento.Value) : null));
            }
        }
        else
        {
            var falecidos = jazigo.Falecidos.Where(f => f.Status == StatusFalecido.Sepultado).ToList();
            if (falecidos.Count == 0)
            {
                linhas.Add(new LinhaFolhaRosto("s/n", "Nenhuma gaveta cadastrada", null, null));
            }
            else
            {
                foreach (var falecido in falecidos)
                {
                    linhas.Add(new LinhaFolhaRosto(
                        "s/n",
                        falecido.Nome,
                        falecido.DataSepultamento,
                        falecido.DataSepultamento.HasValue ? RegrasDocumentos.DataLiberacaoExumacao(falecido.DataSepultamento.Value) : null));
                }
            }
        }

        return linhas;
    }
}
