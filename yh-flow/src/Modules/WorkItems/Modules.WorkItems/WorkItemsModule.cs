using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using YH.Framework.Persistence;
using YH.Framework.Web.Modules;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Services;
using YH.Modules.WorkItems.Features.v1.Estimates.CreateEstimate;
using YH.Modules.WorkItems.Features.v1.Estimates.CreateEstimatePoint;
using YH.Modules.WorkItems.Features.v1.Estimates.DeleteEstimate;
using YH.Modules.WorkItems.Features.v1.Estimates.DeleteEstimatePoint;
using YH.Modules.WorkItems.Features.v1.Estimates.GetEstimate;
using YH.Modules.WorkItems.Features.v1.Estimates.ListEstimates;
using YH.Modules.WorkItems.Features.v1.Estimates.UpdateEstimate;
using YH.Modules.WorkItems.Features.v1.Estimates.UpdateEstimatePoint;
using YH.Modules.WorkItems.Features.v1.Labels.CreateLabel;
using YH.Modules.WorkItems.Features.v1.Labels.DeleteLabel;
using YH.Modules.WorkItems.Features.v1.Labels.GetLabel;
using YH.Modules.WorkItems.Features.v1.Labels.ListLabels;
using YH.Modules.WorkItems.Features.v1.Labels.UpdateLabel;
using YH.Modules.WorkItems.Features.v1.States.CreateState;
using YH.Modules.WorkItems.Features.v1.States.DeleteState;
using YH.Modules.WorkItems.Features.v1.States.GetState;
using YH.Modules.WorkItems.Features.v1.States.ListStates;
using YH.Modules.WorkItems.Features.v1.States.UpdateState;

namespace YH.Modules.WorkItems;

/// <summary>
/// WorkItems module entry point.
/// </summary>
/// <remarks>
/// <b>Wave 2 wiring status (plan 04-02 complete):</b>
/// <list type="bullet">
///   <item><see cref="ConfigureServices"/> registers <see cref="WorkItemsDbContext"/> + health check.</item>
///   <item><see cref="MapEndpoints"/> registers all 5 state CRUD endpoints under
///     <c>/api/v1/workspaces/{slug}/projects/{projectId}/states/</c>.</item>
///   <item><see cref="MapEndpoints"/> registers all 5 label CRUD endpoints under
///     <c>/api/v1/workspaces/{slug}/projects/{projectId}/labels/</c>.</item>
/// </list>
/// <para>
/// <b>Next:</b> Wave 3 — Issue CRUD endpoints.
/// </para>
/// </remarks>
public sealed class WorkItemsModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // DbContext + health check.
        builder.Services.AddHeroDbContext<WorkItemsDbContext>();
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<WorkItemsDbContext>(
                name: "db:work-items",
                failureStatus: HealthStatus.Unhealthy);

        // Domain services
        builder.Services.AddScoped<IIssueSequenceService, IssueSequenceService>();
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

        // WorkItems route group — endpoints wired in subsequent tasks
        // TODO: Wire Issue endpoints (Wave 3)
        // TODO: Wire Intake endpoints (Wave 4)
        // TODO: Wire Import/Export endpoints (Wave 4)
        var estimates = endpoints
            .MapGroup("api/v{version:apiVersion}/workspaces/{slug}/projects/{projectId}/estimates")
            .WithTags("Estimates")
            .WithApiVersionSet(apiVersionSet);

        estimates.MapCreateEstimateEndpoint();
        estimates.MapListEstimatesEndpoint();
        estimates.MapGetEstimateEndpoint();
        estimates.MapUpdateEstimateEndpoint();
        estimates.MapDeleteEstimateEndpoint();

        // EstimatePoint endpoints nested under estimates group
        estimates.MapCreateEstimatePointEndpoint();
        estimates.MapUpdateEstimatePointEndpoint();
        estimates.MapDeleteEstimatePointEndpoint();

        // TODO: Wire Issue endpoints (Wave 3)
        // TODO: Wire Intake endpoints (Wave 4)
        // TODO: Wire Import/Export endpoints (Wave 4)

        // State route group under /workspaces/{slug}/projects/{projectId}/states/
        var states = endpoints
            .MapGroup("api/v{version:apiVersion}/workspaces/{slug}/projects/{projectId}/states")
            .WithTags("States")
            .WithApiVersionSet(apiVersionSet);

        states.MapCreateStateEndpoint();
        states.MapListStatesEndpoint();
        states.MapGetStateEndpoint();
        states.MapUpdateStateEndpoint();
        states.MapDeleteStateEndpoint();

        // Label route group under /workspaces/{slug}/projects/{projectId}/labels/
        var labels = endpoints
            .MapGroup("api/v{version:apiVersion}/workspaces/{slug}/projects/{projectId}/labels")
            .WithTags("Labels")
            .WithApiVersionSet(apiVersionSet);

        labels.MapCreateLabelEndpoint();
        labels.MapListLabelsEndpoint();
        labels.MapGetLabelEndpoint();
        labels.MapUpdateLabelEndpoint();
        labels.MapDeleteLabelEndpoint();
    }
}
