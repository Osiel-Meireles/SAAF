using System;
using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class LancamentoFinanceiro
{
    public int Id { get; set; }

    public int JazigoId { get; set; }
    public Jazigo Jazigo { get; set; } = null!;

    public int? ResponsavelId { get; set; }

    public int Exercicio { get; set; }

    public TipoLancamento Tipo { get; set; }

    public decimal Valor { get; set; }

    public bool Pago { get; set; }

    public DateTime? DataPagamento { get; set; }

    [MaxLength(300)]
    public string? Observacao { get; set; }
}
