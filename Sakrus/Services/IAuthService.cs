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
}
