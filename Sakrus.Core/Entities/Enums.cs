namespace Sakrus.Core.Entities;

/// <summary>
/// Representa o estado do ciclo de vida do falecido no sistema.
/// - NaoSepultado: registro criado, mas sepultamento ainda não realizado.
/// - Sepultado: falecido está atualmente sepultado (jazigo ou gaveta).
/// - Exumado: passou por processo de exumação; não pode ser sepultado novamente sem novo atendimento.
/// </summary>
public enum StatusFalecido { NaoSepultado, Sepultado, Exumado }

public enum PerfilAtendimento { Concessionario, Permissionario, Beneficiario, LiberacaoCorpo, Indigente, NaoIdentificado, VulnerabilidadeSocial, OrdemJudicial }
public enum OrigemAtendimento { AuxilioFuneral, PlanoFuneral, Particular, Segurado, AssistenciaExterna, AssistenciaMunicipal, IML, SVO, OrdemJudicial, HospitalPublico, RequisicaoPolicial }
public enum CausaMorte { Natural, Acidente, Homicidio, Pandemia, Desconhecida }
public enum TipoProcedimento { Sepultamento, PecaAnatomica, ExumacaoJudicial, TransferenciaDespojos, RecebimentoExterno }
public enum ExecutorExumacao { CemiterioMunicipal, FunerariaParceira }

public enum TipoRestosMortais { CorpoInteiro, PecaAnatomica, Cinzas }
public enum TipoOssuario { Geral, Particular }
public enum TipoMovimentacaoEstoque { Entrada, Saida }

/// <summary>
/// Tipo de vínculo de um Responsável com um Jazigo.
/// - Titular: único responsável legal e financeiro perante o cemitério.
/// - CoUsuario: familiar com direito de uso registrado pelo titular.
/// - Beneficiario: pessoa designada pelo titular para sepultamento futuro.
/// - Herdeiro: indicado como sucessor da titularidade (para módulo jurídico futuro).
/// </summary>
public enum TipoVinculoJazigo { Titular, CoUsuario, Beneficiario, Herdeiro }

/// <summary>
/// Instrumento jurídico pelo qual o responsável detém o direito sobre o jazigo.
/// </summary>
public enum TipoTituloJazigo { Concessao, Permissao, Doacao, Heranca }

/// <summary>
/// Categorização do documento para facilitar consulta e futura integração com módulo financeiro.
/// </summary>
public enum TipoDocumentoAnexo
{
    DeclaracaoObito,
    Rg,
    Cpf,
    ComprovanteResidencia,
    ContratoFuneraria,
    AlvaraFuneraria,
    DocumentoJazigo,
    Outro
}