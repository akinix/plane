using YH.Framework.Core.Domain;

namespace YH.Modules.Files.Domain.Events;

public sealed record FileSoftDeletedDomainEvent(
    Guid FileAssetId,
    string ActorUserId,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);
