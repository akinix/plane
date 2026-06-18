using YH.Modules.Workspace.Contracts.v1.Invitations.RejectInvitation;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Workspace.Features.v1.Invitations.RejectInvitation;

/// <summary>
/// POST /api/v1/workspaces/invitations/{token}/reject/ — reject a workspace invitation (REQ-2.4).
/// Top-level route; any caller may invoke (no authentication required — the invitee may not
/// yet have an account). The token hash + IsValid check is the load-bearing gate.
/// </summary>
public static class RejectInvitationEndpoint
{
    internal static RouteHandlerBuilder MapRejectInvitationEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/api/v{version:apiVersion}/workspaces/invitations/{token}/reject/", async (
            string token,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new RejectInvitationCommand { Token = token }, cancellationToken);
            return TypedResults.Ok(result);
        })
        .AllowAnonymous()
        .WithName("RejectWorkspaceInvitation")
        .WithSummary("Reject workspace invitation")
        .WithDescription("Reject a workspace invitation by presenting the raw token from the URL. " +
            "Stale/invalid tokens return Success=false rather than an error.")
        .Produces<RejectInvitationResponse>(StatusCodes.Status200OK);
    }
}
