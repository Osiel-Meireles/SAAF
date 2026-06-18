using Sakrus.Core.Entities;

namespace Sakrus.Services;

public interface IAuthService
{
    /// <summary>
    /// Autentica o usuário e emite o cookie de sessão.
    /// </summary>
    Task<bool> LoginAsync(string email, string senha, bool lembrarMe);

    /// <summary>
    /// Encerra a sessão do usuário atual.
    /// </summary>
    Task LogoutAsync();

    /// <summary>
    /// Registra um novo usuário com senha hasheada.
    /// </summary>
    Task<(bool Sucesso, string Erro)> RegistrarUsuarioAsync(Usuario usuario, string senhaPlana);

    /// <summary>
    /// Altera a senha do usuário após validar a senha atual.
    /// </summary>
    Task<(bool Sucesso, string Erro)> AlterarSenhaAsync(int usuarioId, string senhaAtual, string novaSenha);

    /// <summary>
    /// Retorna o usuário autenticado a partir do contexto atual, ou null.
    /// </summary>
    Task<Usuario?> GetUsuarioAtualAsync();

    /// <summary>
    /// Valida a senha do usuário atualmente logado (para confirmação de operações sensíveis).
    /// </summary>
    Task<bool> ValidarSenhaUsuarioAtualAsync(string senha);

    /// <summary>
    /// Atualiza os dados cadastrais de um usuário existente (sem alterar senha).
    /// </summary>
    Task<(bool Sucesso, string Erro)> AtualizarUsuarioAsync(int usuarioId, string nome, string email, int nivelAcesso, bool ativo);

    /// <summary>
    /// Redefine a senha de um usuário gerando uma nova senha aleatória segura.
    /// Retorna a senha gerada para exibição única ao admin.
    /// </summary>
    Task<(bool Sucesso, string Erro, string SenhaGerada)> RedefinirSenhaAsync(int usuarioId);
}
