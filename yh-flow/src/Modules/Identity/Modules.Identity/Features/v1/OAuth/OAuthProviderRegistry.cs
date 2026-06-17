using System.Collections.Concurrent;
using YH.Modules.Identity.Contracts.DTOs;
using YH.Modules.Identity.Contracts.Services;
using Microsoft.Extensions.DependencyInjection;

namespace YH.Modules.Identity.Features.v1.OAuth;

public class OAuthProviderRegistry
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private ConcurrentDictionary<string, OAuthProviderSettingsDto> _cachedSettings = new(StringComparer.OrdinalIgnoreCase);

    public OAuthProviderRegistry(IServiceScopeFactory serviceScopeFactory)
    {
        ArgumentNullException.ThrowIfNull(serviceScopeFactory);
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task InitializeAsync(CancellationToken ct = default)
        => RefreshCacheAsync(ct);

    public virtual Task RefreshCacheAsync(CancellationToken ct = default)
    {
        return RefreshCacheInternalAsync(ct);
    }

    public virtual IOAuthProvider? GetProvider(string providerName)
    {
        ArgumentNullException.ThrowIfNull(providerName);

        _cachedSettings.TryGetValue(providerName, out _);
        return null;
    }

    public IReadOnlyCollection<string> GetAvailableProviders()
    {
        return _cachedSettings.Keys.OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public OAuthProviderSettingsDto? GetSettings(string providerName)
    {
        ArgumentNullException.ThrowIfNull(providerName);

        return _cachedSettings.TryGetValue(providerName, out var settings)
            ? settings
            : null;
    }

    private async Task RefreshCacheInternalAsync(CancellationToken ct)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var settingsService = scope.ServiceProvider.GetRequiredService<IOAuthProviderSettingsService>();
        var enabledSettings = await settingsService.GetAllEnabledAsync(ct).ConfigureAwait(false);

        var nextCache = new ConcurrentDictionary<string, OAuthProviderSettingsDto>(StringComparer.OrdinalIgnoreCase);
        foreach (var settings in enabledSettings)
        {
            nextCache[settings.ProviderName] = settings;
        }

        _cachedSettings = nextCache;
    }
}
