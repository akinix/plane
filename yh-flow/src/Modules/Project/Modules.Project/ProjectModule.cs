using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using YH.Framework.Persistence;
using YH.Framework.Web.Modules;
using YH.Modules.Project.Data;
using YH.Modules.Project.Features.v1.Projects.CreateProject;
using YH.Modules.Project.Features.v1.Projects.DeleteProject;
using YH.Modules.Project.Features.v1.Projects.GetProject;
using YH.Modules.Project.Features.v1.Projects.ListProjects;
using YH.Modules.Project.Features.v1.Projects.UpdateProject;

namespace YH.Modules.Project;

/// <summary>
/// Project module entry point.
/// </summary>
/// <remarks>
/// <b>Wave 3 wiring status (plan 03-03 complete):</b>
/// <list type="bullet">
///   <item><see cref="ConfigureServices"/> registers <see cref="ProjectDbContext"/> + health check.</item>
///   <item><see cref="MapEndpoints"/> registers all 5 project CRUD endpoints under
///     <c>/api/v1/workspaces/{slug}/projects/</c>.</item>
/// </list>
/// <para>
/// <b>Wave 4 (03-04):</b> will add member route group under <c>/workspaces/{slug}/projects/{projectId}/members/</c>.
/// </para>
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

        var apiVersionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        // Project routes under /workspaces/{slug}/projects/
        var projects = endpoints
            .MapGroup("api/v{version:apiVersion}/workspaces/{slug}/projects")
            .WithTags("Projects")
            .WithApiVersionSet(apiVersionSet);

        // Create + List bind to "/" on the projects group
        projects.MapCreateProjectEndpoint();
        projects.MapListProjectsEndpoint();

        // Get / Update / Delete bind to "/{projectId}" on the same group
        projects.MapGetProjectEndpoint();
        projects.MapUpdateProjectEndpoint();
        projects.MapDeleteProjectEndpoint();

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
