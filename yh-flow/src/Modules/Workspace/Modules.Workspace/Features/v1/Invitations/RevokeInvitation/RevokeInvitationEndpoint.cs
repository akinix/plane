using YH.Framework.Core.Context;
using YH.Framework.Core.Exceptions;
using YH.Modules.Workspace;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Contracts.v1.Invitations.RevokeInvitation;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Workspace.Features.v1.Invitations.RevokeInvitation;

/// <summary>
/// DELETE /api/v1/workspaces/{slug}/invitations/{invitationId}/ — revoke a pending invitation
/// (REQ-2.4, threat T-2-replay [BLOCKING]). Admin-only. After revocation the token is invalid.
/// </summary>
public static class RevokeInvitationEndpoint
{
    internal static RouteHandlerBuilder MapRevokeInvitationEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/{invitationId}", async (
            Guid invitationId,
            ICurrentWorkspaceContext workspaceContext,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var workspaceId = workspaceContext.CurrentWorkspaceId
                ?? throw new NotFoundException("Workspace was not resolved.");

            await mediator.Send(new RevokeInvitationCommand
            {
                WorkspaceId = workspaceId,
                InvitationId = invitationId,
            }, cancellationToken);

            return TypedResults.NoContent();
        })
        .WithName("RevokeWorkspaceInvitation")
        .WithSummary("Revoke workspace invitation")
        .RequireAuthorization()
        .RequireWorkspaceRole(WorkspaceRole.Admin)
        .WithDescription("Revoke a pending invitation. Admin-only. The token becomes invalid.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
