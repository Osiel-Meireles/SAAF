using System;
using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class NumeroRegistro
{
    public int Id { get; set; }

    [Required]
    [MaxLength(40)]
    public string Chave { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string EntidadeTipo { get; set; } = string.Empty;

    public int EntidadeId { get; set; }

    public int Ano { get; set; }
    public int Sequencia { get; set; }

    [Required]
    [MaxLength(20)]
    public string Numero { get; set; } = string.Empty;

    public DateTime CriadoEm { get; set; }
}
