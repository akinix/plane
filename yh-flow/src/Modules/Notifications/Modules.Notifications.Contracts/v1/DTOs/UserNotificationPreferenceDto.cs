namespace YH.Modules.Notifications.Contracts.v1.DTOs;

public sealed class UserNotificationPreferenceDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? WorkspaceId { get; set; }
    public Guid? ProjectId { get; set; }
    public bool PropertyChanged { get; set; }
    public bool StateChanged { get; set; }
    public bool Comment { get; set; }
    public bool Mention { get; set; }
    public bool IssueCompleted { get; set; }
}
