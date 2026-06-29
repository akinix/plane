using Microsoft.Extensions.DependencyInjection;
using YH.Modules.Analytics.Services;

namespace YH.Modules.Analytics.Features.v1.Export.ExportAnalytics;

/// <summary>
/// Hangfire background job that generates analytics CSV export.
/// Resolves <see cref="IAnalyticsExportService"/> via <see cref="IServiceScopeFactory"/>
/// because Hangfire jobs run outside of an HTTP request scope.
/// </summary>
public sealed class ExportAnalyticsJob
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ExportAnalyticsJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    }

    /// <summary>
    /// Hangfire job entry point. Generates CSV and writes to temp directory.
    /// </summary>
    /// <param name="tenantId">Explicit tenant ID (job has no HttpContext).</param>
    /// <param name="slug">Workspace slug.</param>
    /// <param name="projectIds">Optional project ID filter.</param>
    public async Task RunAsync(string tenantId, string slug, string? projectIds)
    {
        using var scope = _scopeFactory.CreateScope();
        var exportService = scope.ServiceProvider.GetRequiredService<IAnalyticsExportService>();
        var csv = await exportService.GenerateCsvAsync(tenantId, slug, projectIds, CancellationToken.None);

        var fileName = $"analytics-export-{slug}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";
        var filePath = Path.Combine(Path.GetTempPath(), fileName);
        await File.WriteAllBytesAsync(filePath, csv);
    }
}
