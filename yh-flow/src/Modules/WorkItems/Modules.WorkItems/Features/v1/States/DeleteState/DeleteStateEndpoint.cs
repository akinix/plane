using YH.Modules.WorkItems.Contracts.v1.States.DeleteState;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.States.DeleteState;

/// <summary>
/// DELETE /states/{stateId}/ — soft-delete a state (REQ-4.1).
/// Requires workspace Admin role. Blocks with 409 if any Issues reference this state.
/// </summary>
public static class DeleteStateEndpoint
{
    internal static RouteHandlerBuilder MapDeleteStateEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/{stateId}", async (Guid projectId, Guid stateId,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeleteStateCommand
            {
                ProjectId = projectId,
                StateId = stateId,
            }, cancellationToken);
            return TypedResults.NoContent();
        })
        .WithName("DeleteState")
        .WithSummary("Delete state")
        .RequireWorkspaceRole(WorkspaceRole.Admin)
        .WithDescription("Soft-delete a state. Only workspace Admins may delete. Returns 409 Conflict if any Issues reference this state.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
