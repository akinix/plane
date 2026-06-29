using Mediator;
using Microsoft.AspNetCore.Http;

namespace YH.Modules.Analytics.Features.v1.Overview.GetProjectAnalytics;

/// <summary>
/// GET /api/v1/workspaces/{slug}/projects/{projectId}/analytics/
/// Returns project-level work-item stats (state-group distribution).
/// </summary>
public sealed record GetProjectAnalyticsQuery(
    string Slug,
    Guid ProjectId,
    string? DateFilter,
    string? StartDate,
    string? EndDate
) : IRequest<IResult>;
