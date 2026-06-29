using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Issues.UpdateIssue;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Issues.UpdateIssue;

/// <summary>
/// PATCH /work-items/{issueId}/ — update an issue (REQ-4.3).
/// Requires workspace Admin or Member role.
/// Enforces closed-state semantics: Completed/Cancelled groups reject updates.
/// Supports M2M assignee/label full replacement.
/// </summary>
public static class UpdateIssueEndpoint
{
    internal static RouteHandlerBuilder MapUpdateIssueEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPatch("/{issueId}", async (Guid projectId, Guid issueId, UpdateIssueCommand command,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            command.ProjectId = projectId;
            command.IssueId = issueId;
            return TypedResults.Ok(await mediator.Send(command, cancellationToken));
        })
        .WithName("UpdateIssue")
        .WithSummary("Update issue")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Update an issue. Closed-state issues (Completed/Cancelled) are blocked from edits. Supports M2M assignee/label full replacement. CompletedAt auto-syncs on state group transitions.")
        .Produces<IssueDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
