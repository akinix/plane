using YH.Framework.Core.Context;
using YH.Framework.Core.Exceptions;
using YH.Framework.Shared.Identity.Claims;
using YH.Modules.Workspace;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Contracts.v1.Members.RemoveMember;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace YH.Modules.Workspace.Features.v1.Members.RemoveMember;

/// <summary>
/// DELETE /api/v1/workspaces/{slug}/members/{memberId}/ — remove (deactivate) a member
/// (REQ-2.2). Admin-driven. Self-removal is rejected (use Leave).
/// </summary>
public static class RemoveMemberEndpoint
{
    internal static RouteHandlerBuilder MapRemoveMemberEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/{memberId}", async (Guid memberId,
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

            await mediator.Send(new RemoveMemberCommand
            {
                WorkspaceId = workspaceId,
                MemberId = memberId,
                CurrentUserId = Guid.Parse(userId),
            }, cancellationToken);

            return TypedResults.NoContent();
        })
        .WithName("RemoveWorkspaceMember")
        .WithSummary("Remove workspace member")
        .RequireAuthorization()
        .RequireWorkspaceRole(WorkspaceRole.Admin)
        .WithDescription("Deactivate a member. Admin-only. Self-removal is rejected (use Leave).")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
