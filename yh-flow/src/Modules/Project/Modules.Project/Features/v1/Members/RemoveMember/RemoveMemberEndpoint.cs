using YH.Framework.Core.Exceptions;
using YH.Framework.Shared.Identity.Claims;
using YH.Modules.Project.Contracts.v1.Members.RemoveMember;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace YH.Modules.Project.Features.v1.Members.RemoveMember;

/// <summary>
/// DELETE /api/v1/workspaces/{slug}/projects/{projectId}/members/{memberId}/ — remove (deactivate)
/// a member from a project (REQ-3.2). Admin-driven. Keeps the row for audit by setting
/// <c>IsActive = false</c>.
/// </summary>
/// <remarks>
/// The endpoint enforces workspace Admin via <c>.RequireWorkspaceRole(Admin)</c>. The handler
/// additionally checks project-level Admin role and enforces the last-Admin guard.
/// </remarks>
public static class RemoveMemberEndpoint
{
    internal static RouteHandlerBuilder MapRemoveMemberEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/{memberId}", async (Guid projectId,
            Guid memberId,
            ClaimsPrincipal user,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException();
            }

            await mediator.Send(new RemoveMemberCommand
            {
                ProjectId = projectId,
                MemberId = memberId,
                CurrentUserId = Guid.Parse(userId),
            }, cancellationToken);

            return TypedResults.NoContent();
        })
        .WithName("RemoveProjectMember")
        .WithSummary("Remove project member")
        .RequireAuthorization()
        .RequireWorkspaceRole(WorkspaceRole.Admin)
        .WithDescription("Deactivate a project member. Admin-only. Last-Admin removal is rejected.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
