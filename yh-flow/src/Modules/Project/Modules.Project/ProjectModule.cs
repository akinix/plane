using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using YH.Framework.Persistence;
using YH.Framework.Web.Modules;
using YH.Modules.Project.Data;

namespace YH.Modules.Project;

/// <summary>
/// Project module entry point.
/// </summary>
/// <remarks>
/// <b>Wave 2 wiring status (plan 03-02 complete):</b>
/// <list type="bullet">
///   <item><see cref="ConfigureServices"/> registers <see cref="ProjectDbContext"/> + health check.</item>
///   <item><see cref="MapEndpoints"/> is a placeholder — Wave 3 (03-03) will add project route group,
///     Wave 4 (03-04) will add member route group.</item>
/// </list>
/// </remarks>
public sealed class ProjectModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // DbContext + health check.
        builder.Services.AddHeroDbContext<ProjectDbContext>();
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<ProjectDbContext>(
                name: "db:project",
                failureStatus: HealthStatus.Unhealthy);
    }

    public void ConfigureMiddleware(IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // No middleware needed for Phase 3.
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        // TODO (03-03): Register project route group under /workspaces/{slug}/projects/
        // var apiVersionSet = endpoints.NewApiVersionSet()
        //     .HasApiVersion(new ApiVersion(1))
        //     .ReportApiVersions()
        //     .Build();
        //
        // var projects = endpoints
        //     .MapGroup("api/v{version:apiVersion}/workspaces/{slug}/projects")
        //     .WithTags("Projects")
        //     .WithApiVersionSet(apiVersionSet);
        //
        // projects.MapCreateProjectEndpoint();
        // projects.MapListProjectsEndpoint();
        // projects.MapGetProjectEndpoint();
        // projects.MapUpdateProjectEndpoint();
        // projects.MapDeleteProjectEndpoint();

        // TODO (03-04): Register member route group under /workspaces/{slug}/projects/{projectId}/members/
        // var members = endpoints
        //     .MapGroup("api/v{version:apiVersion}/workspaces/{slug}/projects/{projectId}/members")
        //     .WithTags("ProjectMembers")
        //     .WithApiVersionSet(apiVersionSet);
        //
        // members.MapAddMemberEndpoint();
        // members.MapListMembersEndpoint();
        // members.MapUpdateMemberRoleEndpoint();
        // members.MapRemoveMemberEndpoint();
    }
}
