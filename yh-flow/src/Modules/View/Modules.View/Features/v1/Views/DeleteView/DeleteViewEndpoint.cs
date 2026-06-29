using YH.Modules.View.Contracts.v1.Views.DeleteView;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.View.Features.v1.Views.DeleteView;

/// <summary>
/// DELETE /api/v1/workspaces/{slug}/projects/{projectId}/views/{viewId} — soft-delete a view (REQ-8.1).
/// </summary>
public static class DeleteViewEndpoint
{
    internal static RouteHandlerBuilder MapDeleteViewEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/{viewId}", async (Guid viewId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeleteViewCommand { ViewId = viewId }, cancellationToken);
            return TypedResults.NoContent();
        })
        .WithName("DeleteView")
        .WithSummary("Delete view")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Soft-delete a view. Workspace Admin or Member role required.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
