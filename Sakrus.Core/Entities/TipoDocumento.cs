using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class TipoDocumento
{
    public int Id { get; set; }

    [Required]
    [MaxLength(30)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Motor { get; set; } = "QuestPdf";

    [MaxLength(300)]
    public string? TemplateRef { get; set; }

    public bool ExigeNumeroUnico { get; set; }

    public bool GeraNumeroNaPrimeiraEmissao { get; set; }

    [MaxLength(40)]
    public string? ChaveNumeracao { get; set; }

    [MaxLength(30)]
    public string EntidadeTipo { get; set; } = string.Empty;

    public int Ordem { get; set; }

    public bool Ativo { get; set; } = true;
}
