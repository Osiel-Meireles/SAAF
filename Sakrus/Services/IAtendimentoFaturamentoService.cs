namespace Sakrus.Services;

public interface IAtendimentoFaturamentoService
{
    Task AdicionarItemFaturadoAsync(int atendimentoId, string categoria, decimal quantidadeOuKm);
    Task GerarOrdemServicoUnificadaAsync(int atendimentoId, string numeroGuia);
}
