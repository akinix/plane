using YH.Modules.Page.Contracts.DTOs;
using YH.Modules.Page.Contracts.v1.Pages.GetPage;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Page.Features.v1.Pages.GetPage;

/// <summary>
/// GET /api/v1/workspaces/{slug}/projects/{projectId}/pages/{pageId} — fetch a page by id (REQ-7.1).
/// Any authenticated user may read page metadata.
/// </summary>
public static class GetPageEndpoint
{
    internal static RouteHandlerBuilder MapGetPageEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/{pageId}", async (Guid pageId, IMediator mediator, CancellationToken cancellationToken) =>
            TypedResults.Ok(await mediator.Send(new GetPageQuery { PageId = pageId }, cancellationToken)))
        .WithName("GetPage")
        .WithSummary("Get page by id")
        .RequireAuthorization()
        .WithDescription("Fetch a page by id. Any authenticated user may read page metadata.")
        .Produces<PageDetailDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}