using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Sakrus.Core.Enums;

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
}