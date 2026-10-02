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

    // Módulo de Documentos (CAAFE)
    public DbSet<Cemiterio> Cemiterios { get; set; }
    public DbSet<ClassificacaoEspaco> ClassificacoesEspaco { get; set; }
    public DbSet<Gaveta> Gavetas { get; set; }
    public DbSet<ServicoAuxilio> ServicosAuxilio { get; set; }
    public DbSet<AtendimentoServicoAuxilio> AtendimentosServicosAuxilio { get; set; }
    public DbSet<AssuntoProtocolo> AssuntosProtocolo { get; set; }
    public DbSet<AssuntoProtocoloDocumento> AssuntosProtocoloDocumentos { get; set; }
    public DbSet<Protocolo> Protocolos { get; set; }
    public DbSet<TipoDocumento> TiposDocumento { get; set; }
    public DbSet<NumeroRegistro> NumerosRegistro { get; set; }
    public DbSet<DocumentoEmitido> DocumentosEmitidos { get; set; }
    public DbSet<ValorMetroQuadrado> ValoresMetroQuadrado { get; set; }
    public DbSet<LancamentoFinanceiro> LancamentosFinanceiros { get; set; }
    public DbSet<ConfiguracaoInstitucional> ConfiguracoesInstitucionais { get; set; }

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

        // ── Módulo de Documentos (CAAFE) ─────────────────────────────────────

        // Cemiterio
        modelBuilder.Entity<Cemiterio>().HasIndex(c => c.Nome).IsUnique();

        // ClassificacaoEspaco
        modelBuilder.Entity<ClassificacaoEspaco>().Property(c => c.Natureza).HasConversion<string>();

        // Gaveta: índice único (JazigoId, Numero); Jazigo Restrict; Falecido SetNull
        modelBuilder.Entity<Gaveta>().HasIndex(g => new { g.JazigoId, g.Numero }).IsUnique();
        modelBuilder.Entity<Gaveta>().HasOne(g => g.Jazigo).WithMany(j => j.Gavetas).HasForeignKey(g => g.JazigoId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Gaveta>().HasOne(g => g.Falecido).WithMany().HasForeignKey(g => g.FalecidoId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Gaveta>().HasOne(g => g.ClassificacaoEspaco).WithMany().HasForeignKey(g => g.ClassificacaoEspacoId).OnDelete(DeleteBehavior.Restrict);

        // ServicoAuxilio: índice único em Codigo
        modelBuilder.Entity<ServicoAuxilio>().HasIndex(s => s.Codigo).IsUnique();

        // AtendimentoServicoAuxilio: índice único (AtendimentoId, ServicoAuxilioId); Atendimento Cascade
        modelBuilder.Entity<AtendimentoServicoAuxilio>().HasIndex(a => new { a.AtendimentoId, a.ServicoAuxilioId }).IsUnique();
        modelBuilder.Entity<AtendimentoServicoAuxilio>().HasOne(a => a.Atendimento).WithMany(at => at.ServicosAuxilio).HasForeignKey(a => a.AtendimentoId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<AtendimentoServicoAuxilio>().HasOne(a => a.ServicoAuxilio).WithMany().HasForeignKey(a => a.ServicoAuxilioId).OnDelete(DeleteBehavior.Restrict);

        // AssuntoProtocoloDocumento: índice único (AssuntoProtocoloId, TipoDocumentoCodigo); Assunto Cascade
        modelBuilder.Entity<AssuntoProtocoloDocumento>().HasIndex(a => new { a.AssuntoProtocoloId, a.TipoDocumentoCodigo }).IsUnique();
        modelBuilder.Entity<AssuntoProtocoloDocumento>().HasOne(a => a.AssuntoProtocolo).WithMany(ap => ap.Documentos).HasForeignKey(a => a.AssuntoProtocoloId).OnDelete(DeleteBehavior.Cascade);

        // Protocolo: índices únicos (Ano, Sequencia) e Numero
        modelBuilder.Entity<Protocolo>().HasIndex(p => new { p.Ano, p.Sequencia }).IsUnique();
        modelBuilder.Entity<Protocolo>().HasIndex(p => p.Numero).IsUnique();
        modelBuilder.Entity<Protocolo>().HasOne(p => p.AssuntoProtocolo).WithMany().HasForeignKey(p => p.AssuntoProtocoloId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Protocolo>().HasOne(p => p.Responsavel).WithMany().HasForeignKey(p => p.ResponsavelId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Protocolo>().HasOne(p => p.Jazigo).WithMany().HasForeignKey(p => p.JazigoId).OnDelete(DeleteBehavior.Restrict);

        // TipoDocumento: índice único em Codigo
        modelBuilder.Entity<TipoDocumento>().HasIndex(t => t.Codigo).IsUnique();

        // NumeroRegistro: índices únicos (Chave, EntidadeTipo, EntidadeId) e (Chave, Ano, Sequencia)
        modelBuilder.Entity<NumeroRegistro>().HasIndex(n => new { n.Chave, n.EntidadeTipo, n.EntidadeId }).IsUnique();
        modelBuilder.Entity<NumeroRegistro>().HasIndex(n => new { n.Chave, n.Ano, n.Sequencia }).IsUnique();

        // DocumentoEmitido: índice (TipoDocumentoId, EntidadeTipo, EntidadeId)
        modelBuilder.Entity<DocumentoEmitido>().HasIndex(d => new { d.TipoDocumentoId, d.EntidadeTipo, d.EntidadeId });
        modelBuilder.Entity<DocumentoEmitido>().HasOne(d => d.TipoDocumento).WithMany().HasForeignKey(d => d.TipoDocumentoId).OnDelete(DeleteBehavior.Restrict);

        // ValorMetroQuadrado: índice único em Exercicio
        modelBuilder.Entity<ValorMetroQuadrado>().HasIndex(v => v.Exercicio).IsUnique();
        modelBuilder.Entity<ValorMetroQuadrado>().Property(v => v.Valor).HasPrecision(10, 2);

        // LancamentoFinanceiro
        modelBuilder.Entity<LancamentoFinanceiro>().HasOne(l => l.Jazigo).WithMany().HasForeignKey(l => l.JazigoId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<LancamentoFinanceiro>().Property(l => l.Valor).HasPrecision(10, 2);
        modelBuilder.Entity<LancamentoFinanceiro>().Property(l => l.Tipo).HasConversion<string>();

        // Jazigo: dimensões e novas FKs
        modelBuilder.Entity<Jazigo>().Property(j => j.Largura).HasPrecision(10, 2);
        modelBuilder.Entity<Jazigo>().Property(j => j.Comprimento).HasPrecision(10, 2);
        modelBuilder.Entity<Jazigo>().HasOne(j => j.Cemiterio).WithMany().HasForeignKey(j => j.CemiterioId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Jazigo>().HasOne(j => j.ClassificacaoEspaco).WithMany().HasForeignKey(j => j.ClassificacaoEspacoId).OnDelete(DeleteBehavior.Restrict);

        // GavetaPublica: novas FKs
        modelBuilder.Entity<GavetaPublica>().HasOne(g => g.Cemiterio).WithMany().HasForeignKey(g => g.CemiterioId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<GavetaPublica>().HasOne(g => g.ClassificacaoEspaco).WithMany().HasForeignKey(g => g.ClassificacaoEspacoId).OnDelete(DeleteBehavior.Restrict);

        // Atendimento: EmpresaExecutora (FK explícita para evitar ambiguidade com FunerariaId)
        modelBuilder.Entity<Atendimento>().HasOne(a => a.EmpresaExecutora).WithMany().HasForeignKey(a => a.EmpresaExecutoraId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Atendimento>().Property(a => a.TipoAtendimento).HasConversion<string>();
    }
}