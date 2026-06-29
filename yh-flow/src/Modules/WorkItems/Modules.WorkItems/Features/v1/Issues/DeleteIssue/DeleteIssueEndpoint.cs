using YH.Modules.WorkItems.Contracts.v1.Issues.DeleteIssue;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Issues.DeleteIssue;

/// <summary>
/// DELETE /work-items/{issueId}/ — soft-delete an issue (REQ-4.3).
/// Requires workspace Admin or Member role.
/// Cascade-deletes IssueAssignee, IssueLabel records via EF configuration.
/// </summary>
public static class DeleteIssueEndpoint
{
    internal static RouteHandlerBuilder MapDeleteIssueEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/{issueId}", async (Guid projectId, Guid issueId,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeleteIssueCommand
            {
                ProjectId = projectId,
                IssueId = issueId,
            }, cancellationToken);
            return TypedResults.NoContent();
        })
        .WithName("DeleteIssue")
        .WithSummary("Delete issue")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Soft-delete an issue. Cascade-deletes assignee and label records. Requires Admin or Member role.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
