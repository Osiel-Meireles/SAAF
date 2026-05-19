using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Data;

/// <summary>
/// Responsável por popular o banco com dados essenciais na primeira execução.
/// </summary>
public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(ApplicationDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await SeedUsuarioAdminAsync();
        await SeedConfiguracaoFinanceiraAsync();
        await SeedFunerariasAsync();
        await SeedProdutosEstoqueAsync();
        await SeedOssuariosAsync();
    }

    private async Task SeedUsuarioAdminAsync()
    {
        if (await _context.Usuarios.AnyAsync())
            return;

        _logger.LogWarning("Nenhum usuário encontrado. Criando usuário administrador padrão...");

        var admin = new Usuario
        {
            Nome = "Administrador",
            Email = "admin@sakrus.local",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            NivelAcesso = 10,
            Ativo = true
        };

        _context.Usuarios.Add(admin);
        await _context.SaveChangesAsync();

        _logger.LogWarning(
            "Usuário admin criado: admin@sakrus.local / Admin@123 — TROQUE ESTA SENHA IMEDIATAMENTE.");
    }

    private async Task SeedConfiguracaoFinanceiraAsync()
    {
        if (await _context.ConfiguracoesFinanceiras.AnyAsync())
            return;

        _logger.LogInformation("Criando configuração financeira padrão...");

        _context.ConfiguracoesFinanceiras.Add(new ConfiguracaoFinanceira
        {
            ValorMetroQuadrado    = 350.00m,
            TaxaManutencaoBase    = 50.00m,
            TaxaConcessaoBase     = 200.00m,
            PrecoUrnaBasica       = 450.00m,
            PrecoUrnaEspecial     = 800.00m,
            PrecoTransladoPorKm   = 2.50m,
            PrecoPompa            = 300.00m,
            PrecoPreparoCorpo     = 250.00m,
            DataUltimaAtualizacao = DateTime.UtcNow,
            AtualizadoPor         = "Sistema (seed inicial)"
        });

        await _context.SaveChangesAsync();
        _logger.LogInformation("Configuração financeira padrão criada com sucesso.");
    }

    private async Task SeedFunerariasAsync()
    {
        if (await _context.Funerarias.AnyAsync())
            return;

        _logger.LogInformation("Criando funerárias padrão...");
        _context.Funerarias.AddRange(
            new Funeraria { Nome = "Funerária Paz Celestial", CNPJ = "00.000.000/0001-01", Telefone = "(11) 9999-9999", Endereco = "Rua das Flores, 123" },
            new Funeraria { Nome = "Funerária Descanso Eterno", CNPJ = "11.111.111/0001-11", Telefone = "(11) 8888-8888", Endereco = "Av. Principal, 456" }
        );
        await _context.SaveChangesAsync();
    }

    private async Task SeedProdutosEstoqueAsync()
    {
        if (await _context.ProdutosEstoque.AnyAsync())
            return;

        _logger.LogInformation("Criando produtos de estoque padrão...");
        _context.ProdutosEstoque.AddRange(
            new ProdutoEstoque { Nome = "Urna Básica", QuantidadeDisponivel = 10, EstoqueMinimo = 5, Custo = 200.00m, ValorVenda = 450.00m },
            new ProdutoEstoque { Nome = "Urna Especial", QuantidadeDisponivel = 5, EstoqueMinimo = 2, Custo = 400.00m, ValorVenda = 800.00m },
            new ProdutoEstoque { Nome = "Placa de Bronze", QuantidadeDisponivel = 20, EstoqueMinimo = 10, Custo = 150.00m, ValorVenda = 300.00m },
            new ProdutoEstoque { Nome = "Coroa de Flores", QuantidadeDisponivel = 15, EstoqueMinimo = 5, Custo = 80.00m, ValorVenda = 150.00m }
        );
        await _context.SaveChangesAsync();
    }

    private async Task SeedOssuariosAsync()
    {
        if (await _context.Ossuarios.AnyAsync())
            return;

        _logger.LogInformation("Criando ossuários padrão...");
        _context.Ossuarios.AddRange(
            new Ossuario { Identificador = "Ossuário Geral - Norte", Tipo = TipoOssuario.Geral, Capacidade = 500, Ocupado = false },
            new Ossuario { Identificador = "Ossuário Geral - Sul", Tipo = TipoOssuario.Geral, Capacidade = 500, Ocupado = false }
        );
        await _context.SaveChangesAsync();
    }
}
