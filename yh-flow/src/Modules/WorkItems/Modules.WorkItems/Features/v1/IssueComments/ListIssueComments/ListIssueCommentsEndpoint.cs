using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.IssueComments.ListIssueComments;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.IssueComments.ListIssueComments;

/// <summary>
/// GET /work-items/{issueId}/comments/ — list issue comments (REQ-4.5).
/// Requires workspace Admin, Member, or Guest role.
/// Returns comments ordered by CreatedOnUtc ascending.
/// </summary>
public static class ListIssueCommentsEndpoint
{
    internal static RouteHandlerBuilder MapListIssueCommentsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/{issueId}/comments", async (Guid projectId, Guid issueId,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new ListIssueCommentsQuery
            {
                IssueId = issueId,
                ProjectId = projectId,
            };
            return TypedResults.Ok(await mediator.Send(query, cancellationToken));
        })
        .WithName("ListIssueComments")
        .WithSummary("List issue comments")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member, WorkspaceRole.Guest)
        .WithDescription("List all non-deleted comments for an issue. Ordered by creation time ascending.")
        .Produces<List<IssueCommentDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
