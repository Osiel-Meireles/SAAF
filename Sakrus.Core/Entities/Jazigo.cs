using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class Jazigo
{
    [Key]
    public int Id { get; set; }
    public string CodigoIdentificador { get; set; } = string.Empty;
    
    public int ModeloJazigoId { get; set; }
    public ModeloJazigo ModeloJazigo { get; set; } = null!;
    
    public bool IsInfantil { get; set; }
    public bool Ocupado { get; set; }
    
    // Regra de Desmembramento: Se este lote foi originado da divisão de outro
    public int? JazigoPaiId { get; set; }
    public Jazigo? JazigoPai { get; set; }
    
    // Coordenadas SVG ou GPS para integração com o Mapa Gráfico
    public string CoordenadasMapa { get; set; } = string.Empty; 
}