using System;
using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class ConfiguracaoFinanceira
{
    public int Id { get; set; }

    // ─── Taxas de Jazigo ──────────────────────────────────────
    [Range(0, double.MaxValue, ErrorMessage = "Valor deve ser positivo")]
    public decimal ValorMetroQuadrado { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Valor deve ser positivo")]
    public decimal TaxaManutencaoBase { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Valor deve ser positivo")]
    public decimal TaxaConcessaoBase { get; set; }

    // ─── Tabela de Preços de Serviços Funerários ──────────────
    [Range(0, double.MaxValue, ErrorMessage = "Valor deve ser positivo")]
    public decimal PrecoUrnaBasica { get; set; } = 450.00m;

    [Range(0, double.MaxValue, ErrorMessage = "Valor deve ser positivo")]
    public decimal PrecoUrnaEspecial { get; set; } = 800.00m;

    [Range(0, double.MaxValue, ErrorMessage = "Valor deve ser positivo")]
    public decimal PrecoTransladoPorKm { get; set; } = 2.50m;

    [Range(0, double.MaxValue, ErrorMessage = "Valor deve ser positivo")]
    public decimal PrecoPompa { get; set; } = 300.00m;

    [Range(0, double.MaxValue, ErrorMessage = "Valor deve ser positivo")]
    public decimal PrecoPreparoCorpo { get; set; } = 250.00m;

    // ─── Controle ─────────────────────────────────────────────
    public DateTime DataUltimaAtualizacao { get; set; }

    [MaxLength(150)]
    public string? AtualizadoPor { get; set; }
}