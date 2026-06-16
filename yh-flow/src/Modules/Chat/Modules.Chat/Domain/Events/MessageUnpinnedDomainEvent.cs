using YH.Framework.Core.Domain;

namespace YH.Modules.Chat.Domain.Events;

public sealed record MessageUnpinnedDomainEvent(
    Guid ChannelId,
    Guid MessageId,
    string UnpinnedByUserId,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);
