using Mediator;

namespace YH.Modules.Chat.Contracts.v1.Commands;

public sealed record AddReactionCommand(Guid MessageId, string Emoji) : ICommand<Unit>;
