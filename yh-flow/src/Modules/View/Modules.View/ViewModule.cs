using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using YH.Framework.Persistence;
using YH.Framework.Web.Modules;
using YH.Modules.View.Data;

namespace YH.Modules.View;

/// <summary>
/// View module entry point.
/// </summary>
/// <remarks>
/// <b>Wave 1 wiring status (plan 08-01 complete):</b>
/// <list type="bullet">
///   <item><see cref="ConfigureServices"/> registers <see cref="ViewDbContext"/> + health check.</item>
///   <item><see cref="MapEndpoints"/> registers project-views and workspace-views route group stubs.</item>
/// </list>
/// </remarks>
public sealed class ViewModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // DbContext + health check.
        builder.Services.AddHeroDbContext<ViewDbContext>();
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<ViewDbContext>(
                name: "db:view",
                failureStatus: HealthStatus.Unhealthy);
    }

    public void ConfigureMiddleware(IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // No middleware needed for Phase 8 Wave 1.
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var apiVersionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        // Project-level View route group
        var projectViews = endpoints
            .MapGroup("api/v{version:apiVersion}/workspaces/{slug}/projects/{projectId}/views")
            .WithTags("Views")
            .WithApiVersionSet(apiVersionSet);

        // Workspace-level View route group (projectId not required)
        var workspaceViews = endpoints
            .MapGroup("api/v{version:apiVersion}/workspaces/{slug}/views")
            .WithTags("Views")
            .WithApiVersionSet(apiVersionSet);
    }
}
