using Mediator;
using Microsoft.AspNetCore.Http;
using YH.Modules.Analytics.Contracts.DTOs;
using YH.Modules.Analytics.Services;

namespace YH.Modules.Analytics.Features.v1.Charts.GetWorkspaceChart;

/// <summary>
/// Query for workspace-level chart data.
/// </summary>
public sealed record GetWorkspaceChartQuery(
    string Slug,
    string Type,
    string? DateFilter,
    string? StartDate,
    string? EndDate,
    string? ProjectIds
) : IRequest<IResult>;

/// <summary>
/// Handles <see cref="GetWorkspaceChartQuery"/> — routes to the appropriate
/// analytics service method based on the <c>type</c> parameter.
/// </summary>
public sealed class GetWorkspaceChartQueryHandler
    : IRequestHandler<GetWorkspaceChartQuery, IResult>
{
    private readonly IAnalyticsQueryService _analyticsService;

    public GetWorkspaceChartQueryHandler(IAnalyticsQueryService analyticsService)
    {
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
    }

    public async ValueTask<IResult> Handle(GetWorkspaceChartQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.Type switch
        {
            "work-items" => TypedResults.Ok(await _analyticsService.GetWorkspaceWorkItemChartAsync(
                request.Slug, request.DateFilter, request.StartDate, request.EndDate, request.ProjectIds, cancellationToken)),
            "projects" => TypedResults.Ok(await _analyticsService.GetProjectSummaryChartAsync(
                request.Slug, request.DateFilter, request.StartDate, request.EndDate, request.ProjectIds, cancellationToken)),
            _ => TypedResults.BadRequest(new { message = "Invalid type parameter. Supported values: work-items, projects." }),
        };
    }
}
