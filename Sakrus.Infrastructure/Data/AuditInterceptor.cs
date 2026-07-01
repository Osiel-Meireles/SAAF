using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Sakrus.Core;
using Sakrus.Core.Entities;

namespace Sakrus.Infrastructure.Data;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    public AuditInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return await base.SavingChangesAsync(eventData, result, cancellationToken);

        int? currentUserId = _currentUserService.UserId;
        string currentUserName = _currentUserService.UserName;

        var auditEntries = new List<AuditLog>();

        context.ChangeTracker.DetectChanges();
        
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            if (entry.Entity is AuditLog) continue;

            var log = new AuditLog
            {
                UsuarioId = currentUserId ?? 0,
                NomeUsuario = currentUserName,
                NomeEntidade = entry.Entity.GetType().Name,
                Acao = entry.State.ToString(),
                DataHoraUtc = DateTime.UtcNow
            };

            // Remover proxies do EF Core
            if (log.NomeEntidade.Contains("Proxy"))
            {
                log.NomeEntidade = entry.Entity.GetType().BaseType?.Name ?? log.NomeEntidade;
            }

            var primaryKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
            log.EntidadeId = primaryKey?.CurrentValue?.ToString() ?? "N/A";

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                var propName = property.Metadata.Name;

                switch (entry.State)
                {
                    case EntityState.Added:
                        if (!IsBinaryArray(property.CurrentValue))
                            newValues[propName] = property.CurrentValue;
                        break;
                    
                    case EntityState.Deleted:
                        if (!IsBinaryArray(property.OriginalValue))
                            oldValues[propName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            if (!IsBinaryArray(property.OriginalValue))
                                oldValues[propName] = property.OriginalValue;
                            
                            if (!IsBinaryArray(property.CurrentValue))
                                newValues[propName] = property.CurrentValue;
                        }
                        break;
                }
            }

            log.ValoresAntigos = oldValues.Count > 0 ? JsonSerializer.Serialize(oldValues) : null;
            log.ValoresNovos = newValues.Count > 0 ? JsonSerializer.Serialize(newValues) : null;

            auditEntries.Add(log);
        }

        if (auditEntries.Count > 0)
        {
            context.Set<AuditLog>().AddRange(auditEntries);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private bool IsBinaryArray(object? value)
    {
        return value is byte[];
    }
}
