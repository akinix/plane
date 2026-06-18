using YH.Framework.Core.Context;
using YH.Framework.Core.Exceptions;
using YH.Modules.Workspace;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Contracts.v1.Invitations.CreateInvitation;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Workspace.Features.v1.Invitations.CreateInvitation;

/// <summary>
/// POST /api/v1/workspaces/{slug}/invitations/ — create a workspace invitation (REQ-2.4,
/// threat T-2-token [BLOCKING]). Admin-only. Returns the raw token exactly once.
/// </summary>
public static class CreateInvitationEndpoint
{
    internal static RouteHandlerBuilder MapCreateInvitationEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/", async (
            CreateInvitationCommand command,
            ICurrentWorkspaceContext workspaceContext,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var workspaceId = workspaceContext.CurrentWorkspaceId
                ?? throw new NotFoundException("Workspace was not resolved.");

            command.WorkspaceId = workspaceId;
            command.Slug = workspaceContext.Slug ?? string.Empty;

            var result = await mediator.Send(command, cancellationToken);
            return TypedResults.Created(
                $"/api/v1/workspaces/{result.Slug}/invitations/{result.InvitationId}",
                result);
        })
        .WithName("CreateWorkspaceInvitation")
        .WithSummary("Create workspace invitation")
        .RequireAuthorization()
        .RequireWorkspaceRole(WorkspaceRole.Admin)
        .WithDescription("Invite a user by email. Admin-only. Returns the raw invitation token exactly once.")
        .Produces<CreateInvitationResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
