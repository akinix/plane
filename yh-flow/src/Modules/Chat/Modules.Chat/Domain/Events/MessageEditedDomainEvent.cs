using YH.Framework.Core.Domain;

namespace YH.Modules.Chat.Domain.Events;

public sealed record MessageEditedDomainEvent(
    Guid ChannelId,
    Guid MessageId,
    string AuthorUserId,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);
