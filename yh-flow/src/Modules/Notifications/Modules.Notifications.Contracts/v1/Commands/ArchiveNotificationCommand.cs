using Mediator;

namespace YH.Modules.Notifications.Contracts.v1.Commands;

public sealed record ArchiveNotificationCommand(Guid Id) : ICommand;
