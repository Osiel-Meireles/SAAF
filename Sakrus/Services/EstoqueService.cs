using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services;

public class EstoqueService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EstoqueService> _logger;

    public EstoqueService(ApplicationDbContext context, ILogger<EstoqueService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task DarBaixaEstoqueAsync(string nomeProduto, int quantidade, string motivo)
    {
        var produto = await _context.ProdutosEstoque
            .FirstOrDefaultAsync(p => p.Nome.ToLower() == nomeProduto.ToLower());

        if (produto == null)
        {
            _logger.LogWarning("Produto {Nome} não encontrado no estoque para dar baixa.", nomeProduto);
            throw new InvalidOperationException($"Produto '{nomeProduto}' não encontrado no estoque. Cadastre-o antes de faturar.");
        }

        if (produto.QuantidadeDisponivel < quantidade)
        {
            var msgErro = $"Estoque insuficiente para {nomeProduto}. Requisitado: {quantidade}, Disponível: {produto.QuantidadeDisponivel}";
            _logger.LogWarning(msgErro);
            throw new InvalidOperationException(msgErro);
        }

        produto.QuantidadeDisponivel -= quantidade;

        var movimentacao = new MovimentacaoEstoque
        {
            ProdutoEstoqueId = produto.Id,
            TipoMovimentacao = TipoMovimentacaoEstoque.Saida,
            Quantidade = quantidade,
            DataMovimentacao = DateTime.UtcNow,
            Motivo = motivo
        };

        _context.MovimentacoesEstoque.Add(movimentacao);
        await _context.SaveChangesAsync();

        if (produto.QuantidadeDisponivel <= produto.EstoqueMinimo)
        {
            _logger.LogWarning("ALERTA DE ESTOQUE: Produto {Nome} atingiu ou caiu abaixo do estoque mínimo ({Qtd}/{Min}).", 
                produto.Nome, produto.QuantidadeDisponivel, produto.EstoqueMinimo);
        }
    }
    
    // Método para ser chamado quando faturar um atendimento
    public async Task ProcessarFaturamentoAtendimentoAsync(int atendimentoId)
    {
        var atendimento = await _context.Atendimentos
            .Include(a => a.ItensFaturados)
            .FirstOrDefaultAsync(a => a.Id == atendimentoId);
            
        if (atendimento == null) return;
        
        foreach (var item in atendimento.ItensFaturados.Where(i => !i.AbatidoDoEstoque))
        {
            // Tenta dar baixa buscando pelo nome da categoria/item
            // "Urna Básica", "Placa de Bronze", etc
            await DarBaixaEstoqueAsync(item.CategoriaItem, (int)item.QuantidadeOuKm, $"Faturamento Atendimento OS {atendimento.NumeroOsAuxilio}");
            item.AbatidoDoEstoque = true;
        }
        await _context.SaveChangesAsync();
    }
}
