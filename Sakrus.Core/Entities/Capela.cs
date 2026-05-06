namespace Sakrus.Core.Entities;

public class Capela
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Localizacao { get; set; } = string.Empty;
    public string Status { get; set; } = "Disponível"; // Disponível, Ocupada, Manutenção
}