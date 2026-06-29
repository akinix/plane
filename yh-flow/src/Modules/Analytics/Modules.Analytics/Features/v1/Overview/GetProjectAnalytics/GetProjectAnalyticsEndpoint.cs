using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using YH.Modules.Analytics.Contracts.DTOs;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;

namespace YH.Modules.Analytics.Features.v1.Overview.GetProjectAnalytics;

/// <summary>
/// GET /api/v1/workspaces/{slug}/projects/{projectId}/analytics/ — project-level work-item stats.
/// Requires workspace Admin or Member role.
/// </summary>
public static class GetProjectAnalyticsEndpoint
{
    internal static RouteHandlerBuilder MapGetProjectAnalyticsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/", async (
            string slug,
            Guid projectId,
            [Microsoft.AspNetCore.Mvc.FromQuery(Name = "date_filter")] string? dateFilter,
            [Microsoft.AspNetCore.Mvc.FromQuery(Name = "start_date")] string? startDate,
            [Microsoft.AspNetCore.Mvc.FromQuery(Name = "end_date")] string? endDate,
            IMediator mediator,
            CancellationToken ct) =>
        {
            return await mediator.Send(
                new GetProjectAnalyticsQuery(slug, projectId, dateFilter, startDate, endDate), ct);
        })
        .WithName("GetProjectAnalytics")
        .WithSummary("Get project-level work-item stats")
        .WithDescription("Returns project-level issue state-group distribution (backlog, unstarted, started, completed, cancelled).")
        .Produces<WorkItemStatsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member);
    }
}
