using System;
using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class MovimentacaoEstoque
{
    public int Id { get; set; }

    public int ProdutoEstoqueId { get; set; }
    public ProdutoEstoque ProdutoEstoque { get; set; } = null!;

    public TipoMovimentacaoEstoque TipoMovimentacao { get; set; }
    
    public int Quantidade { get; set; }
    
    public DateTime DataMovimentacao { get; set; } = DateTime.UtcNow;

    [MaxLength(250)]
    public string Motivo { get; set; } = string.Empty;
}
