using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using YH.Framework.Web.Modules;
using YH.Modules.Analytics.Features.v1.Charts.GetProjectChart;
using YH.Modules.Analytics.Features.v1.Charts.GetWorkspaceChart;
using YH.Modules.Analytics.Features.v1.Overview.GetProjectAnalytics;
using YH.Modules.Analytics.Features.v1.Overview.GetWorkspaceAnalytics;
using YH.Modules.Analytics.Features.v1.Stats.GetProjectStats;
using YH.Modules.Analytics.Features.v1.Stats.GetWorkspaceStats;
using YH.Modules.Analytics.Services;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.Analytics;

/// <summary>
/// Analytics module entry point.
/// </summary>
/// <remarks>
/// <b>Wave 2 wiring status (plan 12-02 complete):</b>
/// <list type="bullet">
///   <item><see cref="ConfigureServices"/> registers <see cref="IAnalyticsQueryService"/> and health check.</item>
///   <item><see cref="MapEndpoints"/> registers Overview + Stats endpoint groups.</item>
/// </list>
/// <b>Wave 3:</b> Chart + Export endpoints will be registered here.
/// </remarks>
public sealed class AnalyticsModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Analytics is a pure query module — no independent DbContext.
        // Reuses WorkItemsDbContext for real-time aggregation queries.
        builder.Services.AddScoped<IAnalyticsQueryService, AnalyticsQueryService>();

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

        var apiVersionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        // Workspace-level analytics routes: /api/v1/workspaces/{slug}/analytics
        var workspaceAnalytics = endpoints
            .MapGroup("api/v{version:apiVersion}/workspaces/{slug}/analytics")
            .WithTags("Analytics")
            .WithApiVersionSet(apiVersionSet);

        workspaceAnalytics.MapGetWorkspaceAnalyticsEndpoint();   // GET /
        workspaceAnalytics.MapGetWorkspaceStatsEndpoint();       // GET /stats

        // Project-level analytics routes: /api/v1/workspaces/{slug}/projects/{projectId}/analytics
        var projectAnalytics = endpoints
            .MapGroup("api/v{version:apiVersion}/workspaces/{slug}/projects/{projectId}/analytics")
            .WithTags("Analytics (Project)")
            .WithApiVersionSet(apiVersionSet);

        projectAnalytics.MapGetProjectAnalyticsEndpoint();       // GET /
        projectAnalytics.MapGetProjectStatsEndpoint();           // GET /stats

        // Wave 3: Chart endpoints
        workspaceAnalytics.MapGetWorkspaceChartEndpoint();       // GET /charts?type=work-items|projects

        // Project-level chart endpoint
        projectAnalytics.MapGetProjectChartEndpoint();           // GET /charts?type=work-items

        // Wave 3: Export endpoint will be registered here in Task 2.
    }
}
