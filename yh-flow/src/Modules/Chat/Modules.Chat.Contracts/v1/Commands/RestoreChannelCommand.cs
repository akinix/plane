using Mediator;

namespace YH.Modules.Chat.Contracts.v1.Commands;

public sealed record RestoreChannelCommand(Guid ChannelId) : ICommand<Unit>;
