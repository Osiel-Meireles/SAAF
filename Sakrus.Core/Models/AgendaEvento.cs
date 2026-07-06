using System;

namespace Sakrus.Core.Models;

public class AgendaEvento
{
    public TimeSpan Horario { get; set; }
    public string TipoEvento { get; set; } = string.Empty; // "Sepultamento" ou "Exumação"
    public string FalecidoNome { get; set; } = string.Empty;
    public string Local { get; set; } = string.Empty;
}
