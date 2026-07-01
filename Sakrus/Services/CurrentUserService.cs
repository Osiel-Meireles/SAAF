using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Sakrus.Core;

namespace Sakrus.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly AuthenticationStateProvider _authStateProvider;

    public CurrentUserService(AuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
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
            catch
            {
                // Fallback
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
            catch
            {
                // Fallback
            }
            return "Sistema";
        }
    }
}
