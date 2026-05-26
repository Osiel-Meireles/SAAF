namespace Sakrus.Core.Enums;

/// <summary>
/// Helper centralizado para mapeamento de NivelAcesso (1-10) para Role string.
/// Escala CRESCENTE: quanto maior o número, maior o acesso.
/// 
/// 8-10 = Admin (acesso total, incluindo logs e gestão de usuários)
/// 5-7  = Operador (operações do dia-a-dia, sem acesso a logs/auditoria)
/// 1-4  = Visualizador (apenas visualização)
/// </summary>
public static class NivelAcessoHelper
{
    public const string RoleAdmin = "Admin";
    public const string RoleOperador = "Operador";
    public const string RoleVisualizador = "Visualizador";

    /// <summary>
    /// Converte NivelAcesso numérico (1-10) para o nome da Role.
    /// </summary>
    public static string ParaRole(int nivelAcesso) => nivelAcesso switch
    {
        >= 8 => RoleAdmin,
        >= 5 => RoleOperador,
        _    => RoleVisualizador
    };

    /// <summary>
    /// Retorna o label amigável em português para exibição na UI.
    /// </summary>
    public static string ParaLabel(int nivelAcesso) => nivelAcesso switch
    {
        >= 8 => "Administrador",
        >= 5 => "Operador",
        _    => "Visualizador"
    };
}
