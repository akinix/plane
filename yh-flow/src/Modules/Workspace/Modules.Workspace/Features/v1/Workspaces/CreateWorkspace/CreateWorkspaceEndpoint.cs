using YH.Framework.Core.Context;
using YH.Framework.Core.Exceptions;
using YH.Modules.Workspace.Contracts.v1.Workspaces.CreateWorkspace;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;
using YH.Framework.Shared.Identity.Claims;

namespace YH.Modules.Workspace.Features.v1.Workspaces.CreateWorkspace;

/// <summary>
/// POST /api/v1/workspaces/ — create a workspace (REQ-2.1). Top-level endpoint (no <c>{slug}</c>);
/// any authenticated user may create a workspace.
/// </summary>
public static class CreateWorkspaceEndpoint
{
    internal static RouteHandlerBuilder MapCreateWorkspaceEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/", async (CreateWorkspaceCommand command,
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
            return TypedResults.Created($"/api/v1/workspaces/{result.Slug}", result);
        })
        .WithName("CreateWorkspace")
        .WithSummary("Create workspace")
        .RequireAuthorization()
        .WithDescription("Create a new workspace. The authenticated user becomes the owner and first Admin member.")
        .Produces<CreateWorkspaceResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status409Conflict);
    }
}
