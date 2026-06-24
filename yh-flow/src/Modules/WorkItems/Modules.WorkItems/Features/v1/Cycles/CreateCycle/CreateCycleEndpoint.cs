using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.Cycles.CreateCycle;
using YH.Modules.WorkItems.Data;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Cycles.CreateCycle;

/// <summary>
/// POST /cycles/ — create a project cycle.
/// Requires workspace Admin or Member role.
/// </summary>
public static class CreateCycleEndpoint
{
    internal static RouteHandlerBuilder MapCreateCycleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/", async (Guid projectId, CreateCycleCommand command,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            command.ProjectId = projectId;
            var result = await mediator.Send(command, cancellationToken);
            return TypedResults.Created($"/api/v1/workspaces/{{slug}}/projects/{projectId}/cycles/{result.Id}", result);
        })
        .WithName("CreateCycle")
        .WithSummary("Create cycle")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Create a new cycle for a project. Name is required. StartDate and EndDate must both be null or both be provided.")
        .Produces<CreateCycleResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
