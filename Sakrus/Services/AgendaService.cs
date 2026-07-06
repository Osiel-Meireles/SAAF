using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Core.Models;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services;

public class AgendaService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public AgendaService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<AgendaEvento>> ObterEventosPorDataAsync(DateTime data)
    {
        using var context = _dbFactory.CreateDbContext();
        var dataInicio = DateTime.SpecifyKind(data.Date, DateTimeKind.Utc);
        var dataFim = dataInicio.AddDays(1);

        var eventos = new List<AgendaEvento>();

        // Busca Sepultamentos
        var sepultamentos = await context.Atendimentos
            .Include(a => a.Falecido)
            .Where(a => a.DataSepultamento >= dataInicio && a.DataSepultamento < dataFim && a.HorarioSepultamento != null)
            .ToListAsync();

        foreach (var s in sepultamentos)
        {
            eventos.Add(new AgendaEvento
            {
                Horario = s.HorarioSepultamento.Value,
                TipoEvento = "Sepultamento",
                FalecidoNome = s.Falecido?.Nome ?? "Desconhecido",
                Local = s.LocalSepultamento
            });
        }

        // Busca Exumações
        var exumacoes = await context.ExumacoesRegistros
            .Include(e => e.Falecido)
            .Include(e => e.Jazigo)
            .Include(e => e.GavetaPublica)
            .Where(e => e.DataExecucao >= dataInicio && e.DataExecucao < dataFim && e.HorarioExecucao != null)
            .ToListAsync();

        foreach (var e in exumacoes)
        {
            string localExumacao = "Desconhecido";
            if (e.Jazigo != null) localExumacao = $"Jazigo {e.Jazigo.CodigoIdentificador}";
            else if (e.GavetaPublica != null) localExumacao = $"Gaveta Pública {e.GavetaPublica.NumeroGaveta}";

            eventos.Add(new AgendaEvento
            {
                Horario = e.HorarioExecucao.Value,
                TipoEvento = "Exumação",
                FalecidoNome = e.Falecido?.Nome ?? "Desconhecido",
                Local = localExumacao
            });
        }

        return eventos.OrderBy(e => e.Horario).ToList();
    }
}
