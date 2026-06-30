namespace Sakrus.Core.Entities;

/// <summary>
/// Representa o estado do ciclo de vida do falecido no sistema.
/// - NaoSepultado: registro criado, mas sepultamento ainda não realizado.
/// - Sepultado: falecido está atualmente sepultado (jazigo ou gaveta).
/// - Exumado: passou por processo de exumação; não pode ser sepultado novamente sem novo atendimento.
/// </summary>
public enum StatusFalecido { NaoSepultado, Sepultado, Exumado }

public enum PerfilAtendimento { Concessionario, Permissionario, Beneficiario, LiberacaoCorpo }
public enum OrigemAtendimento { AuxilioFuneral, PlanoFuneral, Particular, Segurado, AssistenciaExterna }
public enum CausaMorte { Natural, Acidente, Homicidio, Pandemia, Desconhecida }
public enum TipoProcedimento { Sepultamento, PecaAnatomica, ExumacaoJudicial, TransferenciaDespojos, RecebimentoExterno }
public enum ExecutorExumacao { CemiterioMunicipal, FunerariaParceira }

public enum TipoRestosMortais { CorpoInteiro, PecaAnatomica, Cinzas }
public enum TipoOssuario { Geral, Particular }
public enum TipoMovimentacaoEstoque { Entrada, Saida }