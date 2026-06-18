using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.Extensions;
using Finbuckle.MultiTenant.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YH.Framework.Shared.Constants;
using YH.Framework.Shared.Multitenancy;
using YH.Modules.Workspace.MultiTenancy;

namespace YH.Tests.Workspace.Integration;

/// <summary>
/// Integration tests for the Finbuckle slug resolution chain wired in plan 02-02 Task 3
/// (Q1 spike path A — external <c>TryAddEnumerable</c> append) and the
/// <see cref="WorkspaceSlugStrategy"/> ordering invariant (plan 02-03 Task 2).
/// </summary>
/// <remarks>
/// <para>
/// <b>What is under test (D-01):</b>
/// </para>
/// <list type="bullet">
///   <item><see cref="WorkspaceSlugStrategy"/> reads <c>HttpContext.GetRouteValue("slug")</c> and
///   returns the slug as the tenant identifier on workspace-scoped requests.</item>
///   <item>The slug strategy is registered ahead of the Phase 1 claim/header chain
///   (<c>Priority=-100</c>) so a <c>{slug}</c> request resolves to the workspace tenant, not to
///   the JWT claim's tenant. This is the load-bearing threat mitigation for cross-workspace
///   leakage at the resolution layer.</item>
///   <item>The strategy returns null on top-level endpoints (no <c>{slug}</c> route value) so
///   the claim/header chain takes over (Pitfall 2).</item>
/// </list>
/// <para>
/// <b>Q1 path verification (02-01 spike authoritative):</b> this test re-runs the spike-style
/// DI registration (Phase 1 claim strategy + external <c>TryAddEnumerable</c> slug strategy) and
/// confirms the slug strategy survives DI resolution — i.e. it is part of the resolved
/// <c>IEnumerable&lt;IMultiTenantStrategy&gt;</c> collection and ranks first by priority.
/// </para>
/// </remarks>
public sealed class SlugTenantResolveTests
{
    private const string Slug = "acme-resolve-test";

    [Fact]
    public void SlugStrategy_RegistersAheadOfClaimStrategy_ByPriority()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        // Phase 1 baseline chain.
        services.AddMultiTenant<AppTenantInfo>()
            .WithClaimStrategy(ClaimConstants.Tenant);

