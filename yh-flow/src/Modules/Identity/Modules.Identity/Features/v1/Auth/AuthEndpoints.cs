using YH.Framework.Shared.Identity.Authorization;
using YH.Modules.Identity.Features.v1.Auth.CurrentUser;
using YH.Modules.Identity.Features.v1.Auth.SignIn;
using YH.Modules.Identity.Features.v1.Auth.SignOut;
using YH.Modules.Identity.Features.v1.Auth.SignUp;
using YH.Modules.Identity.Features.v1.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Identity.Features.v1.Auth;

internal static class AuthEndpoints
{
    public static void MapPlaneAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var auth = endpoints.MapGroup("/auth")
            .WithTags("PlaneAuth");

        // Anonymous endpoints with rate limiting
        auth.MapPlaneSignInEndpoint().AllowAnonymous().RequireRateLimiting("auth");
        auth.MapPlaneSignUpEndpoint().AllowAnonymous().RequireRateLimiting("auth");
        auth.MapOAuthInitiateEndpoint().AllowAnonymous().RequireRateLimiting("auth");
        auth.MapOAuthCallbackEndpoint().AllowAnonymous().RequireRateLimiting("auth");

        // Authenticated endpoints
        auth.MapPlaneSignOutEndpoint().RequireAuthorization();
        auth.MapPlaneMeEndpoint().RequireAuthorization();
    }
}
