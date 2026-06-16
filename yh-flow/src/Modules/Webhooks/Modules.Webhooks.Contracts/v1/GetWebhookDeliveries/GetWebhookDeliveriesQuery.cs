using YH.Framework.Shared.Persistence;
using YH.Modules.Webhooks.Contracts.Dtos;
using Mediator;

namespace YH.Modules.Webhooks.Contracts.v1.GetWebhookDeliveries;

public sealed record GetWebhookDeliveriesQuery(Guid SubscriptionId, int PageNumber = 1, int PageSize = 10)
    : IQuery<PagedResponse<WebhookDeliveryDto>>;
