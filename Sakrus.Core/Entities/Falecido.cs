using System;
using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities
{
    public class Falecido
    {
        public int Id { get; set; }
        
        [MaxLength(200)]
        public string Nome { get; set; } = string.Empty;
        
        [RegularExpression(@"^\d{11}$", ErrorMessage = "CPF deve conter 11 dígitos")]
        public string? Cpf { get; set; }
        
        /// <summary>
        /// Indica se este registro é de uma pessoa indigente (sem documentos identificados).
        /// Quando true, Nome e Cpf são opcionais e um código auto-incrementado é usado.
        /// </summary>
        public bool EhIndigente { get; set; } = false;

        /// <summary>
        /// Código único para indigentes, gerado automaticamente no formato IND-{ANO}-{ID:D6}.
        /// Nulo para não-indigentes.
        /// </summary>
        [MaxLength(50)]
        public string? CodigoIndigente { get; set; }
        
        public DateTime? DataNascimento { get; set; }
        
        public DateTime DataFalecimento { get; set; }
        
        // Propriedade padronizada usando o seu tipo CausaMorte
        public CausaMorte CausaMorte { get; set; }
        
        // Tipo dos restos mortais
        public TipoRestosMortais TipoRestosMortais { get; set; } = TipoRestosMortais.CorpoInteiro;

        /// <summary>
        /// Estado do ciclo de vida do falecido.
        /// Evita que um falecido exumado seja indevidamente sepultado novamente.
        /// </summary>
        public StatusFalecido Status { get; set; } = StatusFalecido.NaoSepultado;

        // Relacionamento com o Jazigo (Anulável para permitir Exumação e manter histórico)
        public int? JazigoId { get; set; }
        public Jazigo? Jazigo { get; set; }
        
        // Destino pós-exumação
        public int? OssuarioId { get; set; }
        public Ossuario? Ossuario { get; set; }

        // Documentos PDF anexados a este falecido
        public List<DocumentoAnexo> Documentos { get; set; } = new();
    }
}