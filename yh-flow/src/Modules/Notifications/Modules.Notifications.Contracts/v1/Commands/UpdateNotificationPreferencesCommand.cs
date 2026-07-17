using Mediator;

namespace YH.Modules.Notifications.Contracts.v1.Commands;

public sealed record UpdateNotificationPreferencesCommand(
    Guid? WorkspaceId, Guid? ProjectId,
    bool? PropertyChanged, bool? StateChanged, bool? Comment, bool? Mention, bool? IssueCompleted) : ICommand;
