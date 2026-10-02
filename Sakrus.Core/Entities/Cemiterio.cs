using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class Cemiterio
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Municipio { get; set; } = "Luís Eduardo Magalhães";

    [MaxLength(2)]
    public string Uf { get; set; } = "BA";

    public bool Ativo { get; set; } = true;
}
