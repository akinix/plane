using Mediator;

namespace YH.Modules.Tickets.Contracts.v1.Tickets;

public sealed record DeleteTicketCommand(Guid TicketId) : ICommand<Unit>;
