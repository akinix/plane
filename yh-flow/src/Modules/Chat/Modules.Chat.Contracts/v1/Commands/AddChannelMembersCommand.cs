using Mediator;

namespace YH.Modules.Chat.Contracts.v1.Commands;

public sealed record AddChannelMembersCommand(
    Guid ChannelId,
    IReadOnlyList<string> UserIds) : ICommand<Unit>;
