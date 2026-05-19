using System;
using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class RegistroCapela
{
    public int Id { get; set; }
    public int CapelaId { get; set; }
    public int AtendimentoId { get; set; }
    public DateTime HoraEntrada { get; set; }
    public DateTime? HoraSaida { get; set; }
    
    // Propriedades de navegação
    public Capela? Capela { get; set; }
    public Atendimento? Atendimento { get; set; }
}