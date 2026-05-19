using System;
using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class HistoricoTitularidadeJazigo
{
    public int Id { get; set; }
    
    public int JazigoId { get; set; }
    public Jazigo Jazigo { get; set; } = null!;
    
    public int ResponsavelAntigoId { get; set; }
    public Responsavel ResponsavelAntigo { get; set; } = null!;
    
    public int ResponsavelNovoId { get; set; }
    public Responsavel ResponsavelNovo { get; set; } = null!;
    
    public DateTime DataTransferencia { get; set; } = DateTime.UtcNow;
    
    [Required]
    [MaxLength(500)]
    public string Motivo { get; set; } = string.Empty;
    
    // Pode ser logado quem fez a operação se tivermos gestão de usuários (opcional aqui, mas recomendado)
    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
}