        // Q1 path A: WorkspaceModule.ConfigureServices appends the slug strategy externally.
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IMultiTenantStrategy, WorkspaceSlugStrategy>());

        using var provider = services.BuildServiceProvider();
        var strategies = provider.GetServices<IMultiTenantStrategy>().ToArray();

        // Both the slug strategy and the Phase 1 claim strategy must be resolvable.
        strategies.OfType<WorkspaceSlugStrategy>().Count().ShouldBe(1);

        // Ordering by priority (ascending — lower = earlier). The slug strategy MUST be ranked
        // ahead of every Phase 1 strategy so a {slug} request resolves to the workspace tenant.
        var ranked = strategies.OrderBy(s => s.Priority).ToArray();
        ranked[0].ShouldBeOfType<WorkspaceSlugStrategy>();
        ((WorkspaceSlugStrategy)ranked[0]).Priority.ShouldBe(-100);
    }

    [Fact]
    public async Task GetIdentifierAsync_SlugRouteValue_ReturnsSlug()
    {
        var strategy = new WorkspaceSlugStrategy(Substitute.For<ILogger<WorkspaceSlugStrategy>>());
        var ctx = new DefaultHttpContext();
        SetRouteValue(ctx, WorkspaceSlugStrategy.SlugRouteKey, Slug);

        var identifier = await strategy.GetIdentifierAsync(ctx);

        identifier.ShouldBe(Slug);
    }

    [Fact]
    public async Task GetIdentifierAsync_TopLevelRoute_ReturnsNull()
    {
        // No {slug} route value set — top-level endpoint like POST /api/v1/workspaces/.
        var strategy = new WorkspaceSlugStrategy(Substitute.For<ILogger<WorkspaceSlugStrategy>>());
        var ctx = new DefaultHttpContext();

        var identifier = await strategy.GetIdentifierAsync(ctx);

        // Pitfall 2: null return → Finbuckle falls back to the Phase 1 claim/header chain.
        identifier.ShouldBeNull();
    }

    [Fact]
    public async Task GetIdentifierAsync_EmptySlug_ReturnsNull()
    {
        var strategy = new WorkspaceSlugStrategy(Substitute.For<ILogger<WorkspaceSlugStrategy>>());
        var ctx = new DefaultHttpContext();
        SetRouteValue(ctx, WorkspaceSlugStrategy.SlugRouteKey, string.Empty);

        var identifier = await strategy.GetIdentifierAsync(ctx);

        // Empty slug is treated as "not workspace-scoped" (Pitfall 2).
        identifier.ShouldBeNull();
    }

    [Fact]
    public async Task GetIdentifierAsync_NonHttpContext_ReturnsNull()
    {
        // Background scope / non-HTTP resolution — slug strategy does not apply.
        var strategy = new WorkspaceSlugStrategy(Substitute.For<ILogger<WorkspaceSlugStrategy>>());

        var identifier = await strategy.GetIdentifierAsync(new object());

        identifier.ShouldBeNull();
    }

    [Fact]
    public async Task EndToEnd_StrategyAndStoreChain_ResolvesSlugToWorkspaceTenant()
    {
        // Direct strategy→store chain test (without the full Finbuckle resolver middleware, which
        // requires the ASP.NET Core pipeline to stamp the accessor). Verifies the load-bearing
        // D-01 invariant: given a {slug} route value, the strategy returns the slug identifier,
        // and the store resolves it to an AppTenantInfo whose Id is the workspace Guid.
        var strategy = new WorkspaceSlugStrategy(Substitute.For<ILogger<WorkspaceSlugStrategy>>());
        var store = new StubTenantStore();

        var ctx = new DefaultHttpContext();
        SetRouteValue(ctx, WorkspaceSlugStrategy.SlugRouteKey, Slug);

        var identifier = await strategy.GetIdentifierAsync(ctx);
        identifier.ShouldBe(Slug);

        var resolved = await store.GetByIdentifierAsync(identifier!);
        resolved.ShouldNotBeNull();
        resolved.Identifier.ShouldBe(Slug);
        // The workspace Guid stamped as AppTenantInfo.Id — WorkspaceMembershipMiddleware parses
        // this back into a Guid to seed ICurrentWorkspaceContext (D-02).
        Guid.TryParse(resolved.Id, out _).ShouldBeTrue(
            "AppTenantInfo.Id must be a Guid-string so the membership middleware can parse it");
        resolved.Id.ShouldBe(StubTenantStore.StubTenantId);
    }

    /// <summary>
    /// Minimal in-memory tenant store that returns a fixed tenant for any identifier. Mirrors the
    /// 02-01 spike stub pattern — sufficient to drive the slug strategy end-to-end. Parameterless
    /// ctor so Finbuckle's <c>WithStore&lt;T&gt;(lifetime)</c> can construct it from DI.
    /// </summary>
    private sealed class StubTenantStore : IMultiTenantStore<AppTenantInfo>
    {
        public const string StubTenantId = "00000000-0000-0000-0000-0000000000a1";

        private static readonly AppTenantInfo Tenant =
            new(StubTenantId, "acme-resolve-test", "Acme Resolve");

        public Task<bool> AddAsync(AppTenantInfo tenantInfo) => Task.FromResult(true);
        public Task<bool> RemoveAsync(string identifier) => Task.FromResult(true);
        public Task<bool> UpdateAsync(AppTenantInfo tenantInfo) => Task.FromResult(true);
        public Task<AppTenantInfo?> GetByIdentifierAsync(string identifier) =>
            Task.FromResult<AppTenantInfo?>(Tenant);
        public Task<AppTenantInfo?> GetAsync(string id) => Task.FromResult<AppTenantInfo?>(Tenant);
        public Task<IEnumerable<AppTenantInfo>> GetAllAsync() =>
            Task.FromResult<IEnumerable<AppTenantInfo>>(new[] { Tenant });
        public Task<IEnumerable<AppTenantInfo>> GetAllAsync(int take, int skip) =>
            Task.FromResult<IEnumerable<AppTenantInfo>>(new[] { Tenant });
    }

    /// <summary>
    /// Sets a route value on the HttpContext via the routing feature — the same surface
    /// <c>HttpContext.GetRouteValue</c> reads from. The AspNetCore Routing package exposes no
    /// public <c>SetRouteValue</c> extension on HttpContext, so we touch the feature directly.
    /// </summary>
    private static void SetRouteValue(HttpContext ctx, string key, object? value)
    {
        var feature = ctx.Features.Get<IRouteValuesFeature>();
        if (feature is null)
        {
            feature = new RouteValuesFeature();
            ctx.Features.Set(feature);
        }
        feature.RouteValues[key] = value;
    }
}
