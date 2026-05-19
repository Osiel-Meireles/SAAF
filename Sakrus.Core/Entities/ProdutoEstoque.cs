using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class ProdutoEstoque
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    public int QuantidadeDisponivel { get; set; }
    
    // Alerta de estoque mínimo
    public int EstoqueMinimo { get; set; } = 5;

    public decimal Custo { get; set; }
    public decimal ValorVenda { get; set; }

    public List<MovimentacaoEstoque> Movimentacoes { get; set; } = new();
}
