using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Stores;
using Finbuckle.MultiTenant.Extensions;
using Finbuckle.MultiTenant.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using YH.Framework.Shared.Constants;
using YH.Framework.Shared.Multitenancy;
using YH.Modules.Multitenancy.Data;

namespace YH.Tests.Workspace.Spike;

/// <summary>
/// Wave 0 Q1 spike (RESEARCH §Open Questions Q1, plan 02-01 Task 1).
///
/// <b>Question under test:</b> Does Finbuckle 10.1.x allow appending a custom
/// <c>IMultiTenantStrategy&lt;AppTenantInfo&gt;</c> / <c>IMultiTenantStore&lt;AppTenantInfo&gt;</c>
/// registration from <em>outside</em> the <c>AddMultiTenant&lt;AppTenantInfo&gt;(...)</c> builder
/// chain (via <c>services.TryAddEnumerable&lt;...&gt;</c>), so a downstream module (Workspace) can
/// plug in its slug strategy/store without re-invoking the builder and clobbering Phase 1's claim /
/// header / query strategies?
///
/// <b>Why this matters:</b> RESEARCH §Pitfall 1 warns that calling <c>AddMultiTenant&lt;AppTenantInfo&gt;</c>
/// a second time resets the builder and erases Phase 1 configuration. Plan 02-02 needs to know which
/// registration pattern to use. The two outcomes are:
/// <list type="bullet">
///   <item><b>PASS</b> — external <c>TryAddEnumerable</c> works → 02-02 registers the slug strategy/store
///     directly from <c>WorkspaceModule.ConfigureServices</c> (preferred, minimal cross-module coupling).</item>
///   <item><b>FAIL</b> — external <c>TryAddEnumerable</c> is ignored → 02-02 must expose
///     <c>AddWorkspaceTenantResolution(this MultiTenantBuilder&lt;AppTenantInfo&gt;)</c> and call it
///     from inside <c>MultitenancyModule</c> (fallback).</item>
/// </list>
///
/// The fallback design path is documented for 02-02 regardless of the spike result; this test
/// produces the authoritative answer.
/// </summary>
public sealed class FinbuckleExternalStrategyRegistrationTests
{
    /// <summary>
    /// Spike test 1: append a strategy stub AFTER <c>AddMultiTenant&lt;AppTenantInfo&gt;()</c> via
    /// <c>services.TryAddEnumerable</c> and confirm it is resolvable as part of the
    /// <c>IEnumerable&lt;IMultiTenantStrategy&gt;</c> collection (count >= 2:
    /// the Phase 1 <c>ClaimStrategy</c> plus the appended stub).
    /// </summary>
    /// <remarks>
    /// <b>Q1 finding (recorded in 02-01-SUMMARY.md):</b> in Finbuckle 10.1.x the strategy contract is
    /// <b>non-generic</b> <c>IMultiTenantStrategy</c> — there is no <c>IMultiTenantStrategy&lt;T&gt;</c>.
    /// (Stores remain generic <c>IMultiTenantStore&lt;T&gt;</c>.) External registration must therefore
    /// target the non-generic interface.
    /// </remarks>
    [Fact]
    public void TryAddEnumerable_Strategy_AppendedAfterBuilder_IsResolvable()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        // Replicate the Phase 1 MultitenancyModule registration surface (at minimum: claim strategy).
        // We deliberately do NOT include header/query strategies to keep the assertion count crisp;
        // the spike question is purely "does external append survive?".
        services.AddMultiTenant<AppTenantInfo>()
            .WithClaimStrategy(ClaimConstants.Tenant);

