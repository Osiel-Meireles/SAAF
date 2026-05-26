using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Sakrus.Services;

/// <summary>
/// AuthenticationStateProvider para Blazor Server que lê o estado de autenticação
/// da requisição HTTP atual (cookies de autenticação).
/// 
/// Isso permite que o AuthorizeView funcione corretamente com cookie authentication
/// em Blazor Server InteractiveServer.
/// </summary>
public class PersistingAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PersistingAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            // Usuário autenticado: retorna o principal da requisição
            return Task.FromResult(new AuthenticationState(httpContext.User));
        }

        // Usuário anônimo
        var anonymousIdentity = new ClaimsIdentity();
        var anonymousPrincipal = new ClaimsPrincipal(anonymousIdentity);
        return Task.FromResult(new AuthenticationState(anonymousPrincipal));
    }
}
