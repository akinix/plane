using YH.Modules.Page.Contracts.DTOs;
using YH.Modules.Page.Contracts.v1.Pages.GetPageDescription;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Page.Features.v1.Pages.GetPageDescription;

/// <summary>
/// GET /api/v1/workspaces/{slug}/projects/{projectId}/pages/{pageId}/description/ — get page description (REQ-7.2).
/// Any authenticated user may read page description.
/// </summary>
public static class GetPageDescriptionEndpoint
{
    internal static RouteHandlerBuilder MapGetPageDescriptionEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/{pageId}/description", async (Guid pageId, IMediator mediator, CancellationToken cancellationToken) =>
            TypedResults.Ok(await mediator.Send(new GetPageDescriptionQuery { PageId = pageId }, cancellationToken)))
        .WithName("GetPageDescription")
        .WithSummary("Get page description")
        .RequireAuthorization()
        .WithDescription("Fetch a page's description content (description_html, description_stripped, description_json). Any authenticated user may read.")
        .Produces<PageDetailDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}