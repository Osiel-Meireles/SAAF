using System;
using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class Protocolo
{
    public int Id { get; set; }

    public int Ano { get; set; }
    public int Sequencia { get; set; }

    [Required]
    [MaxLength(20)]
    public string Numero { get; set; } = string.Empty;

    public int AssuntoProtocoloId { get; set; }
    public AssuntoProtocolo AssuntoProtocolo { get; set; } = null!;

    public int ResponsavelId { get; set; }
    public Responsavel Responsavel { get; set; } = null!;

    public int? JazigoId { get; set; }
    public Jazigo? Jazigo { get; set; }

    [MaxLength(1000)]
    public string Observacao { get; set; } = string.Empty;

    public DateTime DataAbertura { get; set; }

    public int? UsuarioId { get; set; }
}
