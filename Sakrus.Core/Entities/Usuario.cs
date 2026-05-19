using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class Usuario
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(150)]
    public string Nome { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string SenhaHash { get; set; } = string.Empty;
    
    [Range(1, 10)]
    public int NivelAcesso { get; set; } // Substitui o antigo MM_NVL
    
    public bool Ativo { get; set; } = true;
}