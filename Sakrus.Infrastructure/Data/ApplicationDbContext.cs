using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;

namespace Sakrus.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<ItemFaturado> ItensFaturados { get; set; }
    public DbSet<Responsavel> Responsaveis { get; set; }
    public DbSet<Falecido> Falecidos { get; set; }
    public DbSet<Atendimento> Atendimentos { get; set; }
    public DbSet<RegistroCapela> RegistrosCapela { get; set; }
    public DbSet<GavetaPublica> GavetasPublicas { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Capela> Capelas { get; set; } // Referência para a UI existente
    public DbSet<AuditLog> AuditLogs { get; set; }

    // Novas Entidades
    public DbSet<ConfiguracaoFinanceira> ConfiguracoesFinanceiras { get; set; }
    public DbSet<ModeloJazigo> ModelosJazigos { get; set; }
    public DbSet<Jazigo> Jazigos { get; set; }
    public DbSet<ExumacaoRegistro> ExumacoesRegistros { get; set; }
    public DbSet<HistoricoTitularidadeJazigo> HistoricoTitularidadeJazigos { get; set; }
    public DbSet<Funeraria> Funerarias { get; set; }
    public DbSet<ProdutoEstoque> ProdutosEstoque { get; set; }
    public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; }
    public DbSet<Ossuario> Ossuarios { get; set; }
    public DbSet<DocumentoAnexo> DocumentosAnexos { get; set; }

    /// <summary>Vínculos de propriedade/uso entre Responsáveis e Jazigos.</summary>
    public DbSet<JazigoProprietario> JazigoProprietarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Índices e Constraints da Gaveta Pública
        modelBuilder.Entity<GavetaPublica>().HasIndex(g => g.Ocupada);
        modelBuilder.Entity<GavetaPublica>().HasIndex(g => new { g.Setor, g.Quadra, g.Lote, g.NumeroGaveta }).IsUnique();

        // ROB-03: Índices de Performance
        modelBuilder.Entity<Atendimento>().HasIndex(a => a.DataSepultamento);
        modelBuilder.Entity<Falecido>().HasIndex(f => f.JazigoId);
        modelBuilder.Entity<ExumacaoRegistro>().HasIndex(e => e.FalecidoId);
        modelBuilder.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();

        // Precisão Financeira
        modelBuilder.Entity<ItemFaturado>().Property(i => i.ValorTotalCalculado).HasPrecision(10, 2);
        modelBuilder.Entity<ItemFaturado>().Property(i => i.QuantidadeOuKm).HasPrecision(10, 2);
        modelBuilder.Entity<ConfiguracaoFinanceira>().Property(c => c.ValorMetroQuadrado).HasPrecision(10, 2);
        modelBuilder.Entity<ConfiguracaoFinanceira>().Property(c => c.TaxaManutencaoBase).HasPrecision(10, 2);
        modelBuilder.Entity<ConfiguracaoFinanceira>().Property(c => c.TaxaConcessaoBase).HasPrecision(10, 2);
        modelBuilder.Entity<ConfiguracaoFinanceira>().Property(c => c.PrecoUrnaBasica).HasPrecision(10, 2);
        modelBuilder.Entity<ConfiguracaoFinanceira>().Property(c => c.PrecoUrnaEspecial).HasPrecision(10, 2);
        modelBuilder.Entity<ConfiguracaoFinanceira>().Property(c => c.PrecoTransladoPorKm).HasPrecision(10, 4);
        modelBuilder.Entity<ConfiguracaoFinanceira>().Property(c => c.PrecoPompa).HasPrecision(10, 2);
        modelBuilder.Entity<ConfiguracaoFinanceira>().Property(c => c.PrecoPreparoCorpo).HasPrecision(10, 2);
        modelBuilder.Entity<ModeloJazigo>().Property(m => m.PercentualConcessao).HasPrecision(5, 2);
        modelBuilder.Entity<ModeloJazigo>().Property(m => m.PercentualManutencao).HasPrecision(5, 2);
        modelBuilder.Entity<ModeloJazigo>().Property(m => m.TaxaConstrucao).HasPrecision(10, 2);
        
        // Precisão Estoque
        modelBuilder.Entity<ProdutoEstoque>().Property(p => p.Custo).HasPrecision(10, 2);
        modelBuilder.Entity<ProdutoEstoque>().Property(p => p.ValorVenda).HasPrecision(10, 2);

        // Deleções Restritas (DRY)
        modelBuilder.Entity<Atendimento>().HasOne(a => a.Responsavel).WithMany().HasForeignKey(a => a.ResponsavelId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Atendimento>().HasOne(a => a.Falecido).WithMany().HasForeignKey(a => a.FalecidoId).OnDelete(DeleteBehavior.Restrict);
        
        // Exumação: Se o Jazigo for desvinculado (ou deletado), setar a referência no Falecido como NULL
        modelBuilder.Entity<Falecido>().HasOne(f => f.Jazigo).WithMany(j => j.Falecidos).HasForeignKey(f => f.JazigoId).OnDelete(DeleteBehavior.SetNull);
        
        // Auto-relacionamento (Desmembramento de Jazigo)
        modelBuilder.Entity<Jazigo>().HasOne(j => j.JazigoPai).WithMany().HasForeignKey(j => j.JazigoPaiId).OnDelete(DeleteBehavior.Restrict);

        // Histórico de Titularidade: Evitar ciclos ou cascade delete múltiplos no Responsavel
        modelBuilder.Entity<HistoricoTitularidadeJazigo>().HasOne(h => h.ResponsavelAntigo).WithMany().HasForeignKey(h => h.ResponsavelAntigoId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<HistoricoTitularidadeJazigo>().HasOne(h => h.ResponsavelNovo).WithMany().HasForeignKey(h => h.ResponsavelNovoId).OnDelete(DeleteBehavior.Restrict);

        // ── DocumentoAnexo: FKs todas opcionais com comportamentos corretos ──

        // Falecido: cascade (documentos do falecido são removidos junto)
        modelBuilder.Entity<DocumentoAnexo>()
            .HasOne(d => d.Falecido)
            .WithMany(f => f.Documentos)
            .HasForeignKey(d => d.FalecidoId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        // Atendimento: restrict (preserva docs mesmo que atendimento seja alterado)
        modelBuilder.Entity<DocumentoAnexo>()
            .HasOne(d => d.Atendimento)
            .WithMany()
            .HasForeignKey(d => d.AtendimentoId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Responsavel: restrict (documentos da pessoa não são perdidos se responsável for editado)
        modelBuilder.Entity<DocumentoAnexo>()
            .HasOne(d => d.Responsavel)
            .WithMany(r => r.Documentos)
            .HasForeignKey(d => d.ResponsavelId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Funeraria: restrict
        modelBuilder.Entity<DocumentoAnexo>()
            .HasOne(d => d.Funeraria)
            .WithMany(f => f.Documentos)
            .HasForeignKey(d => d.FunerariaId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // ── JazigoProprietario ───────────────────────────────────────────────

        // Jazigo → JazigoProprietario
        modelBuilder.Entity<JazigoProprietario>()
            .HasOne(jp => jp.Jazigo)
            .WithMany(j => j.Proprietarios)
            .HasForeignKey(jp => jp.JazigoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Responsavel (titular/co-usuário) → JazigoProprietario
        modelBuilder.Entity<JazigoProprietario>()
            .HasOne(jp => jp.Responsavel)
            .WithMany(r => r.JazigoProprietarios)
            .HasForeignKey(jp => jp.ResponsavelId)
            .OnDelete(DeleteBehavior.Restrict);

        // Responsavel (herdeiro previsto) — FK separada para evitar ciclos EF
        modelBuilder.Entity<JazigoProprietario>()
            .HasOne(jp => jp.HerdeiroPrevisto)
            .WithMany()
            .HasForeignKey(jp => jp.HerdeiroPrevistId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Índice de performance para buscar vínculos ativos por jazigo
        modelBuilder.Entity<JazigoProprietario>()
            .HasIndex(jp => new { jp.JazigoId, jp.Ativo });

        // Índice para buscar jazigos de um responsável
        modelBuilder.Entity<JazigoProprietario>()
            .HasIndex(jp => new { jp.ResponsavelId, jp.Ativo });

        // Conversões de Enum (Visibilidade no Postgres)
        modelBuilder.Entity<Atendimento>().Property(a => a.Perfil).HasConversion<string>();
        modelBuilder.Entity<Atendimento>().Property(a => a.Origem).HasConversion<string>();
        modelBuilder.Entity<Atendimento>().Property(a => a.Procedimento).HasConversion<string>();
        modelBuilder.Entity<Falecido>().Property(f => f.CausaMorte).HasConversion<string>();
        modelBuilder.Entity<ExumacaoRegistro>().Property(e => e.Executor).HasConversion<string>();
        modelBuilder.Entity<Falecido>().Property(f => f.TipoRestosMortais).HasConversion<string>();
        modelBuilder.Entity<Ossuario>().Property(o => o.Tipo).HasConversion<string>();
        modelBuilder.Entity<MovimentacaoEstoque>().Property(m => m.TipoMovimentacao).HasConversion<string>();
        modelBuilder.Entity<Falecido>().Property(f => f.Status).HasConversion<string>();

        // Novos enums
        modelBuilder.Entity<JazigoProprietario>().Property(jp => jp.TipoVinculo).HasConversion<string>();
        modelBuilder.Entity<JazigoProprietario>().Property(jp => jp.TipoTitulo).HasConversion<string>();
        modelBuilder.Entity<DocumentoAnexo>().Property(d => d.Tipo).HasConversion<string>();
    }
}