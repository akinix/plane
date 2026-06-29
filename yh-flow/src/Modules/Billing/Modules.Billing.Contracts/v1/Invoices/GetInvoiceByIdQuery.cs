using YH.Modules.Billing.Contracts.Dtos;
using Mediator;

namespace YH.Modules.Billing.Contracts.v1.Invoices;

public sealed record GetInvoiceByIdQuery(Guid InvoiceId) : IQuery<InvoiceDto>;
