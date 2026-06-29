using Mediator;

namespace YH.Modules.Tickets.Contracts.v1.Tickets;

public sealed record CloseTicketCommand(Guid TicketId) : ICommand<Guid>;
