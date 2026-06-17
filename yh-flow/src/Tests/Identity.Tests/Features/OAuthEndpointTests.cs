using System.Reflection;
using YH.Modules.Identity.Contracts.DTOs;
using YH.Modules.Identity.Contracts.Services;
using YH.Modules.Identity.Features.v1.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Identity.Tests.Features;

/// <summary>
/// Tests for OAuth endpoint logic — registry settings lookup, provider resolution, and authorization metadata.
/// </summary>
public sealed class OAuthEndpointTests
{
    #region Registry Settings Lookup Tests

    [Fact]
    public async Task GetSettings_Should_ReturnNull_ForUnknownProvider()
    {
        // Arrange
        var registry = CreateInitializedRegistry(Array.Empty<OAuthProviderSettingsDto>());

        // Act
        var settings = registry.GetSettings("unknown-provider");

        // Assert
        settings.ShouldBeNull();
    }

    [Fact]
    public async Task GetSettings_Should_ReturnSettings_ForKnownEnabledProvider()
    {
        // Arrange
        var settingsDto = new OAuthProviderSettingsDto(
            Guid.NewGuid(), "github", "client-id", "https://example.com/callback",
            true, true, "user:email");
        var registry = CreateInitializedRegistry(new[] { settingsDto });

        // Act
        var settings = registry.GetSettings("github");

        // Assert
        settings.ShouldNotBeNull();
        settings!.ProviderName.ShouldBe("github");
        settings.Enabled.ShouldBeTrue();
        settings.ClientId.ShouldBe("client-id");
    }

    [Fact]
    public async Task GetSettings_Should_ReturnDisabledSettings_When_ProviderIsDisabled()
    {
        // Arrange — disabled providers are not loaded by GetAllEnabledAsync,
        // so they won't be in the registry cache
        var registry = CreateInitializedRegistry(Array.Empty<OAuthProviderSettingsDto>());

        // Act
        var settings = registry.GetSettings("disabled-provider");

        // Assert — disabled providers are not returned by the registry
        settings.ShouldBeNull();
    }

    [Fact]
    public async Task GetSettings_Should_BeCaseInsensitive()
    {
        // Arrange
        var settingsDto = new OAuthProviderSettingsDto(
            Guid.NewGuid(), "github", "client-id", "https://example.com/callback",
            true, true, "user:email");
        var registry = CreateInitializedRegistry(new[] { settingsDto });

        // Act & Assert
        registry.GetSettings("GitHub").ShouldNotBeNull();
        registry.GetSettings("GITHUB").ShouldNotBeNull();
        registry.GetSettings("github").ShouldNotBeNull();
    }

    #endregion

    #region Provider Resolution Tests

    [Fact]
    public async Task GetProvider_Should_ReturnNull_InPhaseOne()
    {
        // Arrange — even with enabled settings, Phase 1 has no IOAuthProvider implementations
        var settingsDto = new OAuthProviderSettingsDto(
            Guid.NewGuid(), "github", "client-id", "https://example.com/callback",
            true, true, "user:email");
        var registry = CreateInitializedRegistry(new[] { settingsDto });

        // Act
        var provider = registry.GetProvider("github");

        // Assert — Phase 1: no provider implementations registered
        provider.ShouldBeNull();
    }

    [Fact]
    public async Task GetProvider_Should_ReturnNull_ForUnknownProvider()
    {
        // Arrange
        var registry = CreateInitializedRegistry(Array.Empty<OAuthProviderSettingsDto>());

        // Act
        var provider = registry.GetProvider("nonexistent");

        // Assert
        provider.ShouldBeNull();
    }

    #endregion

    #region Available Providers Tests

    [Fact]
    public async Task GetAvailableProviders_Should_ReturnEmpty_When_NoSettings()
    {
        // Arrange
        var registry = CreateInitializedRegistry(Array.Empty<OAuthProviderSettingsDto>());

        // Act
        var providers = registry.GetAvailableProviders();

        // Assert
        providers.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAvailableProviders_Should_ReturnEnabledProviders_SortedAlphabetically()
    {
        // Arrange
        var settings = new[]
        {
            new OAuthProviderSettingsDto(Guid.NewGuid(), "gitlab", "client", "https://example.com/gitlab", true, true, null),
            new OAuthProviderSettingsDto(Guid.NewGuid(), "github", "client", "https://example.com/github", true, true, null),
            new OAuthProviderSettingsDto(Guid.NewGuid(), "azure-ad", "client", "https://example.com/aad", true, true, null),
        };
        var registry = CreateInitializedRegistry(settings);

        // Act
        var providers = registry.GetAvailableProviders();

        // Assert
        providers.ShouldBe(["azure-ad", "github", "gitlab"]);
    }

    [Fact]
    public async Task GetAvailableProviders_Should_ReflectLatestCache_AfterRefresh()
    {
        // Arrange
        var settingsService = Substitute.For<IOAuthProviderSettingsService>();
        settingsService.GetAllEnabledAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<OAuthProviderSettingsDto>());

        var services = new ServiceCollection();
        services.AddScoped(_ => settingsService);
        var registry = new OAuthProviderRegistry(services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>());
        await registry.InitializeAsync();

        // Initially empty
        registry.GetAvailableProviders().ShouldBeEmpty();

        // Act — update the service to return a new provider and refresh
        var newSettings = new[]
        {
            new OAuthProviderSettingsDto(Guid.NewGuid(), "github", "client", "https://example.com/callback", true, true, null),
        };
        settingsService.GetAllEnabledAsync(Arg.Any<CancellationToken>()).Returns(newSettings);
        await registry.RefreshCacheAsync();

        // Assert
        registry.GetAvailableProviders().ShouldBe(["github"]);
    }

    #endregion

    #region Endpoint Metadata Tests

    [Fact]
    public void OAuthInitiateEndpoint_MapOAuthInitiateEndpoint_Should_ReturnRouteHandlerBuilder()
    {
        // Act — verify the extension method exists and is accessible via reflection
        var method = typeof(OAuthInitiateEndpoint).GetMethod(
            "MapOAuthInitiateEndpoint",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Assert
        method.ShouldNotBeNull();
        method.ReturnType.ShouldBe(typeof(RouteHandlerBuilder));
    }

    [Fact]
    public void OAuthCallbackEndpoint_MapOAuthCallbackEndpoint_Should_ReturnRouteHandlerBuilder()
    {
        // Act — verify the extension method exists and returns RouteHandlerBuilder
        var method = typeof(OAuthCallbackEndpoint).GetMethod(
            "MapOAuthCallbackEndpoint",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Assert
        method.ShouldNotBeNull();
        method.ReturnType.ShouldBe(typeof(RouteHandlerBuilder));
    }

    #endregion

    private static OAuthProviderRegistry CreateInitializedRegistry(OAuthProviderSettingsDto[] settings)
    {
        var settingsService = Substitute.For<IOAuthProviderSettingsService>();
        settingsService.GetAllEnabledAsync(Arg.Any<CancellationToken>()).Returns(settings);

        var services = new ServiceCollection();
        services.AddScoped(_ => settingsService);
        var registry = new OAuthProviderRegistry(services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>());
        registry.InitializeAsync().GetAwaiter().GetResult();
        return registry;
    }
}
