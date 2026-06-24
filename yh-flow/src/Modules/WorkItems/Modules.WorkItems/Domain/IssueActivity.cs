using YH.Framework.Core.Domain;

namespace YH.Modules.WorkItems.Domain;

/// <summary>
/// IssueActivity entity — records a single field change on an Issue for audit logging
/// (Plane <c>models/issue.py</c> IssueActivity).
/// </summary>
/// <remarks>
/// <b>CRITICAL — recursion prevention:</b> This entity does NOT implement <see cref="IHasDomainEvents"/>.
/// Domain events are raised by the <see cref="Issue"/> entity; <see cref="IssueActivity"/> is the
/// <i>consumer</i> of those events and must never emit events itself (T-4-activity-01).
/// <para>
/// <b>Epoch ordering:</b> <see cref="Epoch"/> is a Unix timestamp (seconds) set by the handler,
/// enabling chronological ordering across time zones without DateTimeOffset conversion.
/// </para>
/// </remarks>
public sealed class IssueActivity : IHasTenant, IAuditableEntity
{
    public Guid Id { get; private set; }

    /// <summary>FK to the tracked Issue.</summary>
    public Guid IssueId { get; private set; }

    /// <summary>Action verb: "created" / "updated" / "deleted" (max 20).</summary>
    public string Verb { get; private set; } = default!;

    /// <summary>Name of the changed field (e.g. "state_id", "priority"). Null for "created"/"deleted".</summary>
    public string? Field { get; private set; }

    /// <summary>Previous value as string (null for "created").</summary>
    public string? OldValue { get; private set; }

    /// <summary>New value as string (null for "deleted").</summary>
    public string? NewValue { get; private set; }

    /// <summary>Optional comment text for manual annotation.</summary>
    public string? Comment { get; private set; }

    /// <summary>Actor who performed the action (scalar user id, no EF FK).</summary>
    public string ActorId { get; private set; } = default!;

    /// <summary>FK to an IssueComment if this activity is related to a comment action.</summary>
    public Guid? IssueCommentId { get; private set; }

    /// <summary>Unix timestamp (seconds) for chronological ordering.</summary>
    public long Epoch { get; private set; }

    // IHasTenant
    public string TenantId { get; private set; } = default!;

    // IAuditableEntity
    public DateTimeOffset CreatedOnUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? LastModifiedOnUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }

    private IssueActivity() { } // EF Core

    /// <summary>
    /// Factory — creates a new <see cref="IssueActivity"/> record.
    /// </summary>
    /// <param name="issueId">The tracked issue.</param>
    /// <param name="verb">Action verb ("created" / "updated" / "deleted").</param>
    /// <param name="actorId">The acting user's id.</param>
    /// <param name="epoch">Unix timestamp (seconds) for ordering.</param>
    /// <param name="field">Optional field name for "updated" verb.</param>
    /// <param name="oldValue">Previous value (for "updated").</param>
    /// <param name="newValue">New value (for "updated"/"created").</param>
    /// <param name="comment">Optional annotation text.</param>
    /// <param name="issueCommentId">FK to a related IssueComment.</param>
    public static IssueActivity Create(
        Guid issueId,
        string verb,
        string actorId,
        long epoch,
        string? field = null,
        string? oldValue = null,
        string? newValue = null,
        string? comment = null,
        Guid? issueCommentId = null)
    {
        if (issueId == Guid.Empty)
            throw new ArgumentException("Issue id is required.", nameof(issueId));
        if (string.IsNullOrWhiteSpace(verb))
            throw new ArgumentException("Verb is required.", nameof(verb));
        if (string.IsNullOrWhiteSpace(actorId))
            throw new ArgumentException("Actor id is required.", nameof(actorId));

        return new IssueActivity
        {
            Id = Guid.NewGuid(),
            IssueId = issueId,
            Verb = verb,
            Field = field,
            OldValue = oldValue,
            NewValue = newValue,
            Comment = comment,
            ActorId = actorId,
            IssueCommentId = issueCommentId,
            Epoch = epoch,
            CreatedOnUtc = DateTimeOffset.UtcNow,
        };
    }
}
