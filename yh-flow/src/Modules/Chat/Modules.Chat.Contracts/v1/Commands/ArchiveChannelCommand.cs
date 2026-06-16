using Mediator;

namespace YH.Modules.Chat.Contracts.v1.Commands;

public sealed record ArchiveChannelCommand(Guid ChannelId) : ICommand<Unit>;
