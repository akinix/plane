using Mediator;
using Microsoft.AspNetCore.Http;
using YH.Modules.Analytics.Contracts.DTOs;
using YH.Modules.Analytics.Services;

namespace YH.Modules.Analytics.Features.v1.Overview.GetProjectAnalytics;

/// <summary>
/// Handles <see cref="GetProjectAnalyticsQuery"/> — returns project-level work-item stats.
/// </summary>
public sealed class GetProjectAnalyticsQueryHandler
    : IRequestHandler<GetProjectAnalyticsQuery, IResult>
{
    private readonly IAnalyticsQueryService _analyticsService;

    public GetProjectAnalyticsQueryHandler(IAnalyticsQueryService analyticsService)
    {
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
    }

    public async ValueTask<IResult> Handle(GetProjectAnalyticsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.ProjectId == Guid.Empty)
        {
            return TypedResults.BadRequest(new { message = "ProjectId is required." });
        }

        var result = await _analyticsService.GetProjectWorkItemStatsAsync(
            request.Slug, request.ProjectId, request.DateFilter, request.StartDate, request.EndDate, cancellationToken);

        return TypedResults.Ok(result);
    }
}
