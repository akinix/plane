namespace YH.Modules.Analytics.Services;

/// <summary>
/// Analytics CSV export service — generates CSV from live Issue data.
/// Designed to be called from Hangfire background jobs (tenantId is passed explicitly).
/// </summary>
public interface IAnalyticsExportService
{
    /// <summary>
    /// Generates analytics data as a CSV byte array.
    /// </summary>
    /// <param name="tenantId">Current tenant ID (explicit for Hangfire jobs without HttpContext).</param>
    /// <param name="slug">Workspace slug.</param>
    /// <param name="projectIds">Optional project ID filter (comma-separated).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<byte[]> GenerateCsvAsync(string tenantId, string slug, string? projectIds, CancellationToken ct);

    /// <summary>
    /// Gets the CSV Content-Type value.
    /// </summary>
    string ContentType { get; }
}
