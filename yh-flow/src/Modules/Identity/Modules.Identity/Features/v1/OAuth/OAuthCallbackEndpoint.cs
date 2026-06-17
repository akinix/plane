using YH.Modules.Identity.Features.v1.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Identity.Features.v1.OAuth;

public static class OAuthCallbackEndpoint
{
    internal static RouteHandlerBuilder MapOAuthCallbackEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/oauth/{provider}/callback", (
            [FromRoute] string provider,
            [FromQuery] string? code,
            OAuthProviderRegistry registry) =>
        {
            var settings = registry.GetSettings(provider);
            if (settings is null)
            {
                return Results.BadRequest(new { error = "unknown_provider", provider });
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                return Results.BadRequest(new { error = "missing_code" });
            }

            var providerInstance = registry.GetProvider(provider);
            if (providerInstance is null)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status501NotImplemented,
                    title: "OAuth callback not yet implemented in Phase 1",
                    detail: $"Provider '{provider}' callback handling is not implemented in Phase 1.");
            }

            return Results.Problem(
                statusCode: StatusCodes.Status501NotImplemented,
                title: "OAuth callback not yet implemented in Phase 1");
        });
    }
}
