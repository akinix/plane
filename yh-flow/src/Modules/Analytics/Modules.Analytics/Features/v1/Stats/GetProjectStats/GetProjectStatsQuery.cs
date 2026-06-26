using Mediator;
using YH.Modules.Analytics.Contracts.DTOs;

namespace YH.Modules.Analytics.Features.v1.Stats.GetProjectStats;

/// <summary>
/// GET /api/v1/workspaces/{slug}/projects/{projectId}/analytics/stats/
/// Returns project-level stats grouped by assignee. Supports type=work-items.
/// </summary>
public sealed record GetProjectStatsQuery(
    string Slug,
    Guid ProjectId,
    string Type,
    string? DateFilter,
    string? StartDate,
    string? EndDate
) : IRequest<List<AssigneeStatsDto>>;
