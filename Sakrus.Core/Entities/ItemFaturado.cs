using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class ItemFaturado
{
    [Key]
    public int Id { get; set; }
    public int AtendimentoId { get; set; }
    public Atendimento Atendimento { get; set; } = null!;
    public string CategoriaItem { get; set; } = string.Empty;
    public decimal QuantidadeOuKm { get; set; }
    public decimal ValorTotalCalculado { get; set; }
    public bool AbatidoDoEstoque { get; set; }
}