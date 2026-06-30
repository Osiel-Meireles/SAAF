using System;
using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

/// <summary>
/// Representa um documento PDF anexado ao registro de um falecido no sistema.
/// Cada documento pertence exclusivamente ao falecido para o qual foi enviado.
/// </summary>
public class DocumentoAnexo
{
    public int Id { get; set; }

    /// <summary>
    /// FK para o Atendimento ao qual este documento está vinculado.
    /// </summary>
    public int AtendimentoId { get; set; }
    public Atendimento Atendimento { get; set; } = null!;

    /// <summary>
    /// FK para o Falecido ao qual este documento pertence.
    /// Garante que documentos nunca se misturem entre falecidos distintos.
    /// </summary>
    public int FalecidoId { get; set; }
    public Falecido Falecido { get; set; } = null!;

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
