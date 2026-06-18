using YH.Framework.Web.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;

namespace YH.Modules.Workspace;

/// <summary>
/// Workspace module entry point (CONTEXT core value; RESEARCH §Architectural Responsibility Map).
/// </summary>
/// <remarks>
/// <b>Wiring roadmap (incremental across Wave 1):</b>
/// <list type="bullet">
///   <item><b>Task 1 (this commit):</b> module skeleton + <c>[FshModule]</c> self-registration.
///     <see cref="ConfigureServices"/> / <see cref="ConfigureMiddleware"/> / <see cref="MapEndpoints"/>
///     are intentionally empty placeholders — <c>WorkspaceDbContext</c> (Task 2) and
///     <c>CurrentWorkspaceContext</c> + slug strategy/store wiring (Task 3) are not yet defined.</item>
///   <item><b>Task 2:</b> <c>WorkspaceDbContext</c> + entity configurations land;
///     <c>AddHeroDbContext&lt;WorkspaceDbContext&gt;()</c> + <c>AddDbContextCheck</c> registered here.</item>
///   <item><b>Task 3:</b> slug strategy + tenant store + <c>CurrentWorkspaceContext</c> wired via
///     <c>TryAddEnumerable</c> (per 02-01 spike Q1 outcome — preferred path A).</item>
///   <item><b>02-03:</b> <c>WorkspaceMembershipMiddleware</c> registered in
///     <see cref="ConfigureMiddleware"/> (D-02).</item>
///   <item><b>02-04 / 02-05:</b> top-level + scoped endpoint groups mapped in <see cref="MapEndpoints"/>.</item>
/// </list>
/// </remarks>
public sealed class WorkspaceModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        // TODO 02-02 Task 2: builder.Services.AddHeroDbContext<WorkspaceDbContext>() + health check.
        // TODO 02-02 Task 3: TryAddEnumerable slug strategy + WorkspaceTenantStore; CurrentWorkspaceContext scoped.
        // TODO 02-05: workspace services (SlugGenerator, InvitationTokenService, WorkspaceMembershipService).
    }

    public void ConfigureMiddleware(IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        // TODO 02-03: app.UseMiddleware<WorkspaceMembershipMiddleware>() (D-02 — populates ICurrentWorkspaceContext).
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        // TODO 02-04: top-level workspace endpoints (POST /api/v1/workspaces/, List user workspaces, Check slug).
        // TODO 02-05: {slug}-scoped workspace detail + members + invitations endpoints.
    }
}
