using System;
using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class DocumentoEmitido
{
    public int Id { get; set; }

    public int TipoDocumentoId { get; set; }
    public TipoDocumento TipoDocumento { get; set; } = null!;

    [MaxLength(30)]
    public string EntidadeTipo { get; set; } = string.Empty;

    public int EntidadeId { get; set; }

    [MaxLength(20)]
    public string? Numero { get; set; }

    public string DadosJson { get; set; } = "{}";

    public int Versao { get; set; }

    public int? EmitidoPorId { get; set; }

    [MaxLength(150)]
    public string EmitidoPorNome { get; set; } = string.Empty;

    public DateTime EmitidoEm { get; set; }

    [MaxLength(300)]
    public string? PdfCaminho { get; set; }
}
