using System;
using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

/// <summary>
/// Modela o vínculo entre um Responsável e um Jazigo.
/// 
/// Regras de negócio:
/// - Apenas 1 registro com TipoVinculo = Titular e Ativo = true por Jazigo (titular legal único).
/// - Vários registros com outros TipoVinculo podem estar ativos simultaneamente (co-usuários, beneficiários).
/// - Ao transferir titularidade, o registro atual do Titular é inativado (Ativo = false)
///   e um novo registro de Titular é criado para o novo responsável.
/// - O campo HerdeiroPrevistId prepara o sistema para o módulo de sucessão futura.
/// </summary>
public class JazigoProprietario
{
    public int Id { get; set; }

    // -- Chaves estrangeiras principais --------------------------------------

    public int JazigoId { get; set; }
    public Jazigo Jazigo { get; set; } = null!;

    public int ResponsavelId { get; set; }
    public Responsavel Responsavel { get; set; } = null!;

    // -- Natureza do vínculo -------------------------------------------------

    /// <summary>
    /// Tipo do vínculo: Titular (único legal) | CoUsuario | Beneficiario | Herdeiro.
    /// </summary>
    public TipoVinculoJazigo TipoVinculo { get; set; } = TipoVinculoJazigo.Titular;

    /// <summary>
    /// Instrumento jurídico: Concessão, Permissão, Doação ou Herança.
    /// </summary>
    public TipoTituloJazigo TipoTitulo { get; set; } = TipoTituloJazigo.Concessao;

    // -- Vigência ------------------------------------------------------------

    public DateTime DataAquisicao { get; set; } = DateTime.UtcNow;

    /// <summary>Data de vencimento do direito de uso (nulo = indeterminado).</summary>
    public DateTime? DataVencimento { get; set; }

    /// <summary>
    /// Indica se o vínculo está ativo.
    /// Inativado em vez de deletado para preservar histórico.
    /// </summary>
    public bool Ativo { get; set; } = true;

    // -- Sucessão futura -----------------------------------------------------

    /// <summary>
    /// Herdeiro indicado pelo titular para assumir a titularidade futuramente.
    /// Preparado para o módulo jurídico/financeiro — não é automático ainda.
    /// </summary>
    public int? HerdeiroPrevistId { get; set; }
    public Responsavel? HerdeiroPrevisto { get; set; }

    // -- Metadados -----------------------------------------------------------

    [MaxLength(500)]
    public string? Observacao { get; set; }

    /// <summary>Usuário que registrou ou encerrou o vínculo.</summary>
    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
}
