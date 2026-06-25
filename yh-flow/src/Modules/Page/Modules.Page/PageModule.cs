using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using YH.Framework.Persistence;
using YH.Framework.Web.Modules;
using YH.Modules.Page.Data;

namespace YH.Modules.Page;

/// <summary>
/// Page module entry point.
/// </summary>
/// <remarks>
/// <b>Wave 1 wiring status (plan 07-01 complete):</b>
/// <list type="bullet">
///   <item><see cref="ConfigureServices"/> registers <see cref="PageDbContext"/> + health check.</item>
///   <item><see cref="MapEndpoints"/> registers pages + pages-summary route group stubs
///     (actual endpoints registered in plan 07-02).</item>
/// </list>
/// </remarks>
public sealed class PageModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // DbContext + health check.
        builder.Services.AddHeroDbContext<PageDbContext>();
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<PageDbContext>(
                name: "db:page",
                failureStatus: HealthStatus.Unhealthy);
    }

    public void ConfigureMiddleware(IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // No middleware needed for Phase 7.
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var apiVersionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        // Page route group (Phase 7) — endpoints registered in plan 07-02
        var pages = endpoints
            .MapGroup("api/v{version:apiVersion}/workspaces/{slug}/projects/{projectId}/pages")
            .WithTags("Pages")
            .WithApiVersionSet(apiVersionSet);

        _ = pages; // placeholder — endpoints added in 07-02

        // Page summary route group (Phase 7) — no {pageId} parameter
        var pageSummary = endpoints
            .MapGroup("api/v{version:apiVersion}/workspaces/{slug}/projects/{projectId}/pages-summary")
            .WithTags("Pages")
            .WithApiVersionSet(apiVersionSet);

        _ = pageSummary; // placeholder — endpoints added in 07-02
    }
}