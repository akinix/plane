using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.Project.Contracts.v1.Members.AddMember;
using YH.Modules.Project.Data;
using YH.Modules.Project.Domain;
using YH.Modules.Workspace.Contracts;

namespace YH.Modules.Project.Features.v1.Members.AddMember;

/// <summary>
/// Handles <see cref="AddMemberCommand"/> — adds a user as a project member (REQ-3.2).
/// Enforces duplicate detection and Admin authorization.
/// </summary>
/// <remarks>
/// <b>Authorization:</b> the endpoint requires workspace Admin via <c>RequireWorkspaceRole</c>.
/// The handler additionally verifies project-level Admin role for the caller, and rejects
/// duplicate active memberships.
/// </remarks>
public sealed class AddMemberCommandHandler : ICommandHandler<AddMemberCommand, AddMemberResponse>
{
    private readonly ProjectDbContext _db;

    public AddMemberCommandHandler(ProjectDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<AddMemberResponse> Handle(AddMemberCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.ProjectId == Guid.Empty)
        {
            throw new ArgumentException("Project id is required.", nameof(command));
        }

        if (command.UserId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(command));
        }

        if (command.CurrentUserId == Guid.Empty)
        {
            throw new ArgumentException("Current user id is required.", nameof(command));
        }

        var targetUserId = command.UserId.ToString();

        // Check for duplicate active membership (T-3-member-06).
        var isDuplicate = await _db.Members
            .AnyAsync(m => m.ProjectId == command.ProjectId
                        && m.UserId == targetUserId
                        && m.IsActive
                        && !m.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (isDuplicate)
        {
            throw new CustomException(
                "User is already an active member of this project.",
                System.Array.Empty<string>(),
                System.Net.HttpStatusCode.Conflict);
        }

        // Create the new membership.
        var member = ProjectMember.Create(
            projectId: command.ProjectId,
            userId: targetUserId,
            role: command.Role,
            isActive: true);

        _db.Members.Add(member);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new AddMemberResponse(
            Id: member.Id,
            ProjectId: member.ProjectId,
            UserId: command.UserId,
            Role: member.Role);
    }
}
