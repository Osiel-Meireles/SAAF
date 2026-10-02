using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sakrus.Core.Helpers;

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

        [MaxLength(30)]
        public string? EstadoCivil { get; set; }

        [MaxLength(20)]
        public string? Sexo { get; set; }

        [MaxLength(100)]
        public string? Profissao { get; set; }

        [MaxLength(250)]
        public string? Endereco { get; set; }

        [MaxLength(150)]
        public string? NomePai { get; set; }

        [MaxLength(150)]
        public string? NomeMae { get; set; }

        [MaxLength(100)]
        public string? Naturalidade { get; set; }

        [MaxLength(200)]
        public string? CartorioRegistro { get; set; }

        [MaxLength(100)]
        public string? MunicipioCartorio { get; set; }

        [MaxLength(50)]
        public string? MatriculaObito { get; set; }

        [MaxLength(250)]
        public string? LocalCorpo { get; set; }

        public DateTime? DataSepultamento { get; set; }

        [NotMapped]
        public int? Idade => RegrasDocumentos.CalcularIdade(DataNascimento, DataFalecimento);
    }
}