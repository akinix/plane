using YH.Tests.Analytics.Fixtures;

namespace YH.Tests.Analytics.Services;

public sealed class AnalyticsQueryServiceTests : IClassFixture<AnalyticsTestFixture>
{
    private readonly AnalyticsTestFixture _fixture;

    public AnalyticsQueryServiceTests(AnalyticsTestFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Baseline_SeedingWithRealContext_ReturnsResults()
    {
        var (context, accessor) = _fixture.CreateDbContextWithAccessor();
        var service = new AnalyticsQueryService(context, accessor);

        var pid = Guid.NewGuid();
        var state = State.Create("Backlog", "#ccc", StateGroup.Backlog, pid, true);
        context.States.Add(state);
        await context.SaveChangesAsync();
        context.Issues.Add(Issue.Create("Test", pid, stateId: state.Id));
        await context.SaveChangesAsync();

        var result = await service.GetWorkItemStatsAsync("test-workspace", null, null, null, null, CancellationToken.None);

        result.ShouldNotBeNull();
        result.TotalWorkItems.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetWorkItemStatsAsync_WithDateFilter_FiltersCorrectly()
    {
        var (context, accessor) = _fixture.CreateDbContextWithAccessor();
        var service = new AnalyticsQueryService(context, accessor);
        var pid = Guid.NewGuid();

        var state = State.Create("Backlog", "#ccc", StateGroup.Backlog, pid, true);
        context.States.Add(state);
        await context.SaveChangesAsync();

        // Old issue (last month) and new issue (this month)
        var oldIssue = Issue.Create("Old", pid, stateId: state.Id);
        typeof(Issue).GetProperty("CreatedOnUtc", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(oldIssue, DateTimeOffset.UtcNow.AddMonths(-1));
        context.Issues.Add(oldIssue);

        var newIssue = Issue.Create("New", pid, stateId: state.Id);
        typeof(Issue).GetProperty("CreatedOnUtc", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(newIssue, DateTimeOffset.UtcNow.AddDays(-2));
        context.Issues.Add(newIssue);
        await context.SaveChangesAsync();

        var result = await service.GetWorkItemStatsAsync("test-workspace", "this_month", null, null, null, CancellationToken.None);

        result.TotalWorkItems.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetWorkItemStatsAsync_WithProjectIds_FiltersByProject()
    {
        var (context, accessor) = _fixture.CreateDbContextWithAccessor();
        var service = new AnalyticsQueryService(context, accessor);
        var projectA = Guid.NewGuid();
        var projectB = Guid.NewGuid();

        var stateA = State.Create("Todo", "#ccc", StateGroup.Backlog, projectA, true);
        var stateB = State.Create("Todo", "#ccc", StateGroup.Backlog, projectB, true);
        context.States.AddRange(stateA, stateB);
        await context.SaveChangesAsync();

        context.Issues.Add(Issue.Create("A-1", projectA, stateId: stateA.Id));
        context.Issues.Add(Issue.Create("B-1", projectB, stateId: stateB.Id));
        context.Issues.Add(Issue.Create("B-2", projectB, stateId: stateB.Id));
        await context.SaveChangesAsync();

        var result = await service.GetWorkItemStatsAsync("test-workspace", null, null, null, projectA.ToString(), CancellationToken.None);

        result.TotalWorkItems.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetWorkspaceOverviewAsync_ReturnsAllSections()
    {
        var (context, accessor) = _fixture.CreateDbContextWithAccessor();
        var service = new AnalyticsQueryService(context, accessor);
        var pid = Guid.NewGuid();

        var state = State.Create("Todo", "#ccc", StateGroup.Backlog, pid, true);
        context.States.Add(state);
        await context.SaveChangesAsync();
        context.Issues.Add(Issue.Create("Issue 1", pid, stateId: state.Id));
        context.Cycles.Add(Cycle.Create("Cycle 1", pid));
        context.Modules.Add(YH.Modules.WorkItems.Domain.Module.Create("Module 1", pid));
        await context.SaveChangesAsync();

        var result = await service.GetWorkspaceOverviewAsync("test-workspace", null, null, null, null, CancellationToken.None);

        result.TotalWorkItems.Count.ShouldBe(1);
        result.TotalCycles.Count.ShouldBe(1);
        result.TotalModules.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetProjectWorkItemStatsAsync_ReturnsProjectScopedStats()
    {
        var (context, accessor) = _fixture.CreateDbContextWithAccessor();
        var service = new AnalyticsQueryService(context, accessor);
        var projectA = Guid.NewGuid();
        var projectB = Guid.NewGuid();

        var stateA = State.Create("Todo", "#ccc", StateGroup.Backlog, projectA, true);
        var stateB = State.Create("Todo", "#ccc", StateGroup.Backlog, projectB, true);
        context.States.AddRange(stateA, stateB);
        await context.SaveChangesAsync();

        context.Issues.Add(Issue.Create("A-1", projectA, stateId: stateA.Id));
        context.Issues.Add(Issue.Create("B-1", projectB, stateId: stateB.Id));
        await context.SaveChangesAsync();

        var result = await service.GetProjectWorkItemStatsAsync("test-workspace", projectA, null, null, null, CancellationToken.None);

        result.TotalWorkItems.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetAssigneeGroupedStatsAsync_ReturnsAssigneeDistribution()
    {
        var (context, accessor) = _fixture.CreateDbContextWithAccessor();
        var service = new AnalyticsQueryService(context, accessor);
        var pid = Guid.NewGuid();

        var state = State.Create("Todo", "#ccc", StateGroup.Backlog, pid, true);
        var done = State.Create("Done", "#22c55e", StateGroup.Completed, pid, false);
        context.States.AddRange(state, done);
        await context.SaveChangesAsync();

        var user1 = Guid.NewGuid().ToString();
        var user2 = Guid.NewGuid().ToString();

        var issue1 = Issue.Create("Issue 1", pid, stateId: state.Id);
        var issue2 = Issue.Create("Issue 2", pid, stateId: done.Id);
        var issue3 = Issue.Create("Issue 3", pid, stateId: state.Id);
        context.Issues.AddRange(issue1, issue2, issue3);
        await context.SaveChangesAsync();

        context.IssueAssignees.Add(IssueAssignee.Create(issue1.Id, user1));
        context.IssueAssignees.Add(IssueAssignee.Create(issue2.Id, user1));
        context.IssueAssignees.Add(IssueAssignee.Create(issue3.Id, user2));
        await context.SaveChangesAsync();

        var result = await service.GetAssigneeGroupedStatsAsync("test-workspace", pid, null, null, null, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        var u1 = result.First(a => a.AssigneeId.ToString() == user1);
        u1.BacklogWorkItems.ShouldBe(1);
        u1.CompletedWorkItems.ShouldBe(1);
        var u2 = result.First(a => a.AssigneeId.ToString() == user2);
        u2.BacklogWorkItems.ShouldBe(1);
    }

    [Fact]
    public async Task GetWorkItemStatsAsync_IncludesPriorityDistribution()
    {
        var (context, accessor) = _fixture.CreateDbContextWithAccessor();
        var service = new AnalyticsQueryService(context, accessor);
        var pid = Guid.NewGuid();

        var state = State.Create("Backlog", "#ccc", StateGroup.Backlog, pid, true);
        context.States.Add(state);
        await context.SaveChangesAsync();

        var urgent = Issue.Create("Urgent", pid, stateId: state.Id, priority: "urgent");
        var high = Issue.Create("High", pid, stateId: state.Id, priority: "high");
        var medium = Issue.Create("Medium", pid, stateId: state.Id, priority: "medium");
        var low = Issue.Create("Low", pid, stateId: state.Id, priority: "low");
        var none = Issue.Create("None", pid, stateId: state.Id, priority: "none");
        context.Issues.AddRange(urgent, high, medium, low, none);
        await context.SaveChangesAsync();

        var result = await service.GetWorkItemStatsAsync("test-workspace", null, null, null, null, CancellationToken.None);

        result.UrgentWorkItems.Count.ShouldBe(1);
        result.HighPriorityWorkItems.Count.ShouldBe(1);
        result.MediumPriorityWorkItems.Count.ShouldBe(1);
        result.LowPriorityWorkItems.Count.ShouldBe(1);
        result.NonePriorityWorkItems.Count.ShouldBe(1);
        result.TotalWorkItems.Count.ShouldBe(5);
    }
}
