using YH.Framework.Core.Domain;

namespace YH.Modules.Identity.Domain;

/// <summary>
/// Stores OAuth provider configuration (client credentials, callback URL, etc.)
/// in the database for dynamic management. Implements <see cref="IGlobalEntity"/>
/// because OAuth providers are shared across all tenants.
/// </summary>
public class OAuthProviderSettings : IGlobalEntity
{
    public Guid Id { get; private set; }
    public string ProviderName { get; private set; } = default!;
    public string ClientId { get; private set; } = default!;
    public string ClientSecret { get; private set; } = default!;
    public string CallbackUrl { get; private set; } = default!;
    public bool Enabled { get; private set; }
    public bool AutoCreateAccount { get; private set; }
    public string? Scope { get; private set; }

    private OAuthProviderSettings() { } // EF Core

    /// <summary>
    /// Factory method to create new OAuth provider settings.
    /// Default state is Enabled = false (must be explicitly enabled by admin).
    /// </summary>
    public static OAuthProviderSettings Create(
        string providerName,
        string clientId,
        string clientSecret,
        string callbackUrl,
        bool autoCreateAccount,
        string? scope)
    {
        return new OAuthProviderSettings
        {
            Id = Guid.NewGuid(),
            ProviderName = providerName,
            ClientId = clientId,
            ClientSecret = clientSecret,
            CallbackUrl = callbackUrl,
            AutoCreateAccount = autoCreateAccount,
            Scope = scope,
            Enabled = false, // default disabled, admin must enable
        };
    }

    /// <summary>
    /// Toggles the enabled/disabled state of this provider.
    /// </summary>
    public void ToggleEnabled(bool enabled)
    {
        Enabled = enabled;
    }

    /// <summary>
    /// Updates mutable configuration fields. ProviderName cannot be changed.
    /// </summary>
    public void Update(
        string clientId,
        string clientSecret,
        string callbackUrl,
        bool autoCreateAccount,
        string? scope)
    {
        ClientId = clientId;
        ClientSecret = clientSecret;
        CallbackUrl = callbackUrl;
        AutoCreateAccount = autoCreateAccount;
        Scope = scope;
    }
}
