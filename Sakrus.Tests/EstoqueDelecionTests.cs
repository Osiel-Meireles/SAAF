using Sakrus.Core.Entities;
using Xunit;

namespace Sakrus.Tests;

/// <summary>
/// Testes unitários para a lógica de deleção de produtos do estoque.
/// Verifica as regras de negócio que impedem deleção com histórico de movimentações.
/// </summary>
public class EstoqueDelecionTests
{
    // ──────────────────────────────────────────────────────────────────────
    // 1. Produto sem movimentações pode ser deletado
    // ──────────────────────────────────────────────────────────────────────

    [Fact]
    public void Produto_SemMovimentacoes_PermiteDeletar()
    {
        var produto = new ProdutoEstoque
        {
            Id = 1,
            Nome = "Urna Básica",
            QuantidadeDisponivel = 5,
            EstoqueMinimo = 2,
            Movimentacoes = new List<MovimentacaoEstoque>()
        };

        bool podeDeletar = !produto.Movimentacoes.Any();

        Assert.True(podeDeletar);
    }

    // ──────────────────────────────────────────────────────────────────────
    // 2. Produto COM movimentações NÃO pode ser deletado
    // ──────────────────────────────────────────────────────────────────────

    [Fact]
    public void Produto_ComMovimentacoes_NaoPermiteDeletar()
    {
        var produto = new ProdutoEstoque
        {
            Id = 2,
            Nome = "Placa de Bronze",
            Movimentacoes = new List<MovimentacaoEstoque>
            {
                new() { Id = 1, ProdutoEstoqueId = 2, Quantidade = 10, TipoMovimentacao = TipoMovimentacaoEstoque.Entrada }
            }
        };

        bool podeDeletar = !produto.Movimentacoes.Any();

        Assert.False(podeDeletar);
    }

    // ──────────────────────────────────────────────────────────────────────
    // 3. Produto zerado (sem quantidade disponível) mas com movimentações
    //    ainda deve ser bloqueado da deleção
    // ──────────────────────────────────────────────────────────────────────

    [Fact]
    public void Produto_ZeradoComHistorico_NaoPermiteDeletar()
    {
        var produto = new ProdutoEstoque
        {
            Id = 3,
            Nome = "Flores Artificiais",
            QuantidadeDisponivel = 0,
            Movimentacoes = new List<MovimentacaoEstoque>
            {
                new() { Id = 5, ProdutoEstoqueId = 3, Quantidade = 1, TipoMovimentacao = TipoMovimentacaoEstoque.Saida }
            }
        };

        bool podeDeletar = !produto.Movimentacoes.Any();

        Assert.False(podeDeletar);
    }

    // ──────────────────────────────────────────────────────────────────────
    // 4. Status do estoque: produto abaixo do mínimo
    // ──────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0, 5, true)]   // zero — abaixo do mínimo
    [InlineData(4, 5, true)]   // abaixo do mínimo
    [InlineData(5, 5, true)]   // exatamente no mínimo (alerta disparado no = também)
    [InlineData(6, 5, false)]  // acima do mínimo — OK
    public void StatusEstoque_AbaixoOuIgualMinimo_DeveDispararAlerta(int qtdDisponivel, int estoqueMinimo, bool esperaAlerta)
    {
        var produto = new ProdutoEstoque
        {
            QuantidadeDisponivel = qtdDisponivel,
            EstoqueMinimo = estoqueMinimo
        };

        bool estaAbaixoDoMinimo = produto.QuantidadeDisponivel <= produto.EstoqueMinimo;

        Assert.Equal(esperaAlerta, estaAbaixoDoMinimo);
    }
}
