using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.Modules.Issues;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.Modules.Issues.RemoveIssueFromModule;

/// <summary>
/// Handles <see cref="RemoveIssueFromModuleCommand"/> — removes an issue from a module via soft-delete.
/// </summary>
public sealed class RemoveIssueFromModuleCommandHandler : ICommandHandler<RemoveIssueFromModuleCommand, Unit>
{
    private readonly WorkItemsDbContext _db;

    public RemoveIssueFromModuleCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<Unit> Handle(RemoveIssueFromModuleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var moduleIssue = await _db.Set<Domain.ModuleIssue>()
            .FirstOrDefaultAsync(mi => mi.ModuleId == command.ModuleId
                                    && mi.IssueId == command.IssueId
                                    && !mi.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (moduleIssue is null)
        {
            throw new NotFoundException($"ModuleIssue for module '{command.ModuleId}' and issue '{command.IssueId}' was not found.");
        }

        moduleIssue.SoftDelete(DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
