using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using YH.Framework.Web.Modules;

namespace YH.Modules.WorkItems;

/// <summary>
/// WorkItems module entry point.
/// </summary>
/// <remarks>
/// <b>Wave 1 wiring status (plan 04-01):</b>
/// <list type="bullet">
///   <item><see cref="ConfigureServices"/> has TODO placeholder — full DbContext + health check
///     registration deferred to Task 3 after domain entities and data layer are created.</item>
///   <item><see cref="MapEndpoints"/> registers the route group with TODO endpoints for subsequent waves.</item>
/// </list>
/// <para>
/// <b>Next:</b> Task 2 creates domain entities, then Task 3 completes this module registration.
/// </para>
/// </remarks>
public sealed class WorkItemsModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // TODO(Task 3): Register WorkItemsDbContext + health check after domain entities are created.
        // builder.Services.AddHeroDbContext<WorkItemsDbContext>();
        // builder.Services.AddHealthChecks()
        //     .AddDbContextCheck<WorkItemsDbContext>(
        //         name: "db:work-items",
        //         failureStatus: HealthStatus.Unhealthy);
    }

    public void ConfigureMiddleware(IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // No middleware needed for WorkItems module.
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var apiVersionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        // WorkItems route group under /workspaces/{slug}/projects/{projectId}/work-items/
        var workItems = endpoints
            .MapGroup("api/v{version:apiVersion}/workspaces/{slug}/projects/{projectId}/work-items")
            .WithTags("WorkItems")
            .WithApiVersionSet(apiVersionSet);

        // TODO: Wire endpoints in subsequent waves:
        // - State endpoints (Wave 2)
        // - Label endpoints (Wave 2)
        // - Issue endpoints (Wave 2/3)
        // - Estimate endpoints (Wave 3)
        // - Intake endpoints (Wave 4)
        // - Import/Export endpoints (Wave 4)
        _ = workItems;
    }
}
