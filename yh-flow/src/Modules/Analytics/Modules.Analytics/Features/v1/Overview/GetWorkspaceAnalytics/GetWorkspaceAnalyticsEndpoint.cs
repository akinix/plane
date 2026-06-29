using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using YH.Modules.Analytics.Contracts.DTOs;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;

namespace YH.Modules.Analytics.Features.v1.Overview.GetWorkspaceAnalytics;

/// <summary>
/// GET /api/v1/workspaces/{slug}/analytics/ — workspace analytics (tab=overview|work-items).
/// Requires workspace Admin or Member role.
/// </summary>
public static class GetWorkspaceAnalyticsEndpoint
{
    internal static RouteHandlerBuilder MapGetWorkspaceAnalyticsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/", async (
            string slug,
            [Microsoft.AspNetCore.Mvc.FromQuery] string? tab,
            [Microsoft.AspNetCore.Mvc.FromQuery(Name = "date_filter")] string? dateFilter,
            [Microsoft.AspNetCore.Mvc.FromQuery(Name = "start_date")] string? startDate,
            [Microsoft.AspNetCore.Mvc.FromQuery(Name = "end_date")] string? endDate,
            [Microsoft.AspNetCore.Mvc.FromQuery(Name = "project_ids")] string? projectIds,
            IMediator mediator,
            CancellationToken ct) =>
        {
            return await mediator.Send(
                new GetWorkspaceAnalyticsQuery(slug, tab ?? "overview", dateFilter, startDate, endDate, projectIds), ct);
        })
        .WithName("GetWorkspaceAnalytics")
        .WithSummary("Get workspace analytics overview or work-item stats")
        .WithDescription("Returns workspace-level analytics. Use tab=overview for aggregate counts or tab=work-items for state-group distribution.")
        .Produces<AnalyticsOverviewDto>(StatusCodes.Status200OK)
        .Produces<WorkItemStatsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member);
    }
}
