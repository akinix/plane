using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.IssueActivities.ListIssueActivities;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.IssueActivities.ListIssueActivities;

/// <summary>
/// GET /work-items/{issueId}/activities/ — list issue activity audit log (REQ-4.6).
/// Returns activities ordered by epoch descending (most recent first).
/// </summary>
public static class ListIssueActivitiesEndpoint
{
    internal static RouteHandlerBuilder MapListIssueActivitiesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/{issueId}/activities", async (Guid projectId, Guid issueId,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new ListIssueActivitiesQuery
            {
                IssueId = issueId,
                ProjectId = projectId,
            };
            return TypedResults.Ok(await mediator.Send(query, cancellationToken));
        })
        .WithName("ListIssueActivities")
        .WithSummary("List issue activities")
        .WithDescription("List the audit log of changes for an issue. Ordered by most recent first.")
        .Produces<List<IssueActivityDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
