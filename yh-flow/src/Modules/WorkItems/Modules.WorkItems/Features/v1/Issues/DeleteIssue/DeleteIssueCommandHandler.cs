using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.Issues.DeleteIssue;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.Issues.DeleteIssue;

/// <summary>
/// Handles <see cref="DeleteIssueCommand"/> — soft-deletes an issue.
/// Cascade deletes IssueAssignee and IssueLabel via EF configuration.
/// </summary>
public sealed class DeleteIssueCommandHandler : ICommandHandler<DeleteIssueCommand>
{
    private readonly WorkItemsDbContext _db;

    public DeleteIssueCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<Unit> Handle(DeleteIssueCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.IssueId == Guid.Empty)
        {
            throw new CustomException("Issue id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        var issue = await _db.Issues
            .FirstOrDefaultAsync(i => i.Id == command.IssueId && i.ProjectId == command.ProjectId && !i.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (issue is null)
        {
            throw new NotFoundException($"Issue '{command.IssueId}' was not found.");
        }

        issue.SoftDelete(DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
