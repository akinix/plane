using YH.Modules.View.Contracts.v1.Views.ArchiveView;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.View.Features.v1.Views.ArchiveView;

/// <summary>
/// POST /api/v1/workspaces/{slug}/projects/{projectId}/views/{viewId}/archive/ — archive a view (REQ-8.1).
/// </summary>
public static class ArchiveViewEndpoint
{
    internal static RouteHandlerBuilder MapArchiveViewEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/{viewId}/archive", async (Guid viewId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return TypedResults.Ok(await mediator.Send(new ArchiveViewCommand { ViewId = viewId }, cancellationToken));
        })
        .WithName("ArchiveView")
        .WithSummary("Archive view")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Archive a view. An archived view is hidden from default views but its data is preserved.")
        .Produces<bool>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
