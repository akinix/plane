using YH.Modules.Identity.Authorization.SessionCookie;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Identity.Features.v1.Auth.SignOut;

public static class PlaneSignOutEndpoint
{
    internal static RouteHandlerBuilder MapPlaneSignOutEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/sign-out", async (HttpContext httpContext) =>
        {
            // Phase 2 enhancement: revoke all server-side sessions (refresh tokens) for the current user
            // via ISessionService.RevokeAllSessionsAsync() to invalidate refresh tokens on sign-out.
            await httpContext.SignOutAsync(SessionCookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
            return Results.Ok();
        });
    }
}
