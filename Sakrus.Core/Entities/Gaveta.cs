using System;
using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class Gaveta
{
    public int Id { get; set; }

    public int JazigoId { get; set; }
    public Jazigo Jazigo { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string Numero { get; set; } = string.Empty;

    public int? ClassificacaoEspacoId { get; set; }
    public ClassificacaoEspaco? ClassificacaoEspaco { get; set; }

    public int? FalecidoId { get; set; }
    public Falecido? Falecido { get; set; }

    public DateTime? DataSepultamento { get; set; }
}
