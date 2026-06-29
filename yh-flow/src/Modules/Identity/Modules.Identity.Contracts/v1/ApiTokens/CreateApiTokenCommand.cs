using Mediator;
using YH.Modules.Identity.Contracts.DTOs;

namespace YH.Modules.Identity.Contracts.v1.ApiTokens;

public sealed record CreateApiTokenCommand(string Name, DateTime? ExpiredAt) : ICommand<ApiTokenCreateResult>;

/// <summary>
/// Creation response that includes the plaintext token (only available once at creation).
/// </summary>
public sealed record ApiTokenCreateResult(
    Guid Id,
    string Name,
    string Prefix,
    string Token,
    DateTime? ExpiredAt,
    DateTime CreatedAt);
