using Mediator;

namespace YH.Modules.Chat.Contracts.v1.Commands;

public sealed record MarkChannelReadCommand(Guid ChannelId, Guid MessageId) : ICommand<Unit>;
