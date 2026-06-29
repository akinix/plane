using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.IssueLinks.DeleteIssueLink;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.IssueLinks.DeleteIssueLink;

/// <summary>
/// Handles <see cref="DeleteIssueLinkCommand"/> — hard-deletes an issue link.
/// </summary>
public sealed class DeleteIssueLinkCommandHandler : ICommandHandler<DeleteIssueLinkCommand>
{
    private readonly WorkItemsDbContext _db;

    public DeleteIssueLinkCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<Unit> Handle(DeleteIssueLinkCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.LinkId == Guid.Empty)
        {
            throw new CustomException("Link id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        var link = await _db.Set<IssueLink>()
            .FirstOrDefaultAsync(l => l.Id == command.LinkId, cancellationToken)
            .ConfigureAwait(false);

        if (link is null)
        {
            throw new NotFoundException($"Issue link '{command.LinkId}' was not found.");
        }

        // Hard-delete per Plane pattern
        _db.Set<IssueLink>().Remove(link);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
