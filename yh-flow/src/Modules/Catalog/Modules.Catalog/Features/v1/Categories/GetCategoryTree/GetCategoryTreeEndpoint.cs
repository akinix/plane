using YH.Modules.Catalog.Contracts.Authorization;
using YH.Framework.Shared.Identity.Authorization;
using YH.Modules.Catalog.Contracts.v1.Categories;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Catalog.Features.v1.Categories.GetCategoryTree;

public static class GetCategoryTreeEndpoint
{
    internal static RouteHandlerBuilder MapGetCategoryTreeEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/categories/tree",
                (IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetCategoryTreeQuery(), ct))
            .WithName("GetCategoryTree")
            .WithSummary("Get the full category tree")
            .RequirePermission(CatalogPermissions.Categories.View);
    }
}
