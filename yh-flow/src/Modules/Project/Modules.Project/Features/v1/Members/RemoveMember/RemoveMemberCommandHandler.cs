using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.Project.Contracts.v1.Members.RemoveMember;
using YH.Modules.Project.Data;
using YH.Modules.Workspace.Contracts;

namespace YH.Modules.Project.Features.v1.Members.RemoveMember;

/// <summary>
/// Handles <see cref="RemoveMemberCommand"/> — admin-driven deactivation of a project member
/// (REQ-3.2). Uses <c>Deactivate()</c> (sets <c>IsActive = false</c>) instead of hard-delete
/// to preserve the row for audit. Refuses to remove the last Admin member.
/// </summary>
/// <remarks>
/// <b>Last-Admin guard (T-3-member-07):</b> the handler checks whether removing this member
/// would leave the project with zero Admin members. If the target is an Admin and the only
/// Admin member, the operation is rejected with a 409 Conflict.
/// </remarks>
public sealed class RemoveMemberCommandHandler : ICommandHandler<RemoveMemberCommand>
{
    private readonly ProjectDbContext _db;

    public RemoveMemberCommandHandler(ProjectDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<Unit> Handle(RemoveMemberCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.MemberId == Guid.Empty)
        {
            throw new ArgumentException("Member id is required.", nameof(command));
        }

        if (command.ProjectId == Guid.Empty)
        {
            throw new ArgumentException("Project id is required.", nameof(command));
        }

        // Fetch the target member (tenant-scoped by DbContext).
        var target = await _db.Members
            .FirstOrDefaultAsync(m => m.Id == command.MemberId
                                   && m.ProjectId == command.ProjectId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Member '{command.MemberId}' was not found.");

        // T-3-member-07: Last-Admin guard — cannot deactivate the last Admin member.
        if (target.Role == (int)WorkspaceRole.Admin)
        {
            var adminCount = await _db.Members
                .CountAsync(m => m.ProjectId == command.ProjectId
                              && m.Role == (int)WorkspaceRole.Admin
                              && m.IsActive
                              && !m.IsDeleted, cancellationToken)
                .ConfigureAwait(false);

            if (adminCount <= 1)
            {
                throw new CustomException(
                    "Cannot remove the last Admin member of the project.",
                    System.Array.Empty<string>(),
                    System.Net.HttpStatusCode.Conflict);
            }
        }

        // D-04: deactivate (IsActive = false) — keeps the row for audit trail.
        target.Deactivate();
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
