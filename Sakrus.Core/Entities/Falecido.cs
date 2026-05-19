using System;
using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities
{
    public class Falecido
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Nome { get; set; } = string.Empty;
        
        [Required]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "CPF deve conter 11 dígitos")]
        public string Cpf { get; set; } = string.Empty;
        
        public DateTime DataFalecimento { get; set; }
        
        // Propriedade padronizada usando o seu tipo CausaMorte
        public CausaMorte CausaMorte { get; set; }
        
        // Tipo dos restos mortais
        public TipoRestosMortais TipoRestosMortais { get; set; } = TipoRestosMortais.CorpoInteiro;

        // Relacionamento com o Jazigo (Anulável para permitir Exumação e manter histórico)
        public int? JazigoId { get; set; }
        public Jazigo? Jazigo { get; set; }
        
        // Destino pós-exumação
        public int? OssuarioId { get; set; }
        public Ossuario? Ossuario { get; set; }
    }
}