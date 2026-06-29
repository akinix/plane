using Mediator;
using YH.Modules.Analytics.Contracts.DTOs;
using YH.Modules.Analytics.Services;

namespace YH.Modules.Analytics.Features.v1.Stats.GetProjectStats;

/// <summary>
/// Handles <see cref="GetProjectStatsQuery"/> — returns project-level stats grouped by assignee.
/// </summary>
public sealed class GetProjectStatsQueryHandler
    : IRequestHandler<GetProjectStatsQuery, List<AssigneeStatsDto>>
{
    private readonly IAnalyticsQueryService _analyticsService;

    public GetProjectStatsQueryHandler(IAnalyticsQueryService analyticsService)
    {
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
    }

    public async ValueTask<List<AssigneeStatsDto>> Handle(GetProjectStatsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.Type switch
        {
            "work-items" => await _analyticsService.GetAssigneeGroupedStatsAsync(
                request.Slug, request.ProjectId, request.DateFilter, request.StartDate, request.EndDate, cancellationToken),
            _ => [],
        };
    }
}
