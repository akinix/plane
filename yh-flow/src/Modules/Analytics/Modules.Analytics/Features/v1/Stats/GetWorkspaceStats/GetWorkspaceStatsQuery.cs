using Mediator;
using YH.Modules.Analytics.Contracts.DTOs;

namespace YH.Modules.Analytics.Features.v1.Stats.GetWorkspaceStats;

/// <summary>
/// GET /api/v1/workspaces/{slug}/analytics/stats/
/// Returns workspace-level stats grouped by project. Supports type=work-items.
/// </summary>
public sealed record GetWorkspaceStatsQuery(
    string Slug,
    string Type,
    string? DateFilter,
    string? StartDate,
    string? EndDate,
    string? ProjectIds
) : IRequest<List<ProjectStatsDto>>;
