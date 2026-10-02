using System.Data;
using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Core.Helpers;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

public interface INumeracaoService
{
    Task<NumeroRegistro> ObterOuGerarAsync(string chave, string entidadeTipo, int entidadeId);
    Task<NumeroRegistro?> ObterAsync(string chave, string entidadeTipo, int entidadeId);
}

public class NumeracaoService : INumeracaoService
{
    private const int MaxTentativas = 5;
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly TimeProvider _timeProvider;

    public NumeracaoService(IDbContextFactory<ApplicationDbContext> dbFactory, TimeProvider timeProvider)
    {
        _dbFactory = dbFactory;
        _timeProvider = timeProvider;
    }

    public async Task<NumeroRegistro> ObterOuGerarAsync(string chave, string entidadeTipo, int entidadeId)
    {
        for (var tentativa = 0; tentativa < MaxTentativas; tentativa++)
        {
            using var db = await _dbFactory.CreateDbContextAsync();

            var existente = await db.NumerosRegistro
                .FirstOrDefaultAsync(n => n.Chave == chave && n.EntidadeTipo == entidadeTipo && n.EntidadeId == entidadeId);
            if (existente != null)
                return existente;

            try
            {
                var ano = _timeProvider.GetUtcNow().Year;
                var sequencia = await ProximaSequenciaAsync(db, chave, ano);
                var novo = new NumeroRegistro
                {
                    Chave = chave,
                    EntidadeTipo = entidadeTipo,
                    EntidadeId = entidadeId,
                    Ano = ano,
                    Sequencia = sequencia,
                    Numero = RegrasDocumentos.FormatarNumeroAnual(sequencia, ano),
                    CriadoEm = _timeProvider.GetUtcNow().UtcDateTime
                };

                if (db.Database.IsRelational())
                {
                    await using var transacao = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
                    db.NumerosRegistro.Add(novo);
                    await db.SaveChangesAsync();
                    await transacao.CommitAsync();
                }
                else
                {
                    db.NumerosRegistro.Add(novo);
                    await db.SaveChangesAsync();
                }

                return novo;
            }
            catch (DbUpdateException) when (tentativa < MaxTentativas - 1)
            {
                // Concorrência: outro processo criou o registro entre a leitura e a gravação.
            }
        }

        throw new InvalidOperationException("Não foi possível gerar o número de registro após múltiplas tentativas.");
    }

    public async Task<NumeroRegistro?> ObterAsync(string chave, string entidadeTipo, int entidadeId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        return await db.NumerosRegistro
            .FirstOrDefaultAsync(n => n.Chave == chave && n.EntidadeTipo == entidadeTipo && n.EntidadeId == entidadeId);
    }

    private static async Task<int> ProximaSequenciaAsync(ApplicationDbContext db, string chave, int ano)
    {
        var maximo = await db.NumerosRegistro
            .Where(n => n.Chave == chave && n.Ano == ano)
            .MaxAsync(n => (int?)n.Sequencia) ?? 0;

        return maximo + 1;
    }
}
