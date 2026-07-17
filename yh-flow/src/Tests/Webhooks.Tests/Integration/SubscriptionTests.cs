using Microsoft.EntityFrameworkCore;
using YH.Modules.Webhooks.Contracts.v1.CreateWebhookSubscription;
using YH.Modules.Webhooks.Contracts.v1.DeleteWebhookSubscription;
using YH.Modules.Webhooks.Contracts.v1.GetWebhookSubscriptions;
using YH.Modules.Webhooks.Features.v1.CreateWebhookSubscription;
using YH.Modules.Webhooks.Features.v1.DeleteWebhookSubscription;
using YH.Modules.Webhooks.Features.v1.GetWebhookSubscriptions;
using YH.Modules.Webhooks.Services;
using Webhooks.Tests.Fixtures;

namespace Webhooks.Tests.Integration;

public sealed class SubscriptionTests
{
    [Fact]
    public async Task Create_Should_Persist_Subscription()
    {
        await using var db = WebhookTestFixture.CreateDbContext();
        var protector = new WebhookSecretProtector(
            new EphemeralDataProtectionProvider());
        var handler = new CreateWebhookSubscriptionCommandHandler(db, protector);

        var command = new CreateWebhookSubscriptionCommand(
            "https://example.com/hook",
            ["issue.created", "issue.updated"],
            "my-secret");

        var id = await handler.Handle(command, CancellationToken.None);

        var saved = await db.Subscriptions.FirstOrDefaultAsync(s => s.Id == id);
        saved.ShouldNotBeNull();
        saved.Url.ShouldBe("https://example.com/hook");
        saved.GetEvents().ShouldBe(["issue.created", "issue.updated"]);
        saved.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Create_Should_Reject_Relative_Url()
    {
        var validator = new CreateWebhookSubscriptionCommandValidator();
        var command = new CreateWebhookSubscriptionCommand(
            "/relative/path", ["issue.created"], null);

        var result = await validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task Create_Should_Reject_Empty_Events()
    {
        var validator = new CreateWebhookSubscriptionCommandValidator();
        var command = new CreateWebhookSubscriptionCommand(
            "https://example.com/hook", [], null);

        var result = await validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task Delete_Should_Remove_Subscription()
    {
        await using var db = WebhookTestFixture.CreateDbContext();
        var protector = new WebhookSecretProtector(
            new EphemeralDataProtectionProvider());
        var createHandler = new CreateWebhookSubscriptionCommandHandler(db, protector);
        var deleteHandler = new DeleteWebhookSubscriptionCommandHandler(db);

        var createCmd = new CreateWebhookSubscriptionCommand(
            "https://example.com/hook", ["issue.created"], null);
        var id = await createHandler.Handle(createCmd, CancellationToken.None);

        var deleteCmd = new DeleteWebhookSubscriptionCommand(id);
        await deleteHandler.Handle(deleteCmd, CancellationToken.None);

        var subscription = await db.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == id);
        subscription.ShouldBeNull();
    }

    [Fact]
    public async Task Delete_Should_Throw_When_Not_Found()
    {
        await using var db = WebhookTestFixture.CreateDbContext();
        var handler = new DeleteWebhookSubscriptionCommandHandler(db);
        var cmd = new DeleteWebhookSubscriptionCommand(Guid.NewGuid());

        await Should.ThrowAsync<Exception>(() =>
            handler.Handle(cmd, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task List_Should_Return_Paginated_Active_Subscriptions()
    {
        await using var db = WebhookTestFixture.CreateDbContext();
        var protector = new WebhookSecretProtector(
            new EphemeralDataProtectionProvider());
        var createHandler = new CreateWebhookSubscriptionCommandHandler(db, protector);

        var cmd1 = new CreateWebhookSubscriptionCommand(
            "https://example.com/hook1", ["issue.created"], null);
        var cmd2 = new CreateWebhookSubscriptionCommand(
            "https://example.com/hook2", ["issue.updated"], null);
        await createHandler.Handle(cmd1, CancellationToken.None);
        await createHandler.Handle(cmd2, CancellationToken.None);

        var handler = new GetWebhookSubscriptionsQueryHandler(db);
        var query = new GetWebhookSubscriptionsQuery();
        var result = await handler.Handle(query, CancellationToken.None);

        result.Items.Count.ShouldBe(2);
        result.TotalCount.ShouldBe(2);
    }

    [Fact]
    public async Task List_Should_Exclude_Deleted_Subscriptions()
    {
        await using var db = WebhookTestFixture.CreateDbContext();
        var protector = new WebhookSecretProtector(
            new EphemeralDataProtectionProvider());
        var createHandler = new CreateWebhookSubscriptionCommandHandler(db, protector);
        var deleteHandler = new DeleteWebhookSubscriptionCommandHandler(db);

        var cmd = new CreateWebhookSubscriptionCommand(
            "https://example.com/hook", ["issue.created"], null);
        var id = await createHandler.Handle(cmd, CancellationToken.None);

        var deleteCmd = new DeleteWebhookSubscriptionCommand(id);
        await deleteHandler.Handle(deleteCmd, CancellationToken.None);

        var handler = new GetWebhookSubscriptionsQueryHandler(db);
        var query = new GetWebhookSubscriptionsQuery();
        var result = await handler.Handle(query, CancellationToken.None);

        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }
}
