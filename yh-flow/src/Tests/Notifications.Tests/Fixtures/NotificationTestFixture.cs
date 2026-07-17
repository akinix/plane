using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using YH.Framework.Shared.Persistence;
using YH.Framework.Shared.Multitenancy;

namespace Notifications.Tests.Fixtures;

public sealed class NotificationTestFixture
{
    private NotificationTestFixture() { }

    public static NotificationsDbContext CreateDbContext()
    {
        var accessor = Substitute.For<IMultiTenantContextAccessor<AppTenantInfo>>();
        var tenant = new AppTenantInfo("00000000-0000-0000-0000-000000000001", "test-workspace", "Test Workspace");
        var context = new MultiTenantContext<AppTenantInfo>(tenant);
        accessor.MultiTenantContext.Returns(context);

        var options = new DbContextOptionsBuilder<NotificationsDbContext>()
            .UseInMemoryDatabase($"notif-{Guid.NewGuid():n}")
            .Options;
        var databaseOptions = Options.Create(new DatabaseOptions { Provider = "inmemory" });
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns("Development");

        return new NotificationsDbContext(accessor, options, databaseOptions, environment);
    }
}
