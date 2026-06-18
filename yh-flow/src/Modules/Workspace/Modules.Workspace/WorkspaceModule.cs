using Asp.Versioning;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using YH.Framework.Persistence;
using YH.Framework.Shared.Multitenancy;
using YH.Framework.Web.Modules;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Configuration;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Data;
using YH.Modules.Workspace.Features.v1.Workspaces.CheckWorkspaceSlug;
using YH.Modules.Workspace.Features.v1.Workspaces.CreateWorkspace;
using YH.Modules.Workspace.Features.v1.Workspaces.DeleteWorkspace;
using YH.Modules.Workspace.Features.v1.Workspaces.GetWorkspace;
using YH.Modules.Workspace.Features.v1.Workspaces.ListUserWorkspaces;
using YH.Modules.Workspace.Features.v1.Workspaces.UpdateWorkspace;
using YH.Modules.Workspace.Middleware;
using YH.Modules.Workspace.MultiTenancy;
using YH.Modules.Workspace.Services;

namespace YH.Modules.Workspace;

/// <summary>
/// Workspace module entry point (CONTEXT core value; RESEARCH §Architectural Responsibility Map).
/// </summary>
/// <remarks>
/// <b>Wave 1 wiring status (plan 02-02 complete):</b>
/// <list type="bullet">
///   <item><see cref="ConfigureServices"/> registers <see cref="WorkspaceDbContext"/> +
///     health check, the scoped <see cref="CurrentWorkspaceContext"/> (D-03), and the Finbuckle
///     slug strategy + tenant store via external <c>TryAddEnumerable</c> (per 02-01 spike Q1 —
///     path A, preferred).</item>
///   <item><see cref="ConfigureMiddleware"/> / <see cref="MapEndpoints"/> remain TODO for
///     02-03 (WorkspaceMembershipMiddleware) and 02-04/05 (endpoints) respectively.</item>
/// </list>
/// <para>
/// <b>Finbuckle wiring (D-01, threats T-2-fintenant [BLOCKING] + T-2-slugstrategy):</b>
/// the slug strategy + WorkspaceTenantStore are appended to the existing Phase 1 chain via
/// <c>services.TryAddEnumerable</c>. The 02-01 spike (Q1) proved this works: external append
/// joins the resolved <c>IEnumerable&lt;IMultiTenantStrategy&gt;</c> /
/// <c>IEnumerable&lt;IMultiTenantStore&lt;AppTenantInfo&gt;&gt;</c> collections without re-invoking
/// the <c>AddMultiTenant&lt;AppTenantInfo&gt;()</c> builder (Pitfall 1 — second AddMultiTenant call
/// would reset the builder and erase Phase 1's claim/header/query strategies).
/// <see cref="IMultiTenantStrategy"/> is the NON-generic interface in Finbuckle 10.1.x — there is
/// no <c>IMultiTenantStrategy&lt;T&gt;</c> (02-01 spike authoritative finding).
/// </para>
/// <para>
/// <b>Module order (AssemblyInfo <c>Order=200</c>):</b> Workspace MUST run after Multitenancy
/// (so the builder chain is in place) and before Auditing (300). The slug strategy's
/// <c>Priority=-100</c> additionally ensures Finbuckle evaluates it AHEAD of the Phase 1
/// claim/header strategies at request time.
/// </para>
/// </remarks>
public sealed class WorkspaceModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // DbContext + health check.
        builder.Services.AddHeroDbContext<WorkspaceDbContext>();
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<WorkspaceDbContext>(
                name: "db:workspace",
                failureStatus: HealthStatus.Unhealthy);

        // D-03 — per-request workspace context. WorkspaceMembershipMiddleware (02-03) writes;
        // handlers / RequireWorkspaceRoleAuthorizationHandler read.
        builder.Services.AddScoped<ICurrentWorkspaceContext, CurrentWorkspaceContext>();

        // D-01 Finbuckle wiring — preferred path A (02-01 spike Q1 outcome).
        //
        // TryAddEnumerable (NOT TryAdd) appends to the existing IEnumerable<> collection rather
        // than no-op'ing on first match. The strategy is registered as Singleton (stateless —
        // reads HttpContext only); the store is Scoped because it depends on the scoped
        // WorkspaceDbContext. This wiring is additive: Phase 1's claim/header/query strategies
        // and EFCoreStore<TenantDbContext> / DistributedCacheStore are untouched.
        //
        // CRITICAL: the strategy contract is the NON-generic IMultiTenantStrategy. The generic
        // form `IMultiTenantStrategy<AppTenantInfo>` does NOT compile in Finbuckle 10.1.x —
        // PATTERNS.md / RESEARCH.md samples that show the generic form are wrong; the 02-01 spike
        // is the authoritative source.
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IMultiTenantStrategy, WorkspaceSlugStrategy>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Scoped<
            IMultiTenantStore<AppTenantInfo>, WorkspaceTenantStore>());

        // D-11 — workspace-role authorization handler. Reads pre-populated ICurrentWorkspaceContext
        // (no DB hit — populated by WorkspaceMembershipMiddleware above). Multi-registered alongside
        // Identity's RequiredPermissionAuthorizationHandler via IAuthorizationHandler IEnumerable;
        // the two coexist without conflict because each handles its own requirement type.
        builder.Services.TryAddEnumerable(ServiceDescriptor.Scoped<
            IAuthorizationHandler, RequireWorkspaceRoleAuthorizationHandler>());

        // D-07/D-08/D-09 — slug generation service (plan 02-04). Scoped because it depends on the
        // scoped WorkspaceDbContext (collision probe in GenerateUniqueSlugAsync).
        builder.Services.AddScoped<ISlugGenerator, SlugGenerator>();

        // D-12 — invitation token service (plan 02-05). Scoped because it depends on the scoped
        // WorkspaceDbContext + WorkspaceTokenOptions (bound from Workspace:InvitationTokenTtlDays).
        builder.Services.Configure<WorkspaceTokenOptions>(
            builder.Configuration.GetSection("Workspace"));
        builder.Services.AddScoped<IInvitationTokenService, InvitationTokenService>();

        // D-04/D-06 — single entry point for workspace membership mutations (plan 02-05). Scoped
        // because it depends on the scoped WorkspaceDbContext.
        builder.Services.AddScoped<WorkspaceMembershipService>();
    }

    public void ConfigureMiddleware(IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // D-02 — populate ICurrentWorkspaceContext after Finbuckle resolves the tenant and after
        // authentication. Module Order=200 guarantees this runs after Identity/Multitenancy's
        // middleware (auth + tenant resolution) but before Auditing (300). The middleware is the
        // SINGLE writer of ICurrentWorkspaceContext per scope (threat T-2-memberskip).
        app.UseMiddleware<WorkspaceMembershipMiddleware>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var apiVersionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        // Top-level workspace routes (no {slug} segment): create, list-mine, slug-check. Plane uses
        // /api/v1/workspaces/ for create + slug-check, and /api/v1/users/me/workspaces/ for list-mine.
        var topLevel = endpoints
            .MapGroup("api/v{version:apiVersion}/workspaces")
            .WithTags("Workspaces")
            .WithApiVersionSet(apiVersionSet);

        topLevel.MapCreateWorkspaceEndpoint();
        topLevel.MapVerifyWorkspaceSlugEndpoint();

        // list-mine lives under /api/v1/users/me/workspaces/ (Plane-compatible path). Registered on
        // the root endpoint route builder so the path is distinct from the /workspaces group above.
        endpoints.MapListUserWorkspacesEndpoint();

        // Scoped workspace routes (with {slug} segment): get / update / delete. These rely on the
        // Finbuckle slug strategy resolving {slug} to a tenant, after which
        // WorkspaceMembershipMiddleware populates ICurrentWorkspaceContext and
        // [RequireWorkspaceRole] enforces Admin on mutations (T-2-eop mitigation).
        var scoped = endpoints
            .MapGroup("api/v{version:apiVersion}/workspaces/{slug}")
            .WithTags("Workspaces")
            .WithApiVersionSet(apiVersionSet);

        scoped.MapGetWorkspaceEndpoint();
        scoped.MapUpdateWorkspaceEndpoint();
        scoped.MapDeleteWorkspaceEndpoint();

        // TODO 02-05: {slug}-scoped member + invitation endpoints
        // (MapGroup("api/v{version:apiVersion}/workspaces/{slug}/members") etc.).
    }
}
