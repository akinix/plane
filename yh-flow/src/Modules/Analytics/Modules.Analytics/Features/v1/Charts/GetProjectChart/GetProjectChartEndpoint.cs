using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using YH.Modules.Analytics.Contracts.DTOs;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;

namespace YH.Modules.Analytics.Features.v1.Charts.GetProjectChart;

/// <summary>
/// GET /api/v1/workspaces/{slug}/projects/{projectId}/analytics/charts/?type=work-items&amp;cycle_id=&amp;module_id=
/// Returns project-level chart data. Supports cycle/module scoped daily aggregation.
/// Requires workspace Admin or Member role.
/// </summary>
public static class GetProjectChartEndpoint
{
    internal static RouteHandlerBuilder MapGetProjectChartEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/charts", async (
            string slug,
            Guid projectId,
            [Microsoft.AspNetCore.Mvc.FromQuery] string? type,
            [Microsoft.AspNetCore.Mvc.FromQuery(Name = "date_filter")] string? dateFilter,
            [Microsoft.AspNetCore.Mvc.FromQuery(Name = "start_date")] string? startDate,
            [Microsoft.AspNetCore.Mvc.FromQuery(Name = "end_date")] string? endDate,
            [Microsoft.AspNetCore.Mvc.FromQuery(Name = "cycle_id")] Guid? cycleId,
            [Microsoft.AspNetCore.Mvc.FromQuery(Name = "module_id")] Guid? moduleId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            return await mediator.Send(
                new GetProjectChartQuery(slug, projectId, type ?? "work-items", dateFilter, startDate, endDate, cycleId, moduleId), ct);
        })
        .WithName("GetProjectChart")
        .WithSummary("Get project-level analytics chart data")
        .WithDescription("Returns project-level chart data. Use type=work-items for issue trend. Supports cycle_id and module_id for scoped daily aggregation.")
        .Produces<AnalyticsChartDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member);
    }
}
