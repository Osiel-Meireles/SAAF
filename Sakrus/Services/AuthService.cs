using System.Security.Claims;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Core.Enums;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuthService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<bool> LoginAsync(string email, string senha, bool lembrarMe)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null) return false;

        // Busca o usuário ativo pelo e-mail (case-insensitive)
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.Ativo);

        if (usuario is null)
        {
            _logger.LogWarning("Tentativa de login com e-mail não encontrado: {Email}", email);
            return false;
        }

        // SEC-04: Verifica se o usuário está bloqueado por excesso de tentativas
        if (usuario.BloqueadoAte.HasValue && usuario.BloqueadoAte.Value > DateTime.UtcNow)
        {
            _logger.LogWarning("Conta temporariamente bloqueada (brute-force): {Email}", email);
            return false;
        }

        // Verifica a senha contra o hash armazenado
        if (!BCrypt.Net.BCrypt.Verify(senha, usuario.SenhaHash))
        {
            _logger.LogWarning("Senha incorreta para o usuário: {Email}", email);
            
            // Incrementa falhas e bloqueia se necessário
            usuario.TentativasLoginFalhas++;
            if (usuario.TentativasLoginFalhas >= 5)
            {
                usuario.BloqueadoAte = DateTime.UtcNow.AddMinutes(15);
                _logger.LogWarning("Conta bloqueada por 15 minutos (brute-force): {Email}", email);
            }
            
            await _context.SaveChangesAsync();
            return false;
        }

        // Reseta tentativas após login com sucesso
        usuario.TentativasLoginFalhas = 0;
        usuario.BloqueadoAte = null;
        await _context.SaveChangesAsync();

        // Cria as claims da sessão usando o helper centralizado
        var role = NivelAcessoHelper.ParaRole(usuario.NivelAcesso);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new("NivelAcesso", usuario.NivelAcesso.ToString()),
            new(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = lembrarMe,
            ExpiresUtc = lembrarMe
                ? DateTimeOffset.UtcNow.AddDays(7)
                : DateTimeOffset.UtcNow.AddHours(8),
            AllowRefresh = true
        };

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authProperties);

        _logger.LogInformation("Login realizado com sucesso: {Email} (Role: {Role})",
            usuario.Email, role);

        return true;
    }

    public async Task LogoutAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null) return;

        var email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        _logger.LogInformation("Logout realizado: {Email}", email);
    }

    public async Task<(bool Sucesso, string Erro)> RegistrarUsuarioAsync(Usuario usuario, string senhaPlana)
    {
        if (await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == usuario.Email.ToLower()))
            return (false, "Já existe um usuário cadastrado com este e-mail.");

        if (string.IsNullOrWhiteSpace(senhaPlana) || senhaPlana.Length < 6)
            return (false, "A senha deve ter no mínimo 6 caracteres.");

        usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(senhaPlana);
        usuario.Ativo = true;

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Novo usuário registrado: {Email}", usuario.Email);
        return (true, string.Empty);
    }

    public async Task<(bool Sucesso, string Erro)> AlterarSenhaAsync(int usuarioId, string senhaAtual, string novaSenha)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario is null)
            return (false, "Usuário não encontrado.");

        if (!BCrypt.Net.BCrypt.Verify(senhaAtual, usuario.SenhaHash))
            return (false, "Senha atual incorreta.");

        if (novaSenha.Length < 6)
            return (false, "A nova senha deve ter no mínimo 6 caracteres.");

        usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(novaSenha);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Senha alterada para o usuário ID: {Id}", usuarioId);
        return (true, string.Empty);
    }

    public async Task<Usuario?> GetUsuarioAtualAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null || !httpContext.User.Identity?.IsAuthenticated == true)
            return null;

        var idClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idClaim, out var id)) return null;

        return await _context.Usuarios.FindAsync(id);
    }
}
