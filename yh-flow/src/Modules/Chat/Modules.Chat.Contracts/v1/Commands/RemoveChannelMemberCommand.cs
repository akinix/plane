using Mediator;

namespace YH.Modules.Chat.Contracts.v1.Commands;

public sealed record RemoveChannelMemberCommand(
    Guid ChannelId,
    string UserId) : ICommand<Unit>;
