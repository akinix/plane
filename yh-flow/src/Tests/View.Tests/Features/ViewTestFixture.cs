#pragma warning disable CA1822, S2325 // Disable "member does not access instance data" — kept as instance for IClassFixture pattern compatibility

using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using YH.Framework.Shared.Persistence;
using YH.Framework.Shared.Multitenancy;

namespace YH.Tests.View.Features;

/// <summary>
/// Shared test fixture for View handler tests.
/// Creates isolated InMemory <see cref="ViewDbContext"/> instances,
/// matching the pattern established by <c>PageTestFixture</c>.
/// </summary>
public sealed class ViewTestFixture : IDisposable
{
    private static readonly Guid DefaultTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private const string DefaultSlug = "test-workspace";
    private const string DefaultName = "Test Workspace";

    /// <summary>
    /// Creates a fresh InMemory <see cref="ViewDbContext"/> with a unique database name
    /// (isolated per call) and a stubbed <see cref="IMultiTenantContextAccessor{AppTenantInfo}"/>
    /// pointing at the default test tenant.
    /// </summary>
    public ViewDbContext CreateDbContext()
    {
        var accessor = Substitute.For<IMultiTenantContextAccessor<AppTenantInfo>>();
        var tenant = new AppTenantInfo(DefaultTenantId.ToString(), DefaultSlug, DefaultName);
        var tenantContext = new MultiTenantContext<AppTenantInfo>(tenant);
        accessor.MultiTenantContext.Returns(tenantContext);

        var options = new DbContextOptionsBuilder<ViewDbContext>()
            .UseInMemoryDatabase($"view-{Guid.NewGuid():n}")
            .Options;
        var databaseOptions = Options.Create(new DatabaseOptions
        {
            Provider = "inmemory"
        });
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns("Development");

        return new ViewDbContext(accessor, options, databaseOptions, environment);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
