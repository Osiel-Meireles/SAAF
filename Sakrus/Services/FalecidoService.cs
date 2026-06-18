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
            if (falecido.JazigoId.HasValue)
            {
                var jazigo = await _context.Jazigos.FindAsync(falecido.JazigoId.Value);
                
                if (jazigo == null)
                    throw new InvalidOperationException("Jazigo especificado não encontrado.");
                    
                if (jazigo.Ocupado)
                    throw new InvalidOperationException("Este jazigo já está ocupado. Verifique a disponibilidade antes de registrar o sepultamento.");
                    
                // Verifica também se já não existe outro falecido ativo neste jazigo (para redundância)
                var jaTemFalecido = await _context.Falecidos.AnyAsync(f => f.JazigoId == jazigo.Id);
                if (jaTemFalecido && !jazigo.IsInfantil) // Jazigos infantis permitem múltiplos sepultamentos se divididos adequadamente
                {
                    throw new InvalidOperationException("Este jazigo já contém restos mortais vinculados.");
                }

                jazigo.Ocupado = true;
                _context.Jazigos.Update(jazigo);
            }

            // 1. Adiciona o registro do falecido no banco
            _context.Falecidos.Add(falecido);

            // 2. Salva todas as alterações em uma única transação
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