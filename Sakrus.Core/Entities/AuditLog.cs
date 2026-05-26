using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class AuditLog
{
    public int Id { get; set; }
    
    [Required]
    public int UsuarioId { get; set; }
    
    [Required]
    [MaxLength(150)]
    public string NomeUsuario { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Acao { get; set; } = string.Empty; // CREATE, UPDATE, DELETE

    [Required]
    [MaxLength(100)]
    public string NomeEntidade { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string EntidadeId { get; set; } = string.Empty;
    
    [Required]
    public DateTime DataHoraUtc { get; set; } = DateTime.UtcNow;

    public string? ValoresAntigos { get; set; } // JSON
    public string? ValoresNovos { get; set; } // JSON
}
