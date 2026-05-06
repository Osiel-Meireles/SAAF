using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services;

public class JazigoService
{
    private readonly ApplicationDbContext _context;

    public JazigoService(ApplicationDbContext context)
    {
        _context = context;
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
        var jazigoPai = await _context.Jazigos.FindAsync(jazigoPaiId);
        
        if (jazigoPai == null) 
            throw new InvalidOperationException("Jazigo pai não encontrado.");
        if (jazigoPai.Ocupado) 
            throw new InvalidOperationException("Não é possível desmembrar um jazigo que possui corpos sepultados.");

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

        return novosLotes;
    }
}