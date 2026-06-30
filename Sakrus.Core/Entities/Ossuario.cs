using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class Ossuario
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Identificador { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Quadra { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Ala { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string NumeroLote { get; set; } = string.Empty;

    public TipoOssuario Tipo { get; set; }

    // Se for particular, está vinculado a um Jazigo da família
    public int? JazigoVinculadoId { get; set; }
    public Jazigo? JazigoVinculado { get; set; }

    public int Capacidade { get; set; }
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool Ocupado => Falecidos != null && Falecidos.Count >= Capacidade;

    public List<Falecido> Falecidos { get; set; } = new();
}
