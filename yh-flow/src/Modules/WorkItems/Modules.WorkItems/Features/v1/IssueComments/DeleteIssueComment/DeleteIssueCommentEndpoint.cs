using YH.Modules.WorkItems.Contracts.v1.IssueComments.DeleteIssueComment;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using YH.Framework.Shared.Identity.Claims;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace YH.Modules.WorkItems.Features.v1.IssueComments.DeleteIssueComment;

/// <summary>
/// DELETE /work-items/{issueId}/comments/{commentId}/ — delete an issue comment (REQ-4.5).
/// Requires workspace Admin, Member, or Guest role.
/// Only the comment author or an Admin can delete. Soft-deletes the comment.
/// </summary>
public static class DeleteIssueCommentEndpoint
{
    internal static RouteHandlerBuilder MapDeleteIssueCommentEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/{issueId}/comments/{commentId}", async (Guid projectId, Guid issueId, Guid commentId,
            ClaimsPrincipal user,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteIssueCommentCommand
            {
                IssueId = issueId,
                CommentId = commentId,
                ProjectId = projectId,
            };
            await mediator.Send(command, cancellationToken);
            return TypedResults.NoContent();
        })
        .WithName("DeleteIssueComment")
        .WithSummary("Delete issue comment")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member, WorkspaceRole.Guest)
        .WithDescription("Soft-delete a comment. Only the comment author can delete. Returns 204.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
