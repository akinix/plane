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
using YH.Modules.WorkItems.Features.v1.Issues.CreateIssue;
using YH.Modules.WorkItems.Features.v1.Issues.GetIssue;
using YH.Modules.WorkItems.Features.v1.Issues.UpdateIssue;
using YH.Modules.WorkItems.Features.v1.Issues.DeleteIssue;
using YH.Modules.WorkItems.Features.v1.Issues.ListIssues;
using YH.Modules.WorkItems.Features.v1.IssueLinks.CreateIssueLink;
using YH.Modules.WorkItems.Features.v1.IssueLinks.DeleteIssueLink;
using YH.Modules.WorkItems.Features.v1.IssueLinks.ListIssueLinks;
using YH.Modules.WorkItems.Features.v1.States.UpdateState;
using YH.Modules.WorkItems.Features.v1.IssueComments.CreateIssueComment;
using YH.Modules.WorkItems.Features.v1.IssueComments.ListIssueComments;
using YH.Modules.WorkItems.Features.v1.IssueComments.UpdateIssueComment;
using YH.Modules.WorkItems.Features.v1.IssueComments.DeleteIssueComment;
using YH.Modules.WorkItems.Features.v1.IssueActivities.ListIssueActivities;
using YH.Modules.WorkItems.Features.v1.Issues.BulkUpdateIssues;
using YH.Modules.WorkItems.Features.v1.Intake.CreateIntakeIssue;
using YH.Modules.WorkItems.Features.v1.Intake.ListIntakeIssues;
using YH.Modules.WorkItems.Features.v1.Intake.UpdateIntakeIssue;
using YH.Modules.WorkItems.Features.v1.ImportExport.ExportIssues;
using YH.Modules.WorkItems.Features.v1.ImportExport.ImportIssues;
using YH.Modules.WorkItems.Features.v1.Cycles.CreateCycle;
using YH.Modules.WorkItems.Features.v1.Cycles.GetCycle;
using YH.Modules.WorkItems.Features.v1.Cycles.UpdateCycle;
using YH.Modules.WorkItems.Features.v1.Cycles.DeleteCycle;
using YH.Modules.WorkItems.Features.v1.Cycles.ListCycles;
using YH.Modules.WorkItems.Features.v1.Cycles.DateCheckCycle;
using YH.Modules.WorkItems.Features.v1.Cycles.Issues.AddIssuesToCycle;
using YH.Modules.WorkItems.Features.v1.Cycles.Issues.RemoveIssueFromCycle;
using YH.Modules.WorkItems.Features.v1.Cycles.Issues.ListCycleIssues;
using YH.Modules.WorkItems.Features.v1.Cycles.TransferCycleIssues;
using YH.Modules.WorkItems.Features.v1.Cycles.GetCycleProgress;
using YH.Modules.WorkItems.Features.v1.Cycles.ArchiveCycle;
using YH.Modules.WorkItems.Features.v1.Cycles.UnarchiveCycle;
using YH.Modules.WorkItems.Features.v1.Cycles.ListArchivedCycles;
using YH.Modules.WorkItems.Features.v1.Modules.CreateModule;
using YH.Modules.WorkItems.Features.v1.Modules.GetModule;
using YH.Modules.WorkItems.Features.v1.Modules.UpdateModule;
using YH.Modules.WorkItems.Features.v1.Modules.DeleteModule;
using YH.Modules.WorkItems.Features.v1.Modules.ListModules;
using YH.Modules.WorkItems.Features.v1.Modules.ArchiveModule;
using YH.Modules.WorkItems.Features.v1.Modules.UnarchiveModule;
using YH.Modules.WorkItems.Features.v1.Modules.ListArchivedModules;
using YH.Modules.WorkItems.Features.v1.Modules.Issues.AddIssuesToModule;
using YH.Modules.WorkItems.Features.v1.Modules.Issues.RemoveIssueFromModule;
using YH.Modules.WorkItems.Features.v1.Modules.Issues.ListModuleIssues;
using YH.Modules.WorkItems.Features.v1.Modules.GetModuleProgress;
using YH.Modules.WorkItems.Features.v1.Modules.Links.AddModuleLink;
using YH.Modules.WorkItems.Features.v1.Modules.Links.RemoveModuleLink;
using YH.Modules.WorkItems.Features.v1.Modules.Links.ListModuleLinks;

namespace YH.Modules.WorkItems;

