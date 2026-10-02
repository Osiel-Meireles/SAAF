namespace Sakrus.Core.Entities;

public class AtendimentoServicoAuxilio
{
    public int Id { get; set; }

    public int AtendimentoId { get; set; }
    public Atendimento Atendimento { get; set; } = null!;

    public int ServicoAuxilioId { get; set; }
    public ServicoAuxilio ServicoAuxilio { get; set; } = null!;
}
