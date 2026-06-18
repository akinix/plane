using YH.Framework.Core.Context;
using YH.Framework.Core.Exceptions;
using YH.Framework.Shared.Identity.Claims;
using YH.Modules.Workspace;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Contracts.v1.Members.LeaveWorkspace;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace YH.Modules.Workspace.Features.v1.Members.LeaveWorkspace;

/// <summary>
/// POST /api/v1/workspaces/{slug}/members/leave/ — self-removal from the resolved workspace
/// (REQ-2.2). Any active member may call this.
/// </summary>
public static class LeaveWorkspaceEndpoint
{
    internal static RouteHandlerBuilder MapLeaveWorkspaceEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/leave", async (
            ClaimsPrincipal user,
            ICurrentWorkspaceContext workspaceContext,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var workspaceId = workspaceContext.CurrentWorkspaceId
                ?? throw new NotFoundException("Workspace was not resolved.");

            var userId = user.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException();
            }

            await mediator.Send(new LeaveWorkspaceCommand
            {
                WorkspaceId = workspaceId,
                CurrentUserId = Guid.Parse(userId),
            }, cancellationToken);

            return TypedResults.NoContent();
        })
        .WithName("LeaveWorkspace")
        .WithSummary("Leave the current workspace")
        .RequireAuthorization()
        .RequireWorkspaceRole(WorkspaceRole.Guest, WorkspaceRole.Member, WorkspaceRole.Admin)
        .WithDescription("Deactivate the caller's own membership in the resolved workspace. Idempotent.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
