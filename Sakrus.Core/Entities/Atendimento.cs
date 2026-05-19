using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class Atendimento
{
    public int Id { get; set; }
    
    public int ResponsavelId { get; set; }
    public Responsavel Responsavel { get; set; } = null!;
    
    public int FalecidoId { get; set; }
    public Falecido Falecido { get; set; } = null!;

    public int? FunerariaId { get; set; }
    public Funeraria? Funeraria { get; set; }

    public PerfilAtendimento Perfil { get; set; }
    public OrigemAtendimento Origem { get; set; }
    public TipoProcedimento Procedimento { get; set; }

    [MaxLength(50)]
    public string NumeroOsAuxilio { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string NumeroDeclaracaoObito { get; set; } = string.Empty;
    
    [MaxLength(250)]
    public string LocalFalecimento { get; set; } = string.Empty;
    
    [MaxLength(250)]
    public string LocalSepultamento { get; set; } = string.Empty;

    public DateTime? DataSepultamento { get; set; }
    public TimeSpan? HorarioSepultamento { get; set; }

    [MaxLength(250)]
    public string? PecaAnatomicaIdentificacao { get; set; }
    public DateTime? PecaAnatomicaDataCirurgia { get; set; }
    
    [MaxLength(250)]
    public string? PecaAnatomicaHospital { get; set; }
    
    [MaxLength(150)]
    public string? PecaAnatomicaCidadeOrigem { get; set; }
    
    [MaxLength(2)]
    public string? PecaAnatomicaEstadoOrigem { get; set; }

    public List<ItemFaturado> ItensFaturados { get; set; } = new();
}