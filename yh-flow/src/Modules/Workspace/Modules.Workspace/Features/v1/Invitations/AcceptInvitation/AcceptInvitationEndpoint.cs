using YH.Framework.Core.Exceptions;
using YH.Framework.Shared.Identity.Claims;
using YH.Modules.Workspace.Contracts.v1.Invitations.AcceptInvitation;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace YH.Modules.Workspace.Features.v1.Invitations.AcceptInvitation;

/// <summary>
/// POST /api/v1/workspaces/invitations/{token}/accept/ — accept a workspace invitation
/// (REQ-2.4, threats T-2-acceptpublic + T-2-acceptdouble). Top-level route (no <c>{slug}</c>)
/// because the invitee may not yet be a member of any workspace.
/// </summary>
/// <remarks>
/// Any authenticated user may call this; the token hash + IsValid check is the load-bearing
/// gate. The handler creates the new WorkspaceMember row + transitions the invitation to
/// Accepted atomically.
/// </remarks>
public static class AcceptInvitationEndpoint
{
    internal static RouteHandlerBuilder MapAcceptInvitationEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/api/v{version:apiVersion}/workspaces/invitations/{token}/accept/", async (
            string token,
            ClaimsPrincipal user,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException();
            }

            var result = await mediator.Send(new AcceptInvitationCommand
            {
                Token = token,
                CurrentUserId = Guid.Parse(userId),
            }, cancellationToken);

            return TypedResults.Ok(result);
        })
        .WithName("AcceptWorkspaceInvitation")
        .WithSummary("Accept workspace invitation")
        .RequireAuthorization()
        .WithDescription("Accept a workspace invitation by presenting the raw token from the URL. " +
            "Any authenticated user may call; the token hash + IsValid check is the load-bearing gate.")
        .Produces<AcceptInvitationResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}
