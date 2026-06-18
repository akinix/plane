using YH.Framework.Core.Context;
using YH.Framework.Core.Exceptions;
using YH.Framework.Shared.Identity.Claims;
using YH.Modules.Workspace;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Contracts.DTOs;
using YH.Modules.Workspace.Contracts.v1.Members.UpdateMemberRole;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace YH.Modules.Workspace.Features.v1.Members.UpdateMemberRole;

/// <summary>
/// PATCH /api/v1/workspaces/{slug}/members/{memberId}/ — change a member's role (REQ-2.2,
/// threat T-2-eop-self [BLOCKING]).
/// </summary>
/// <remarks>
/// Workspace-scoped route, decorated <c>.RequireWorkspaceRole(Admin)</c>. The handler enforces
/// the self-promotion guard (cannot promote self to Admin).
/// </remarks>
public static class UpdateMemberRoleEndpoint
{
    internal static RouteHandlerBuilder MapUpdateMemberRoleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPatch("/{memberId}", async (Guid memberId,
            UpdateMemberRoleCommand command,
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

            command.WorkspaceId = workspaceId;
            command.MemberId = memberId;
            command.CurrentUserId = Guid.Parse(userId);

            var result = await mediator.Send(command, cancellationToken);
            return TypedResults.Ok(result);
        })
        .WithName("UpdateWorkspaceMemberRole")
        .WithSummary("Update workspace member role")
        .RequireAuthorization()
        .RequireWorkspaceRole(WorkspaceRole.Admin)
        .WithDescription("Change a member's role. Admin-only. Self-promotion to Admin is rejected.")
        .Produces<WorkspaceMemberDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
