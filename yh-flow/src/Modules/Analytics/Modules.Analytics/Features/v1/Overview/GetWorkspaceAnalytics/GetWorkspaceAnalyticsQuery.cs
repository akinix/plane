using Mediator;
using Microsoft.AspNetCore.Http;

namespace YH.Modules.Analytics.Features.v1.Overview.GetWorkspaceAnalytics;

/// <summary>
/// GET /api/v1/workspaces/{slug}/analytics/?tab=overview|work-items
/// Returns workspace-level analytics overview or work-item stats depending on the <see cref="Tab"/> parameter.
/// </summary>
public sealed record GetWorkspaceAnalyticsQuery(
    string Slug,
    string Tab,
    string? DateFilter,
    string? StartDate,
    string? EndDate,
    string? ProjectIds
) : IRequest<IResult>;
