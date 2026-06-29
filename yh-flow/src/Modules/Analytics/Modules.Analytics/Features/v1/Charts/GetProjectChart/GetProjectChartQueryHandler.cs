using Mediator;
using Microsoft.AspNetCore.Http;
using YH.Modules.Analytics.Contracts.DTOs;
using YH.Modules.Analytics.Services;

namespace YH.Modules.Analytics.Features.v1.Charts.GetProjectChart;

/// <summary>
/// Query for project-level chart data (optional cycle_id / module_id scoped).
/// </summary>
public sealed record GetProjectChartQuery(
    string Slug,
    Guid ProjectId,
    string Type,
    string? DateFilter,
    string? StartDate,
    string? EndDate,
    Guid? CycleId,
    Guid? ModuleId
) : IRequest<IResult>;

/// <summary>
/// Handles <see cref="GetProjectChartQuery"/> — currently only supports <c>type=work-items</c>.
/// </summary>
public sealed class GetProjectChartQueryHandler
    : IRequestHandler<GetProjectChartQuery, IResult>
{
    private readonly IAnalyticsQueryService _analyticsService;

    public GetProjectChartQueryHandler(IAnalyticsQueryService analyticsService)
    {
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
    }

    public async ValueTask<IResult> Handle(GetProjectChartQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.Type switch
        {
            "work-items" => TypedResults.Ok(await _analyticsService.GetProjectWorkItemChartAsync(
                request.Slug, request.ProjectId, request.DateFilter,
                request.StartDate, request.EndDate, request.CycleId, request.ModuleId, cancellationToken)),
            _ => TypedResults.BadRequest(new { message = "Invalid type. Supported value: work-items." }),
        };
    }
}
