using YH.Modules.View.Contracts.v1.Views.ArchiveView;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.View.Features.v1.Views.ArchiveView;

/// <summary>
/// DELETE /api/v1/workspaces/{slug}/projects/{projectId}/views/{viewId}/archive/ — unarchive (restore) a view (REQ-8.1).
/// </summary>
public static class UnarchiveViewEndpoint
{
    internal static RouteHandlerBuilder MapUnarchiveViewEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/{viewId}/archive", async (Guid viewId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return TypedResults.Ok(await mediator.Send(new UnarchiveViewCommand { ViewId = viewId }, cancellationToken));
        })
        .WithName("UnarchiveView")
        .WithSummary("Unarchive view")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Restore an archived view back to active status.")
        .Produces<bool>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
