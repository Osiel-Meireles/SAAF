using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class Funeraria
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(20)]
    public string CNPJ { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Telefone { get; set; } = string.Empty;

    [MaxLength(250)]
    public string Endereco { get; set; } = string.Empty;

    public List<Atendimento> Atendimentos { get; set; } = new();
}
