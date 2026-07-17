using Mediator;

namespace YH.Modules.Notifications.Contracts.v1.Commands;

public sealed record UnarchiveNotificationCommand(Guid Id) : ICommand;
