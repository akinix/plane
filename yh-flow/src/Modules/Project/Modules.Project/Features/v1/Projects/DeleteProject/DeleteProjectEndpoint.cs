using YH.Framework.Core.Exceptions;
using YH.Modules.Project.Contracts.v1.Projects.DeleteProject;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using YH.Framework.Shared.Identity.Claims;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace YH.Modules.Project.Features.v1.Projects.DeleteProject;

/// <summary>
/// DELETE /api/v1/workspaces/{slug}/projects/{projectId} — soft-delete a project (REQ-3.1, D-03).
/// </summary>
/// <remarks>
/// Decorated with <c>.RequireWorkspaceRole(WorkspaceRole.Admin)</c> AND the handler additionally
/// checks project-level Admin role (dual-gate per D-09).
/// </remarks>
public static class DeleteProjectEndpoint
{
    internal static RouteHandlerBuilder MapDeleteProjectEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/{projectId}", async (Guid projectId,
            ClaimsPrincipal user,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException();
            }

            await mediator.Send(new DeleteProjectCommand
            {
                ProjectId = projectId,
                CurrentUserId = Guid.Parse(userId),
            }, cancellationToken);
            return TypedResults.NoContent();
        })
        .WithName("DeleteProject")
        .WithSummary("Delete project")
        .RequireWorkspaceRole(WorkspaceRole.Admin)
        .WithDescription("Soft-delete a project. Only workspace Admins or project Admins may delete. Slug is released for reuse with __{epoch} suffix.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
