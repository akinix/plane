using YH.Framework.Core.Domain;

namespace YH.Modules.Notifications.Domain;

/// <summary>
/// Per-user notification channel preferences, scoped optionally to a workspace/project.
/// Controls which event categories trigger in-app and email notifications.
/// Tenant-scoped via IHasTenant — tenant isolation applied by Finbuckle.
/// </summary>
public sealed class UserNotificationPreference : IHasTenant
{
    public Guid Id { get; private set; }

    /// <summary>User id these preferences belong to.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Optional workspace scope; null means global default.</summary>
    public Guid? WorkspaceId { get; private set; }

    /// <summary>Optional project scope; null means workspace-wide default.</summary>
    public Guid? ProjectId { get; private set; }

    /// <summary>Whether to notify on property changes.</summary>
    public bool PropertyChanged { get; private set; } = true;

    /// <summary>Whether to notify on state changes.</summary>
    public bool StateChanged { get; private set; } = true;

    /// <summary>Whether to notify on new comments.</summary>
    public bool Comment { get; private set; } = true;

    /// <summary>Whether to notify on mentions.</summary>
    public bool Mention { get; private set; } = true;

    /// <summary>Whether to notify on issue completion.</summary>
    public bool IssueCompleted { get; private set; } = true;

    /// <summary>Finbuckle-managed tenant id (workspace Guid as string).</summary>
    public string TenantId { get; private set; } = default!;

    private UserNotificationPreference() { } // EF Core

    /// <summary>
    /// Factory — creates a new UserNotificationPreference with default (all-on) settings.
    /// </summary>
    /// <param name="userId">User id (non-empty).</param>
    /// <param name="workspaceId">Optional workspace scope.</param>
    /// <param name="projectId">Optional project scope.</param>
    public static UserNotificationPreference Create(Guid userId, Guid? workspaceId = null, Guid? projectId = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User id is required.", nameof(userId));

        return new UserNotificationPreference
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            WorkspaceId = workspaceId,
            ProjectId = projectId,
        };
    }

    /// <summary>
    /// Updates preference toggles. Null values leave the current setting unchanged.
    /// </summary>
    public void Update(
        bool? propertyChanged = null,
        bool? stateChanged = null,
        bool? comment = null,
        bool? mention = null,
        bool? issueCompleted = null)
    {
        if (propertyChanged.HasValue) PropertyChanged = propertyChanged.Value;
        if (stateChanged.HasValue) StateChanged = stateChanged.Value;
        if (comment.HasValue) Comment = comment.Value;
        if (mention.HasValue) Mention = mention.Value;
        if (issueCompleted.HasValue) IssueCompleted = issueCompleted.Value;
    }
}
