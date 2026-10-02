using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class AssuntoProtocolo
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    public bool GeraRegularizacao { get; set; }

    public bool Ativo { get; set; } = true;

    public List<AssuntoProtocoloDocumento> Documentos { get; set; } = new();
}
