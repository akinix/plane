using YH.Framework.Eventing.Abstractions;
using YH.Framework.Mailing.Services;
using YH.Modules.Multitenancy.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace YH.Modules.Notifications.IntegrationEventHandlers;

/// <summary>Emails the tenant admin that their subscription is nearing expiry.</summary>
public sealed class TenantNearingExpiryEmailHandler(
    IMailService mailService,
    ILogger<TenantNearingExpiryEmailHandler> logger)
    : IIntegrationEventHandler<TenantNearingExpiryIntegrationEvent>
{
    public async Task HandleAsync(TenantNearingExpiryIntegrationEvent @event, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(@event);
        var (subject, body) = BillingEmailBodies.NearingExpiry(
            @event.TenantName, @event.PlanKey, @event.ValidUpto, @event.DaysRemaining);
        await BillingEmailSender.SendAsync(mailService, logger, @event.AdminEmail, subject, body, "nearing-expiry", ct)
            .ConfigureAwait(false);
    }
}
