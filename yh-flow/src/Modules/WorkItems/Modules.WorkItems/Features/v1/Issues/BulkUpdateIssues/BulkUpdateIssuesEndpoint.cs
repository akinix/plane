using YH.Modules.WorkItems.Contracts.v1.Issues.BulkUpdateIssues;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Issues.BulkUpdateIssues;

/// <summary>
/// POST /work-items/bulk/ — batch-update multiple issues (REQ-4.3).
/// Requires workspace Admin or Member role.
/// Atomically updates state, assignees, and/or priority on all specified issues.
/// Issues in Completed/Cancelled state groups are skipped.
/// </summary>
public static class BulkUpdateIssuesEndpoint
{
    internal static RouteHandlerBuilder MapBulkUpdateIssuesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/bulk", async (Guid projectId, BulkUpdateIssuesCommand command,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            command.ProjectId = projectId;
            var result = await mediator.Send(command, cancellationToken);
            return TypedResults.Ok(result);
        })
        .WithName("BulkUpdateIssues")
        .WithSummary("Bulk update issues")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Atomically update state, assignees, and/or priority across multiple issues. Issues in closed states (Completed/Cancelled) are skipped. Returns a summary of updated/skipped/errors.")
        .Produces<BulkUpdateResultDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
