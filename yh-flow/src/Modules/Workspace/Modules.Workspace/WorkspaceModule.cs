using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using YH.Framework.Persistence;
using YH.Framework.Shared.Multitenancy;
using YH.Framework.Web.Modules;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Data;
using YH.Modules.Workspace.MultiTenancy;

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

        // TODO 02-05: workspace services (ISlugGenerator + SlugGenerator,
        // IInvitationTokenService + InvitationTokenService, WorkspaceMembershipService).
    }

    public void ConfigureMiddleware(IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        // TODO 02-03: app.UseMiddleware<WorkspaceMembershipMiddleware>() (D-02 — populates
        // ICurrentWorkspaceContext after Finbuckle resolves the tenant and after authentication).
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        // TODO 02-04: top-level workspace endpoints (POST /api/v1/workspaces/, List user workspaces,
        // Check slug availability).
        // TODO 02-05: {slug}-scoped endpoints — workspace detail + members + invitations.
    }
}
