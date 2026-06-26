using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using YH.Framework.Shared.Multitenancy;
using YH.Modules.Analytics.Features.v1.Charts.GetWorkspaceChart;
using YH.Modules.Analytics.Features.v1.Charts.GetProjectChart;
using YH.Tests.Analytics.Fixtures;
using YH.Tests.Analytics.TestData;

namespace YH.Tests.Analytics.Features;

public sealed class ChartAnalyticsTests : IClassFixture<AnalyticsTestFixture>
{
    private readonly AnalyticsTestFixture _fixture;

    public ChartAnalyticsTests(AnalyticsTestFixture fixture) => _fixture = fixture;

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
    public async Task WorkspaceWorkItemChart_EmptyIssues_ReturnsMonths()
    {
        using var context = _fixture.CreateDbContext();
        var service = CreateService(context);
        var handler = new GetWorkspaceChartQueryHandler(service);

        var result = await handler.Handle(
            new GetWorkspaceChartQuery("test-workspace", "work-items", null, null, null, null),
            CancellationToken.None);

        var okResult = result.ShouldBeOfType<Ok<AnalyticsChartDto>>();
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Data.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ProjectWorkItemChart_WithInvalidType_ReturnsBadRequest()
    {
        using var context = _fixture.CreateDbContext();
        var service = CreateService(context);
        var handler = new GetProjectChartQueryHandler(service);

        var result = await handler.Handle(
            new GetProjectChartQuery("test-workspace", Guid.NewGuid(), "invalid-type", null, null, null, null, null),
            CancellationToken.None);

        ((IStatusCodeHttpResult)result).StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
    }
}
