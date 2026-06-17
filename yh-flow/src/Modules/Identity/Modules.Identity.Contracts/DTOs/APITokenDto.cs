namespace YH.Modules.Identity.Contracts.DTOs;

public sealed record APITokenDto(
    Guid Id,
    string Name,
    string Prefix,
    string UserId,
    string TenantId,
    DateTime? ExpiredAt,
    bool IsActive,
    DateTime? LastUsed,
    DateTime CreatedAt);
