using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using YH.Framework.Shared.Multitenancy;
using YH.Modules.Analytics.Features.v1.Overview.GetWorkspaceAnalytics;
using YH.Modules.Analytics.Features.v1.Stats.GetWorkspaceStats;
using YH.Tests.Analytics.Fixtures;
using YH.Tests.Analytics.TestData;

namespace YH.Tests.Analytics.Features;

/// <summary>
/// Handler tests for workspace analytics. Uses real AnalyticsQueryService with
/// InMemory DbContext. Handler authorization checks require a non-empty TenantId
/// (Finbuckle bug with InMemory sets TenantId to "").
/// </summary>
public sealed class WorkspaceAnalyticsTests : IClassFixture<AnalyticsTestFixture>
{
    private readonly AnalyticsTestFixture _fixture;

    public WorkspaceAnalyticsTests(AnalyticsTestFixture fixture) => _fixture = fixture;

    /// <summary>Creates a tenant accessor with a non-empty tenant ID for handler tests.</summary>
    private static IMultiTenantContextAccessor<AppTenantInfo> CreateValidAccessor()
    {
        var accessor = Substitute.For<IMultiTenantContextAccessor<AppTenantInfo>>();
        var tenant = new AppTenantInfo("test-tenant", "test-workspace", "Test Workspace");
        var tenantContext = new MultiTenantContext<AppTenantInfo>(tenant);
        accessor.MultiTenantContext.Returns(tenantContext);
        return accessor;
    }

    private static AnalyticsQueryService CreateService(WorkItemsDbContext context)
    {
        // NOTE: Service uses empty-string accessor matching InMemory Finbuckle behavior.
        // Production uses a real TenantId via DI registered interceptor.
        var accessor = AnalyticsTestFixture.CreateTenantAccessor();
        return new AnalyticsQueryService(context, accessor);
    }

    [Fact]
    public async Task WorkspaceOverview_WithOverviewTab_ReturnsOverviewDto()
    {
        using var context = _fixture.CreateDbContext();
        var service = CreateService(context);
        var handler = new GetWorkspaceAnalyticsQueryHandler(service, CreateValidAccessor());

        var result = await handler.Handle(
            new GetWorkspaceAnalyticsQuery("test-workspace", "overview", null, null, null, null),
            CancellationToken.None);

        result.ShouldBeOfType<Ok<AnalyticsOverviewDto>>();
    }

    [Fact]
    public async Task WorkspaceAnalytics_WithWorkItemsTab_ReturnsWorkItemStatsDto()
    {
        using var context = _fixture.CreateDbContext();
        var service = CreateService(context);
        var handler = new GetWorkspaceAnalyticsQueryHandler(service, CreateValidAccessor());

        var result = await handler.Handle(
            new GetWorkspaceAnalyticsQuery("test-workspace", "work-items", null, null, null, null),
            CancellationToken.None);

        result.ShouldBeOfType<Ok<WorkItemStatsDto>>();
    }

    [Fact]
    public async Task WorkspaceAnalytics_WithInvalidTab_ReturnsBadRequest()
    {
        using var context = _fixture.CreateDbContext();
        var service = CreateService(context);
        var handler = new GetWorkspaceAnalyticsQueryHandler(service, CreateValidAccessor());

        var result = await handler.Handle(
            new GetWorkspaceAnalyticsQuery("test-workspace", "invalid-tab", null, null, null, null),
            CancellationToken.None);

        ((IStatusCodeHttpResult)result).StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task WorkspaceAnalytics_WithoutTenant_ReturnsUnauthorized()
    {
        using var context = _fixture.CreateDbContext();
        var service = CreateService(context);
        var nullAccessor = Substitute.For<IMultiTenantContextAccessor<AppTenantInfo>>();
        nullAccessor.MultiTenantContext.Returns((IMultiTenantContext<AppTenantInfo>?)null);
        var handler = new GetWorkspaceAnalyticsQueryHandler(service, nullAccessor);

        var result = await handler.Handle(
            new GetWorkspaceAnalyticsQuery("test-workspace", "overview", null, null, null, null),
            CancellationToken.None);

        result.ShouldBeOfType<UnauthorizedHttpResult>();
    }

    [Fact]
    public async Task WorkspaceStats_WithWorkItemsType_ReturnsProjectGroupedStats()
    {
        using var context = _fixture.CreateDbContext();
        var service = CreateService(context);
        var handler = new GetWorkspaceStatsQueryHandler(service);

        var result = await handler.Handle(
            new GetWorkspaceStatsQuery("test-workspace", "work-items", null, null, null, null),
            CancellationToken.None);

        result.ShouldBeOfType<List<ProjectStatsDto>>();
    }

    [Fact]
    public async Task WorkspaceStats_WithInvalidType_ReturnsEmptyList()
    {
        using var context = _fixture.CreateDbContext();
        var service = CreateService(context);
        var handler = new GetWorkspaceStatsQueryHandler(service);

        var result = await handler.Handle(
            new GetWorkspaceStatsQuery("test-workspace", "invalid-type", null, null, null, null),
            CancellationToken.None);

        result.ShouldBeEmpty();
    }
}
