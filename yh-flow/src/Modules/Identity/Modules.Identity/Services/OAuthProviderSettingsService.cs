using YH.Modules.Identity.Contracts.DTOs;
using YH.Modules.Identity.Contracts.Services;
using YH.Modules.Identity.Data;
using YH.Modules.Identity.Domain;
using YH.Modules.Identity.Features.v1.OAuth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace YH.Modules.Identity.Services;

public sealed class OAuthProviderSettingsService : IOAuthProviderSettingsService
{
    private readonly IdentityDbContext _dbContext;
    private readonly OAuthProviderRegistry _registry;
    private readonly ILogger<OAuthProviderSettingsService> _logger;

    public OAuthProviderSettingsService(
        IdentityDbContext dbContext,
        OAuthProviderRegistry registry,
        ILogger<OAuthProviderSettingsService> logger)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(logger);

        _dbContext = dbContext;
        _registry = registry;
        _logger = logger;
    }

    public async Task<IReadOnlyList<OAuthProviderSettingsDto>> GetAllAsync(CancellationToken ct)
    {
        var settings = await _dbContext.OAuthProviderSettings
            .AsNoTracking()
            .OrderBy(x => x.ProviderName)
            .Select(MapToDtoExpression)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return settings;
    }

    public async Task<IReadOnlyList<OAuthProviderSettingsDto>> GetAllEnabledAsync(CancellationToken ct)
    {
        var settings = await _dbContext.OAuthProviderSettings
            .AsNoTracking()
            .Where(x => x.Enabled)
            .OrderBy(x => x.ProviderName)
            .Select(MapToDtoExpression)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return settings;
    }

    public async Task<OAuthProviderSettingsDto?> GetByProviderNameAsync(string providerName, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(providerName);

        var normalizedProviderName = providerName.ToUpperInvariant();
        var settings = await _dbContext.OAuthProviderSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProviderName.ToUpper() == normalizedProviderName, ct)
            .ConfigureAwait(false);

        return settings is null ? null : MapToDto(settings);
    }

    public async Task<OAuthProviderSettingsDto> CreateAsync(
        string providerName,
        string clientId,
        string clientSecret,
        string callbackUrl,
        bool autoCreateAccount,
        string? scope,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(providerName);
        ArgumentNullException.ThrowIfNull(clientId);
        ArgumentNullException.ThrowIfNull(clientSecret);
        ArgumentNullException.ThrowIfNull(callbackUrl);

        var existing = await _dbContext.OAuthProviderSettings
            .AnyAsync(x => x.ProviderName == providerName, ct)
            .ConfigureAwait(false);

        if (existing)
        {
            throw new InvalidOperationException($"OAuth provider '{providerName}' already exists.");
        }

        var settings = OAuthProviderSettings.Create(providerName, clientId, clientSecret, callbackUrl, autoCreateAccount, scope);
        _dbContext.OAuthProviderSettings.Add(settings);
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        await _registry.RefreshCacheAsync(ct).ConfigureAwait(false);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Created OAuth provider settings for {ProviderName}", providerName);
        }

        return MapToDto(settings);
    }

    public async Task<OAuthProviderSettingsDto> UpdateAsync(
        Guid id,
        string clientId,
        string clientSecret,
        string callbackUrl,
        bool autoCreateAccount,
        string? scope,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(clientId);
        ArgumentNullException.ThrowIfNull(clientSecret);
        ArgumentNullException.ThrowIfNull(callbackUrl);

        var settings = await _dbContext.OAuthProviderSettings
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"OAuth provider settings '{id}' were not found.");

        settings.Update(clientId, clientSecret, callbackUrl, autoCreateAccount, scope);
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        await _registry.RefreshCacheAsync(ct).ConfigureAwait(false);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Updated OAuth provider settings for {ProviderName}", settings.ProviderName);
        }

        return MapToDto(settings);
    }

    public async Task ToggleEnabledAsync(Guid id, bool enabled, CancellationToken ct)
    {
        var settings = await _dbContext.OAuthProviderSettings
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"OAuth provider settings '{id}' were not found.");

        settings.ToggleEnabled(enabled);
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        await _registry.RefreshCacheAsync(ct).ConfigureAwait(false);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Set OAuth provider {ProviderName} enabled={Enabled}", settings.ProviderName, enabled);
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var settings = await _dbContext.OAuthProviderSettings
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"OAuth provider settings '{id}' were not found.");

        _dbContext.OAuthProviderSettings.Remove(settings);
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        await _registry.RefreshCacheAsync(ct).ConfigureAwait(false);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Deleted OAuth provider settings for {ProviderName}", settings.ProviderName);
        }
    }

    private static OAuthProviderSettingsDto MapToDto(OAuthProviderSettings settings)
    {
        return new OAuthProviderSettingsDto(
            settings.Id,
            settings.ProviderName,
            settings.ClientId,
            settings.CallbackUrl,
            settings.Enabled,
            settings.AutoCreateAccount,
            settings.Scope);
    }

    private static readonly System.Linq.Expressions.Expression<Func<OAuthProviderSettings, OAuthProviderSettingsDto>> MapToDtoExpression
        = settings => new OAuthProviderSettingsDto(
            settings.Id,
            settings.ProviderName,
            settings.ClientId,
            settings.CallbackUrl,
            settings.Enabled,
            settings.AutoCreateAccount,
            settings.Scope);
}
