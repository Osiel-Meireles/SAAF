using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Sakrus.Services;

namespace Sakrus.Endpoints;

/// <summary>
/// Endpoints HTTP para autenticação com cookie.
/// Em Blazor Interactive Server, o SignInAsync/SignOutAsync precisa
/// ser executado durante uma requisição HTTP real (não via SignalR/WebSocket).
/// </summary>
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var auth = app.MapGroup("/api/auth");

        // Login: aceita apenas POST com antiforgery habilitado
        auth.MapPost("/login", async (
            HttpContext httpContext,
            IAuthService authService,
            [FromForm] string email,
            [FromForm] string senha,
            [FromForm] bool lembrarMe = false,
            [FromForm] string? returnUrl = null) =>
        {
            var sucesso = await authService.LoginAsync(email, senha, lembrarMe);

            if (!sucesso)
            {
                var loginUrl = string.IsNullOrWhiteSpace(returnUrl)
                    ? "/login?error=1"
                    : $"/login?error=1&returnUrl={Uri.EscapeDataString(returnUrl)}";
                return Results.Redirect(loginUrl);
            }

            // SEC-03: Validação de Open Redirect — aceita apenas URLs locais (relativas)
            var destino = "/";
            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                var decoded = Uri.UnescapeDataString(returnUrl);
                // Garante que é um caminho relativo local (começa com / e sem //)
                if (decoded.StartsWith('/') && !decoded.StartsWith("//") && !decoded.Contains(':'))
                {
                    destino = decoded;
                }
            }

            return Results.Redirect(destino);
        }).AllowAnonymous(); // Antiforgery habilitado por padrão (SEC-02)

        // SEC-05: Logout via GET (para simplificar no Blazor Server SSR interativo)
        auth.MapGet("/logout", async (
            HttpContext httpContext,
            IAuthService authService) =>
        {
            await authService.LogoutAsync();
            return Results.Redirect("/login");
        }).AllowAnonymous();
    }
}
