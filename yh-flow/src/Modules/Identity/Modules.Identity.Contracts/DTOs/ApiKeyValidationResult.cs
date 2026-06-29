namespace YH.Modules.Identity.Contracts.DTOs;

/// <summary>
/// Result of validating an API key. Contains the owning user's identity
/// and their resolved permissions for the current tenant.
/// </summary>
public sealed record ApiKeyValidationResult(
    string UserId,
    string Email,
    string TenantId,
    IReadOnlyList<string> Permissions,
    Guid TokenId);
