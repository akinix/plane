using YH.Framework.Shared.Identity.Authorization;
using YH.Framework.Shared.Multitenancy;
using YH.Modules.Multitenancy.Contracts.Authorization;
using YH.Modules.Multitenancy.Contracts.v1.ResetTenantTheme;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Multitenancy.Features.v1.ResetTenantTheme;

public static class ResetTenantThemeEndpoint
{
    public static RouteHandlerBuilder Map(IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/theme/reset", async (IMediator mediator, CancellationToken cancellationToken) =>
            {
                await mediator.Send(new ResetTenantThemeCommand(), cancellationToken);
                return TypedResults.NoContent();
            })
            .WithName("ResetTenantTheme")
            .WithSummary("Reset tenant theme to defaults")
            .WithDescription("Reset the theme settings for the current tenant to the default values.")
            .RequirePermission(MultitenancyPermissions.Tenants.UpdateTheme)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }
}