using Finbuckle.MultiTenant.Abstractions;
using YH.Framework.Shared.Multitenancy;
using YH.Framework.Shared.Persistence;
using YH.Modules.Identity.Data;
using YH.Modules.Identity.Domain;
using YH.Modules.Identity.Contracts.DTOs;
using YH.Modules.Identity.Contracts.Services;
using YH.Modules.Identity.Features.v1.OAuth;
using YH.Modules.Identity.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Identity.Tests.Authorization;

public sealed class OAuthProviderFrameworkTests
{
    [Fact]
    public void IOAuthProvider_Should_Define_RequiredMembers()
    {
        typeof(IOAuthProvider).GetProperty(nameof(IOAuthProvider.ProviderName)).ShouldNotBeNull();
        typeof(IOAuthProvider).GetMethod(nameof(IOAuthProvider.GetAuthUrl)).ShouldNotBeNull();
        typeof(IOAuthProvider).GetMethod(nameof(IOAuthProvider.ExchangeCodeAsync)).ShouldNotBeNull();
    }

    [Fact]
    public async Task GetAvailableProviders_Should_ReturnEmptyList_When_NoSettingsLoaded()
    {
        var settingsService = Substitute.For<IOAuthProviderSettingsService>();
        settingsService.GetAllEnabledAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<OAuthProviderSettingsDto>());
        var registry = CreateRegistry(settingsService);

        await registry.InitializeAsync(CancellationToken.None);

        registry.GetAvailableProviders().ShouldBeEmpty();
    }

    [Fact]
    public void GetProvider_Should_ReturnNull_InPhaseOne()
    {
        var registry = CreateRegistry(Substitute.For<IOAuthProviderSettingsService>());

        registry.GetProvider("github").ShouldBeNull();
    }

    [Fact]
    public async Task InitializeAsync_Should_LoadEnabledSettings_FromService()
    {
        var settings = new[]
        {
            new OAuthProviderSettingsDto(Guid.NewGuid(), "github", "client", "https://example.com/callback", true, true, "user:email")
        };
        var settingsService = Substitute.For<IOAuthProviderSettingsService>();
        settingsService.GetAllEnabledAsync(Arg.Any<CancellationToken>()).Returns(settings);
        var registry = CreateRegistry(settingsService);

        await registry.InitializeAsync(CancellationToken.None);

        registry.GetAvailableProviders().ShouldBe(["github"]);
        registry.GetSettings("github").ShouldBe(settings[0]);
        await settingsService.Received(1).GetAllEnabledAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task OAuthProviderSettingsService_GetAllAsync_Should_ReturnAllSettings()
    {
        await using var db = CreateDbContext();
        db.OAuthProviderSettings.Add(OAuthProviderSettings.Create("github", "client-1", "secret", "https://example.com/github", true, "user"));
        db.OAuthProviderSettings.Add(OAuthProviderSettings.Create("gitlab", "client-2", "secret", "https://example.com/gitlab", false, null));
        await db.SaveChangesAsync();
        var service = CreateSettingsService(db);

        var result = await service.GetAllAsync(CancellationToken.None);

        result.Count.ShouldBe(2);
        result.Select(x => x.ProviderName).ShouldBe(["github", "gitlab"], ignoreOrder: true);
    }

    [Fact]
    public async Task OAuthProviderSettingsService_GetAllEnabledAsync_Should_ReturnOnlyEnabledSettings()
    {
        await using var db = CreateDbContext();
        var enabled = OAuthProviderSettings.Create("github", "client-1", "secret", "https://example.com/github", true, "user");
        enabled.ToggleEnabled(true);
        db.OAuthProviderSettings.Add(enabled);
        db.OAuthProviderSettings.Add(OAuthProviderSettings.Create("gitlab", "client-2", "secret", "https://example.com/gitlab", false, null));
        await db.SaveChangesAsync();
        var service = CreateSettingsService(db);

        var result = await service.GetAllEnabledAsync(CancellationToken.None);

        result.Single().ProviderName.ShouldBe("github");
    }

    [Fact]
    public async Task OAuthProviderSettingsService_GetByProviderNameAsync_Should_ReturnMatchingSettings()
    {
        await using var db = CreateDbContext();
        db.OAuthProviderSettings.Add(OAuthProviderSettings.Create("github", "client-1", "secret", "https://example.com/github", true, "user"));
        await db.SaveChangesAsync();
        var service = CreateSettingsService(db);

        var result = await service.GetByProviderNameAsync("GitHub", CancellationToken.None);

        result.ShouldNotBeNull();
        result.ProviderName.ShouldBe("github");
    }

    private static OAuthProviderRegistry CreateRegistry(IOAuthProviderSettingsService settingsService)
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => settingsService);
        var provider = services.BuildServiceProvider();
        return new OAuthProviderRegistry(provider.GetRequiredService<IServiceScopeFactory>());
    }

    private static OAuthProviderSettingsService CreateSettingsService(IdentityDbContext db)
    {
        var registry = Substitute.For<OAuthProviderRegistry>(Substitute.For<IServiceScopeFactory>());
        registry.RefreshCacheAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        var logger = Substitute.For<ILogger<OAuthProviderSettingsService>>();
        return new OAuthProviderSettingsService(db, registry, logger);
    }

    private static IdentityDbContext CreateDbContext()
    {
        var accessor = Substitute.For<IMultiTenantContextAccessor<AppTenantInfo>>();
        var tenant = new AppTenantInfo(MultitenancyConstants.Root.Id, MultitenancyConstants.Root.Id);
        var context = new MultiTenantContext<AppTenantInfo>(tenant);
        accessor.MultiTenantContext.Returns(context);

        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase($"OAuthProviderFramework_{Guid.NewGuid()}")
            .Options;
        var databaseOptions = Options.Create(new DatabaseOptions { Provider = "inmemory" });
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);

        return new IdentityDbContext(accessor, options, databaseOptions, environment);
    }
}
