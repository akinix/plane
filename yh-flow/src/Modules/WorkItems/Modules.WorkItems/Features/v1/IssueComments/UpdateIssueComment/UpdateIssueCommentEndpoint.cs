using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.IssueComments.UpdateIssueComment;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using YH.Framework.Shared.Identity.Claims;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace YH.Modules.WorkItems.Features.v1.IssueComments.UpdateIssueComment;

/// <summary>
/// PATCH /work-items/{issueId}/comments/{commentId}/ — update an issue comment (REQ-4.5).
/// Requires workspace Admin, Member, or Guest role.
/// Only the comment author can edit. Sets EditedAt timestamp.
/// </summary>
public static class UpdateIssueCommentEndpoint
{
    internal static RouteHandlerBuilder MapUpdateIssueCommentEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPatch("/{issueId}/comments/{commentId}", async (Guid projectId, Guid issueId, Guid commentId,
            UpdateIssueCommentCommand command,
            ClaimsPrincipal user,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            command.IssueId = issueId;
            command.CommentId = commentId;
            command.ProjectId = projectId;
            return TypedResults.Ok(await mediator.Send(command, cancellationToken));
        })
        .WithName("UpdateIssueComment")
        .WithSummary("Update issue comment")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member, WorkspaceRole.Guest)
        .WithDescription("Update a comment. Only the comment author can edit. Sets EditedAt to current time. HTML is sanitized.")
        .Produces<IssueCommentDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
