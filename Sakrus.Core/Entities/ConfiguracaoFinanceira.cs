using System;
using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class ConfiguracaoFinanceira
{
    [Key]
    public int Id { get; set; }
    public decimal ValorMetroQuadrado { get; set; }
    public decimal TaxaManutencaoBase { get; set; }
    public decimal TaxaConcessaoBase { get; set; }
    public DateTime DataUltimaAtualizacao { get; set; }
}