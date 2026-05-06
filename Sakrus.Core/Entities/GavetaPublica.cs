using System;
using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class GavetaPublica
{
    [Key]
    public int Id { get; set; }
    public string Setor { get; set; } = string.Empty;
    public string Quadra { get; set; } = string.Empty;
    public string Lote { get; set; } = string.Empty;
    public string NumeroGaveta { get; set; } = string.Empty;
    
    public bool Ocupada { get; set; }
    public int? FalecidoId { get; set; }
    public DateTime? DataOcupacao { get; set; }
    public DateTime? DataPrevisaoExumacao { get; set; }
}