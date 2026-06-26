using Finbuckle.MultiTenant.Abstractions;
using Mediator;
using Microsoft.AspNetCore.Http;
using YH.Framework.Jobs.Services;
using YH.Framework.Shared.Multitenancy;
using YH.Modules.Analytics.Services;

namespace YH.Modules.Analytics.Features.v1.Export.ExportAnalytics;

/// <summary>
/// Command to trigger analytics CSV export.
/// </summary>
public sealed record ExportAnalyticsCommand(
    string Slug,
    string? ProjectIds
) : IRequest<IResult>;

/// <summary>
/// Handles <see cref="ExportAnalyticsCommand"/> — enqueues a Hangfire background job
/// that generates the CSV export asynchronously.
/// </summary>
public sealed class ExportAnalyticsCommandHandler
    : IRequestHandler<ExportAnalyticsCommand, IResult>
{
    private readonly IJobService _jobService;
    private readonly IMultiTenantContextAccessor<AppTenantInfo> _tenantAccessor;

    public ExportAnalyticsCommandHandler(
        IJobService jobService,
        IMultiTenantContextAccessor<AppTenantInfo> tenantAccessor)
    {
        _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
        _tenantAccessor = tenantAccessor ?? throw new ArgumentNullException(nameof(tenantAccessor));
    }

    public async ValueTask<IResult> Handle(ExportAnalyticsCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tenantId = _tenantAccessor.MultiTenantContext?.TenantInfo?.Id;
        if (string.IsNullOrEmpty(tenantId))
        {
            return TypedResults.Unauthorized();
        }

        // Enqueue Hangfire background job
        _jobService.Enqueue<ExportAnalyticsJob>(job =>
            job.RunAsync(tenantId, request.Slug, request.ProjectIds));

        return TypedResults.Ok(new
        {
            message = "Once the export is ready it will be available for download."
        });
    }
}
