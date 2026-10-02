using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class AssuntoProtocoloDocumento
{
    public int Id { get; set; }

    public int AssuntoProtocoloId { get; set; }
    public AssuntoProtocolo AssuntoProtocolo { get; set; } = null!;

    [Required]
    [MaxLength(30)]
    public string TipoDocumentoCodigo { get; set; } = string.Empty;

    public int Ordem { get; set; }
}
