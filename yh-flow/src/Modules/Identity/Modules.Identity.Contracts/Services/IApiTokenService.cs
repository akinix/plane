using YH.Modules.Identity.Contracts.DTOs;

namespace YH.Modules.Identity.Contracts.Services;

/// <summary>
/// Service for managing API tokens (CRUD) and validating raw API keys.
/// </summary>
public interface IApiTokenService
{
    /// <summary>
    /// Validates a raw API key (hashes it, looks it up, checks expiry/active status).
    /// Returns the owning user's identity info if valid, or null if invalid/expired/revoked.
    /// </summary>
    Task<ApiKeyValidationResult?> ValidateAndGetOwnerAsync(string rawKey, CancellationToken ct);

    /// <summary>
    /// Creates a new API token for the specified user. Returns the DTO (never the plaintext key).
    /// The plaintext key is only available in the creation response from the Feature handler.
    /// </summary>
    Task<APITokenDto> CreateAsync(string name, string userId, string tenantId, DateTime? expiredAt, CancellationToken ct);

    /// <summary>
    /// Lists all API tokens for a given user.
    /// </summary>
    Task<IReadOnlyList<APITokenDto>> ListForUserAsync(string userId, CancellationToken ct);

    /// <summary>
    /// Revokes an API token. Only the token owner can revoke their own tokens.
    /// </summary>
    Task RevokeAsync(Guid tokenId, string userId, CancellationToken ct);
}
