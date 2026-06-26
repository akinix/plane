using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using YH.Modules.Analytics.Contracts.DTOs;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;

namespace YH.Modules.Analytics.Features.v1.Charts.GetWorkspaceChart;

/// <summary>
/// GET /api/v1/workspaces/{slug}/analytics/charts/?type=work-items|projects
/// Returns workspace-level chart data (monthly issue trends or project summary).
/// Requires workspace Admin or Member role.
/// </summary>
public static class GetWorkspaceChartEndpoint
{
    internal static RouteHandlerBuilder MapGetWorkspaceChartEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/charts", async (
            string slug,
            [Microsoft.AspNetCore.Mvc.FromQuery] string? type,
            [Microsoft.AspNetCore.Mvc.FromQuery(Name = "date_filter")] string? dateFilter,
            [Microsoft.AspNetCore.Mvc.FromQuery(Name = "start_date")] string? startDate,
            [Microsoft.AspNetCore.Mvc.FromQuery(Name = "end_date")] string? endDate,
            [Microsoft.AspNetCore.Mvc.FromQuery(Name = "project_ids")] string? projectIds,
            IMediator mediator,
            CancellationToken ct) =>
        {
            return await mediator.Send(
                new GetWorkspaceChartQuery(slug, type ?? "work-items", dateFilter, startDate, endDate, projectIds), ct);
        })
        .WithName("GetWorkspaceChart")
        .WithSummary("Get workspace analytics chart data")
        .WithDescription("Returns workspace-level chart data. Use type=work-items for monthly issue trend or type=projects for category summary.")
        .Produces<AnalyticsChartDto>(StatusCodes.Status200OK)
        .Produces<List<ChartDataPoint>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member);
    }
}
