namespace YH.Tests.WorkItems.TestData;

/// <summary>
/// Shared test fixture scaffolding for WorkItems module tests.
/// Creates InMemory <see cref="WorkItemsDbContext"/> instances with a stubbed tenant accessor
/// matching the pattern established by <c>WorkspaceTestFixture</c>.
/// </summary>
public sealed class WorkItemsTestFixture : IDisposable
{
    private static readonly Guid DefaultTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private const string DefaultSlug = "test-workspace";
    private const string DefaultName = "Test Workspace";

    /// <summary>Returns a stable root tenant id usable across tests.</summary>
    public static Guid TenantId => DefaultTenantId;

    /// <summary>
    /// Creates a fresh InMemory <see cref="WorkItemsDbContext"/> with a unique database name
    /// (isolated per call) and a stubbed <see cref="IMultiTenantContextAccessor{AppTenantInfo}"/>
    /// pointing at the default test tenant.
    /// </summary>
    public static WorkItemsDbContext CreateInMemoryContext()
    {
        var accessor = Substitute.For<IMultiTenantContextAccessor<AppTenantInfo>>();
        var tenant = new AppTenantInfo(DefaultTenantId.ToString(), DefaultSlug, DefaultName);
        var tenantContext = new MultiTenantContext<AppTenantInfo>(tenant);
        accessor.MultiTenantContext.Returns(tenantContext);

        var options = new DbContextOptionsBuilder<WorkItemsDbContext>()
            .UseInMemoryDatabase($"workitems-{Guid.NewGuid():n}")
            .Options;
        var databaseOptions = Options.Create(new DatabaseOptions { Provider = "inmemory" });
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);

        return new WorkItemsDbContext(accessor, options, databaseOptions, environment);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
