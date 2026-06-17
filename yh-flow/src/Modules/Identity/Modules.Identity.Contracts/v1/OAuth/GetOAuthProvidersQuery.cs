using Mediator;
using YH.Modules.Identity.Contracts.DTOs;

namespace YH.Modules.Identity.Contracts.v1.OAuth;

/// <summary>
/// Query to list all OAuth provider settings.
/// </summary>
public sealed record GetOAuthProvidersQuery : IQuery<IReadOnlyList<OAuthProviderSettingsDto>>;