/// <summary>
/// WorkItems module entry point.
/// </summary>
/// <remarks>
/// <b>Wave 4 wiring status (plan 04-04) + Wave 5 (plan 05-02):</b>
/// <list type="bullet">
///   <item><see cref="ConfigureServices"/> registers <see cref="WorkItemsDbContext"/> + health check.</item>
///   <item><see cref="MapEndpoints"/> registers all state, label, estimate, issue, issue-link endpoints.</item>
///   <item><see cref="MapEndpoints"/> registers IssueComment CRUD (4 endpoints) under
///     <c>/work-items/{issueId}/comments/</c>.</item>
///   <item><see cref="MapEndpoints"/> registers IssueActivity list endpoint under
///     <c>/work-items/{issueId}/activities/</c>.</item>
///   <item><see cref="MapEndpoints"/> registers BulkUpdateIssues endpoint under
///     <c>/work-items/bulk/</c>.</item>
///   <item><see cref="MapEndpoints"/> registers Intake CRUD (3 endpoints) under
///     <c>/work-items/intake/</c>.</item>
///   <item><see cref="MapEndpoints"/> registers ExportIssues endpoint under
///     <c>/work-items/export-issues/</c>.</item>
///   <item><see cref="MapEndpoints"/> registers ImportIssues endpoint under
///     <c>/work-items/import-issues/</c>.</item>
/// </list>
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
        builder.Services.AddScoped<IBurndownCalculator, BurndownCalculator>();
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

        workItems.MapCreateIssueEndpoint();
        workItems.MapListIssuesEndpoint();
        workItems.MapGetIssueEndpoint();
        workItems.MapUpdateIssueEndpoint();
        workItems.MapDeleteIssueEndpoint();

        // IssueLink endpoints nested under work-items group (/{issueId}/links/)
        workItems.MapCreateIssueLinkEndpoint();
        workItems.MapListIssueLinksEndpoint();
        workItems.MapDeleteIssueLinkEndpoint();

        // IssueComment endpoints (/{issueId}/comments/)
        workItems.MapCreateIssueCommentEndpoint();
        workItems.MapListIssueCommentsEndpoint();
        workItems.MapUpdateIssueCommentEndpoint();
        workItems.MapDeleteIssueCommentEndpoint();

        // IssueActivity endpoint (/{issueId}/activities/)
        workItems.MapListIssueActivitiesEndpoint();

        // Batch operations (/bulk/)
        workItems.MapBulkUpdateIssuesEndpoint();

        // Intake endpoints (/intake/, /intake/{intakeIssueId})
        workItems.MapCreateIntakeIssueEndpoint();
        workItems.MapListIntakeIssuesEndpoint();
        workItems.MapUpdateIntakeIssueEndpoint();

        // Export/Import endpoints (/export-issues/, /import-issues/)
        workItems.MapExportIssuesEndpoint();
        workItems.MapImportIssuesEndpoint();

        // Estimate route group under /workspaces/{slug}/projects/{projectId}/estimates/
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

        // Cycle route group under /workspaces/{slug}/projects/{projectId}/cycles/
        var cycles = endpoints
            .MapGroup("api/v{version:apiVersion}/workspaces/{slug}/projects/{projectId}/cycles")
            .WithTags("Cycles")
            .WithApiVersionSet(apiVersionSet);

        cycles.MapCreateCycleEndpoint();
        cycles.MapListCyclesEndpoint();
        cycles.MapGetCycleEndpoint();
        cycles.MapUpdateCycleEndpoint();
        cycles.MapDeleteCycleEndpoint();
        cycles.MapDateCheckCycleEndpoint();

        // Cycle-Issue endpoints
        cycles.MapAddIssuesToCycleEndpoint();
        cycles.MapRemoveIssueFromCycleEndpoint();
        cycles.MapListCycleIssuesEndpoint();

        // Transfer + Progress
        cycles.MapTransferCycleIssuesEndpoint();
        cycles.MapGetCycleProgressEndpoint();

        // Archive
        cycles.MapArchiveCycleEndpoint();

        // Archived cycles route group
        var archived = endpoints
            .MapGroup("api/v{version:apiVersion}/workspaces/{slug}/projects/{projectId}/archived-cycles")
            .WithTags("Cycles (Archived)")
            .WithApiVersionSet(apiVersionSet);

        archived.MapListArchivedCyclesEndpoint();
        archived.MapUnarchiveCycleEndpoint();

        // Module route group (Phase 6)
        var modules = endpoints
            .MapGroup("api/v{version:apiVersion}/workspaces/{slug}/projects/{projectId}/modules")
            .WithTags("Modules")
            .WithApiVersionSet(apiVersionSet);

        modules.MapCreateModuleEndpoint();
        modules.MapListModulesEndpoint();
        modules.MapGetModuleEndpoint();
        modules.MapUpdateModuleEndpoint();
        modules.MapDeleteModuleEndpoint();
        modules.MapArchiveModuleEndpoint();

        // Module-Issue endpoints
        modules.MapAddIssuesToModuleEndpoint();
        modules.MapRemoveIssueFromModuleEndpoint();
        modules.MapListModuleIssuesEndpoint();

        // Module progress
        modules.MapGetModuleProgressEndpoint();

        // ModuleLink endpoints
        modules.MapAddModuleLinkEndpoint();
        modules.MapRemoveModuleLinkEndpoint();
        modules.MapListModuleLinksEndpoint();

        // Archived modules route group
        var archivedModules = endpoints
            .MapGroup("api/v{version:apiVersion}/workspaces/{slug}/projects/{projectId}/archived-modules")
            .WithTags("Modules (Archived)")
            .WithApiVersionSet(apiVersionSet);

        archivedModules.MapListArchivedModulesEndpoint();
        archivedModules.MapUnarchiveModuleEndpoint();
    }
}
