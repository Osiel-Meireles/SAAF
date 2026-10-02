using System.Data;
using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Core.Helpers;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

public interface IProtocoloService
{
    Task<Protocolo> AbrirAsync(int responsavelId, int assuntoId, int? jazigoId, string? observacao, DateTime? dataAbertura = null);
    Task<IReadOnlyList<TipoDocumento>> DocumentosDoProtocoloAsync(int protocoloId);
    Task<List<Protocolo>> ListarAsync(int? ano = null);
}

public class ProtocoloService : IProtocoloService
{
    private const int MaxTentativas = 5;
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly TimeProvider _timeProvider;

    public ProtocoloService(IDbContextFactory<ApplicationDbContext> dbFactory, TimeProvider timeProvider)
    {
        _dbFactory = dbFactory;
        _timeProvider = timeProvider;
    }

    public async Task<Protocolo> AbrirAsync(int responsavelId, int assuntoId, int? jazigoId, string? observacao, DateTime? dataAbertura = null)
    {
        for (var tentativa = 0; tentativa < MaxTentativas; tentativa++)
        {
            using var db = await _dbFactory.CreateDbContextAsync();

            var responsavel = await db.Responsaveis.FindAsync(responsavelId);
            if (responsavel == null)
                throw new DocumentoBloqueadoException("Responsável não encontrado");

            var assunto = await db.AssuntosProtocolo.FirstOrDefaultAsync(a => a.Id == assuntoId && a.Ativo);
            if (assunto == null)
                throw new DocumentoBloqueadoException("Assunto de protocolo não encontrado ou inativo");

            var agora = _timeProvider.GetUtcNow();
            var dataAberturaEfetiva = dataAbertura ?? agora.UtcDateTime;
            var ano = dataAberturaEfetiva.Year;

            try
            {
                var sequencia = await ProximaSequenciaAsync(db, ano);
                var protocolo = new Protocolo
                {
                    Ano = ano,
                    Sequencia = sequencia,
                    Numero = RegrasDocumentos.FormatarNumeroAnual(sequencia, ano),
                    AssuntoProtocoloId = assuntoId,
                    ResponsavelId = responsavelId,
                    JazigoId = jazigoId,
                    Observacao = observacao ?? string.Empty,
                    DataAbertura = dataAberturaEfetiva
                };

                if (db.Database.IsRelational())
                {
                    await using var transacao = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
                    db.Protocolos.Add(protocolo);
                    await db.SaveChangesAsync();
                    db.NumerosRegistro.Add(new NumeroRegistro
                    {
                        Chave = "PROTOCOLO",
                        EntidadeTipo = "Protocolo",
                        EntidadeId = protocolo.Id,
                        Ano = ano,
                        Sequencia = sequencia,
                        Numero = protocolo.Numero,
                        CriadoEm = agora.UtcDateTime
                    });
                    await db.SaveChangesAsync();
                    await transacao.CommitAsync();
                }
                else
                {
                    db.Protocolos.Add(protocolo);
                    await db.SaveChangesAsync();
                    db.NumerosRegistro.Add(new NumeroRegistro
                    {
                        Chave = "PROTOCOLO",
                        EntidadeTipo = "Protocolo",
                        EntidadeId = protocolo.Id,
                        Ano = ano,
                        Sequencia = sequencia,
                        Numero = protocolo.Numero,
                        CriadoEm = agora.UtcDateTime
                    });
                    await db.SaveChangesAsync();
                }

                return protocolo;
            }
            catch (DbUpdateException) when (tentativa < MaxTentativas - 1)
            {
                // Concorrência: outro processo gerou a mesma sequência entre a leitura e a gravação.
            }
        }

        throw new InvalidOperationException("Não foi possível abrir o protocolo após múltiplas tentativas.");
    }

    public async Task<IReadOnlyList<TipoDocumento>> DocumentosDoProtocoloAsync(int protocoloId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        var protocolo = await db.Protocolos.FindAsync(protocoloId);
        if (protocolo == null)
            throw new DocumentoBloqueadoException("Protocolo não encontrado");

        return await (from apd in db.AssuntosProtocoloDocumentos
                      join td in db.TiposDocumento on apd.TipoDocumentoCodigo equals td.Codigo
                      where apd.AssuntoProtocoloId == protocolo.AssuntoProtocoloId && td.Ativo
                      orderby apd.Ordem
                      select td).ToListAsync();
    }

    public async Task<List<Protocolo>> ListarAsync(int? ano = null)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        var query = db.Protocolos
            .Include(p => p.AssuntoProtocolo)
            .Include(p => p.Responsavel)
            .AsQueryable();

        if (ano.HasValue)
            query = query.Where(p => p.Ano == ano.Value);

        return await query.OrderByDescending(p => p.DataAbertura).ToListAsync();
    }

    private static async Task<int> ProximaSequenciaAsync(ApplicationDbContext db, int ano)
    {
        var maximo = await db.Protocolos
            .Where(p => p.Ano == ano)
            .MaxAsync(p => (int?)p.Sequencia) ?? 0;

        return maximo + 1;
    }
}
