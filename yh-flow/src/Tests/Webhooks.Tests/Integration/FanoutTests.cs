using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using NSubstitute;
using YH.Framework.Eventing.Abstractions;
using YH.Framework.Shared.Multitenancy;
using YH.Modules.Webhooks.Domain;
using YH.Modules.Webhooks.Services;
using Webhooks.Tests.Fixtures;

namespace Webhooks.Tests.Integration;

public sealed class FanoutTests
{
    private static WebhookSubscription CreateSubscription(WebhookDbContext db, string[] events)
    {
        var sub = WebhookSubscription.Create("https://example.com/hook", events, null);
        db.Subscriptions.Add(sub);
        db.SaveChanges();
        return sub;
    }

    private static TestTenantAccessor CreateAccessor(string tenantId)
    {
        var accessor = new TestTenantAccessor();
        accessor.SetTenant(tenantId);
        return accessor;
    }

    [Fact]
    public async Task HandleAsync_Should_Enqueue_For_Matching_Subscription()
    {
        await using var db = WebhookTestFixture.CreateDbContext();
        CreateSubscription(db, ["TestEvent"]);

        var dispatcher = Substitute.For<IWebhookDispatcher>();
        var serializer = Substitute.For<IEventSerializer>();
        serializer.Serialize(Arg.Any<IIntegrationEvent>()).Returns("{}");
        var logger = Substitute.For<ILogger<WebhookFanoutHandler<TestEvent>>>();
        var handler = new WebhookFanoutHandler<TestEvent>(db, dispatcher, serializer, CreateAccessor("1"), logger);

        var evt = new TestEvent { TenantId = "1" };
        await handler.HandleAsync(evt, CancellationToken.None);

        await dispatcher.Received(1).EnqueueAsync(
            "1", Arg.Any<Guid>(), "TestEvent", "{}", CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_Should_Not_Enqueue_When_No_Match()
    {
        await using var db = WebhookTestFixture.CreateDbContext();
        CreateSubscription(db, ["SomeOtherEvent"]);

        var dispatcher = Substitute.For<IWebhookDispatcher>();
        var serializer = Substitute.For<IEventSerializer>();
        serializer.Serialize(Arg.Any<IIntegrationEvent>()).Returns("{}");
        var logger = Substitute.For<ILogger<WebhookFanoutHandler<TestEvent>>>();
        var handler = new WebhookFanoutHandler<TestEvent>(db, dispatcher, serializer, CreateAccessor("1"), logger);

        var evt = new TestEvent { TenantId = "1" };
        await handler.HandleAsync(evt, CancellationToken.None);

        await dispatcher.DidNotReceive().EnqueueAsync(
            Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_Should_Match_Wildcard()
    {
        await using var db = WebhookTestFixture.CreateDbContext();
        CreateSubscription(db, ["*"]);

        var dispatcher = Substitute.For<IWebhookDispatcher>();
        var serializer = Substitute.For<IEventSerializer>();
        serializer.Serialize(Arg.Any<IIntegrationEvent>()).Returns("{}");
        var logger = Substitute.For<ILogger<WebhookFanoutHandler<TestEvent>>>();
        var handler = new WebhookFanoutHandler<TestEvent>(db, dispatcher, serializer, CreateAccessor("1"), logger);

        var evt = new TestEvent { TenantId = "1" };
        await handler.HandleAsync(evt, CancellationToken.None);

        await dispatcher.Received(1).EnqueueAsync(
            Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_Should_Skip_Inactive_Subscription()
    {
        await using var db = WebhookTestFixture.CreateDbContext();
        var sub = CreateSubscription(db, ["issue.created"]);
        sub.Deactivate();
        await db.SaveChangesAsync();

        var dispatcher = Substitute.For<IWebhookDispatcher>();
        var serializer = Substitute.For<IEventSerializer>();
        serializer.Serialize(Arg.Any<IIntegrationEvent>()).Returns("{}");
        var logger = Substitute.For<ILogger<WebhookFanoutHandler<TestEvent>>>();
        var handler = new WebhookFanoutHandler<TestEvent>(db, dispatcher, serializer, CreateAccessor("1"), logger);

        var evt = new TestEvent { TenantId = "1" };
        await handler.HandleAsync(evt, CancellationToken.None);

        await dispatcher.DidNotReceive().EnqueueAsync(
            Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_Should_Skip_When_TenantId_Null()
    {
        await using var db = WebhookTestFixture.CreateDbContext();
        CreateSubscription(db, ["*"]);

        var dispatcher = Substitute.For<IWebhookDispatcher>();
        var serializer = Substitute.For<IEventSerializer>();
        var logger = Substitute.For<ILogger<WebhookFanoutHandler<TestEvent>>>();
        var handler = new WebhookFanoutHandler<TestEvent>(db, dispatcher, serializer, CreateAccessor("1"), logger);

        var evt = new TestEvent { TenantId = null };
        await handler.HandleAsync(evt, CancellationToken.None);

        await dispatcher.DidNotReceive().EnqueueAsync(
            Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    public sealed class TestEvent : IIntegrationEvent
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public DateTime OccurredOnUtc { get; set; } = DateTime.UtcNow;
        public string? TenantId { get; set; }
        public string CorrelationId { get; set; } = Guid.CreateVersion7().ToString();
        public string Source { get; set; } = "test";
    }

    private sealed class TestTenantAccessor : IMultiTenantContextAccessor<AppTenantInfo>, IMultiTenantContextSetter
    {
        private IMultiTenantContext<AppTenantInfo> _context = new MultiTenantContext<AppTenantInfo>(new AppTenantInfo());

        public IMultiTenantContext<AppTenantInfo> MultiTenantContext => _context;

        IMultiTenantContext IMultiTenantContextAccessor.MultiTenantContext => _context;

        IMultiTenantContext IMultiTenantContextSetter.MultiTenantContext
        {
            set => _context = (IMultiTenantContext<AppTenantInfo>)value;
        }

        public void SetTenant(string tenantId) =>
            ((IMultiTenantContextSetter)this).MultiTenantContext =
                new MultiTenantContext<AppTenantInfo>(new AppTenantInfo(tenantId, tenantId));
    }
}
