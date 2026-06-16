using Mediator;

namespace YH.Modules.Tickets.Contracts.v1.Tickets;

public sealed record ReopenTicketCommand(Guid TicketId) : ICommand<Guid>;
