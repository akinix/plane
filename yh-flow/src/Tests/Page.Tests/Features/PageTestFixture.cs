#pragma warning disable CA1822 // Disable "member does not access instance data" — kept as instance for IClassFixture pattern compatibility

namespace YH.Tests.Page.Features;

/// <summary>
/// Shared test fixture for Page handler tests.
/// Creates isolated InMemory <see cref="PageDbContext"/> instances,
/// matching the pattern established by <c>WorkItemsTestFixture</c>.
/// </summary>
public sealed class PageTestFixture : IDisposable
{
    private static readonly Guid DefaultTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private const string DefaultSlug = "test-workspace";
    private const string DefaultName = "Test Workspace";

    /// <summary>
    /// Creates a fresh InMemory <see cref="PageDbContext"/> with a unique database name
    /// (isolated per call) and a stubbed <see cref="IMultiTenantContextAccessor{AppTenantInfo}"/>
    /// pointing at the default test tenant.
    /// </summary>
    public PageDbContext CreateDbContext()
    {
        var accessor = Substitute.For<IMultiTenantContextAccessor<AppTenantInfo>>();
        var tenant = new AppTenantInfo(DefaultTenantId.ToString(), DefaultSlug, DefaultName);
        var tenantContext = new MultiTenantContext<AppTenantInfo>(tenant);
        accessor.MultiTenantContext.Returns(tenantContext);

        var options = new DbContextOptionsBuilder<PageDbContext>()
            .UseInMemoryDatabase($"page-{Guid.NewGuid():n}")
            .Options;
        var databaseOptions = Options.Create(new DatabaseOptions
        {
            Provider = "inmemory"
        });
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns("Development");

        return new PageDbContext(accessor, options, databaseOptions, environment);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
