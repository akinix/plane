using YH.Modules.Tickets.Contracts.Dtos;
using Mediator;

namespace YH.Modules.Tickets.Contracts.v1.Tickets;

public sealed record GetTicketByIdQuery(Guid TicketId) : IQuery<TicketDto>;
