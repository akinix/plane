#pragma warning disable CA1822, S2325 // Disable "member does not access instance data" — kept as instance for IClassFixture pattern compatibility

using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using YH.Framework.Shared.Persistence;
using YH.Framework.Shared.Multitenancy;
using YH.Tests.Analytics.TestData;
using WorkItemModule = YH.Modules.WorkItems.Domain.Module;

namespace YH.Tests.Analytics.Fixtures;

/// <summary>
/// Shared test fixture for Analytics integration tests.
/// Creates isolated InMemory <see cref="WorkItemsDbContext"/> instances,
/// matching the pattern established by <c>ViewTestFixture</c>.
/// </summary>
public sealed class AnalyticsTestFixture : IDisposable
{
    private static readonly Guid DefaultTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private const string DefaultSlug = "test-workspace";
    private const string DefaultName = "Test Workspace";

    /// <summary>
    /// Default tenant ID exposed for test use.
    /// </summary>
    internal static string DefaultTenantIdString => DefaultTenantId.ToString();

    /// <summary>
    /// Creates a fresh InMemory <see cref="WorkItemsDbContext"/> with a unique database name
    /// (isolated per call) and a stubbed <see cref="IMultiTenantContextAccessor{AppTenantInfo}"/>
    /// pointing at the default test tenant.
    /// </summary>
    public WorkItemsDbContext CreateDbContext()
    {
        var accessor = Substitute.For<IMultiTenantContextAccessor<AppTenantInfo>>();
        var tenant = new AppTenantInfo(DefaultTenantId.ToString(), DefaultSlug, DefaultName);
        var tenantContext = new MultiTenantContext<AppTenantInfo>(tenant);
        accessor.MultiTenantContext.Returns(tenantContext);

        var options = new DbContextOptionsBuilder<WorkItemsDbContext>()
            .UseInMemoryDatabase($"analytics-{Guid.NewGuid():n}")
            .Options;
        var databaseOptions = Options.Create(new DatabaseOptions
        {
            Provider = "inmemory"
        });
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns("Development");

        return new WorkItemsDbContext(accessor, options, databaseOptions, environment);
    }

    /// <summary>
    /// Seeds the provided <see cref="WorkItemsDbContext"/> with standard test data:
    /// 4 states, 3 issues (backlog/started/completed), 1 cycle, 1 module.
    /// </summary>
    /// <param name="context">The DbContext to seed.</param>
    /// <param name="projectId">Optional project ID override. If null, a random one is generated.</param>
    /// <returns>The seeded DbContext.</returns>
    public async Task<WorkItemsDbContext> SeedAsync(WorkItemsDbContext context, Guid? projectId = null)
    {
        var pid = projectId ?? Guid.NewGuid();

        // Create states
        var backlogState = TestIssueFactory.CreateState("Backlog", StateGroup.Backlog, pid, "#cccccc", isDefault: true);
        var startedState = TestIssueFactory.CreateState("Started", StateGroup.Started, pid, "#3b82f6");
        var completedState = TestIssueFactory.CreateState("Completed", StateGroup.Completed, pid, "#22c55e");
        var cancelledState = TestIssueFactory.CreateState("Cancelled", StateGroup.Cancelled, pid, "#ef4444");

        context.States.AddRange(backlogState, startedState, completedState, cancelledState);
        await context.SaveChangesAsync();

        // Create issues with different states
        context.Issues.Add(TestIssueFactory.CreateIssue("Backlog Issue", pid, stateId: backlogState.Id));
        context.Issues.Add(TestIssueFactory.CreateIssue("Started Issue", pid, stateId: startedState.Id));
        context.Issues.Add(TestIssueFactory.CreateIssue("Completed Issue", pid, stateId: completedState.Id,
            completedAt: DateTimeOffset.UtcNow.AddDays(-1)));

        // Create cycles and modules
        var cycle = Cycle.Create("Cycle 1", pid);
        context.Cycles.Add(cycle);
        context.Modules.Add(WorkItemModule.Create("Module 1", pid));

        await context.SaveChangesAsync();
        return context;
    }

    /// <summary>
    /// Seeds the provided <see cref="WorkItemsDbContext"/> with data spanning multiple projects.
    /// </summary>
    public async Task<WorkItemsDbContext> SeedMultiProjectAsync(WorkItemsDbContext context)
    {
        var projectA = Guid.NewGuid();
        var projectB = Guid.NewGuid();

        // States for project A
        var stateA = TestIssueFactory.CreateState("Todo", StateGroup.Backlog, projectA, "#cccccc", isDefault: true);
        var doneA = TestIssueFactory.CreateState("Done", StateGroup.Completed, projectA, "#22c55e");
        context.States.AddRange(stateA, doneA);

        // States for project B
        var stateB = TestIssueFactory.CreateState("Todo", StateGroup.Backlog, projectB, "#cccccc", isDefault: true);
        var doneB = TestIssueFactory.CreateState("Done", StateGroup.Completed, projectB, "#22c55e");
        context.States.AddRange(stateB, doneB);
        await context.SaveChangesAsync();

        // Issues: 2 in project A, 3 in project B
        context.Issues.Add(TestIssueFactory.CreateIssue("A-1", projectA, stateId: stateA.Id));
        context.Issues.Add(TestIssueFactory.CreateIssue("A-2", projectA, stateId: doneA.Id,
            completedAt: DateTimeOffset.UtcNow.AddDays(-2)));
        context.Issues.Add(TestIssueFactory.CreateIssue("B-1", projectB, stateId: stateB.Id));
        context.Issues.Add(TestIssueFactory.CreateIssue("B-2", projectB, stateId: stateB.Id));
        context.Issues.Add(TestIssueFactory.CreateIssue("B-3", projectB, stateId: doneB.Id,
            completedAt: DateTimeOffset.UtcNow.AddDays(-1)));

        // Cycles
        context.Cycles.Add(Cycle.Create("Cycle A", projectA));
        context.Cycles.Add(Cycle.Create("Cycle B", projectB));

        await context.SaveChangesAsync();
        return context;
    }

    /// <summary>
    /// Creates an <see cref="IMultiTenantContextAccessor{AppTenantInfo}"/> pointing at the default test tenant.
    /// </summary>
    internal static IMultiTenantContextAccessor<AppTenantInfo> CreateTenantAccessor()
    {
        var accessor = Substitute.For<IMultiTenantContextAccessor<AppTenantInfo>>();
        var tenant = new AppTenantInfo(DefaultTenantId.ToString(), DefaultSlug, DefaultName);
        var tenantContext = new MultiTenantContext<AppTenantInfo>(tenant);
        accessor.MultiTenantContext.Returns(tenantContext);
        return accessor;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
