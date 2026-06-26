using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using YH.Framework.Web.Modules;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.Analytics;

/// <summary>
/// Analytics module entry point.
/// </summary>
/// <remarks>
/// <b>Wave 1 wiring status (plan 12-01 complete):</b>
/// <list type="bullet">
///   <item><see cref="ConfigureServices"/> registers health check on <see cref="WorkItemsDbContext"/>.</item>
///   <item><see cref="MapEndpoints"/> is a stub — endpoints will be registered in Wave 2 and Wave 3.</item>
/// </list>
/// </remarks>
public sealed class AnalyticsModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Analytics is a pure query module — no independent DbContext.
        // Reuses WorkItemsDbContext for real-time aggregation queries.
        // AnalyticsQueryService will be injected via DI in Wave 2.
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<WorkItemsDbContext>(
                name: "db:work-items",
                failureStatus: HealthStatus.Unhealthy);
    }

    public void ConfigureMiddleware(IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // No middleware needed for Phase 12.
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        // Wave 2: Analytics Overview + Stats endpoints will be registered here.
        // Wave 3: Chart + Export endpoints will be registered here.
    }
}
