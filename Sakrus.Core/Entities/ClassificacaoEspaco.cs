using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class ClassificacaoEspaco
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    public NaturezaEspaco Natureza { get; set; }

    public bool Ativo { get; set; } = true;
}
