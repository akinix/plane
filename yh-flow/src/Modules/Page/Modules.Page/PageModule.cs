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
using YH.Modules.Page.Features.v1.Pages.AddFavorite;
using YH.Modules.Page.Features.v1.Pages.ArchivePage;
using YH.Modules.Page.Features.v1.Pages.CreatePage;
using YH.Modules.Page.Features.v1.Pages.DeletePage;
using YH.Modules.Page.Features.v1.Pages.GetPage;
using YH.Modules.Page.Features.v1.Pages.GetPageDescription;
using YH.Modules.Page.Features.v1.Pages.GetPageSummary;
using YH.Modules.Page.Features.v1.Pages.ListPages;
using YH.Modules.Page.Features.v1.Pages.RemoveFavorite;
using YH.Modules.Page.Features.v1.Pages.UpdatePage;
using YH.Modules.Page.Features.v1.Pages.UpdatePageDescription;

namespace YH.Modules.Page;

/// <summary>
/// Page module entry point.
/// </summary>
/// <remarks>
/// <b>Wave 2 wiring status (plan 07-02 complete):</b>
/// <list type="bullet">
///   <item><see cref="ConfigureServices"/> registers <see cref="PageDbContext"/> + health check.</item>
///   <item><see cref="MapEndpoints"/> registers all 12 page endpoint groups under
///     <c>/api/v1/workspaces/{slug}/projects/{projectId}/pages/</c>.</item>
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

        // Page routes under /workspaces/{slug}/projects/{projectId}/pages/
        var pages = endpoints
            .MapGroup("api/v{version:apiVersion}/workspaces/{slug}/projects/{projectId}/pages")
            .WithTags("Pages")
            .WithApiVersionSet(apiVersionSet);

        // Core CRUD
        pages.MapCreatePageEndpoint();
        pages.MapGetPageEndpoint();
        pages.MapUpdatePageEndpoint();
        pages.MapDeletePageEndpoint();
        pages.MapListPagesEndpoint();

        // Archive / Unarchive (bound to /{pageId}/archive)
        pages.MapArchivePageEndpoint();
        pages.MapUnarchivePageEndpoint();

        // Description (bound to /{pageId}/description)
        pages.MapGetPageDescriptionEndpoint();
        pages.MapUpdatePageDescriptionEndpoint();

        // Favorite (bound to /{pageId}/favorite)
        pages.MapAddFavoriteEndpoint();
        pages.MapRemoveFavoriteEndpoint();

        // Page summary route group (no {pageId} parameter)
        var pageSummary = endpoints
            .MapGroup("api/v{version:apiVersion}/workspaces/{slug}/projects/{projectId}/pages-summary")
            .WithTags("Pages")
            .WithApiVersionSet(apiVersionSet);

        pageSummary.MapGetPageSummaryEndpoint();
    }
}