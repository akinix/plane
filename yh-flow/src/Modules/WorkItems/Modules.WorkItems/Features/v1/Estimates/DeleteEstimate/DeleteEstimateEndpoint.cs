using YH.Modules.WorkItems.Contracts.v1.Estimates.DeleteEstimate;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Estimates.DeleteEstimate;

/// <summary>
/// DELETE /estimates/{estimateId}/ — hard-delete an estimate system (REQ-4.7).
/// Requires workspace Admin role. WARNING: Cascade-deletes all EstimatePoints.
/// Estimate is NOT soft-deletable (per Plane pattern — hard-delete).
/// </summary>
public static class DeleteEstimateEndpoint
{
    internal static RouteHandlerBuilder MapDeleteEstimateEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/{estimateId}", async (Guid projectId, Guid estimateId,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeleteEstimateCommand
            {
                ProjectId = projectId,
                EstimateId = estimateId,
            }, cancellationToken);
            return TypedResults.NoContent();
        })
        .WithName("DeleteEstimate")
        .WithSummary("Delete estimate")
        .RequireWorkspaceRole(WorkspaceRole.Admin)
        .WithDescription("Hard-delete an estimate system. WARNING: Cascade-deletes all EstimatePoints. Not soft-deletable per Plane pattern.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
