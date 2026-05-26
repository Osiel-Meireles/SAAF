using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services;

public interface IAuditService
{
    Task LogAcaoAsync(int usuarioId, string nomeUsuario, string acao, string nomeEntidade, string entidadeId, object? valoresAntigos = null, object? valoresNovos = null);
}

public class AuditService : IAuditService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public AuditService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task LogAcaoAsync(int usuarioId, string nomeUsuario, string acao, string nomeEntidade, string entidadeId, object? valoresAntigos = null, object? valoresNovos = null)
    {
        using var context = await _dbFactory.CreateDbContextAsync();

        var log = new AuditLog
        {
            UsuarioId = usuarioId,
            NomeUsuario = nomeUsuario,
            Acao = acao,
            NomeEntidade = nomeEntidade,
            EntidadeId = entidadeId,
            DataHoraUtc = DateTime.UtcNow,
            ValoresAntigos = valoresAntigos != null ? JsonSerializer.Serialize(valoresAntigos) : null,
            ValoresNovos = valoresNovos != null ? JsonSerializer.Serialize(valoresNovos) : null
        };

        context.Set<AuditLog>().Add(log);
        await context.SaveChangesAsync();
    }
}
