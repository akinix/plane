using YH.Modules.Identity.Features.v1.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Identity.Features.v1.OAuth;

public static class OAuthInitiateEndpoint
{
    internal static RouteHandlerBuilder MapOAuthInitiateEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/oauth/{provider}", (
            [FromRoute] string provider,
            OAuthProviderRegistry registry,
            HttpContext httpContext) =>
        {
            var settings = registry.GetSettings(provider);
            if (settings is null)
            {
                return Results.BadRequest(new { error = "unknown_provider", provider });
            }

            if (!settings.Enabled)
            {
                return Results.BadRequest(new { error = "provider_disabled", provider });
            }

            var providerInstance = registry.GetProvider(provider);
            if (providerInstance is null)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status501NotImplemented,
                    title: "OAuth provider not yet implemented in Phase 1",
                    detail: $"Provider '{provider}' is enabled but has no implementation in Phase 1.");
            }

            var callbackUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host.Value}/auth/oauth/{provider}/callback";
            var state = Guid.NewGuid().ToString("N");
            var authUrl = providerInstance.GetAuthUrl(state, callbackUrl);

            return Results.Redirect(authUrl, permanent: false);
        });
    }
}
