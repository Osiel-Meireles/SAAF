using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Sakrus.Services;
using Sakrus.Infrastructure.Data;

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
        }).AllowAnonymous().RequireRateLimiting("login"); // HIGH-03: Rate limiting anti brute-force

        // SEC-05: Logout via POST — protegido contra CSRF
        auth.MapPost("/logout", async (
            HttpContext httpContext,
            IAuthService authService) =>
        {
            await authService.LogoutAsync();
            return Results.Redirect("/login");
        }).AllowAnonymous();

        // CRIT-02: Download seguro de documentos — exige autenticação obrigatória.
        // Arquivos estão fora do wwwroot e são servidos somente por este endpoint autenticado.
        app.MapGet("/api/documentos/{id:int}", async (
            int id,
            HttpContext httpContext,
            ApplicationDbContext db,
            FileStorageService fileStorage,
            ILogger<Program> logger) =>
        {
            var user = httpContext.User;
            if (user.Identity?.IsAuthenticated != true)
                return Results.Unauthorized();

            var doc = await db.DocumentosAnexos.FindAsync(id);
            if (doc is null)
                return Results.NotFound();

            try
            {
                var bytes = await fileStorage.LerArquivoAsync(doc.CaminhoArquivo);
                logger.LogInformation("Download de documento ID {DocId} por {User}", id, user.Identity.Name);
                return Results.File(bytes, "application/pdf", doc.NomeArquivo);
            }
            catch (FileNotFoundException)
            {
                return Results.NotFound("Arquivo não encontrado no servidor.");
            }
            catch (UnauthorizedAccessException)
            {
                logger.LogCritical("Tentativa de path traversal no download do documento ID {DocId}", id);
                return Results.BadRequest("Caminho de arquivo inválido.");
            }
        }).RequireAuthorization();
    }
}
