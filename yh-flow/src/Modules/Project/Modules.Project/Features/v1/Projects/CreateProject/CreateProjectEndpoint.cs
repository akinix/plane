using YH.Framework.Core.Exceptions;
using YH.Modules.Project.Contracts.v1.Projects.CreateProject;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using YH.Framework.Shared.Identity.Claims;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace YH.Modules.Project.Features.v1.Projects.CreateProject;

/// <summary>
/// POST /api/v1/workspaces/{slug}/projects/ — create a project (REQ-3.1).
/// Requires workspace Admin or Member role.
/// </summary>
public static class CreateProjectEndpoint
{
    internal static RouteHandlerBuilder MapCreateProjectEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/", async (CreateProjectCommand command,
            ClaimsPrincipal user,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            // Force OwnerUserId from the authenticated principal — [JsonIgnore] on the command
            // already prevents client-supplied values; this is a belt-and-braces enforcement.
            var userId = user.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException();
            }

            command.OwnerUserId = Guid.Parse(userId);
            var result = await mediator.Send(command, cancellationToken);
            return TypedResults.Created($"/api/v1/workspaces/{result.Slug}/projects/{result.Id}", result);
        })
        .WithName("CreateProject")
        .WithSummary("Create project")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Create a new project. The authenticated user becomes the owner and first Admin member.")
        .Produces<CreateProjectResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status409Conflict);
    }
}
