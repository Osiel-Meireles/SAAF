using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class Responsavel
{
    [Key]
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public string RG { get; set; } = string.Empty;
    public string OrgaoEmissor { get; set; } = string.Empty;
    public string Profissao { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
}