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

    // Novas Entidades
    public DbSet<ConfiguracaoFinanceira> ConfiguracoesFinanceiras { get; set; }
    public DbSet<ModeloJazigo> ModelosJazigos { get; set; }
    public DbSet<Jazigo> Jazigos { get; set; }
    public DbSet<ExumacaoRegistro> ExumacoesRegistros { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Índices e Constraints da Gaveta Pública
        modelBuilder.Entity<GavetaPublica>().HasIndex(g => g.Ocupada);
        modelBuilder.Entity<GavetaPublica>().HasIndex(g => new { g.Setor, g.Quadra, g.Lote, g.NumeroGaveta }).IsUnique();

        // Precisão Financeira
        modelBuilder.Entity<ItemFaturado>().Property(i => i.ValorTotalCalculado).HasPrecision(10, 2);
        modelBuilder.Entity<ItemFaturado>().Property(i => i.QuantidadeOuKm).HasPrecision(10, 2);
        modelBuilder.Entity<ConfiguracaoFinanceira>().Property(c => c.ValorMetroQuadrado).HasPrecision(10, 2);
        modelBuilder.Entity<ConfiguracaoFinanceira>().Property(c => c.TaxaManutencaoBase).HasPrecision(10, 2);
        modelBuilder.Entity<ConfiguracaoFinanceira>().Property(c => c.TaxaConcessaoBase).HasPrecision(10, 2);
        modelBuilder.Entity<ModeloJazigo>().Property(m => m.PercentualConcessao).HasPrecision(5, 2);
        modelBuilder.Entity<ModeloJazigo>().Property(m => m.PercentualManutencao).HasPrecision(5, 2);
        modelBuilder.Entity<ModeloJazigo>().Property(m => m.TaxaConstrucao).HasPrecision(10, 2);

        // Deleções Restritas (DRY)
        modelBuilder.Entity<Atendimento>().HasOne(a => a.Responsavel).WithMany().HasForeignKey(a => a.ResponsavelId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Atendimento>().HasOne(a => a.Falecido).WithMany().HasForeignKey(a => a.FalecidoId).OnDelete(DeleteBehavior.Restrict);
        
        // Auto-relacionamento (Desmembramento de Jazigo)
        modelBuilder.Entity<Jazigo>().HasOne(j => j.JazigoPai).WithMany().HasForeignKey(j => j.JazigoPaiId).OnDelete(DeleteBehavior.Restrict);

        // Conversões de Enum (Visibilidade no Postgres)
        modelBuilder.Entity<Atendimento>().Property(a => a.Perfil).HasConversion<string>();
        modelBuilder.Entity<Atendimento>().Property(a => a.Origem).HasConversion<string>();
        modelBuilder.Entity<Atendimento>().Property(a => a.Procedimento).HasConversion<string>();
        modelBuilder.Entity<Falecido>().Property(f => f.CausaDaMorte).HasConversion<string>();
        modelBuilder.Entity<ExumacaoRegistro>().Property(e => e.Executor).HasConversion<string>();
    }
}