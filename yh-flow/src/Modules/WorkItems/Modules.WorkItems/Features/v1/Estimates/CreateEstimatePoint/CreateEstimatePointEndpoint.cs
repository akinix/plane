using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.EstimatePoints.CreateEstimatePoint;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Estimates.CreateEstimatePoint;

/// <summary>
/// POST /estimates/{estimateId}/points/ — create an estimate point (REQ-4.7).
/// Requires workspace Admin or Member role.
/// Key must be unique within the estimate system (DB unique index).
/// </summary>
public static class CreateEstimatePointEndpoint
{
    internal static RouteHandlerBuilder MapCreateEstimatePointEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/{estimateId}/points", async (Guid estimateId, CreateEstimatePointCommand command,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            command.EstimateId = estimateId;
            var result = await mediator.Send(command, cancellationToken);
            return TypedResults.Created($"/api/v1/workspaces/{{slug}}/projects/{{projectId}}/estimates/{estimateId}/points/{result.Id}", result);
        })
        .WithName("CreateEstimatePoint")
        .WithSummary("Create estimate point")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Create an estimate point. Key must be >= 0 and unique within the estimate system (enforced by DB unique index).")
        .Produces<EstimatePointDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status409Conflict);
    }
}
