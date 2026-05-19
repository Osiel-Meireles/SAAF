using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class Ossuario
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Identificador { get; set; } = string.Empty;

    public TipoOssuario Tipo { get; set; }

    // Se for particular, está vinculado a um Jazigo da família
    public int? JazigoVinculadoId { get; set; }
    public Jazigo? JazigoVinculado { get; set; }

    public int Capacidade { get; set; }
    
    public bool Ocupado { get; set; }

    public List<Falecido> Falecidos { get; set; } = new();
}
