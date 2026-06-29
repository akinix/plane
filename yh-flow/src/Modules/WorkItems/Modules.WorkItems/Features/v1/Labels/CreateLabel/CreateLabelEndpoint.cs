using YH.Modules.WorkItems.Contracts.v1.Labels.CreateLabel;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Labels.CreateLabel;

/// <summary>
/// POST /labels/ — create a project label (REQ-4.2).
/// Requires workspace Admin role per CONTEXT (labels CRUD limited to Admin).
/// </summary>
public static class CreateLabelEndpoint
{
    internal static RouteHandlerBuilder MapCreateLabelEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/", async (Guid projectId, CreateLabelCommand command,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            command.ProjectId = projectId;
            var result = await mediator.Send(command, cancellationToken);
            return TypedResults.Created($"/api/v1/workspaces/{{slug}}/projects/{projectId}/labels/{result.Id}", result);
        })
        .WithName("CreateLabel")
        .WithSummary("Create label")
        .RequireWorkspaceRole(WorkspaceRole.Admin)
        .WithDescription("Create a new label for a project. Name is required and must be unique within the project. ParentId must reference an existing label in the same project.")
        .Produces<CreateLabelResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status409Conflict);
    }
}
