using System.Collections.Generic;
using System.Threading.Tasks;
using Sakrus.Core.Entities;

namespace Sakrus.Services;

public interface ICapelaService
{
    Task<List<Capela>> ObterTodasAsync();
    Task<List<RegistroCapela>> ObterRegistrosAtivosAsync();
    Task<RegistroCapela> AgendarCapelaAsync(int capelaId, int atendimentoId, System.DateTime horaEntrada, System.DateTime? horaSaidaPrevista);
    Task EncerrarVelorioAsync(int registroId);
}
