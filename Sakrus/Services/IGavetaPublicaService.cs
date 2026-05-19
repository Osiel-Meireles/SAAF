using Sakrus.Core.Entities;

namespace Sakrus.Services;

public interface IGavetaPublicaService
{
    Task AdicionarGavetaAsync(GavetaPublica novaGaveta);
    Task EfetivarExumacaoAsync(int gavetaId, ExecutorExumacao executor, string observacoes);
}
