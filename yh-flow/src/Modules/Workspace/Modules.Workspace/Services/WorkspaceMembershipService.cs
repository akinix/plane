using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Data;
using YH.Modules.Workspace.Domain;

namespace YH.Modules.Workspace.Services;

/// <summary>
/// Single entry point for workspace membership mutations + queries (CONTEXT D-04 / D-06).
/// </summary>
/// <remarks>
/// <para>
/// Centralises the operations that previously lived inline in <c>CreateWorkspaceCommandHandler</c>
/// (02-04, D-06). Member/Invitation handlers (02-05) call this service rather than touching
/// <see cref="WorkspaceDbContext.Members"/> directly, keeping the membership lifecycle rules
/// (role validation, self-protection, audit stamping) in one place.
/// </para>
/// <para>
/// <b>Scoped lifetime:</b> the service depends on the scoped <see cref="WorkspaceDbContext"/>
/// (Finbuckle tenant-filtered per request).
/// </para>
/// <para>
/// <b>EOP mitigations (threat T-2-eop-self [BLOCKING]):</b> handlers still perform the explicit
/// "cannot promote self to Admin" / "cannot remove self (use leave)" checks BEFORE calling
/// <see cref="UpdateRoleAsync"/> / <see cref="RemoveAsync"/>. The service re-validates role
/// domain constraints (role must be one of Guest/Member/Admin) as a belt-and-braces guard.
/// </para>
/// </remarks>
public sealed class WorkspaceMembershipService
{
    private readonly WorkspaceDbContext _db;

    public WorkspaceMembershipService(WorkspaceDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    /// <summary>
    /// Creates the first Admin member for a freshly-created workspace (D-06). Used by
    /// <c>CreateWorkspaceCommandHandler</c> — same semantics as the inline 02-04 path.
    /// </summary>
    public async Task<WorkspaceMember> AddOwnerAsync(
        Guid workspaceId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (workspaceId == Guid.Empty)
        {
            throw new ArgumentException("Workspace id is required.", nameof(workspaceId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        var member = WorkspaceMember.Create(
            workspaceId: workspaceId,
            userId: userId.ToString(),
            role: (int)WorkspaceRole.Admin,
            isActive: true);

        _db.Members.Add(member);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return member;
    }

    /// <summary>
    /// Lists all members of the resolved workspace (active + inactive). Tenant filter applied
    /// automatically by the scoped DbContext.
    /// </summary>
    public async Task<IReadOnlyList<WorkspaceMember>> ListAsync(
        Guid workspaceId,
        CancellationToken cancellationToken)
    {
        return await _db.Members
            .AsNoTracking()
            .Where(m => m.WorkspaceId == workspaceId && !m.IsDeleted)
            .OrderBy(m => m.Role)
            .ThenBy(m => m.CreatedOnUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Updates a member's role. Throws <see cref="NotFoundException"/> when the member does not
    /// exist (or belongs to a different tenant — indistinguishable by design).
    /// </summary>
    public async Task<WorkspaceMember> UpdateRoleAsync(
        Guid memberId,
        WorkspaceRole newRole,
        CancellationToken cancellationToken)
    {
        if (newRole is WorkspaceRole.None)
        {
            throw new ArgumentException("Role must be Guest, Member, or Admin.", nameof(newRole));
        }

        var member = await _db.Members
            .FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Member '{memberId}' was not found.");

        member.UpdateRole((int)newRole);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return member;
    }

    /// <summary>
    /// Soft-deactivates a member row (keeps the row for audit; Plane semantics). Throws
    /// <see cref="NotFoundException"/> when the member does not exist.
    /// </summary>
    public async Task RemoveAsync(Guid memberId, CancellationToken cancellationToken)
    {
        var member = await _db.Members
            .FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Member '{memberId}' was not found.");

        member.Deactivate();
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Self-removal — the current user leaves the resolved workspace by deactivating their own
    /// membership. Distinct from <see cref="RemoveAsync"/> (admin-driven) at the handler layer;
    /// both converge on <c>Deactivate()</c> at the entity level.
    /// </summary>
    public async Task LeaveAsync(
        Guid workspaceId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var member = await _db.Members
            .FirstOrDefaultAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId.ToString(),
                cancellationToken)
            .ConfigureAwait(false);

        if (member is null)
        {
            // Already not a member — idempotent.
            return;
        }

        member.Deactivate();
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
