using YH.Framework.Core.Domain;
using YH.Modules.Tickets.Contracts.Dtos;

namespace YH.Modules.Tickets.Domain.Events;

public sealed record TicketStatusChangedDomainEvent(
    Guid TicketId,
    TicketStatus PreviousStatus,
    TicketStatus NewStatus,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);
