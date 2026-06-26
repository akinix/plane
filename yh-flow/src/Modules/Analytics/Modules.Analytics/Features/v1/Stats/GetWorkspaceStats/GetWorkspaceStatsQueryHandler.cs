using Mediator;
using YH.Modules.Analytics.Contracts.DTOs;
using YH.Modules.Analytics.Services;

namespace YH.Modules.Analytics.Features.v1.Stats.GetWorkspaceStats;

/// <summary>
/// Handles <see cref="GetWorkspaceStatsQuery"/> — returns workspace stats grouped by project.
/// </summary>
public sealed class GetWorkspaceStatsQueryHandler
    : IRequestHandler<GetWorkspaceStatsQuery, List<ProjectStatsDto>>
{
    private readonly IAnalyticsQueryService _analyticsService;

    public GetWorkspaceStatsQueryHandler(IAnalyticsQueryService analyticsService)
    {
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
    }

    public async ValueTask<List<ProjectStatsDto>> Handle(GetWorkspaceStatsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.Type switch
        {
            "work-items" => await _analyticsService.GetProjectGroupedStatsAsync(
                request.Slug, request.DateFilter, request.StartDate, request.EndDate, request.ProjectIds, cancellationToken),
            _ => [],
        };
    }
}
