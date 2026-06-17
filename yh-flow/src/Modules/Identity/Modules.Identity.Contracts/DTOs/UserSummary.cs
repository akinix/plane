namespace YH.Modules.Identity.Contracts.DTOs;

/// <summary>
/// Lightweight cross-module user summary used by other modules (e.g. Workspace member lists per D-04/D-05)
/// to resolve user details in a single batch call without leaking sensitive fields (no password hash,
/// no security stamp). Slimmed from <see cref="UserDto"/> per CONTEXT §Claude's Discretion (4 fields).
/// Threat T-02-02 (information disclosure) mitigated by explicit field allow-list.
/// </summary>
/// <param name="Id">User identifier. Serialized as string for JSON compatibility with Plane conventions.</param>
/// <param name="DisplayName">Display name (first + last, or username fallback).</param>
/// <param name="Email">Primary email, or null if unavailable.</param>
/// <param name="AvatarUrl">Avatar / profile image URL, or null.</param>
public record UserSummary(Guid Id, string? DisplayName, string? Email, string? AvatarUrl);
