using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using YH.Framework.Shared.Multitenancy;
using YH.Modules.Analytics.Features.v1.Overview.GetProjectAnalytics;
using YH.Modules.Analytics.Features.v1.Stats.GetProjectStats;
using YH.Tests.Analytics.Fixtures;
using YH.Tests.Analytics.TestData;

namespace YH.Tests.Analytics.Features;

public sealed class ProjectAnalyticsTests : IClassFixture<AnalyticsTestFixture>
{
    private readonly AnalyticsTestFixture _fixture;

    public ProjectAnalyticsTests(AnalyticsTestFixture fixture) => _fixture = fixture;

    private static IMultiTenantContextAccessor<AppTenantInfo> ValidAccessor()
    {
        var a = Substitute.For<IMultiTenantContextAccessor<AppTenantInfo>>();
        a.MultiTenantContext.Returns(new MultiTenantContext<AppTenantInfo>(
            new AppTenantInfo("test-tenant", "test-workspace", "Test")));
        return a;
    }

    private static AnalyticsQueryService CreateService(WorkItemsDbContext context)
    {
        return new AnalyticsQueryService(context, AnalyticsTestFixture.CreateTenantAccessor());
    }

    [Fact]
    public async Task ProjectAnalytics_ReturnsProjectWorkItemStats()
    {
        using var context = _fixture.CreateDbContext();
        var service = CreateService(context);
        var handler = new GetProjectAnalyticsQueryHandler(service);

        var result = await handler.Handle(
            new GetProjectAnalyticsQuery("test-workspace", Guid.NewGuid(), null, null, null),
            CancellationToken.None);

        var okResult = result.ShouldBeOfType<Ok<WorkItemStatsDto>>();
        okResult.Value.ShouldNotBeNull();
    }

    [Fact]
    public async Task ProjectAnalytics_WithEmptyProjectId_ReturnsBadRequest()
    {
        using var context = _fixture.CreateDbContext();
        var service = CreateService(context);
        var handler = new GetProjectAnalyticsQueryHandler(service);

        var result = await handler.Handle(
            new GetProjectAnalyticsQuery("test-workspace", Guid.Empty, null, null, null),
            CancellationToken.None);

        ((IStatusCodeHttpResult)result).StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task ProjectStats_WithInvalidType_ReturnsEmptyList()
    {
        using var context = _fixture.CreateDbContext();
        var service = CreateService(context);
        var handler = new GetProjectStatsQueryHandler(service);

        var result = await handler.Handle(
            new GetProjectStatsQuery("test-workspace", Guid.NewGuid(), "invalid-type", null, null, null),
            CancellationToken.None);

        result.ShouldBeEmpty();
    }
}
