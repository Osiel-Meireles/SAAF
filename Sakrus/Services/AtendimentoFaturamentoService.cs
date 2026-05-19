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
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AtendimentoFaturamentoService> _logger;

    public AtendimentoFaturamentoService(ApplicationDbContext context, ILogger<AtendimentoFaturamentoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // 1. Regra de Negócio: Abatimento e Precificação Dinâmica
    public async Task AdicionarItemFaturadoAsync(int atendimentoId, string categoria, decimal quantidadeOuKm)
    {
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
            AbatidoDoEstoque     = true
        };

        _context.ItensFaturados.Add(item);
        await _context.SaveChangesAsync();
    }

    // 2. Regra de Negócio: DRY e Documentação (Gerar OS e Auxílio)
    public async Task GerarOrdemServicoUnificadaAsync(int atendimentoId, string numeroGuia)
    {
        var atendimento = await _context.Atendimentos
            .Include(a => a.ItensFaturados)
            .FirstOrDefaultAsync(a => a.Id == atendimentoId);

        if (atendimento == null)
            throw new InvalidOperationException("Atendimento não encontrado.");

        if (atendimento.ItensFaturados.Count == 0)
            throw new InvalidOperationException("Não é possível gerar uma OS sem itens faturados.");

        // Regra DRY: A Ordem de Serviço e o Auxílio Funeral devem ter obrigatoriamente a mesma numeração.
        // Se já tiver uma numeração, não gera novamente. Se não tiver, usa o número da Guia ou cria um sequencial.
        if (string.IsNullOrWhiteSpace(atendimento.NumeroOsAuxilio))
        {
            // Padrão de numeração: ANO-MÊS-NUMERO_GUIA
            atendimento.NumeroOsAuxilio = $"{DateTime.UtcNow.Year}{DateTime.UtcNow.Month:D2}-{numeroGuia}";
        }

        // Simulação da geração de PDF ou extrato (Aqui você faria a chamada para sua biblioteca de PDF)
        var totalFaturado = atendimento.ItensFaturados.Sum(i => i.ValorTotalCalculado);
        
        // Em um cenário real, você dispararia um evento (ex: RabbitMQ/MediatR) ou chamaria o serviço de PDF
        _logger.LogInformation($"OS/Auxílio Gerada: {atendimento.NumeroOsAuxilio} | Total a pagar à Funerária: R$ {totalFaturado:N2}");

        _context.Atendimentos.Update(atendimento);
        await _context.SaveChangesAsync();
    }
}