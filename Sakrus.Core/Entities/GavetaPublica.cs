using System;
using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class GavetaPublica
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Setor { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Quadra { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Lote { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string NumeroGaveta { get; set; } = string.Empty;
    
    public bool Ocupada { get; set; }
    public int? FalecidoId { get; set; }
    public Falecido? Falecido { get; set; }
    public DateTime? DataOcupacao { get; set; }
    public DateTime? DataPrevisaoExumacao { get; set; }
}