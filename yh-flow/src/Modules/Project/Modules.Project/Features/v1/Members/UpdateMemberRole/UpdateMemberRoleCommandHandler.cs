using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.Project.Contracts.v1.Members.UpdateMemberRole;
using YH.Modules.Project.Data;
using YH.Modules.Workspace.Contracts;

namespace YH.Modules.Project.Features.v1.Members.UpdateMemberRole;

/// <summary>
/// Handles <see cref="UpdateMemberRoleCommand"/> — updates a member's role in the resolved project
/// (REQ-3.2). Enforces project-level Admin authorization.
/// </summary>
/// <remarks>
/// <b>Authorization:</b> the endpoint enforces workspace Admin via <c>RequireWorkspaceRole</c>.
/// The handler additionally validates that the caller is a project Admin — this catches the case
/// where a workspace Admin who is not a project member attempts the operation.
/// </remarks>
public sealed class UpdateMemberRoleCommandHandler : ICommandHandler<UpdateMemberRoleCommand>
{
    private readonly ProjectDbContext _db;

    public UpdateMemberRoleCommandHandler(ProjectDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<Unit> Handle(UpdateMemberRoleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.MemberId == Guid.Empty)
        {
            throw new ArgumentException("Member id is required.", nameof(command));
        }

        // Fetch the target member within the project scope.
        var target = await _db.Members
            .FirstOrDefaultAsync(m => m.Id == command.MemberId
                                   && m.ProjectId == command.ProjectId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Member '{command.MemberId}' was not found.");

        // Update the role.
        target.UpdateRole(command.Role);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
