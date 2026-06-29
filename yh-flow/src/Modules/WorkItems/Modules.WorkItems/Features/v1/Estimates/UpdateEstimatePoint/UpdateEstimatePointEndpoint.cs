using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.EstimatePoints.UpdateEstimatePoint;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Estimates.UpdateEstimatePoint;

/// <summary>
/// PATCH /estimates/{estimateId}/points/{estimatePointId}/ — update an estimate point (REQ-4.7).
/// Requires workspace Admin role per CONTEXT.
/// </summary>
public static class UpdateEstimatePointEndpoint
{
    internal static RouteHandlerBuilder MapUpdateEstimatePointEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPatch("/{estimateId}/points/{estimatePointId}", async (Guid estimatePointId, UpdateEstimatePointCommand command,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            command.EstimatePointId = estimatePointId;
            return TypedResults.Ok(await mediator.Send(command, cancellationToken));
        })
        .WithName("UpdateEstimatePoint")
        .WithSummary("Update estimate point")
        .RequireWorkspaceRole(WorkspaceRole.Admin)
        .WithDescription("Update an estimate point's key and/or value.")
        .Produces<EstimatePointDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
