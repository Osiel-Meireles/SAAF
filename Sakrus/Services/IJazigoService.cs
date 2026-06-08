using Sakrus.Core.Entities;

namespace Sakrus.Services;

public interface IJazigoService
{
    Task<List<Jazigo>> ObterTodosAsync();
    Task<ModeloJazigo> CadastrarModeloJazigoAsync(string nome, decimal pctConcessao, decimal pctManutencao, decimal taxaConstrucao);
    Task<Jazigo> AdicionarJazigoInfantilAsync(string codigoIdentificador, int modeloId);
    Task<List<Jazigo>> DesmembrarJazigoAsync(int jazigoPaiId, int quantidadePartes);
    Task DesfazerDesmembramentoJazigoAsync(int jazigoPaiId);
    Task ExumarJazigoAsync(int falecidoId, int jazigoId, ExecutorExumacao executor, string observacoes = "");
    Task TentarDesfazerDivisaoAutomaticaAsync(int jazigoId);
    Task GarantirOssuarioGeralExisteAsync();
}
