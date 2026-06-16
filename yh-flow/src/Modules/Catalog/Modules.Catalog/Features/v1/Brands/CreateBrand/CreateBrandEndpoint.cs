using YH.Modules.Catalog.Contracts.Authorization;
using YH.Framework.Shared.Identity.Authorization;
using YH.Framework.Web.Idempotency;
using YH.Modules.Catalog.Contracts.v1.Brands;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Catalog.Features.v1.Brands.CreateBrand;

public static class CreateBrandEndpoint
{
    internal static RouteHandlerBuilder MapCreateBrandEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/brands",
                async (CreateBrandCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateBrand")
            .WithSummary("Create a brand")
            .RequirePermission(CatalogPermissions.Brands.Create)
            .WithIdempotency();
    }
}
