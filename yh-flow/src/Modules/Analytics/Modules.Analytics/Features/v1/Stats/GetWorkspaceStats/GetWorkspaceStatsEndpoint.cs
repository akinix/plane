using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using YH.Modules.Analytics.Contracts.DTOs;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;

namespace YH.Modules.Analytics.Features.v1.Stats.GetWorkspaceStats;

/// <summary>
/// GET /api/v1/workspaces/{slug}/analytics/stats/ — workspace stats grouped by project.
/// Requires workspace Admin or Member role.
/// </summary>
public static class GetWorkspaceStatsEndpoint
{
    internal static RouteHandlerBuilder MapGetWorkspaceStatsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/stats", async (
            string slug,
            [Microsoft.AspNetCore.Mvc.FromQuery] string? type,
            [Microsoft.AspNetCore.Mvc.FromQuery(Name = "date_filter")] string? dateFilter,
            [Microsoft.AspNetCore.Mvc.FromQuery(Name = "start_date")] string? startDate,
            [Microsoft.AspNetCore.Mvc.FromQuery(Name = "end_date")] string? endDate,
            [Microsoft.AspNetCore.Mvc.FromQuery(Name = "project_ids")] string? projectIds,
            IMediator mediator,
            CancellationToken ct) =>
        {
            return TypedResults.Ok(await mediator.Send(
                new GetWorkspaceStatsQuery(slug, type ?? "work-items", dateFilter, startDate, endDate, projectIds), ct));
        })
        .WithName("GetWorkspaceStats")
        .WithSummary("Get workspace stats grouped by project")
        .WithDescription("Returns issue counts grouped by project, with per-state-group breakdown. Supports type=work-items.")
        .Produces<List<ProjectStatsDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member);
    }
}
