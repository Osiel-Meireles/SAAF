using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services
{
    public class FalecidoService : IFalecidoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IJazigoService _jazigoService;

        public FalecidoService(ApplicationDbContext context, IJazigoService jazigoService)
        {
            _context = context;
            _jazigoService = jazigoService;
        }

        public async Task<Falecido> RegistrarSepultamentoAsync(Falecido falecido)
        {
            // 1. Adiciona o registro do falecido no banco
            _context.Falecidos.Add(falecido);

            // 2. Busca o jazigo associado para marcar como ocupado
            var jazigo = await _context.Jazigos.FindAsync(falecido.JazigoId);
            if (jazigo != null)
            {
                jazigo.Ocupado = true; // Usando a sua propriedade booleana
                _context.Jazigos.Update(jazigo);
            }

            // 3. Salva todas as alterações em uma única transação
            await _context.SaveChangesAsync();

            return falecido;
        }

        public async Task ExumarAsync(int falecidoId, ExecutorExumacao executor, string observacoes = "")
        {
            // Busca o falecido e seu jazigo
            var falecido = await _context.Falecidos
                .Include(f => f.Jazigo)
                .FirstOrDefaultAsync(f => f.Id == falecidoId);

            if (falecido == null)
            {
                throw new InvalidOperationException("Falecido não encontrado.");
            }

            if (falecido.Jazigo == null)
            {
                throw new InvalidOperationException("Falecido não possui jazigo vinculado.");
            }

            // Delega ao JazigoService para liberar o jazigo e registrar a exumação
            // Isso mantém a lógica centralizada e segue o SRP
            await _jazigoService.ExumarJazigoAsync(falecido.Id, falecido.JazigoId.Value, executor, observacoes);
        }
    }
}