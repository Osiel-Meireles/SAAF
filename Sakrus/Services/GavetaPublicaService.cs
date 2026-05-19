using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services;

public class GavetaPublicaService : IGavetaPublicaService
{
    private readonly ApplicationDbContext _context;

    public GavetaPublicaService(ApplicationDbContext context)
    {
        _context = context;
    }

    // 1. Regra de Negócio: Limite Rígido de 256 Posições
    public async Task AdicionarGavetaAsync(GavetaPublica novaGaveta)
    {
        var totalGavetas = await _context.GavetasPublicas.CountAsync();
        
        if (totalGavetas >= 256)
        {
            throw new InvalidOperationException("A capacidade máxima do setor de gavetas públicas (256 posições) foi atingida.");
        }

        _context.GavetasPublicas.Add(novaGaveta);
        await _context.SaveChangesAsync();
    }

    // 2. Regra de Negócio: Ciclo de Vida, Exumação e Desvinculação
    public async Task EfetivarExumacaoAsync(int gavetaId, ExecutorExumacao executor, string observacoes)
    {
        // Utiliza transação para garantir consistência dos dados
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var gaveta = await _context.GavetasPublicas
                .FirstOrDefaultAsync(g => g.Id == gavetaId);

            if (gaveta == null || !gaveta.Ocupada || gaveta.FalecidoId == null)
            {
                throw new InvalidOperationException("Gaveta não encontrada, já está livre ou não possui falecido vinculado.");
            }

            // Cria o registro histórico da exumação (Rastreabilidade)
            var exumacao = new ExumacaoRegistro
            {
                FalecidoId = gaveta.FalecidoId.Value,
                GavetaPublicaId = gaveta.Id,
                DataAutorizacao = DateTime.UtcNow, // A data de autorização idealmente vem de um fluxo anterior do CAAF
                SetorAutorizador = "CAAF",
                DataExecucao = DateTime.UtcNow, // Momento da efetivação real
                Executor = executor,
                Observacoes = observacoes
            };

            _context.ExumacoesRegistros.Add(exumacao);

            // Limpa a gaveta para permitir um novo cadastro do zero
            gaveta.Ocupada = false;
            gaveta.FalecidoId = null;
            gaveta.DataOcupacao = null;
            gaveta.DataPrevisaoExumacao = null;

            _context.GavetasPublicas.Update(gaveta);
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}