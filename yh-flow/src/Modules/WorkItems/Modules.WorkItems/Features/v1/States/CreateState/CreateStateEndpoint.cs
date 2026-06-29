using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.States.CreateState;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.States.CreateState;

/// <summary>
/// POST /states/ — create a project state (REQ-4.1).
/// Requires workspace Admin or Member role.
/// </summary>
public static class CreateStateEndpoint
{
    internal static RouteHandlerBuilder MapCreateStateEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/", async (Guid projectId, CreateStateCommand command,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            command.ProjectId = projectId;
            var result = await mediator.Send(command, cancellationToken);
            return TypedResults.Created($"/api/v1/workspaces/{{slug}}/projects/{projectId}/states/{result.Id}", result);
        })
        .WithName("CreateState")
        .WithSummary("Create state")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Create a new state for a project. Name is required, Group must be 0-4 (Backlog/Unstarted/Started/Completed/Cancelled).")
        .Produces<CreateStateResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status409Conflict);
    }
}
