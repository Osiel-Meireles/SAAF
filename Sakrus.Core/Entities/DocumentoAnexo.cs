using System;
using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

/// <summary>
/// Representa um documento PDF anexado a qualquer entidade do sistema que exija documentação.
/// Ao menos uma das FK contextuais deve estar preenchida:
///   - FalecidoId: documento do falecido (ex: Declaração de Óbito)
///   - AtendimentoId: vínculo com o atendimento (suplementar ao FalecidoId)
///   - ResponsavelId: documento da Pessoa/Responsável (ex: RG, CPF, comprovante)
///   - FunerariaId: documento da funerária parceira (ex: alvará, contrato)
/// </summary>
public class DocumentoAnexo
{
    public int Id { get; set; }

    // ── Contexto: Falecido / Atendimento ────────────────────────────────────

    /// <summary>FK para o Atendimento. Nulo quando o documento pertence apenas à Pessoa ou Funerária.</summary>
    public int? AtendimentoId { get; set; }
    public Atendimento? Atendimento { get; set; }

    /// <summary>FK para o Falecido. Nulo quando o documento pertence à Pessoa ou Funerária.</summary>
    public int? FalecidoId { get; set; }
    public Falecido? Falecido { get; set; }

    // ── Contexto: Responsável (Pessoa) ──────────────────────────────────────

    /// <summary>FK para o Responsável/Pessoa. Nulo quando o documento pertence ao Falecido ou Funerária.</summary>
    public int? ResponsavelId { get; set; }
    public Responsavel? Responsavel { get; set; }

    // ── Contexto: Funerária ─────────────────────────────────────────────────

    /// <summary>FK para a Funerária parceira. Nulo quando o documento pertence ao Falecido ou Pessoa.</summary>
    public int? FunerariaId { get; set; }
    public Funeraria? Funeraria { get; set; }

    // ── Categorização ────────────────────────────────────────────────────────

    /// <summary>Tipo do documento para facilitar consulta e organização.</summary>
    public TipoDocumentoAnexo Tipo { get; set; } = TipoDocumentoAnexo.Outro;

    // ── Arquivo ─────────────────────────────────────────────────────────────

    /// <summary>Nome original do arquivo enviado pelo usuário.</summary>
    [Required]
    [MaxLength(260)]
    public string NomeArquivo { get; set; } = string.Empty;

    /// <summary>Caminho relativo em disco onde o arquivo foi salvo (relativo a wwwroot).</summary>
    [Required]
    [MaxLength(512)]
    public string CaminhoArquivo { get; set; } = string.Empty;

    /// <summary>Tamanho do arquivo em bytes.</summary>
    public long TamanhoBytes { get; set; }

    /// <summary>Data e hora (UTC) em que o documento foi anexado.</summary>
    public DateTime DataAnexo { get; set; } = DateTime.UtcNow;
}
