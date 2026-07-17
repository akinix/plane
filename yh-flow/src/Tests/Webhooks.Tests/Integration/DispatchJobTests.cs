#pragma warning disable CA2000 // DbContext returned from helper is disposed by caller

using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YH.Framework.Shared.Multitenancy;
using YH.Modules.Webhooks.Domain;
using YH.Modules.Webhooks.Services;
using Webhooks.Tests.Fixtures;

namespace Webhooks.Tests.Integration;

public sealed class DispatchJobTests
{
    private static readonly IDataProtectionProvider SharedProtector =
        new EphemeralDataProtectionProvider();

    private static (WebhookDispatchJob job, WebhookDbContext db, HttpClient httpClient) CreateSut(
        HttpStatusCode statusCode, string responseBody = "")
    {
        var db = WebhookTestFixture.CreateDbContext();
        var handler = new FakeHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(statusCode) { Content = new StringContent(responseBody) }));

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.com") };
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient("Webhooks").Returns(httpClient);

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var store = Substitute.For<IMultiTenantStore<AppTenantInfo>>();
        store.GetAsync("1").Returns(new AppTenantInfo("1", "test", "Test"));

        var scope = Substitute.For<IServiceScope>();
        var sp = Substitute.For<IServiceProvider>();
        sp.GetService(typeof(IMultiTenantStore<AppTenantInfo>)).Returns(store);
        var setter = Substitute.For<IMultiTenantContextSetter>();
        sp.GetService(typeof(IMultiTenantContextSetter)).Returns(setter);
        sp.GetService(typeof(WebhookDbContext)).Returns(db);
        scope.ServiceProvider.Returns(sp);
        scopeFactory.CreateScope().Returns(scope);

        var protector = new WebhookSecretProtector(
            SharedProtector);
        var logger = Substitute.For<ILogger<WebhookDispatchJob>>();

        var job = new WebhookDispatchJob(scopeFactory, httpClientFactory, protector, logger);
        return (job, db, httpClient);
    }

    private static WebhookSubscription CreateSubscription(WebhookDbContext db, string secret = "test-secret")
    {
        var protector = new WebhookSecretProtector(
            SharedProtector);
        var sub = WebhookSubscription.Create(
            "https://example.com/hook",
            ["issue.created"],
            protector.Protect(secret));
        db.Subscriptions.Add(sub);
        db.SaveChanges();
        return sub;
    }

    [Fact]
    public async Task DispatchAsync_Should_Record_Successful_Delivery()
    {
        var (job, db, _) = CreateSut(HttpStatusCode.OK);
        var sub = CreateSubscription(db);
        await job.DispatchAsync(sub.Id, "1", "issue.created",
            """{"event":"created"}""", null, CancellationToken.None);

        var delivery = await db.Deliveries.FirstOrDefaultAsync();
        delivery.ShouldNotBeNull();
        delivery.SubscriptionId.ShouldBe(sub.Id);
        delivery.EventType.ShouldBe("issue.created");
        delivery.Success.ShouldBeTrue();
        delivery.HttpStatusCode.ShouldBe(200);
        delivery.AttemptCount.ShouldBe(1);
    }

    [Fact]
    public async Task DispatchAsync_Should_Throw_On_Server_Error()
    {
        var (job, db, _) = CreateSut(HttpStatusCode.InternalServerError);
        var sub = CreateSubscription(db);

        await Should.ThrowAsync<WebhookDeliveryFailedException>(() =>
            job.DispatchAsync(sub.Id, "1", "issue.created",
                """{"event":"created"}""", null, CancellationToken.None));
    }

    [Fact]
    public async Task DispatchAsync_Should_Skip_Inactive_Subscription()
    {
        var (job, db, _) = CreateSut(HttpStatusCode.OK);
        var sub = CreateSubscription(db);
        sub.Deactivate();
        await db.SaveChangesAsync();
        await job.DispatchAsync(sub.Id, "1", "issue.created",
            """{"event":"created"}""", null, CancellationToken.None);

        (await db.Deliveries.AnyAsync()).ShouldBeFalse();
    }

    [Fact]
    public async Task DispatchAsync_Should_Record_Network_Error()
    {
        var db = WebhookTestFixture.CreateDbContext();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var handler = new HttpClientHandler { Proxy = new WebProxy("http://invalid.proxy:0"), CheckCertificateRevocationList = true };
        var httpClient = new HttpClient(handler);
        httpClientFactory.CreateClient("Webhooks").Returns(httpClient);

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var store = Substitute.For<IMultiTenantStore<AppTenantInfo>>();
        store.GetAsync("1").Returns(new AppTenantInfo("1", "test", "Test"));

        var scope = Substitute.For<IServiceScope>();
        var sp = Substitute.For<IServiceProvider>();
        sp.GetService(typeof(IMultiTenantStore<AppTenantInfo>)).Returns(store);
        var setter = Substitute.For<IMultiTenantContextSetter>();
        sp.GetService(typeof(IMultiTenantContextSetter)).Returns(setter);
        sp.GetService(typeof(WebhookDbContext)).Returns(db);
        scope.ServiceProvider.Returns(sp);
        scopeFactory.CreateScope().Returns(scope);

        var protector = new WebhookSecretProtector(
            SharedProtector);
        var logger = Substitute.For<ILogger<WebhookDispatchJob>>();
        var job = new WebhookDispatchJob(scopeFactory, httpClientFactory, protector, logger);

        var sub = CreateSubscription(db);

        await Should.ThrowAsync<WebhookDeliveryFailedException>(() =>
            job.DispatchAsync(sub.Id, "1", "issue.created",
                """{"event":"created"}""", null, CancellationToken.None));
    }
}

internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _sendAsync;

    public FakeHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> sendAsync)
        => _sendAsync = sendAsync;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => _sendAsync(request);
}
