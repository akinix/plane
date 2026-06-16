using FluentValidation;
using YH.Modules.Webhooks.Contracts.v1.TestWebhookSubscription;

namespace YH.Modules.Webhooks.Features.v1.TestWebhookSubscription;

public sealed class TestWebhookSubscriptionCommandValidator : AbstractValidator<TestWebhookSubscriptionCommand>
{
    public TestWebhookSubscriptionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
