using Mediator;
using YH.Framework.Core.Domain;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Domain.Events;

/// <summary>
/// Domain event raised when an Issue's fields are updated.
/// Consumed by <see cref="IssueActivityHandler"/> to persist audit log entries.
/// </summary>
/// <remarks>
/// <b>Recursion prevention (T-4-activity-01):</b> <see cref="IssueActivity"/> does NOT implement
/// <see cref="IHasDomainEvents"/>, so the handler writing activity records
/// will never re-trigger this event.
/// <para>
/// Uses <see cref="Issue.FieldChange"/> (nested record in <see cref="Issue"/>) to describe each change.
/// </para>
/// </remarks>
public sealed class IssueUpdatedDomainEvent : IDomainEvent
{
    /// <summary>The id of the issue that was updated.</summary>
    public Guid IssueId { get; }

    /// <summary>List of changed fields with old and new values.</summary>
    public IReadOnlyList<Issue.FieldChange> Changes { get; }

    /// <summary>Actor who performed the update (scalar user id).</summary>
    public string ActorId { get; }

    // IDomainEvent
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOnUtc { get; } = DateTimeOffset.UtcNow;
    public string? CorrelationId { get; }
    public string? TenantId { get; }

    public IssueUpdatedDomainEvent(Guid issueId, IReadOnlyList<Issue.FieldChange> changes, string actorId)
    {
        IssueId = issueId;
        Changes = changes ?? throw new ArgumentNullException(nameof(changes));
        ActorId = actorId ?? throw new ArgumentNullException(nameof(actorId));
    }
}
