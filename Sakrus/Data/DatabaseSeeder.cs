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
    private readonly IConfiguration _configuration;

    public DatabaseSeeder(ApplicationDbContext context, ILogger<DatabaseSeeder> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        await SeedUsuarioAdminAsync();
        await SeedConfiguracaoFinanceiraAsync();
        await SeedFunerariasAsync();
        await SeedProdutosEstoqueAsync();
        await SeedOssuariosAsync();
        await SeedCemiteriosAsync();
        await SeedEmpresaExecutoraAsync();
        await SeedServicosAuxilioAsync();
        await SeedAssuntosProtocoloAsync();
        await SeedConfiguracaoInstitucionalAsync();
        await SeedCatalogoDocumentosAsync();
    }

    private async Task SeedUsuarioAdminAsync()
    {
        if (await _context.Usuarios.AnyAsync())
            return;

        _logger.LogWarning("Nenhum usuário encontrado. Criando usuário administrador padrão...");

        var initialPassword = _configuration["ADMIN_INITIAL_PASSWORD"] ?? "Admin@123";

        var admin = new Usuario
        {
            Nome = "Administrador",
            Email = "admin@sakrus.local",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(initialPassword),
            NivelAcesso = 10,
            Ativo = true
        };

        _context.Usuarios.Add(admin);
        await _context.SaveChangesAsync();

        _logger.LogWarning(
            "Usuário admin criado: admin@sakrus.local — TROQUE ESTA SENHA IMEDIATAMENTE após o primeiro login.");
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
            new Funeraria { Nome = "Funerária Paz Celestial", CNPJ = "11222333000181", Telefone = "(77) 9999-9999", Endereco = "Rua das Flores, 123" },
            new Funeraria { Nome = "Funerária Descanso Eterno", CNPJ = "44555666000199", Telefone = "(77) 8888-8888", Endereco = "Av. Principal, 456" }
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
            new Ossuario { Identificador = "Ossuário Geral - Norte", Tipo = TipoOssuario.Geral, Capacidade = 500 },
            new Ossuario { Identificador = "Ossuário Geral - Sul", Tipo = TipoOssuario.Geral, Capacidade = 500 }
        );
        await _context.SaveChangesAsync();
    }

    private async Task SeedCemiteriosAsync()
    {
        if (await _context.Cemiterios.AnyAsync())
            return;

        _logger.LogInformation("Criando cemitérios padrão...");
        _context.Cemiterios.AddRange(
            new Cemiterio { Nome = "CEMITÉRIO DA BELA VISTA" },
            new Cemiterio { Nome = "CEMITÉRIO DA VILA II" },
            new Cemiterio { Nome = "CEMITÉRIO DO POVOADO MURIÇOCA" },
            new Cemiterio { Nome = "CEMITÉRIO MUNICIPAL JARDIM DA SAUDADE" },
            new Cemiterio { Nome = "CEMITÉRIO SÃO JOSÉ - NOVO PARANÁ" }
        );
        await _context.SaveChangesAsync();
    }

    private async Task SeedEmpresaExecutoraAsync()
    {
        var executora = await _context.Funerarias.FirstOrDefaultAsync(f => f.Nome == "HPF SERVICE LTDA");
        if (executora == null)
        {
            _context.Funerarias.Add(new Funeraria { Nome = "HPF SERVICE LTDA", EhExecutora = true, Ativo = true });
        }
        else
        {
            executora.EhExecutora = true;
            executora.Ativo = true;
        }
        await _context.SaveChangesAsync();
    }

    private async Task SeedServicosAuxilioAsync()
    {
        if (await _context.ServicosAuxilio.AnyAsync())
            return;

        _logger.LogInformation("Criando serviços de auxílio padrão...");
        _context.ServicosAuxilio.AddRange(
            new ServicoAuxilio { Codigo = "01", Descricao = "Liberação" },
            new ServicoAuxilio { Codigo = "02", Descricao = "Urna funerária" },
            new ServicoAuxilio { Codigo = "03", Descricao = "Preparo" },
            new ServicoAuxilio { Codigo = "04", Descricao = "Translado" }
        );
        await _context.SaveChangesAsync();
    }

    private async Task SeedAssuntosProtocoloAsync()
    {
        if (await _context.AssuntosProtocolo.AnyAsync())
            return;

        _logger.LogInformation("Criando assuntos de protocolo padrão...");
        _context.AssuntosProtocolo.AddRange(
            new AssuntoProtocolo
            {
                Nome = "Regularização de lote",
                GeraRegularizacao = true,
                Documentos = new List<AssuntoProtocoloDocumento>
                {
                    new() { TipoDocumentoCodigo = "14", Ordem = 1 },
                    new() { TipoDocumentoCodigo = "14-DESPACHO", Ordem = 2 },
                    new() { TipoDocumentoCodigo = "14-REGULARIZACAO", Ordem = 3 }
                }
            },
            new AssuntoProtocolo
            {
                Nome = "Regularização com construção",
                GeraRegularizacao = true,
                Documentos = new List<AssuntoProtocoloDocumento>
                {
                    new() { TipoDocumentoCodigo = "14", Ordem = 1 },
                    new() { TipoDocumentoCodigo = "14-DESPACHO", Ordem = 2 },
                    new() { TipoDocumentoCodigo = "14-REGULARIZACAO", Ordem = 3 }
                }
            },
            new AssuntoProtocolo
            {
                Nome = "Aquisição de lote",
                Documentos = new List<AssuntoProtocoloDocumento>
                {
                    new() { TipoDocumentoCodigo = "14", Ordem = 1 },
                    new() { TipoDocumentoCodigo = "14-DESPACHO", Ordem = 2 }
                }
            },
            new AssuntoProtocolo
            {
                Nome = "Autorização para construção",
                Documentos = new List<AssuntoProtocoloDocumento>
                {
                    new() { TipoDocumentoCodigo = "14", Ordem = 1 },
                    new() { TipoDocumentoCodigo = "14-DESPACHO", Ordem = 2 }
                }
            }
        );
        await _context.SaveChangesAsync();
    }

    private async Task SeedConfiguracaoInstitucionalAsync()
    {
        if (await _context.ConfiguracoesInstitucionais.AnyAsync())
            return;

        _context.ConfiguracoesInstitucionais.Add(new ConfiguracaoInstitucional());
        await _context.SaveChangesAsync();
    }

    private async Task SeedCatalogoDocumentosAsync()
    {
        var catalogo = new List<(string Codigo, string Nome, string EntidadeTipo, bool ExigeNumeroUnico, bool GeraNumeroNaPrimeiraEmissao, string? ChaveNumeracao)>
        {
            ("1", "Termo de Sepultamento", "Atendimento", false, false, null),
            ("2", "Termo de Sepultamento de Peças Anatômicas", "Atendimento", false, false, null),
            ("3", "Termo de Translado", "Atendimento", false, false, null),
            ("4", "Termo de Uso de Capela", "Atendimento", false, false, null),
            ("5", "Termo de Dispensa de Uso da Capela", "Atendimento", false, false, null),
            ("6", "Auxílio Funeral — Requerimento e Ordem de Serviço", "Atendimento", false, false, null),
            ("7", "Termo de Exumação e Transferência de Despojos", "Falecido", false, false, null),
            ("8", "Termo de Exumação e Transferência de Despojos para Outro Cemitério", "Falecido", false, false, null),
            ("9", "Termo de Exumação por Mandado Judicial", "Falecido", false, false, null),
            ("10", "Guia de Sepultamento de Despojos", "Falecido", true, false, "AUTORIZACAO_TRANSLADACAO"),
            ("10-A", "Autorização para Transladação de Despojos", "Falecido", true, true, "AUTORIZACAO_TRANSLADACAO"),
            ("11", "Termo de Responsabilidade para Tratar de Assuntos Funerários", "Jazigo", false, false, null),
            ("12", "Termo de Concessão", "Concessao", true, true, "CONTRATO"),
            ("13", "Encaminhamento para Assinatura do Termo de Concessão", "Concessao", true, false, "CONTRATO"),
            ("14", "Protocolo Administrativo", "Protocolo", true, true, "PROTOCOLO"),
            ("14-DESPACHO", "Folha de Despacho", "Protocolo", false, false, null),
            ("14-REGULARIZACAO", "Documento de Regularização de Lote", "Protocolo", false, false, null),
            ("15", "Requerimento para Aquisição de Lote Cemiterial", "Responsavel", false, false, null),
            ("16", "Autorização para Construção ou Reforma de Jazigo", "Jazigo", false, false, null),
            ("17", "Informação de Disponibilidade e Localização para Sepultamento de Despojos", "Responsavel", false, false, null),
            ("18", "Folha de Rosto", "Jazigo", false, false, null),
            ("19", "Folha Financeira", "Jazigo", false, false, null),
            ("20", "Folha de Regularização", "Jazigo", false, false, null)
        };

        var codigosExistentes = await _context.TiposDocumento.Select(t => t.Codigo).ToListAsync();
        var novos = new List<TipoDocumento>();
        for (var i = 0; i < catalogo.Count; i++)
        {
            var item = catalogo[i];
            if (codigosExistentes.Contains(item.Codigo))
                continue;

            novos.Add(new TipoDocumento
            {
                Codigo = item.Codigo,
                Nome = item.Nome,
                EntidadeTipo = item.EntidadeTipo,
                ExigeNumeroUnico = item.ExigeNumeroUnico,
                GeraNumeroNaPrimeiraEmissao = item.GeraNumeroNaPrimeiraEmissao,
                ChaveNumeracao = item.ChaveNumeracao,
                Ordem = i + 1,
                Motor = "QuestPdf"
            });
        }

        if (novos.Count == 0)
            return;

        _logger.LogInformation("Inserindo {Quantidade} tipos de documento no catálogo...", novos.Count);
        _context.TiposDocumento.AddRange(novos);
        await _context.SaveChangesAsync();
    }
}
