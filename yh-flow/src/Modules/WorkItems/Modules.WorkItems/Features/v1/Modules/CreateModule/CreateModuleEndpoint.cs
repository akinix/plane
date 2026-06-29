using YH.Modules.WorkItems.Contracts.v1.Modules.CreateModule;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Modules.CreateModule;

/// <summary>
/// POST /modules/ — create a project module.
/// Requires workspace Admin or Member role.
/// </summary>
public static class CreateModuleEndpoint
{
    internal static RouteHandlerBuilder MapCreateModuleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/", async (Guid projectId, CreateModuleCommand command,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            command.ProjectId = projectId;
            var result = await mediator.Send(command, cancellationToken);
            return TypedResults.Created($"/api/v1/workspaces/{{slug}}/projects/{projectId}/modules/{result.Id}", result);
        })
        .WithName("CreateModule")
        .WithSummary("Create module")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Create a new module for a project. Name is required. Status defaults to \"planned\".")
        .Produces<CreateModuleResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
