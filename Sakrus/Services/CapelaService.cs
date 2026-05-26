using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services;

public class CapelaService : ICapelaService
{
    private readonly ApplicationDbContext _context;

    public CapelaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Capela>> ObterTodasAsync()
    {
        var capelas = await _context.Capelas.ToListAsync();
        
        // BUG-11: Auto-correção (Self-healing) dos status das capelas
        var ativos = await ObterRegistrosAtivosAsync();
        bool precisaSalvar = false;

        foreach (var capela in capelas)
        {
            bool deveriaEstarOcupada = ativos.Any(r => r.CapelaId == capela.Id);
            string statusCorreto = deveriaEstarOcupada ? "Ocupada" : "Disponível";
            
            if (capela.Status != statusCorreto)
            {
                capela.Status = statusCorreto;
                _context.Capelas.Update(capela);
                precisaSalvar = true;
            }
        }

        if (precisaSalvar)
        {
            await _context.SaveChangesAsync();
        }

        return capelas;
    }

    public async Task<List<RegistroCapela>> ObterRegistrosAtivosAsync()
    {
        // Retorna todos os registros que ainda não foram encerrados
        return await _context.RegistrosCapela
            .Include(r => r.Capela)
            .Include(r => r.Atendimento)
                .ThenInclude(a => a.Falecido)
            .Where(r => r.HoraSaida == null || r.HoraSaida > DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<RegistroCapela> AgendarCapelaAsync(int capelaId, int atendimentoId, DateTime horaEntrada, DateTime? horaSaidaPrevista)
    {
        var capela = await _context.Capelas.FindAsync(capelaId);
        if (capela == null)
            throw new InvalidOperationException("Capela não encontrada.");

        // BUG-11: Verifica se há conflito de horários (se não tem data de saída, ou se a data de saída ainda está no futuro)
        var ocupada = await _context.RegistrosCapela
            .AnyAsync(r => r.CapelaId == capelaId && (r.HoraSaida == null || r.HoraSaida > DateTime.UtcNow));

        if (ocupada)
            throw new InvalidOperationException("Esta capela já encontra-se ocupada no momento.");

        var registro = new RegistroCapela
        {
            CapelaId = capelaId,
            AtendimentoId = atendimentoId,
            HoraEntrada = horaEntrada,
            HoraSaida = horaSaidaPrevista // Opcional no agendamento inicial
        };

        capela.Status = "Ocupada";

        _context.RegistrosCapela.Add(registro);
        _context.Capelas.Update(capela);
        
        await _context.SaveChangesAsync();

        return registro;
    }

    public async Task EncerrarVelorioAsync(int registroId)
    {
        var registro = await _context.RegistrosCapela
            .Include(r => r.Capela)
            .FirstOrDefaultAsync(r => r.Id == registroId);

        if (registro == null)
            throw new InvalidOperationException("Registro de capela não encontrado.");

        registro.HoraSaida = DateTime.UtcNow;

        if (registro.Capela != null)
        {
            registro.Capela.Status = "Disponível";
            _context.Capelas.Update(registro.Capela);
        }

        _context.RegistrosCapela.Update(registro);
        await _context.SaveChangesAsync();
    }
}
