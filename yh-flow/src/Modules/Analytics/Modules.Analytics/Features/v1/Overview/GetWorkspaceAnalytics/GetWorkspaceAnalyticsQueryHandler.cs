using Finbuckle.MultiTenant.Abstractions;
using Mediator;
using Microsoft.AspNetCore.Http;
using YH.Framework.Shared.Multitenancy;
using YH.Modules.Analytics.Contracts.DTOs;
using YH.Modules.Analytics.Services;

namespace YH.Modules.Analytics.Features.v1.Overview.GetWorkspaceAnalytics;

/// <summary>
/// Handles <see cref="GetWorkspaceAnalyticsQuery"/> — routes to the appropriate
/// analytics service method based on the <c>tab</c> parameter.
/// </summary>
public sealed class GetWorkspaceAnalyticsQueryHandler
    : IRequestHandler<GetWorkspaceAnalyticsQuery, IResult>
{
    private readonly IAnalyticsQueryService _analyticsService;
    private readonly IMultiTenantContextAccessor<AppTenantInfo> _tenantAccessor;

    public GetWorkspaceAnalyticsQueryHandler(
        IAnalyticsQueryService analyticsService,
        IMultiTenantContextAccessor<AppTenantInfo> tenantAccessor)
    {
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
        _tenantAccessor = tenantAccessor ?? throw new ArgumentNullException(nameof(tenantAccessor));
    }

    public async ValueTask<IResult> Handle(GetWorkspaceAnalyticsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tenantId = _tenantAccessor.MultiTenantContext?.TenantInfo?.Id;
        if (string.IsNullOrEmpty(tenantId))
        {
            return TypedResults.Unauthorized();
        }

        return request.Tab switch
        {
            "overview" => TypedResults.Ok(await _analyticsService.GetWorkspaceOverviewAsync(
                request.Slug, request.DateFilter, request.StartDate, request.EndDate, request.ProjectIds, cancellationToken)),
            "work-items" => TypedResults.Ok(await _analyticsService.GetWorkItemStatsAsync(
                request.Slug, request.DateFilter, request.StartDate, request.EndDate, request.ProjectIds, cancellationToken)),
            _ => TypedResults.BadRequest(new { message = "Invalid tab parameter. Supported values: overview, work-items." }),
        };
    }
}
