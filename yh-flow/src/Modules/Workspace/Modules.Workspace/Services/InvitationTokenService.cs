using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using YH.Modules.Workspace.Configuration;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Data;
using YH.Modules.Workspace.Domain;

namespace YH.Modules.Workspace.Services;

/// <summary>
/// <see cref="IInvitationTokenService"/> implementation — CSPRNG token + SHA-256 hash storage
/// (CONTEXT D-12; threat T-2-token [BLOCKING], T-2-ttl, T-2-tokenleak).
/// </summary>
/// <remarks>
/// <para>
/// Mirrors the Phase 1 <c>ApiTokenService.cs:175-187</c> crypto pattern: CSPRNG bytes → hex
/// lowercase raw → SHA-256 hash of the UTF-8 raw bytes. The raw token is returned ONCE at
/// create time and is NEVER persisted; the database only ever sees <see cref="WorkspaceInvitation.TokenHash"/>.
/// </para>
/// <para>
/// <b>TTL source (T-2-ttl):</b> the default TTL is read from <see cref="WorkspaceTokenOptions"/>
/// (<c>Workspace:InvitationTokenTtlDays</c>, default 7 days). <see cref="CreateAsync"/>'s
/// <c>ttlDays</c> parameter overrides the default per-call (handlers usually pass the option value).
/// </para>
/// </remarks>
public sealed class InvitationTokenService : IInvitationTokenService
{
    /// <summary>Number of random bytes per raw token (32 → 256-bit token strength).</summary>
    public const int TokenByteLength = 32;

    /// <summary>
    /// Length of the raw token's hex form = <see cref="TokenByteLength"/> × 2 = 64 lowercase
    /// hex chars. Matches <c>ApiTokenService</c>'s 32-byte pattern.
    /// </summary>
    public const int TokenHexLength = TokenByteLength * 2;

    private readonly WorkspaceDbContext _db;
    private readonly WorkspaceTokenOptions _options;

    public InvitationTokenService(
        WorkspaceDbContext db,
        IOptions<WorkspaceTokenOptions> options)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
    }

    /// <inheritdoc />
    public static (string RawToken, string Hash) GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(TokenByteLength);
        // CA1308 (ToLowerInvariant) suppressed: D-12 mandates lowercase hex for hash storage
        // consistency (SHA-256 hex is canonicalised lowercase in ApiTokenService precedent).
#pragma warning disable CA1308
        var raw = Convert.ToHexString(bytes).ToLowerInvariant();
#pragma warning restore CA1308
        return (raw, HashToken(raw));
    }

    /// <inheritdoc />
    public static string HashToken(string rawToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            throw new ArgumentException("Raw token is required.", nameof(rawToken));
        }

        var bytes = Encoding.UTF8.GetBytes(rawToken);
        var hashBytes = SHA256.HashData(bytes);
#pragma warning disable CA1308
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
#pragma warning restore CA1308
    }

    // Instance wrappers so callers can inject the service and use either surface.
    (string RawToken, string Hash) IInvitationTokenService.GenerateToken() => GenerateToken();
    string IInvitationTokenService.HashToken(string rawToken) => HashToken(rawToken);

    /// <inheritdoc />
    public async Task<(WorkspaceInvitation Invitation, string RawToken)> CreateAsync(
        Guid workspaceId,
        string email,
        WorkspaceRole role,
        int ttlDays,
        string? message,
        CancellationToken cancellationToken)
    {
        if (workspaceId == Guid.Empty)
        {
            throw new ArgumentException("Workspace id is required.", nameof(workspaceId));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        // Resolve TTL — caller override wins; fall back to options default; never fall below 1.
        var effectiveTtl = ttlDays > 0
            ? ttlDays
            : Math.Max(1, _options.InvitationTokenTtlDays);

        var (rawToken, hash) = GenerateToken();
        // D-12 mandates lowercased emails so a case-difference in the invitee address cannot
        // produce duplicate invitations for the same mailbox.
#pragma warning disable CA1308
        var normalizedEmail = email.Trim().ToLowerInvariant();
#pragma warning restore CA1308
        var invitation = WorkspaceInvitation.Create(
            workspaceId: workspaceId,
            email: normalizedEmail,
            tokenHash: hash,
            role: (int)role,
            ttlDays: effectiveTtl,
            message: message);

        _db.Invitations.Add(invitation);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (invitation, rawToken);
    }

    /// <inheritdoc />
    public async Task<WorkspaceInvitation?> ValidateAsync(string rawToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return null;
        }

        var hash = HashToken(rawToken);
        // CR-01 (02-REVIEW.md): hash 全局唯一、租户无关查找；顶层 accept 端点 DbContext 作用域 ≠
        // 邀请所属 workspace，必须 IgnoreQueryFilters 才能找到邀请。TokenHash 是 256-bit
        // CSPRNG 的 SHA-256 hex（不可枚举），与 T-2-token 威胁登记一致。
        var invitation = await _db.Invitations
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.TokenHash == hash, cancellationToken)
            .ConfigureAwait(false);

        // IsValid: not accepted, not responded-to, not expired (delegates to entity state machine).
        return invitation is { IsValid: true } ? invitation : null;
    }
}
