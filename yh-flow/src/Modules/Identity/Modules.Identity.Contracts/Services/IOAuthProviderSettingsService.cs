using YH.Modules.Identity.Contracts.DTOs;

namespace YH.Modules.Identity.Contracts.Services;

/// <summary>
/// Service for managing OAuth provider settings (CRUD + enable/disable).
/// </summary>
public interface IOAuthProviderSettingsService
{
    /// <summary>
    /// Gets all OAuth provider settings (both enabled and disabled).
    /// </summary>
    Task<IReadOnlyList<OAuthProviderSettingsDto>> GetAllAsync(CancellationToken ct);

    /// <summary>
    /// Gets only enabled OAuth provider settings.
    /// </summary>
    Task<IReadOnlyList<OAuthProviderSettingsDto>> GetAllEnabledAsync(CancellationToken ct);

    /// <summary>
    /// Gets settings for a specific provider by name (e.g., "github", "gitlab").
    /// Returns null if not found.
    /// </summary>
    Task<OAuthProviderSettingsDto?> GetByProviderNameAsync(string providerName, CancellationToken ct);

    /// <summary>
    /// Creates new OAuth provider settings. Returns the created DTO.
    /// </summary>
    Task<OAuthProviderSettingsDto> CreateAsync(
        string providerName,
        string clientId,
        string clientSecret,
        string callbackUrl,
        bool autoCreateAccount,
        string? scope,
        CancellationToken ct);

    /// <summary>
    /// Updates an existing provider's configuration. Returns the updated DTO.
    /// </summary>
    Task<OAuthProviderSettingsDto> UpdateAsync(
        Guid id,
        string clientId,
        string clientSecret,
        string callbackUrl,
        bool autoCreateAccount,
        string? scope,
        CancellationToken ct);

    /// <summary>
    /// Enables or disables a provider.
    /// </summary>
    Task ToggleEnabledAsync(Guid id, bool enabled, CancellationToken ct);

    /// <summary>
    /// Deletes a provider configuration.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct);
}
