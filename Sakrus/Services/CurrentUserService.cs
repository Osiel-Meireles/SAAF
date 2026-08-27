using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Sakrus.Core;

namespace Sakrus.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ILogger<CurrentUserService> _logger;

    public CurrentUserService(AuthenticationStateProvider authStateProvider, ILogger<CurrentUserService> logger)
    {
        _authStateProvider = authStateProvider;
        _logger = logger;
    }

    public int? UserId
    {
        get
        {
            try
            {
                // Note: GetAuthenticationStateAsync might need to be called synchronously 
                // or we use the underlying HttpContext in Blazor Server, but since this is 
                // a synchronous property in the interface, we'll use Task.Run as a workaround
                // OR we can rely on HttpContextAccessor if available.
                // Let's use Task.Run for the AuthState.
                var authStateTask = _authStateProvider.GetAuthenticationStateAsync();
                var authState = authStateTask.IsCompleted ? authStateTask.Result : authStateTask.GetAwaiter().GetResult();
                
                var user = authState.User;
                if (user.Identity?.IsAuthenticated == true)
                {
                    var idClaim = user.FindFirst(ClaimTypes.NameIdentifier);
                    if (idClaim != null && int.TryParse(idClaim.Value, out var uid))
                    {
                        return uid;
                    }
                }
            }
            catch (Exception ex)
            {
                // HIGH-05: Log exceções em vez de suprimi-las silenciosamente
                _logger.LogError(ex, "Falha ao resolver UserId do CurrentUserService");
            }
            return null;
        }
    }

    public string UserName
    {
        get
        {
            try
            {
                var authStateTask = _authStateProvider.GetAuthenticationStateAsync();
                var authState = authStateTask.IsCompleted ? authStateTask.Result : authStateTask.GetAwaiter().GetResult();
                
                var user = authState.User;
                if (user.Identity?.IsAuthenticated == true)
                {
                    return user.Identity.Name ?? "Sistema";
                }
            }
            catch (Exception ex)
            {
                // HIGH-05: Log exceções em vez de suprimi-las silenciosamente
                _logger.LogError(ex, "Falha ao resolver UserName do CurrentUserService");
            }
            return "Sistema";
        }
    }
}
