using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Domain.Events;

/// <summary>
/// Handles <see cref="IssueUpdatedDomainEvent"/> — persists audit log entries to
/// the <see cref="IssueActivity"/> table.
/// </summary>
/// <remarks>
/// <b>Recursion prevention (T-4-activity-01):</b> <see cref="IssueActivity"/> does NOT implement
/// <see cref="YH.Framework.Core.Domain.IHasDomainEvents"/>. This handler writes activity records
/// directly via <see cref="WorkItemsDbContext"/> without any domain event re-trigger.
/// <para>
/// <b>Epoch:</b> Uses <see cref="TimeProvider.GetUtcNow"/> → UnixTimeSeconds for
/// consistent cross-timezone chronological ordering.
/// </para>
/// </remarks>
public sealed class IssueActivityHandler : INotificationHandler<IssueUpdatedDomainEvent>
{
    private readonly WorkItemsDbContext _db;
    private readonly TimeProvider _timeProvider;

    public IssueActivityHandler(WorkItemsDbContext db, TimeProvider timeProvider)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <summary>
    /// For each changed field in the domain event, creates an <see cref="IssueActivity"/> record.
    /// </summary>
    public async ValueTask Handle(IssueUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var epoch = _timeProvider.GetUtcNow().ToUnixTimeSeconds();

        foreach (var change in notification.Changes)
        {
            var activity = IssueActivity.Create(
                issueId: notification.IssueId,
                verb: "updated",
                actorId: notification.ActorId,
                epoch: epoch,
                field: change.Field,
                oldValue: change.OldValue,
                newValue: change.NewValue);

            _db.Set<IssueActivity>().Add(activity);
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
