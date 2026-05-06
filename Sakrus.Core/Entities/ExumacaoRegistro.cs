using System;
using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class ExumacaoRegistro
{
    [Key]
    public int Id { get; set; }
    
    // Mantém o histórico do corpo desvinculado
    public int FalecidoId { get; set; }
    public Falecido Falecido { get; set; } = null!;
    
    // Rastreabilidade: De onde o corpo foi tirado?
    public int? GavetaPublicaId { get; set; }
    public GavetaPublica? GavetaPublica { get; set; }
    
    public int? JazigoId { get; set; }
    public Jazigo? Jazigo { get; set; }
    
    public DateTime DataAutorizacao { get; set; }
    public string SetorAutorizador { get; set; } = "CAAF";
    
    public DateTime? DataExecucao { get; set; }
    public ExecutorExumacao Executor { get; set; }
    public string Observacoes { get; set; } = string.Empty;
}