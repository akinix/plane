using YH.Modules.Identity.Contracts.DTOs;

namespace YH.Modules.Identity.Contracts.Services;

/// <summary>
/// Cross-module user identity resolution contract (CONTEXT D-04 / D-05).
/// Workspace and other downstream modules call this service to resolve user details by id without
/// creating a cross-module EF Core foreign key. Implementations live in <c>YH.Modules.Identity.Services</c>
/// (see <c>UserIdentityService</c>, scheduled for Phase 2 plan 02-05) and MUST satisfy:
/// <list type="bullet">
///   <item>Single SQL batch via <c>Where(u => userIds.Contains(u.Id)).AsNoTracking()</c> — avoids N+1 (RESEARCH Pitfall 3).</item>
///   <item>Select projection into <see cref="UserSummary"/> only — never materialize full <c>FshUser</c> (T-02-02).</item>
///   <item>Exclude <c>PasswordHash</c>/<c>SecurityStamp</c>/<c>ConcurrencyStamp</c> from the projection.</item>
/// </list>
/// This contract is ADDITIVE: existing <see cref="IUserService"/> surface is unchanged.
/// </summary>
public interface IUserIdentityService
{
    /// <summary>
    /// Batch-resolve user summaries for the given ids. Unknown ids are omitted from the result
    /// (caller treats absent keys as "user not found"). The returned dictionary is keyed by user id.
    /// </summary>
    /// <param name="userIds">User ids to resolve. Implementations should dedupe before querying.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only map of <c>userId -> UserSummary</c>; never null (may be empty).</returns>
    Task<IReadOnlyDictionary<Guid, UserSummary>> GetUsersByIdsAsync(
        IEnumerable<Guid> userIds,
        CancellationToken ct);
}
