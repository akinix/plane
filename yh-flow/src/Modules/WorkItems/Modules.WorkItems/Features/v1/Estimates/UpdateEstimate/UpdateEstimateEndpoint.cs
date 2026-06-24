using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Estimates.UpdateEstimate;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Estimates.UpdateEstimate;

/// <summary>
/// PATCH /estimates/{estimateId}/ — update an estimate system (REQ-4.7).
/// Requires workspace Admin role per CONTEXT.
/// </summary>
public static class UpdateEstimateEndpoint
{
    internal static RouteHandlerBuilder MapUpdateEstimateEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPatch("/{estimateId}", async (Guid projectId, Guid estimateId, UpdateEstimateCommand command,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            command.ProjectId = projectId;
            command.EstimateId = estimateId;
            return TypedResults.Ok(await mediator.Send(command, cancellationToken));
        })
        .WithName("UpdateEstimate")
        .WithSummary("Update estimate")
        .RequireWorkspaceRole(WorkspaceRole.Admin)
        .WithDescription("Update an estimate system. Only workspace Admins may update. Updates mutable fields: Name, Type, IsLastUsed.")
        .Produces<EstimateDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
