using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class Atendimento
{
    [Key]
    public int Id { get; set; }
    
    public int ResponsavelId { get; set; }
    public Responsavel Responsavel { get; set; } = null!;
    
    public int FalecidoId { get; set; }
    public Falecido Falecido { get; set; } = null!;

    public PerfilAtendimento Perfil { get; set; }
    public OrigemAtendimento Origem { get; set; }
    public TipoProcedimento Procedimento { get; set; }

    public string NumeroOsAuxilio { get; set; } = string.Empty; 
    public string NumeroDeclaracaoObito { get; set; } = string.Empty;
    public string LocalFalecimento { get; set; } = string.Empty;
    public string LocalSepultamento { get; set; } = string.Empty;

    public DateTime? DataSepultamento { get; set; }
    public TimeSpan? HorarioSepultamento { get; set; }

    public string? PecaAnatomicaIdentificacao { get; set; }
    public DateTime? PecaAnatomicaDataCirurgia { get; set; }
    public string? PecaAnatomicaHospital { get; set; }
    public string? PecaAnatomicaCidadeOrigem { get; set; }
    public string? PecaAnatomicaEstadoOrigem { get; set; }

    public List<ItemFaturado> ItensFaturados { get; set; } = new();
}