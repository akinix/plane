using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Contracts.DTOs;
using YH.Modules.Workspace.Contracts.v1.Members.UpdateMemberRole;
using YH.Modules.Workspace.Data;
using YH.Modules.Workspace.Services;

namespace YH.Modules.Workspace.Features.v1.Members.UpdateMemberRole;

/// <summary>
/// Handles <see cref="UpdateMemberRoleCommand"/> — updates a member's role in the resolved
/// workspace (REQ-2.2). Enforces the self-promotion guard (T-2-eop-self [BLOCKING]) before
/// delegating to <see cref="WorkspaceMembershipService"/>.
/// </summary>
/// <remarks>
/// <para><b>EOP mitigation (T-2-eop-self):</b> a Member cannot promote THEMSELVES to Admin via
/// this endpoint. The endpoint's <c>.RequireWorkspaceRole(Admin)</c> already limits callers to
/// Admins, but an Admin demoting themselves and then re-promoting via a colleague's session is
/// the realistic attack vector — the handler therefore explicitly checks that the caller's own
/// id is not the target when promoting TO Admin. Belt-and-braces.</para>
/// <para>
/// For non-Admin target roles (Guest/Member), self-targeting is permitted (an Admin may demote
/// themselves). The service re-validates role domain constraints.
/// </para>
/// </remarks>
public sealed class UpdateMemberRoleCommandHandler : ICommandHandler<UpdateMemberRoleCommand, WorkspaceMemberDto>
{
    private readonly WorkspaceDbContext _db;
    private readonly WorkspaceMembershipService _membership;

    public UpdateMemberRoleCommandHandler(
        WorkspaceDbContext db,
        WorkspaceMembershipService membership)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _membership = membership ?? throw new ArgumentNullException(nameof(membership));
    }

    public async ValueTask<WorkspaceMemberDto> Handle(UpdateMemberRoleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.MemberId == Guid.Empty)
        {
            throw new ArgumentException("Member id is required.", nameof(command));
        }

        // Look up the target member first — we need its UserId for the self-promotion check.
        var target = await _db.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == command.MemberId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Member '{command.MemberId}' was not found.");

        // T-2-eop-self: cannot promote oneself to Admin. Other role transitions are allowed.
        if (command.Role == WorkspaceRole.Admin
            && Guid.TryParse(target.UserId, out var targetUserId)
            && targetUserId == command.CurrentUserId)
        {
            throw new ForbiddenException("Cannot promote self to Admin.");
        }

        var updated = await _membership.UpdateRoleAsync(command.MemberId, command.Role, cancellationToken)
            .ConfigureAwait(false);

        // User summary is not resolved here — the consumer of the response is the admin already
        // viewing the roster via ListMembers; returning a null User keeps the payload minimal.
        // Clients that need the User field should re-query ListMembers.
        return new WorkspaceMemberDto
        {
            Id = updated.Id,
            Role = updated.Role,
            IsActive = updated.IsActive,
            User = null,
            WorkspaceId = updated.WorkspaceId,
            CreatedAt = updated.CreatedOnUtc,
            UpdatedAt = updated.LastModifiedOnUtc,
        };
    }
}
