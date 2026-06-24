using YH.Modules.WorkItems.Contracts.v1.EstimatePoints.DeleteEstimatePoint;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Estimates.DeleteEstimatePoint;

/// <summary>
/// DELETE /estimates/{estimateId}/points/{estimatePointId}/ — delete an estimate point (REQ-4.7).
/// Requires workspace Admin role. Returns 409 if any Issue references this EstimatePoint.
/// </summary>
public static class DeleteEstimatePointEndpoint
{
    internal static RouteHandlerBuilder MapDeleteEstimatePointEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/{estimateId}/points/{estimatePointId}", async (Guid estimatePointId,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeleteEstimatePointCommand
            {
                EstimatePointId = estimatePointId,
            }, cancellationToken);
            return TypedResults.NoContent();
        })
        .WithName("DeleteEstimatePoint")
        .WithSummary("Delete estimate point")
        .RequireWorkspaceRole(WorkspaceRole.Admin)
        .WithDescription("Delete an estimate point. Returns 409 Conflict if any Issue references this estimate point.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
