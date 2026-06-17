using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace YH.Modules.Identity.Authorization.SessionCookie;

/// <summary>
/// Extension methods for registering the session cookie authentication scheme.
/// </summary>
internal static class SessionCookieAuthenticationExtensions
{
    /// <summary>
    /// Registers cookie authentication for Plane-compatible browser session requests.
    /// </summary>
    internal static IServiceCollection ConfigureSessionCookieAuth(this IServiceCollection services)
    {
        services
            .AddAuthentication()
            .AddCookie(SessionCookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = SessionCookieAuthenticationDefaults.CookieName;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
            });

        return services;
    }
}
