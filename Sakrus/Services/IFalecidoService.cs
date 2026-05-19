using System.Threading.Tasks;
using Sakrus.Core.Entities;

namespace Sakrus.Services
{
    public interface IFalecidoService
    {
        Task<Falecido> RegistrarSepultamentoAsync(Falecido falecido);
        Task ExumarAsync(int falecidoId, ExecutorExumacao executor, string observacoes = "");
    }
}