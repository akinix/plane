using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Cycles.Issues;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Cycles.Issues.ListCycleIssues;

/// <summary>
/// GET /cycles/{cycleId}/cycle-issues — list issues in a cycle.
/// Any authenticated user may list cycle issues.
/// </summary>
public static class ListCycleIssuesEndpoint
{
    internal static RouteHandlerBuilder MapListCycleIssuesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/{cycleId}/cycle-issues", async (Guid projectId, Guid cycleId,
            IMediator mediator, CancellationToken cancellationToken) =>
            TypedResults.Ok(await mediator.Send(new ListCycleIssuesQuery
            {
                ProjectId = projectId,
                CycleId = cycleId,
            }, cancellationToken)))
        .WithName("ListCycleIssues")
        .WithSummary("List cycle issues")
        .RequireAuthorization()
        .WithDescription("List all issues in a cycle.")
        .Produces<List<IssueDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
