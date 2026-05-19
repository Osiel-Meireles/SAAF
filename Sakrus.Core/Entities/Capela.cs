using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class Capela
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(150)]
    public string Nome { get; set; } = string.Empty;
    
    [MaxLength(250)]
    public string Localizacao { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Status { get; set; } = "Disponível"; // Disponível, Ocupada, Manutenção
}