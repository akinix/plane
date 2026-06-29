using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Intake.ListIntakeIssues;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Intake.ListIntakeIssues;

/// <summary>
/// GET /work-items/intake/ — list intake issues (REQ-4.8).
/// Requires workspace Admin or Member role.
/// Optional status filter. Returns issues ordered by creation date descending.
/// </summary>
public static class ListIntakeIssuesEndpoint
{
    internal static RouteHandlerBuilder MapListIntakeIssuesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/intake", async (Guid projectId, int? status,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new ListIntakeIssuesQuery
            {
                ProjectId = projectId,
                Status = status,
            };
            return TypedResults.Ok(await mediator.Send(query, cancellationToken));
        })
        .WithName("ListIntakeIssues")
        .WithSummary("List intake issues")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("List intake (draft) issues for a project. Optional status filter: -2 Pending, -1 Rejected, 0 Snoozed, 1 Accepted, 2 Duplicate.")
        .Produces<List<IntakeIssueDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
