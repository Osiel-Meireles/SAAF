using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sakrus.Core.Enums;
using Sakrus.Core.Helpers;

namespace Sakrus.Core.Entities;

public class Jazigo
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string CodigoIdentificador { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Quadra { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Ala { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string NumeroLote { get; set; } = string.Empty;
    
    public int ModeloJazigoId { get; set; }
    public ModeloJazigo ModeloJazigo { get; set; } = null!;
    
    public bool IsInfantil { get; set; }
    public bool Ocupado { get; set; }
    
    // Regra de Desmembramento: Se este lote foi originado da divisão de outro
    public int? JazigoPaiId { get; set; }
    public Jazigo? JazigoPai { get; set; }
    
    // Coordenadas SVG ou GPS para integração com o Mapa Gráfico
    [MaxLength(250)]
    public string CoordenadasMapa { get; set; } = string.Empty;
    
    // Relacionamento 1:N com Falecidos sepultados neste Jazigo
    public List<Falecido> Falecidos { get; set; } = new();

    /// <summary>
    /// Vínculos de propriedade/uso deste jazigo com Responsáveis.
    /// Inclui titular legal único (TipoVinculo = Titular, Ativo = true)
    /// e possíveis co-usuários ou beneficiários.
    /// </summary>
    public List<JazigoProprietario> Proprietarios { get; set; } = new();

    public int? CemiterioId { get; set; }
    public Cemiterio? Cemiterio { get; set; }

    public decimal? Largura { get; set; }
    public decimal? Comprimento { get; set; }

    [MaxLength(150)]
    public string? Revestimento { get; set; }

    public int? ClassificacaoEspacoId { get; set; }
    public ClassificacaoEspaco? ClassificacaoEspaco { get; set; }

    public List<Gaveta> Gavetas { get; set; } = new();

    [NotMapped]
    public decimal? AreaM2 => RegrasDocumentos.CalcularAreaM2(Largura, Comprimento);
}