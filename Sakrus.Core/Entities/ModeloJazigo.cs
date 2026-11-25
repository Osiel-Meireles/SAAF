using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class ModeloJazigo
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty; // Ex: Tradicional, Duplo, Mausoléu
    
    [Range(0, 100)]
    public decimal PercentualConcessao { get; set; }
    
    [Range(0, 100)]
    public decimal PercentualManutencao { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal TaxaConstrucao { get; set; }

    /// <summary>
    /// Valor anual da taxa de manutenção cobrado para este tipo de jazigo.
    /// É o valor vigente aplicado a todos os exercícios em aberto na folha financeira.
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal ValorManutencaoAnual { get; set; }
}