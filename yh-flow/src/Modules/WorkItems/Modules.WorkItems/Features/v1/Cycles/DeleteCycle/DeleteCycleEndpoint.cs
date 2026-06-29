using YH.Modules.WorkItems.Contracts.v1.Cycles.DeleteCycle;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Cycles.DeleteCycle;

/// <summary>
/// DELETE /cycles/{cycleId}/ — soft-delete a cycle.
/// Requires workspace Admin or Member role.
/// </summary>
public static class DeleteCycleEndpoint
{
    internal static RouteHandlerBuilder MapDeleteCycleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/{cycleId}", async (Guid projectId, Guid cycleId,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeleteCycleCommand
            {
                ProjectId = projectId,
                CycleId = cycleId,
            }, cancellationToken);
            return TypedResults.NoContent();
        })
        .WithName("DeleteCycle")
        .WithSummary("Delete cycle")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Soft-delete a cycle.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
