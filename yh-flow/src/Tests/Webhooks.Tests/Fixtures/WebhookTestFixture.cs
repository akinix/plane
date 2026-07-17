using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using YH.Framework.Shared.Persistence;
using YH.Framework.Shared.Multitenancy;
using YH.Modules.Webhooks.Data;

namespace Webhooks.Tests.Fixtures;

public sealed class WebhookTestFixture : IDisposable
{
    private static readonly Guid DefaultTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private const string DefaultSlug = "test-workspace";
    private const string DefaultName = "Test Workspace";

    public static WebhookDbContext CreateDbContext()
    {
        var accessor = Substitute.For<IMultiTenantContextAccessor<AppTenantInfo>>();
        var tenant = new AppTenantInfo(DefaultTenantId.ToString(), DefaultSlug, DefaultName);
        var tenantContext = new MultiTenantContext<AppTenantInfo>(tenant);
        accessor.MultiTenantContext.Returns(tenantContext);

        var options = new DbContextOptionsBuilder<WebhookDbContext>()
            .UseInMemoryDatabase($"wh-{Guid.NewGuid():n}")
            .Options;
        var databaseOptions = Options.Create(new DatabaseOptions
        {
            Provider = "inmemory"
        });
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns("Development");

        return new WebhookDbContext(accessor, options, databaseOptions, environment);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
