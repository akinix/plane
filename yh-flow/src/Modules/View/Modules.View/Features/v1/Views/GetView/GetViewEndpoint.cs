using YH.Modules.View.Contracts.DTOs;
using YH.Modules.View.Contracts.v1.Views.GetView;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.View.Features.v1.Views.GetView;

/// <summary>
/// GET /api/v1/workspaces/{slug}/projects/{projectId}/views/{viewId} — fetch a view by id (REQ-8.1).
/// Any authenticated user may read view metadata.
/// </summary>
public static class GetViewEndpoint
{
    internal static RouteHandlerBuilder MapGetViewEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/{viewId}", async (Guid viewId, IMediator mediator, CancellationToken cancellationToken) =>
            TypedResults.Ok(await mediator.Send(new GetViewQuery { ViewId = viewId }, cancellationToken)))
        .WithName("GetView")
        .WithSummary("Get view by id")
        .RequireAuthorization()
        .WithDescription("Fetch a view by id. Any authenticated user may read view metadata.")
        .Produces<ViewDetailDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}
