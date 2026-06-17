using Mediator;
using YH.Modules.Identity.Contracts.DTOs;

namespace YH.Modules.Identity.Contracts.v1.OAuth;

/// <summary>
/// Command to create a new OAuth provider configuration.
/// </summary>
public sealed record CreateOAuthProviderCommand(
    string ProviderName,
    string ClientId,
    string ClientSecret,
    string CallbackUrl,
    bool AutoCreateAccount,
    string? Scope) : ICommand<OAuthProviderSettingsDto>;

/// <summary>
/// Command to update an existing OAuth provider configuration.
/// </summary>
public sealed record UpdateOAuthProviderCommand(
    Guid Id,
    string ClientId,
    string ClientSecret,
    string CallbackUrl,
    bool AutoCreateAccount,
    string? Scope) : ICommand<OAuthProviderSettingsDto>;

/// <summary>
/// Command to toggle an OAuth provider's enabled/disabled state.
/// </summary>
public sealed record ToggleOAuthProviderCommand(Guid Id, bool Enabled) : ICommand<bool>;

/// <summary>
/// Command to delete an OAuth provider configuration.
/// </summary>
public sealed record DeleteOAuthProviderCommand(Guid Id) : ICommand<bool>;
