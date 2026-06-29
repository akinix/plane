using YH.Framework.Core.Exceptions;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Contracts.v1.Workspaces.DeleteWorkspace;
using YH.Framework.Shared.Identity.Claims;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace YH.Modules.Workspace.Features.v1.Workspaces.DeleteWorkspace;

/// <summary>
/// DELETE /api/v1/workspaces/{slug} — soft-delete a workspace (REQ-2.1, D-08).
/// </summary>
/// <remarks>
/// Decorated with <c>.RequireWorkspaceRole(WorkspaceRole.Admin)</c> AND the handler additionally
/// asserts ownership (threat T-2-eop-delete [BLOCKING] — double gate).
/// </remarks>
public static class DeleteWorkspaceEndpoint
{
    internal static RouteHandlerBuilder MapDeleteWorkspaceEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/{slug}", async (string slug,
            ClaimsPrincipal user,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException();
            }

            await mediator.Send(new DeleteWorkspaceCommand
            {
                Slug = slug,
                CurrentUserId = Guid.Parse(userId),
            }, cancellationToken);
            return TypedResults.NoContent();
        })
        .WithName("DeleteWorkspace")
        .WithSummary("Delete workspace")
        .RequireWorkspaceRole(WorkspaceRole.Admin)
        .WithDescription("Soft-delete a workspace. Only the workspace owner may delete (Admin role required + owner check). Slug is released for reuse.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
