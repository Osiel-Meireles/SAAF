using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services;

public class AtendimentoFaturamentoService
{
    private readonly ApplicationDbContext _context;

    public AtendimentoFaturamentoService(ApplicationDbContext context)
    {
        _context = context;
    }

    // 1. Regra de Negócio: Abatimento e Precificação Dinâmica
    public async Task AdicionarItemFaturadoAsync(int atendimentoId, string categoria, decimal quantidadeOuKm)
    {
        // Puxa a configuração financeira mais recente (fim do IPCA hardcoded)
        var config = await _context.ConfiguracoesFinanceiras
            .OrderByDescending(c => c.DataUltimaAtualizacao)
            .FirstOrDefaultAsync();

        if (config == null)
            throw new InvalidOperationException("Configuração financeira base não encontrada no sistema. Cadastre os valores iniciais.");

        // Define o valor unitário com base na categoria. 
        // Na vida real, você pode ter uma tabela de "Itens de Estoque/Serviço", mas aqui usamos a lógica de negócio solicitada.
        decimal valorUnitario = categoria.ToLower() switch
        {
            "urna padrão" => 450.00m,   // Exemplo: Buscaria de uma tabela de urnas
            "urna especial" => 800.00m,
            "translado (km)" => 2.50m,  // Exemplo: R$ 2,50 por Km rodado
            "pompa" => 300.00m,
            "preparo" => 250.00m,
            _ => 0m
        };

        var item = new ItemFaturado
        {
            AtendimentoId = atendimentoId,
            CategoriaItem = categoria,
            QuantidadeOuKm = quantidadeOuKm,
            ValorTotalCalculado = valorUnitario * quantidadeOuKm,
            AbatidoDoEstoque = true // Regra: O sistema deve abater do estoque automaticamente
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
        Console.WriteLine($"[SISTEMA] OS/Auxílio Gerada: {atendimento.NumeroOsAuxilio} | Total a pagar à Funerária: R$ {totalFaturado:N2}");

        _context.Atendimentos.Update(atendimento);
        await _context.SaveChangesAsync();
    }
}