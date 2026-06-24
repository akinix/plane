using System.Net;
using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.Labels.DeleteLabel;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.Labels.DeleteLabel;

/// <summary>
/// Handles <see cref="DeleteLabelCommand"/> — soft-deletes a label.
/// Blocks with 409 Conflict if any Issues reference this label (via IssueLabels table).
/// </summary>
public sealed class DeleteLabelCommandHandler : ICommandHandler<DeleteLabelCommand>
{
    private readonly WorkItemsDbContext _db;

    public DeleteLabelCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<Unit> Handle(DeleteLabelCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.LabelId == Guid.Empty)
        {
            throw new CustomException("Label id is required.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        var label = await _db.Labels
            .FirstOrDefaultAsync(l => l.Id == command.LabelId && l.ProjectId == command.ProjectId && !l.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (label is null)
        {
            throw new NotFoundException($"Label '{command.LabelId}' was not found.");
        }

        // T-4-crud-04: check if any Issues reference this label via IssueLabels table.
        var hasIssues = await _db.IssueLabels
            .AnyAsync(il => il.LabelId == command.LabelId, cancellationToken)
            .ConfigureAwait(false);

        if (hasIssues)
        {
            throw new CustomException(
                "Cannot delete this label because it is referenced by one or more issues.",
                Array.Empty<string>(),
                HttpStatusCode.Conflict);
        }

        label.SoftDelete(DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
