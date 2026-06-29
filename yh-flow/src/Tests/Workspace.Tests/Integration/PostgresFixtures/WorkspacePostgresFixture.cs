using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;
using Testcontainers.PostgreSql;
using YH.Framework.Persistence.Context;
using YH.Framework.Shared.Multitenancy;
using YH.Framework.Shared.Persistence;
using YH.Modules.Workspace.Data;
using YH.Modules.Workspace.Domain;

namespace YH.Tests.Workspace.Integration.PostgresFixtures;

/// <summary>
/// Testcontainers-driven PostgreSQL fixture for the Workspace module (Wave 6 / plan 02-07).
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this fixture exists (CR-01/02/03 single source of truth — <c>02-REVIEW.md §Notes on the
/// 02-06 closeout tests</c>):</b> the InMemory EF Core provider does NOT apply Finbuckle's
/// <c>AdjustUniqueIndexes</c> pipeline nor the auto-applied <c>TenantId</c> query filter
/// (<c>MembershipMiddlewareTests.cs:43-49</c> + <c>TenantIsolationTests.cs:34-52</c> self-document
/// this). Three CRITICAL tenant-scoping defects (CR-01/02/03) are therefore structurally
/// uncatchable by the existing 96/96 InMemory suite. This fixture spins up a real
/// <c>postgres:17-alpine</c> container, runs the production <c>InitialWorkspace</c> migration
/// (so the composite unique index <c>IX_WorkspaceMembers_Tenant_User</c> + shadow
/// <c>TenantId</c> column exist on real PG), and lets cross-tenant tests assert what InMemory
/// cannot.
/// </para>
/// <para>
/// <b>Reused by 02-08:</b> the accept-invitation cross-tenant test (CR-01/CR-03 fix) drives
/// <see cref="CreateContextForTenant"/> + <see cref="FinbuckleTestTenantScope"/> to seed invitation
/// rows in workspace A and accept them while the DbContext is scoped to a different tenant.
/// </para>
/// <para>
/// <b>Finbuckle DI wiring (for 02-08 plan reference):</b> this fixture registers a singleton
/// <see cref="AsyncLocalMultiTenantContextAccessor{AppTenantInfo}"/> (implements both
/// <c>IMultiTenantContextAccessor&lt;AppTenantInfo&gt;</c> and <c>IMultiTenantContextSetter</c>).
/// Tests obtain the setter via <c>ServiceProvider.GetRequiredService&lt;IMultiTenantContextSetter&gt;()</c>
/// — the same DI resolution path as <c>FshJobActivator.cs:40</c> and
/// <c>FshWebApplicationFactory.cs:277</c>.
/// </para>
/// </remarks>
public sealed class WorkspacePostgresFixture : IAsyncLifetime, IDisposable
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("workspace_tests")
        .WithUsername("postgres")
        .WithPassword("integration_test_pwd")
        .WithAutoRemove(true)
        .WithCleanUp(true)
        .Build();

    private ServiceProvider? _rootServiceProvider;
    private bool _disposed;

    /// <summary>
    /// Root tenant id used to seed workspaces (which are themselves tenants). Mirrors
    /// <c>FshWebApplicationFactory.cs:263-268</c> but uses the lightweight 3-arg
    /// <see cref="AppTenantInfo"/> constructor (no ConnectionString — connection is supplied via
    /// DbContextOptions, not via TenantInfo).
    /// </summary>
    public static readonly AppTenantInfo RootTenant = new(
        id: MultitenancyConstants.Root.Id,
        identifier: MultitenancyConstants.Root.Id,
        name: MultitenancyConstants.Root.Name);

    /// <summary>Postgres connection string exposed by the container (host port mapped lazily).</summary>
    public string ConnectionString => _postgres.GetConnectionString();

    /// <summary>Root DI container — resolves <see cref="IMultiTenantContextSetter"/> for test scopes.</summary>
    public ServiceProvider Services => _rootServiceProvider ?? throw new InvalidOperationException("Fixture not initialized.");

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        _rootServiceProvider = BuildServiceProvider();

        // Apply production migration on real PG so AdjustUniqueIndexes actually widen the composite
        // unique indexes to (TenantId, UserId) — the InMemory provider never runs this pipeline.
        await using (var ctx = CreateContextForTenant(RootTenant))
        {
            await ctx.Database.MigrateAsync();
        }
    }

    public async Task DisposeAsync()
    {
        if (_rootServiceProvider is not null)
        {
            await _rootServiceProvider.DisposeAsync();
        }
        await _postgres.DisposeAsync();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        DisposeAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Builds a scoped <see cref="WorkspaceDbContext"/> whose Finbuckle
    /// <c>MultiTenantContext.TenantInfo</c> is set to <paramref name="tenant"/>. Finbuckle's
    /// <c>AdjustUniqueIndexes</c> + <c>ApplyTenantIsolationByDefault</c> therefore run against
    /// real PG schema with the correct tenant resolution, and the auto-applied tenant query
    /// filter activates — letting cross-tenant tests assert what InMemory cannot.
    /// </summary>
    /// <param name="tenant">Tenant to scope the new DbContext to. Use <see cref="RootTenant"/> for global aggregates.</param>
    /// <returns>A tracked <see cref="WorkspaceDbContext"/>; caller is responsible for disposing.</returns>
    public WorkspaceDbContext CreateContextForTenant(AppTenantInfo tenant)
    {
        if (_rootServiceProvider is null)
        {
            throw new InvalidOperationException("Fixture not initialized — call InitializeAsync first.");
        }

        // Pull the shared AsyncLocal accessor from the root container and stamp the requested tenant
        // onto it. Each CreateContextForTenant call therefore observes the correct tenant on its
        // DbContext construction. Tests that need to flip tenants mid-test should use
        // FinbuckleTestTenantScope instead of calling CreateContextForTenant again.
        var setter = _rootServiceProvider.GetRequiredService<IMultiTenantContextSetter>();
        setter.MultiTenantContext = new MultiTenantContext<AppTenantInfo>(tenant);

        var options = new DbContextOptionsBuilder<WorkspaceDbContext>()
            .UseNpgsql(_postgres.GetConnectionString(), sql => sql.MigrationsAssembly("YH.Flow.Migrations.PostgreSQL"))
            .UseApplicationServiceProvider(_rootServiceProvider)
            .Options;

        var databaseOptions = _rootServiceProvider.GetRequiredService<IOptions<DatabaseOptions>>();
        var hostEnv = _rootServiceProvider.GetRequiredService<IHostEnvironment>();
        return new WorkspaceDbContext(
            _rootServiceProvider.GetRequiredService<IMultiTenantContextAccessor<AppTenantInfo>>(),
            options,
            databaseOptions,
            hostEnv);
    }

    /// <summary>Builds the minimal DI container required by <see cref="WorkspaceDbContext"/>.</summary>
    private ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.Configure<DatabaseOptions>(options =>
        {
            options.Provider = "POSTGRESQL";
            options.ConnectionString = _postgres.GetConnectionString();
            options.MigrationsAssembly = "YH.Flow.Migrations.PostgreSQL";
        });

        // IHostEnvironment — BaseDbContext.OnConfiguring only reads environment.IsDevelopment()
        // when a tenant ConnectionString is set; we never set one (connection comes via options builder),
        // so the mock is never consulted on the hot path. It MUST exist because the WorkspaceDbContext
        // constructor parameter is non-nullable. NSubstitute mock matches the pattern in
        // CreateWorkspaceTests.cs:138 and the rest of the InMemory suite.
        var hostEnv = Substitute.For<IHostEnvironment>();
        hostEnv.EnvironmentName.Returns(Environments.Development);
        services.AddSingleton(hostEnv);

        // Finbuckle accessor — single instance shared across all scopes so CreateContextForTenant and
        // FinbuckleTestTenantScope mutate the same AsyncLocal slot (matches FshWebApplicationFactory's
        // production DI semantics).
        services.AddSingleton<AsyncLocalMultiTenantContextAccessor<AppTenantInfo>>();
        services.AddSingleton<IMultiTenantContextAccessor<AppTenantInfo>>(sp => sp.GetRequiredService<AsyncLocalMultiTenantContextAccessor<AppTenantInfo>>());
        services.AddSingleton<IMultiTenantContextAccessor>(sp => sp.GetRequiredService<AsyncLocalMultiTenantContextAccessor<AppTenantInfo>>());
        services.AddSingleton<IMultiTenantContextSetter>(sp => sp.GetRequiredService<AsyncLocalMultiTenantContextAccessor<AppTenantInfo>>());

        return services.BuildServiceProvider(validateScopes: false);
    }
}
