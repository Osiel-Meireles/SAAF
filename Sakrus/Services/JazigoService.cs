using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services;

public class JazigoService : IJazigoService
{
    private readonly ApplicationDbContext _context;

    public JazigoService(ApplicationDbContext context)
    {
        _context = context;
    }

    // --- NOVO MÉTODO PARA O MAPA DE JAZIGOS ---
    // Retorna todos os jazigos cadastrados no banco de dados
    public async Task<List<Jazigo>> ObterTodosAsync()
    {
        return await _context.Jazigos
            .Include(j => j.ModeloJazigo) // Traz os dados do modelo vinculado (opcional, mas recomendado)
            .ToListAsync();
    }

    // --- Regra 4: Expansão Paramétrica ---
    // Permite criar novos tipos de jazigos dinamicamente (Ex: "Mausoléu Premium")
    public async Task<ModeloJazigo> CadastrarModeloJazigoAsync(string nome, decimal pctConcessao, decimal pctManutencao, decimal taxaConstrucao)
    {
        var modelo = new ModeloJazigo
        {
            Nome = nome,
            PercentualConcessao = pctConcessao,
            PercentualManutencao = pctManutencao,
            TaxaConstrucao = taxaConstrucao
        };

        _context.ModelosJazigos.Add(modelo);
        await _context.SaveChangesAsync();

        return modelo;
    }

    // --- Regra 3: Autonomia - Jazigos Infantis ---
    // Sem limites de quantidade. Adiciona um lote sob demanda.
    public async Task<Jazigo> AdicionarJazigoInfantilAsync(string codigoIdentificador, int modeloId)
    {
        var modelo = await _context.ModelosJazigos.FindAsync(modeloId);
        if (modelo == null) 
            throw new InvalidOperationException("Modelo de jazigo não encontrado.");

        var novoJazigo = new Jazigo
        {
            CodigoIdentificador = codigoIdentificador,
            ModeloJazigoId = modeloId,
            IsInfantil = true,
            Ocupado = false
        };

        _context.Jazigos.Add(novoJazigo);
        await _context.SaveChangesAsync();
        
        return novoJazigo;
    }

    // --- Regra 3: Desmembramento de Jazigos ---
    // Divide um Lote grande em lotes menores (Ex: Lote 10 vira 10-A, 10-B, 10-C...)
    public async Task<List<Jazigo>> DesmembrarJazigoAsync(int jazigoPaiId, int quantidadePartes)
    {
        // Validação: máximo 26 partes (A-Z)
        if (quantidadePartes > 26)
            throw new InvalidOperationException("Não é possível criar mais de 26 partes (A-Z) em um desmembramento.");
        if (quantidadePartes <= 0)
            throw new InvalidOperationException("A quantidade de partes deve ser maior que zero.");

        var jazigoPai = await _context.Jazigos.FindAsync(jazigoPaiId);
        
        if (jazigoPai == null) 
            throw new InvalidOperationException("Jazigo pai não encontrado.");
        if (jazigoPai.Ocupado) 
            throw new InvalidOperationException("Não é possível desmembrar um jazigo que possui corpos sepultados.");

        // Utiliza transação para garantir consistência dos dados
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var novosLotes = new List<Jazigo>();
            
            // Gera sufixos automáticos em ordem alfabética (A, B, C...)
            char sufixo = 'A';
            for (int i = 0; i < quantidadePartes; i++)
            {
                novosLotes.Add(new Jazigo
                {
                    CodigoIdentificador = $"{jazigoPai.CodigoIdentificador}-{sufixo}",
                    ModeloJazigoId = jazigoPai.ModeloJazigoId, // Herda a parametrização do pai
                    IsInfantil = jazigoPai.IsInfantil,
                    Ocupado = false,
                    JazigoPaiId = jazigoPai.Id
                });
                sufixo++;
            }

            // "Bloqueia" o pai para evitar que ele seja vendido inteiro novamente após o desmembramento
            jazigoPai.Ocupado = true; 

            _context.Jazigos.AddRange(novosLotes);
            _context.Jazigos.Update(jazigoPai);
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return novosLotes;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // --- Regra de Ouro: Exumação de Jazigo ---
    // Libera o jazigo ao remover o falecido, movendo dados para ExumacaoRegistro
    // Implementa a regra crítica: "A gaveta DEVE libertar o Jazigo para novo sepultamento"
    public async Task ExumarJazigoAsync(int jazigoId, ExecutorExumacao executor, string observacoes = "")
    {
        // Utiliza transação para garantir consistência dos dados
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var jazigo = await _context.Jazigos
                .Include(j => j.Falecidos)
                .FirstOrDefaultAsync(j => j.Id == jazigoId);

            if (jazigo == null || !jazigo.Ocupado)
            {
                throw new InvalidOperationException("Jazigo não encontrado ou já está vago.");
            }

            if (jazigo.Falecidos.Count == 0)
            {
                throw new InvalidOperationException("Nenhum falecido vinculado a este jazigo.");
            }

            // Processa cada falecido relacionado (para casos de múltiplos)
            foreach (var falecido in jazigo.Falecidos)
            {
                // Cria o registro histórico da exumação (Rastreabilidade)
                var exumacao = new ExumacaoRegistro
                {
                    FalecidoId = falecido.Id,
                    JazigoId = jazigo.Id,
                    DataAutorizacao = DateTime.UtcNow,
                    SetorAutorizador = "CAAF",
                    DataExecucao = DateTime.UtcNow,
                    Executor = executor,
                    Observacoes = observacoes
                };

                _context.ExumacoesRegistros.Add(exumacao);
                
                // Desvincula o falecido do jazigo (O corpo foi removido)
                falecido.JazigoId = null;
            }

            // 🔥 REGRA DE OURO: Libera o Jazigo para novo sepultamento
            jazigo.Ocupado = false;
            _context.Jazigos.Update(jazigo);
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}