using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class ItemFaturado
{
    public int Id { get; set; }
    public int AtendimentoId { get; set; }
    public Atendimento Atendimento { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    public string CategoriaItem { get; set; } = string.Empty;
    
    [Range(0.01, double.MaxValue)]
    public decimal QuantidadeOuKm { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal ValorTotalCalculado { get; set; }
    
    public bool AbatidoDoEstoque { get; set; }
}