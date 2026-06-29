using YH.Modules.WorkItems.Contracts.v1.Labels.DeleteLabel;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Labels.DeleteLabel;

/// <summary>
/// DELETE /labels/{labelId}/ — soft-delete a label (REQ-4.2).
/// Requires workspace Admin role per CONTEXT. Blocks with 409 if any Issues reference this label.
/// </summary>
public static class DeleteLabelEndpoint
{
    internal static RouteHandlerBuilder MapDeleteLabelEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/{labelId}", async (Guid projectId, Guid labelId,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeleteLabelCommand
            {
                ProjectId = projectId,
                LabelId = labelId,
            }, cancellationToken);
            return TypedResults.NoContent();
        })
        .WithName("DeleteLabel")
        .WithSummary("Delete label")
        .RequireWorkspaceRole(WorkspaceRole.Admin)
        .WithDescription("Soft-delete a label. Only workspace Admins may delete. Returns 409 Conflict if any Issues reference this label.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
