using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using YH.Modules.Analytics.Contracts.DTOs;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;

namespace YH.Modules.Analytics.Features.v1.Stats.GetProjectStats;

/// <summary>
/// GET /api/v1/workspaces/{slug}/projects/{projectId}/analytics/stats/ — project stats grouped by assignee.
/// Requires workspace Admin or Member role.
/// </summary>
public static class GetProjectStatsEndpoint
{
    internal static RouteHandlerBuilder MapGetProjectStatsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/stats", async (
            string slug,
            Guid projectId,
            [Microsoft.AspNetCore.Mvc.FromQuery] string? type,
            [Microsoft.AspNetCore.Mvc.FromQuery(Name = "date_filter")] string? dateFilter,
            [Microsoft.AspNetCore.Mvc.FromQuery(Name = "start_date")] string? startDate,
            [Microsoft.AspNetCore.Mvc.FromQuery(Name = "end_date")] string? endDate,
            IMediator mediator,
            CancellationToken ct) =>
        {
            return TypedResults.Ok(await mediator.Send(
                new GetProjectStatsQuery(slug, projectId, type ?? "work-items", dateFilter, startDate, endDate), ct));
        })
        .WithName("GetProjectStats")
        .WithSummary("Get project stats grouped by assignee")
        .WithDescription("Returns issue counts grouped by assignee, with per-state-group breakdown. Supports type=work-items.")
        .Produces<List<AssigneeStatsDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member);
    }
}