        // === The behaviour under test: external module appends a strategy outside the builder. ===
        // Note: IMultiTenantStrategy is NON-generic in Finbuckle 10.1.x.
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IMultiTenantStrategy, WorkspaceSlugStrategyStub>());

        using var provider = services.BuildServiceProvider();
        var strategies = provider.GetServices<IMultiTenantStrategy>().ToArray();

        // Pass condition: at least one stub strategy was appended to the existing chain.
        // (ClaimStrategy registers as a strategy; with the stub appended the count must be >= 2.)
        strategies.Length.ShouldBeGreaterThanOrEqualTo(2);
        strategies.OfType<WorkspaceSlugStrategyStub>().Count().ShouldBe(1);
    }

    /// <summary>
    /// Spike test 2: same pattern for <c>IMultiTenantStore&lt;AppTenantInfo&gt;</c>. Append a stub
    /// store AFTER the builder and confirm it joins the existing
    /// <c>EFCoreStore&lt;TenantDbContext, AppTenantInfo&gt;</c> in the resolved collection.
    /// </summary>
    [Fact]
    public void TryAddEnumerable_Store_AppendedAfterBuilder_IsResolvable()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<TenantDbContext>(opts => opts.UseInMemoryDatabase($"ws-spike-{Guid.NewGuid()}"));

        services.AddMultiTenant<AppTenantInfo>()
            .WithClaimStrategy(ClaimConstants.Tenant)
            .WithStore<EFCoreStore<TenantDbContext, AppTenantInfo>>(ServiceLifetime.Scoped);

        // === The behaviour under test: external module appends a store outside the builder. ===
        services.TryAddEnumerable(ServiceDescriptor.Scoped<
            IMultiTenantStore<AppTenantInfo>, WorkspaceTenantStoreStub>());

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var stores = scope.ServiceProvider.GetServices<IMultiTenantStore<AppTenantInfo>>().ToArray();

        // Pass condition: at least two stores (EFCoreStore + stub).
        stores.Length.ShouldBeGreaterThanOrEqualTo(2);
        stores.OfType<WorkspaceTenantStoreStub>().Count().ShouldBe(1);
    }

    // ─────────────────────────────────────────────────────────────────────────────────
    // Minimal stubs (private nested). The real WorkspaceSlugStrategy / WorkspaceTenantStore
    // are implemented in plan 02-02 once the spike answers Q1.
    // ─────────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Stub slug strategy — returns a constant identifier so the Finbuckle chain has something to
    /// process. The real implementation will read <c>HttpContext.GetRouteValue("slug")</c>.
    /// Implements the <b>non-generic</b> <c>IMultiTenantStrategy</c> (Finbuckle 10.1.x has no
    /// <c>IMultiTenantStrategy&lt;T&gt;</c>).
    /// </summary>
    private sealed class WorkspaceSlugStrategyStub : IMultiTenantStrategy
    {
        // Strategies are evaluated by the Finbuckle resolver; Priority affects ordering.
        // 0 = "I want to run early" (the real slug strategy should out-prioritise claim/header).
        public int Priority => 0;

        public Task<string?> GetIdentifierAsync(object context)
        {
            // Real impl: read HttpContext.GetRouteValue("slug"). For the spike we return a stub
            // identifier to prove only that the registration survives DI resolution.
            return Task.FromResult<string?>("acme-stub");
        }
    }

    /// <summary>
    /// Stub tenant store — returns a tenant for any identifier. The real implementation will query
    /// <c>WorkspaceDbContext</c> for a workspace with the matching slug (D-01).
    /// </summary>
    private sealed class WorkspaceTenantStoreStub : IMultiTenantStore<AppTenantInfo>
    {
        private static readonly AppTenantInfo StubTenant =
            new("00000000-0000-0000-0000-0000000000AC", "acme-stub", "Acme");

        public Task<bool> AddAsync(AppTenantInfo tenantInfo) => Task.FromResult(true);
        public Task<bool> RemoveAsync(string identifier) => Task.FromResult(true);
        public Task<bool> UpdateAsync(AppTenantInfo tenantInfo) => Task.FromResult(true);
        public Task<AppTenantInfo?> GetByIdentifierAsync(string identifier) =>
            Task.FromResult<AppTenantInfo?>(StubTenant);
        public Task<AppTenantInfo?> GetAsync(string id) =>
            Task.FromResult<AppTenantInfo?>(StubTenant);
        public Task<IEnumerable<AppTenantInfo>> GetAllAsync() =>
            Task.FromResult<IEnumerable<AppTenantInfo>>(new[] { StubTenant });
        public Task<IEnumerable<AppTenantInfo>> GetAllAsync(int take, int skip) =>
            Task.FromResult<IEnumerable<AppTenantInfo>>(new[] { StubTenant });
    }
}
