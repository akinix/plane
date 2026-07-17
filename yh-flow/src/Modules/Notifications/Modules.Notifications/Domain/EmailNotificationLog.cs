using YH.Framework.Core.Domain;

namespace YH.Modules.Notifications.Domain;

/// <summary>
/// Audit log tracking when email notifications are sent by the system. Records what was sent,
/// to whom, by whom, and what changed. Tenant-scoped via IHasTenant.
/// </summary>
public sealed class EmailNotificationLog : IHasTenant
{
    public Guid Id { get; private set; }

    /// <summary>User id who received the email.</summary>
    public Guid ReceiverId { get; private set; }

    /// <summary>User id who triggered the notification.</summary>
    public Guid TriggeredById { get; private set; }

    /// <summary>Optional entity identifier the event relates to (e.g. issue id).</summary>
    public Guid? EntityIdentifier { get; private set; }

    /// <summary>Entity type name, e.g. "Issue", "ProjectPage".</summary>
    public string EntityName { get; private set; } = default!;

    /// <summary>Opaque JSON blob with event-specific data.</summary>
    public string? Data { get; private set; }

    /// <summary>UTC timestamp when the email was sent; null before dispatch.</summary>
    public DateTime? SentAt { get; private set; }

    /// <summary>The event verb, e.g. "PropertyChanged", "CommentAdded".</summary>
    public string Entity { get; private set; } = default!;

    /// <summary>Previous value of the changed property (if applicable).</summary>
    public string? OldValue { get; private set; }

    /// <summary>New value of the changed property (if applicable).</summary>
    public string? NewValue { get; private set; }

    /// <summary>Finbuckle-managed tenant id (workspace Guid as string).</summary>
    public string TenantId { get; private set; } = default!;

    private EmailNotificationLog() { } // EF Core

    /// <summary>
    /// Factory — creates a new EmailNotificationLog entry before dispatch.
    /// </summary>
    public static EmailNotificationLog Create(
        Guid receiverId,
        Guid triggeredById,
        string entityName,
        string entity,
        Guid? entityIdentifier = null,
        string? data = null,
        string? oldValue = null,
        string? newValue = null)
    {
        if (receiverId == Guid.Empty)
            throw new ArgumentException("Receiver id is required.", nameof(receiverId));
        if (triggeredById == Guid.Empty)
            throw new ArgumentException("Triggered by id is required.", nameof(triggeredById));
        ArgumentException.ThrowIfNullOrWhiteSpace(entityName);
        ArgumentException.ThrowIfNullOrWhiteSpace(entity);

        return new EmailNotificationLog
        {
            Id = Guid.CreateVersion7(),
            ReceiverId = receiverId,
            TriggeredById = triggeredById,
            EntityIdentifier = entityIdentifier,
            EntityName = entityName,
            Data = data,
            Entity = entity,
            OldValue = oldValue,
            NewValue = newValue,
        };
    }

    /// <summary>Marks this log entry as sent with the current UTC timestamp.</summary>
    public void MarkSent()
    {
        SentAt = DateTime.UtcNow;
    }
}
