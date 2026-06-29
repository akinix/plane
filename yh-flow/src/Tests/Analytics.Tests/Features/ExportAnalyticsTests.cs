using System.Text;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using YH.Framework.Jobs.Services;
using YH.Framework.Shared.Multitenancy;
using YH.Modules.Analytics.Features.v1.Export.ExportAnalytics;
using YH.Modules.Analytics.Services;
using YH.Tests.Analytics.Fixtures;
using YH.Tests.Analytics.TestData;

namespace YH.Tests.Analytics.Features;

/// <summary>
/// Integration tests for CSV export and Hangfire job enqueue.
/// </summary>
public sealed class ExportAnalyticsTests : IClassFixture<AnalyticsTestFixture>
{
    private readonly AnalyticsTestFixture _fixture;

    public ExportAnalyticsTests(AnalyticsTestFixture fixture) => _fixture = fixture;

    /// <summary>Creates a non-empty tenant accessor for handler tests.</summary>
    private static IMultiTenantContextAccessor<AppTenantInfo> ValidAccessor()
    {
        var accessor = Substitute.For<IMultiTenantContextAccessor<AppTenantInfo>>();
        var tenant = new AppTenantInfo("test-tenant", "test-workspace", "Test");
        var tenantContext = new MultiTenantContext<AppTenantInfo>(tenant);
        accessor.MultiTenantContext.Returns(tenantContext);
        return accessor;
    }

    /// <summary>Strips BOM from UTF-8 CSV bytes for assertion.</summary>
    private static string CsvText(byte[] csvBytes)
    {
        var bomless = csvBytes.AsSpan(Encoding.UTF8.GetPreamble().Length).ToArray();
        return Encoding.UTF8.GetString(bomless);
    }

    [Fact]
    public async Task ExportCsv_GeneratesValidCsv()
    {
        using var context = _fixture.CreateDbContext();
        var pid = Guid.NewGuid();
        var state = State.Create("Todo", "#ccc", StateGroup.Backlog, pid, true);
        var done = State.Create("Done", "#22c55e", StateGroup.Completed, pid, false);
        context.States.AddRange(state, done);
        await context.SaveChangesAsync();

        context.Issues.Add(Issue.Create("Test Issue 1", pid, stateId: state.Id));
        context.Issues.Add(Issue.Create("Test Issue 2", pid, stateId: done.Id));
        await context.SaveChangesAsync();

        var sp = new ServiceCollection().AddScoped(_ => context).BuildServiceProvider();
        var exportService = new AnalyticsExportService(sp.GetRequiredService<IServiceScopeFactory>());

        var csvBytes = await exportService.GenerateCsvAsync(
            AnalyticsTestFixture.DefaultTenantIdString, "test-workspace", null, CancellationToken.None);

        var csvText = CsvText(csvBytes);
        csvText.ShouldStartWith("Issue ID,Name,State Group,Priority,Project Name,Assignee,Created At,Completed At");
        csvText.ShouldContain("Test Issue 1");
        var lines = csvText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Length.ShouldBe(3);
    }

    [Fact]
    public async Task ExportCsv_EmptyWorkspace_ReturnsHeaderOnly()
    {
        using var context = _fixture.CreateDbContext();
        var sp = new ServiceCollection().AddScoped(_ => context).BuildServiceProvider();
        var exportService = new AnalyticsExportService(sp.GetRequiredService<IServiceScopeFactory>());

        var csvBytes = await exportService.GenerateCsvAsync(
            AnalyticsTestFixture.DefaultTenantIdString, "test-workspace", null, CancellationToken.None);

        var csvText = CsvText(csvBytes);
        csvText.ShouldStartWith("Issue ID,Name,State Group,Priority,Project Name,Assignee,Created At,Completed At");
        var lines = csvText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Length.ShouldBe(1);
    }

    [Fact]
    public async Task ExportAnalyticsCommand_EnqueuesHangfireJob()
    {
        var jobService = Substitute.For<IJobService>();
        var handler = new ExportAnalyticsCommandHandler(jobService, ValidAccessor());

        // Act
        var result = await handler.Handle(
            new ExportAnalyticsCommand("test-workspace", null),
            CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();

        // Verify Hangfire job was enqueued
        jobService.Received(1).Enqueue<ExportAnalyticsJob>(Arg.Any<System.Linq.Expressions.Expression<System.Func<ExportAnalyticsJob, System.Threading.Tasks.Task>>>());
    }

    [Fact]
    public async Task ExportAnalyticsCommand_WithoutTenant_ReturnsUnauthorized()
    {
        // Arrange
        var jobService = Substitute.For<IJobService>();
        var nullAccessor = Substitute.For<IMultiTenantContextAccessor<AppTenantInfo>>();
        nullAccessor.MultiTenantContext.Returns((IMultiTenantContext<AppTenantInfo>?)null);
        var handler = new ExportAnalyticsCommandHandler(jobService, nullAccessor);

        // Act
        var result = await handler.Handle(
            new ExportAnalyticsCommand("test-workspace", null),
            CancellationToken.None);

        // Assert
        result.ShouldBeOfType<UnauthorizedHttpResult>();
        jobService.DidNotReceiveWithAnyArgs().Enqueue<ExportAnalyticsJob>(default!);
    }
}
