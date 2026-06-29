using YH.Framework.Core.Domain;

namespace YH.Modules.Tickets.Domain.Events;

public sealed record TicketAssignedDomainEvent(
    Guid TicketId,
    Guid? PreviousAssigneeUserId,
    Guid? NewAssigneeUserId,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);
