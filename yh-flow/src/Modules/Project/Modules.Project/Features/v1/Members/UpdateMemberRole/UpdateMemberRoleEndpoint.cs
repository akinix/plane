using YH.Framework.Core.Context;
using YH.Framework.Core.Exceptions;
using YH.Framework.Shared.Identity.Claims;
using YH.Modules.Project.Contracts.v1.Members.UpdateMemberRole;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace YH.Modules.Project.Features.v1.Members.UpdateMemberRole;

/// <summary>
/// PATCH /api/v1/workspaces/{slug}/projects/{projectId}/members/{memberId}/ — update a member's
/// role (REQ-3.2). Admin-only.
/// </summary>
/// <remarks>
/// The endpoint enforces workspace Admin via <c>.RequireWorkspaceRole(Admin)</c>. The handler
/// additionally checks project-level Admin role for the caller.
/// </remarks>
public static class UpdateMemberRoleEndpoint
{
    internal static RouteHandlerBuilder MapUpdateMemberRoleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPatch("/{memberId}", async (Guid projectId,
            Guid memberId,
            UpdateMemberRoleCommand command,
            ClaimsPrincipal user,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException();
            }

            command.ProjectId = projectId;
            command.MemberId = memberId;

            await mediator.Send(command, cancellationToken);
            return TypedResults.NoContent();
        })
        .WithName("UpdateProjectMemberRole")
        .WithSummary("Update project member role")
        .RequireAuthorization()
        .RequireWorkspaceRole(WorkspaceRole.Admin)
        .WithDescription("Change a member's role. Admin-only.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
