using YH.Framework.Shared.Identity.Authorization;
using YH.Framework.Shared.Multitenancy;
using YH.Modules.Multitenancy.Contracts.Authorization;
using YH.Modules.Multitenancy.Contracts.Dtos;
using YH.Modules.Multitenancy.Contracts.v1.GetTenantMigrations;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Multitenancy.Features.v1.GetTenantMigrations;

public static class TenantMigrationsEndpoint
{
    public static RouteHandlerBuilder Map(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet(
                "/migrations",
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    IReadOnlyCollection<TenantMigrationStatusDto> result =
                        await mediator.Send(new GetTenantMigrationsQuery(), cancellationToken);

                    return TypedResults.Ok(result);
                })
            .WithName("GetTenantMigrations")
            .RequirePermission(MultitenancyPermissions.Tenants.View)
            .WithSummary("Get per-tenant migration status")
            .WithDescription("Retrieve migration status for each tenant, including pending migrations and provider information.")
            .Produces<IReadOnlyCollection<TenantMigrationStatusDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }
}