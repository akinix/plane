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
using YH.Modules.View.Features.v1.Views.AddFavorite;
using YH.Modules.View.Features.v1.Views.ArchiveView;
using YH.Modules.View.Features.v1.Views.CreateView;
using YH.Modules.View.Features.v1.Views.DeleteView;
using YH.Modules.View.Features.v1.Views.GetView;
using YH.Modules.View.Features.v1.Views.ListViews;
using YH.Modules.View.Features.v1.Views.RemoveFavorite;
using YH.Modules.View.Features.v1.Views.UpdateView;
using YH.Modules.View.Features.v1.Views.WorkspaceViewsList;

namespace YH.Modules.View;

/// <summary>
/// View module entry point.
/// </summary>
/// <remarks>
/// <b>Wave 2 wiring status (plan 08-02 complete):</b>
/// <list type="bullet">
///   <item><see cref="ConfigureServices"/> registers <see cref="ViewDbContext"/> + health check.</item>
///   <item><see cref="MapEndpoints"/> registers all 10 view endpoint groups under
///     <c>/api/v1/workspaces/{slug}/projects/{projectId}/views/</c>
///     and workspace-level list under <c>/api/v1/workspaces/{slug}/views/</c>.</item>
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

        // No middleware needed for Phase 8 Wave 2.
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var apiVersionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        // Project-level View routes under /workspaces/{slug}/projects/{projectId}/views/
        var projectViews = endpoints
            .MapGroup("api/v{version:apiVersion}/workspaces/{slug}/projects/{projectId}/views")
            .WithTags("Views")
            .WithApiVersionSet(apiVersionSet);

        // Core CRUD
        projectViews.MapCreateViewEndpoint();
        projectViews.MapGetViewEndpoint();
        projectViews.MapUpdateViewEndpoint();
        projectViews.MapDeleteViewEndpoint();
        projectViews.MapListViewsEndpoint();

        // Archive / Unarchive (bound to /{viewId}/archive)
        projectViews.MapArchiveViewEndpoint();
        projectViews.MapUnarchiveViewEndpoint();

        // Favorite (bound to /{viewId}/favorite)
        projectViews.MapAddFavoriteViewEndpoint();
        projectViews.MapRemoveFavoriteViewEndpoint();

        // Workspace-level View routes under /workspaces/{slug}/views/
        var workspaceViews = endpoints
            .MapGroup("api/v{version:apiVersion}/workspaces/{slug}/views")
            .WithTags("Views")
            .WithApiVersionSet(apiVersionSet);

        workspaceViews.MapWorkspaceViewsListEndpoint();
    }
}
