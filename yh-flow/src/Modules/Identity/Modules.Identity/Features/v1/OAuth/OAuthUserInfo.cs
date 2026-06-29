namespace YH.Modules.Identity.Features.v1.OAuth;

public sealed record OAuthUserInfo(
    string ProviderName,
    string ProviderId,
    string Email,
    string? Avatar,
    string? FirstName,
    string? LastName);
