using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using YH.Framework.Web.Modules;
using YH.Modules.Analytics.Features.v1.Overview.GetProjectAnalytics;
using YH.Modules.Analytics.Features.v1.Overview.GetWorkspaceAnalytics;
using YH.Modules.Analytics.Services;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.Analytics;

/// <summary>
/// Analytics module entry point.
/// </summary>
/// <remarks>
/// <b>Wave 2 wiring status (plan 12-02):</b>
/// <list type="bullet">
///   <item><see cref="ConfigureServices"/> registers <see cref="IAnalyticsQueryService"/> and health check.</item>
///   <item><see cref="MapEndpoints"/> registers Overview endpoint groups.</item>
/// </list>
/// <b>Wave 2 remaining:</b> Stats endpoint registrations to be added next.
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

        // Project-level analytics routes: /api/v1/workspaces/{slug}/projects/{projectId}/analytics
        var projectAnalytics = endpoints
            .MapGroup("api/v{version:apiVersion}/workspaces/{slug}/projects/{projectId}/analytics")
            .WithTags("Analytics (Project)")
            .WithApiVersionSet(apiVersionSet);

        projectAnalytics.MapGetProjectAnalyticsEndpoint();       // GET /

        // Wave 2 remaining: Stats endpoints will be registered next.
        // Wave 3: Chart + Export endpoints will be registered here.
    }
}
