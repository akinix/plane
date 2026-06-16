using FluentValidation;
using YH.Modules.Webhooks.Contracts.v1.DeleteWebhookSubscription;

namespace YH.Modules.Webhooks.Features.v1.DeleteWebhookSubscription;

public sealed class DeleteWebhookSubscriptionCommandValidator : AbstractValidator<DeleteWebhookSubscriptionCommand>
{
    public DeleteWebhookSubscriptionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
