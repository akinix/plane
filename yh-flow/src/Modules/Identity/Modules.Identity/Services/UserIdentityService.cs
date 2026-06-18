using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using YH.Modules.Identity.Contracts.DTOs;
using YH.Modules.Identity.Contracts.Services;
using YH.Modules.Identity.Domain;

namespace YH.Modules.Identity.Services;

/// <summary>
/// Cross-module user identity resolution (CONTEXT D-04 / D-05).
/// </summary>
/// <remarks>
/// <para>
/// <b>N+1 avoidance (RESEARCH Pitfall 3, threat T-2-n1 [BLOCKING]):</b>
/// <see cref="GetUsersByIdsAsync"/> issues a SINGLE SQL batch via
/// <c>Where(u => userIds.Contains(u.Id)).AsNoTracking()</c> — EF Core translates the
/// <c>Contains</c> predicate into a parameterised <c>IN (...)</c>, avoiding the per-member
/// round trip that the Workspace <c>ListMembers</c> handler would otherwise trigger.
/// </para>
/// <para>
/// <b>Projection (T-2-crossmodule mitigation):</b> only the 4
/// <see cref="UserSummary"/> fields are projected server-side via <c>Select</c>; the full
/// <c>FshUser</c> (with <c>PasswordHash</c> / <c>SecurityStamp</c> / <c>ConcurrencyStamp</c>)
/// is NEVER materialised. Unknown ids are silently omitted (callers treat absent keys as
/// "user deleted").
/// </para>
/// <para>
/// <b>User id format:</b> <see cref="FshUser"/> inherits <c>IdentityUser</c> so <c>Id</c> is
/// <c>string</c>. The Workspace module stores <c>WorkspaceMember.UserId</c> as a string and
/// passes <c>Guid</c> values here for batch resolution; we convert both directions without
/// allocation overhead in the common path.
/// </para>
/// </remarks>
internal sealed class UserIdentityService : IUserIdentityService
{
    private readonly UserManager<FshUser> _userManager;

    public UserIdentityService(UserManager<FshUser> userManager)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, UserSummary>> GetUsersByIdsAsync(
        IEnumerable<Guid> userIds,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(userIds);

        // Dedupe + stringify — FshUser.Id is string. We dedupe at the boundary so EF Core's
        // parameter list stays bounded even when the caller passes overlapping ids.
        var distinctIds = userIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .Select(id => id.ToString())
            .ToList();

        if (distinctIds.Count == 0)
        {
            return new Dictionary<Guid, UserSummary>();
        }

        // Single SQL batch — D-05. Project to UserSummary (4 fields); never materialise FshUser.
        // Unknown ids simply do not surface (caller treats absent keys as "deleted").
        var summaries = await _userManager.Users
            .AsNoTracking()
            .Where(u => distinctIds.Contains(u.Id))
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.UserName,
                u.Email,
                ImageUrl = u.ImageUrl != null ? u.ImageUrl.ToString() : null,
            })
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var result = new Dictionary<Guid, UserSummary>(summaries.Count);
        foreach (var s in summaries)
        {
            // Skip rows whose id is not a valid Guid (legacy/non-Guid ids are out of contract).
            if (!Guid.TryParse(s.Id, out var guid))
            {
                continue;
            }

            var displayName = !string.IsNullOrWhiteSpace(s.FirstName) || !string.IsNullOrWhiteSpace(s.LastName)
                ? $"{s.FirstName} {s.LastName}".Trim()
                : s.UserName;

            result[guid] = new UserSummary(guid, displayName, s.Email, s.ImageUrl);
        }

        return result;
    }
}
