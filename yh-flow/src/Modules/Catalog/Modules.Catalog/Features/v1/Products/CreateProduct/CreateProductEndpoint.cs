using YH.Modules.Catalog.Contracts.Authorization;
using YH.Framework.Shared.Identity.Authorization;
using YH.Framework.Web.Idempotency;
using YH.Modules.Catalog.Contracts.v1.Products;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Catalog.Features.v1.Products.CreateProduct;

public static class CreateProductEndpoint
{
    internal static RouteHandlerBuilder MapCreateProductEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/products",
                async (CreateProductCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateProduct")
            .WithSummary("Create a product")
            .RequirePermission(CatalogPermissions.Products.Create)
            .WithIdempotency();
    }
}
