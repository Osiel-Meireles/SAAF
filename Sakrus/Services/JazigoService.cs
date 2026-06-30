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
    private readonly ILogger<JazigoService> _logger;

    public JazigoService(ApplicationDbContext context, ILogger<JazigoService> logger)
    {
        _context = context;
        _logger = logger;
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
                    Quadra = jazigoPai.Quadra,
                    Ala = jazigoPai.Ala,
                    NumeroLote = $"{jazigoPai.NumeroLote}-{sufixo}",
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

    public async Task DesfazerDesmembramentoJazigoAsync(int jazigoPaiId)
    {
        var jazigoPai = await _context.Jazigos
            .Include(j => j.Falecidos)
            .FirstOrDefaultAsync(j => j.Id == jazigoPaiId);
        
        if (jazigoPai == null)
            throw new InvalidOperationException("Jazigo pai não encontrado.");
            
        // Obter os sub-lotes
        var subLotes = await _context.Jazigos
            .Include(j => j.Falecidos)
            .Where(j => j.JazigoPaiId == jazigoPaiId)
            .ToListAsync();
            
        if (!subLotes.Any())
            throw new InvalidOperationException("Este jazigo não possui subdivisões para desfazer.");
            
        // Verificar se algum sub-lote está ocupado ou tem falecidos associados
        if (subLotes.Any(s => s.Ocupado || s.Falecidos.Any()))
            throw new InvalidOperationException("Não é possível desfazer a divisão pois um ou mais sub-lotes estão ocupados ou contêm sepultamentos.");

        var subLotesIds = subLotes.Select(s => s.Id).ToList();

        // Verificar se há histórico de titularidade vinculado aos sub-lotes
        var temHistorico = await _context.HistoricoTitularidadeJazigos.AnyAsync(h => subLotesIds.Contains(h.JazigoId));
        if (temHistorico)
            throw new InvalidOperationException("Não é possível desfazer a divisão pois existem registros de histórico de titularidade vinculados aos sub-lotes.");

        // Verificar se há exumações vinculadas aos sub-lotes
        var temExumacao = await _context.ExumacoesRegistros.AnyAsync(e => e.JazigoId != null && subLotesIds.Contains(e.JazigoId.Value));
        if (temExumacao)
            throw new InvalidOperationException("Não é possível desfazer a divisão pois existem registros de exumação vinculados aos sub-lotes.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Remover todos os sub-lotes
            _context.Jazigos.RemoveRange(subLotes);
            
            // Liberar o pai
            jazigoPai.Ocupado = false;
            _context.Jazigos.Update(jazigoPai);
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
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
    public async Task ExumarJazigoAsync(int falecidoId, int jazigoId, ExecutorExumacao executor, string observacoes = "")
    {
        // Utiliza transação para garantir consistência dos dados
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var jazigo = await _context.Jazigos
                .FirstOrDefaultAsync(j => j.Id == jazigoId);

            if (jazigo == null || !jazigo.Ocupado)
            {
                throw new InvalidOperationException("Jazigo não encontrado ou já está vago.");
            }

            var falecido = await _context.Falecidos
                .FirstOrDefaultAsync(f => f.Id == falecidoId && f.JazigoId == jazigoId);
            if (falecido == null)
            {
                throw new InvalidOperationException("O falecido especificado não está vinculado a este jazigo.");
            }

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
            // BUGFIX: Marcar o falecido como Exumado para impedir re-sepultamento indevido
            falecido.Status = StatusFalecido.Exumado;
            _context.Falecidos.Update(falecido);

            // Verifica via query direta ao banco se restam outros falecidos neste jazigo
            var existeOutroFalecido = await _context.Falecidos
                .AnyAsync(f => f.JazigoId == jazigoId && f.Id != falecidoId);

            if (!existeOutroFalecido)
            {
                jazigo.Ocupado = false;
                _context.Jazigos.Update(jazigo);
            }
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Após a transação, tenta desfazer a divisão automaticamente se aplicável
            await TentarDesfazerDivisaoAutomaticaAsync(jazigoId);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // --- Desfazimento Automático de Divisão ---
    // Após exumação, se todos os sub-lotes de um jazigo pai ficarem livres,
    // desfaz o desmembramento automaticamente (remove sub-lotes, libera o pai)
    public async Task TentarDesfazerDivisaoAutomaticaAsync(int jazigoId)
    {
        // Busca o jazigo para verificar se é um sub-lote (tem JazigoPaiId)
        var jazigo = await _context.Jazigos.FirstOrDefaultAsync(j => j.Id == jazigoId);
        if (jazigo == null || jazigo.JazigoPaiId == null)
            return; // Não é sub-lote, nada a fazer

        int jazigoPaiId = jazigo.JazigoPaiId.Value;

        // Busca todos os sub-lotes do mesmo pai
        var subLotes = await _context.Jazigos
            .Where(j => j.JazigoPaiId == jazigoPaiId)
            .ToListAsync();

        if (!subLotes.Any())
            return;

        // Verifica se TODOS os sub-lotes estão livres (não ocupados E sem falecidos)
        var subLotesIds = subLotes.Select(s => s.Id).ToList();
        var algumOcupado = subLotes.Any(s => s.Ocupado);
        var algumComFalecido = await _context.Falecidos
            .AnyAsync(f => f.JazigoId != null && subLotesIds.Contains(f.JazigoId.Value));

        if (algumOcupado || algumComFalecido)
            return; // Ainda há sub-lotes em uso, não desfaz

        // Tudo limpo — desfazer a divisão automaticamente
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var jazigoPai = await _context.Jazigos.FindAsync(jazigoPaiId);
            if (jazigoPai == null)
                return;

            // Remove todos os sub-lotes
            _context.Jazigos.RemoveRange(subLotes);

            // Libera o pai
            jazigoPai.Ocupado = false;
            _context.Jazigos.Update(jazigoPai);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            // ROB-01: Falha silenciosa para a UI, mas agora rastreável via log
            _logger.LogWarning(ex, "Falha ao tentar desfazer divisão automática do jazigo pai {JazigoPaiId}.", jazigoPaiId);
        }
    }

    // --- Garantir Ossuário Geral ---
    // Cria um ossuário geral padrão caso nenhum exista no sistema
    public async Task GarantirOssuarioGeralExisteAsync()
    {
        var existeGeral = await _context.Ossuarios.AnyAsync(o => o.Tipo == TipoOssuario.Geral);
        if (!existeGeral)
        {
            var ossuario = new Ossuario
            {
                Identificador = "Ossuário Geral Municipal",
                Tipo = TipoOssuario.Geral,
                Capacidade = 500
            };
            _context.Ossuarios.Add(ossuario);
            await _context.SaveChangesAsync();
        }
    }
}