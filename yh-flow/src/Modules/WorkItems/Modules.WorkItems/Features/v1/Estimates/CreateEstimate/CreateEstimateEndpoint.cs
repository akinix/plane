using YH.Modules.WorkItems.Contracts.v1.Estimates.CreateEstimate;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Estimates.CreateEstimate;

/// <summary>
/// POST /estimates/ — create an estimate system (REQ-4.7).
/// Requires workspace Admin or Member role.
/// </summary>
public static class CreateEstimateEndpoint
{
    internal static RouteHandlerBuilder MapCreateEstimateEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/", async (Guid projectId, CreateEstimateCommand command,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            command.ProjectId = projectId;
            var result = await mediator.Send(command, cancellationToken);
            return TypedResults.Created($"/api/v1/workspaces/{{slug}}/projects/{projectId}/estimates/{result.Id}", result);
        })
        .WithName("CreateEstimate")
        .WithSummary("Create estimate")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Create a new estimate system. Name and Type are required. Optionally seeds initial estimate points.")
        .Produces<CreateEstimateResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status409Conflict);
    }
}
