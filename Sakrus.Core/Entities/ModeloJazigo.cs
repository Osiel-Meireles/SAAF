using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class ModeloJazigo
{
    [Key]
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty; // Ex: Tradicional, Duplo, Mausoléu
    public decimal PercentualConcessao { get; set; }
    public decimal PercentualManutencao { get; set; }
    public decimal TaxaConstrucao { get; set; }
}