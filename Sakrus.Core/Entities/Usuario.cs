namespace Sakrus.Core.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public int NivelAcesso { get; set; } // Substitui o antigo MM_NVL
    public bool Ativo { get; set; } = true;
}