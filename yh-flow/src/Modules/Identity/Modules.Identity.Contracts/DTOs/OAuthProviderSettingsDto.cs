namespace YH.Modules.Identity.Contracts.DTOs;

/// <summary>
/// DTO for OAuth provider settings. Does NOT include ClientSecret (security).
/// </summary>
public sealed record OAuthProviderSettingsDto(
    Guid Id,
    string ProviderName,
    string ClientId,
    string CallbackUrl,
    bool Enabled,
    bool AutoCreateAccount,
    string? Scope);
