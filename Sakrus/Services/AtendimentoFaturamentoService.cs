using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services;

public class AtendimentoFaturamentoService : IAtendimentoFaturamentoService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogger<AtendimentoFaturamentoService> _logger;
    private readonly EstoqueService _estoqueService;

    public AtendimentoFaturamentoService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogger<AtendimentoFaturamentoService> logger, EstoqueService estoqueService)
    {
        _dbFactory = dbFactory;
        _logger = logger;
        _estoqueService = estoqueService;
    }

    // 1. Regra de Negócio: Abatimento e Precificação Dinâmica
    public async Task AdicionarItemFaturadoAsync(int atendimentoId, string categoria, decimal quantidadeOuKm)
    {
        using var _context = await _dbFactory.CreateDbContextAsync();
        // Puxa a configuração financeira mais recente
        var config = await _context.ConfiguracoesFinanceiras
            .OrderByDescending(c => c.DataUltimaAtualizacao)
            .FirstOrDefaultAsync();

        if (config == null)
            throw new InvalidOperationException(
                "Configuração financeira não encontrada. Acesse Configurações > Financeiro para cadastrar os valores iniciais.");

        // Lê o valor unitário da configuração do banco (sem hardcode)
        decimal valorUnitario = categoria.ToLower() switch
        {
            "urna padrão"    => config.PrecoUrnaBasica,
            "urna especial"  => config.PrecoUrnaEspecial,
            "translado (km)" => config.PrecoTransladoPorKm,
            "pompa"          => config.PrecoPompa,
            "preparo"        => config.PrecoPreparoCorpo,
            _                => 0m
        };

        var item = new ItemFaturado
        {
            AtendimentoId        = atendimentoId,
            CategoriaItem        = categoria,
            QuantidadeOuKm       = quantidadeOuKm,
            ValorTotalCalculado  = valorUnitario * quantidadeOuKm,
            AbatidoDoEstoque     = false
        };

        _context.ItensFaturados.Add(item);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverItemFaturadoAsync(int itemId)
    {
        using var _context = await _dbFactory.CreateDbContextAsync();
        var item = await _context.ItensFaturados.FindAsync(itemId);
        if (item == null) throw new InvalidOperationException("Item não encontrado.");
        _context.ItensFaturados.Remove(item);
        await _context.SaveChangesAsync();
    }

    public async Task EditarItemFaturadoAsync(int itemId, string categoria, decimal quantidadeOuKm)
    {
        using var _context = await _dbFactory.CreateDbContextAsync();
        var item = await _context.ItensFaturados.FindAsync(itemId);
        if (item == null) throw new InvalidOperationException("Item não encontrado.");

        var config = await _context.ConfiguracoesFinanceiras
            .OrderByDescending(c => c.DataUltimaAtualizacao)
            .FirstOrDefaultAsync();

        decimal valorUnitario = config != null ? categoria.ToLower() switch
        {
            "urna padrão"    => config.PrecoUrnaBasica,
            "urna especial"  => config.PrecoUrnaEspecial,
            "translado (km)" => config.PrecoTransladoPorKm,
            "pompa"          => config.PrecoPompa,
            "preparo"        => config.PrecoPreparoCorpo,
            _                => 0m
        } : 0m;

        item.CategoriaItem       = categoria;
        item.QuantidadeOuKm      = quantidadeOuKm;
        item.ValorTotalCalculado = valorUnitario * quantidadeOuKm;

        _context.ItensFaturados.Update(item);
        await _context.SaveChangesAsync();
    }


    public async Task GerarOrdemServicoUnificadaAsync(int atendimentoId, string numeroGuia)
    {
        using var _context = await _dbFactory.CreateDbContextAsync();
        var atendimento = await _context.Atendimentos
            .Include(a => a.ItensFaturados)
            .FirstOrDefaultAsync(a => a.Id == atendimentoId);

        if (atendimento == null)
            throw new InvalidOperationException("Atendimento não encontrado.");

        if (atendimento.ItensFaturados.Count == 0)
            throw new InvalidOperationException("Não é possível gerar uma OS sem itens faturados.");

        // BUG-03: Processar a baixa de estoque ANTES de gerar a OS
        // Se der erro (estoque negativo), vai dar um throw e não finaliza a OS.
        await _estoqueService.ProcessarFaturamentoAtendimentoAsync(atendimentoId);

        // Marcar a OS como finalizada — separado do NumeroOsAuxilio (protocolo de criação)
        atendimento.OsFinalizada = true;

        // Simulação da geração de PDF ou extrato (Aqui você faria a chamada para sua biblioteca de PDF)
        var totalFaturado = atendimento.ItensFaturados.Sum(i => i.ValorTotalCalculado);
        
        // Em um cenário real, você dispararia um evento (ex: RabbitMQ/MediatR) ou chamaria o serviço de PDF
        _logger.LogInformation($"OS/Auxílio Finalizada: {atendimento.NumeroOsAuxilio} | Total a pagar à Funerária: R$ {totalFaturado:N2}");

        _context.Atendimentos.Update(atendimento);
        await _context.SaveChangesAsync();
    }
}