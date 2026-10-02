using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class ServicoAuxilio
{
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Descricao { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;
}
