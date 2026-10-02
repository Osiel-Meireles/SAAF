using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class ConfiguracaoInstitucional
{
    public int Id { get; set; }

    [MaxLength(150)]
    public string NomeCoordenador { get; set; } = "";

    [MaxLength(100)]
    public string CargoCoordenador { get; set; } = "Coordenador(a) da CAAFE";

    [MaxLength(150)]
    public string NomePresidenteConselho { get; set; } = "";

    [MaxLength(100)]
    public string MunicipioEmissao { get; set; } = "Luís Eduardo Magalhães";

    [MaxLength(2)]
    public string UfEmissao { get; set; } = "BA";

    [MaxLength(200)]
    public string CabecalhoLinha1 { get; set; } = "MUNICÍPIO DE LUÍS EDUARDO MAGALHÃES";

    [MaxLength(200)]
    public string CabecalhoLinha2 { get; set; } = "CAAFE";
}
