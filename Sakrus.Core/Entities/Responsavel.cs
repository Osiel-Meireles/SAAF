using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class Responsavel
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(150)]
    public string Nome { get; set; } = string.Empty;
    
    [Required]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "CPF deve conter 11 dígitos")]
    public string CPF { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string RG { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string OrgaoEmissor { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Profissao { get; set; } = string.Empty;
    
    [MaxLength(250)]
    public string Endereco { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string Telefone { get; set; } = string.Empty;
}