using YH.Modules.Project.Contracts.DTOs;
using YH.Modules.Project.Contracts.v1.Projects.UpdateProject;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Project.Features.v1.Projects.UpdateProject;

/// <summary>
/// PATCH /api/v1/workspaces/{slug}/projects/{projectId} — update project settings (REQ-3.1 / REQ-3.3).
/// </summary>
/// <remarks>
/// Decorated with <c>.RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)</c> — only
/// workspace Admins and Members may update projects. The handler additionally checks project-level
/// Admin role.
/// </remarks>
public static class UpdateProjectEndpoint
{
    private static readonly string[] HttpPatch = ["PATCH"];

    internal static RouteHandlerBuilder MapUpdateProjectEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapMethods("/{projectId}", HttpPatch,
            async (Guid projectId, UpdateProjectCommand command, IMediator mediator, CancellationToken cancellationToken) =>
            {
                command.ProjectId = projectId;
                return TypedResults.Ok(await mediator.Send(command, cancellationToken));
            })
        .WithName("UpdateProject")
        .WithSummary("Update project settings")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Update mutable project settings (name, description, cover_image, network, feature toggles, ...). Identifier and slug are not editable. Workspace Admin or Member required.")
        .Produces<ProjectDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
