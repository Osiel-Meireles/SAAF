namespace Sakrus.Core.Entities;

public enum PerfilAtendimento { Concessionario, Permissionario, Beneficiario, LiberacaoCorpo }
public enum OrigemAtendimento { AuxilioFuneral, PlanoFuneral, Particular, Segurado, AssistenciaExterna }
public enum CausaMorte { Natural, Acidente, Homicidio, Pandemia, Desconhecida }
public enum TipoProcedimento { Sepultamento, PecaAnatomica, ExumacaoJudicial, TransferenciaDespojos, RecebimentoExterno }
public enum ExecutorExumacao { CemiterioMunicipal, FunerariaParceira }