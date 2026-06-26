#pragma warning disable CA1822, S2325 // Disable "member does not access instance data" — kept as instance for IClassFixture pattern compatibility

using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using YH.Framework.Shared.Persistence;
using YH.Framework.Shared.Multitenancy;
using WorkItemModule = YH.Modules.WorkItems.Domain.Module;

namespace YH.Tests.Analytics.Fixtures;

/// <summary>
/// Shared test fixture for Analytics integration tests.
/// Creates isolated InMemory <see cref="WorkItemsDbContext"/> instances.
/// Tests must seed data directly using the real context's SaveChangesAsync
/// (which triggers Finbuckle's TenantNotSetMode.Overwrite).
/// </summary>
public sealed class AnalyticsTestFixture : IDisposable
{
    private const string DefaultSlug = "test-workspace";

    // NOTE: Finbuckle 10.1.0 with InMemory sets TenantId to string.Empty
    // (not the actual tenant ID) when entity implements IHasTenant without IMultiTenant.
    // We use string.Empty as the test tenant ID to match Finbuckle's behavior.
    // With real databases (PostgreSQL), Finbuckle correctly sets the actual tenant ID.
    internal static string DefaultTenantIdString => string.Empty;

    public (WorkItemsDbContext context, IMultiTenantContextAccessor<AppTenantInfo> tenantAccessor) CreateDbContextWithAccessor()
    {
        var accessor = CreateTenantAccessor();
        var options = new DbContextOptionsBuilder<WorkItemsDbContext>()
            .UseInMemoryDatabase($"analytics-{Guid.NewGuid():n}")
            .Options;
        var databaseOptions = Options.Create(new DatabaseOptions { Provider = "inmemory" });
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns("Development");

        var context = new WorkItemsDbContext(accessor, options, databaseOptions, environment);
        return (context, accessor);
    }

    public WorkItemsDbContext CreateDbContext() => CreateDbContextWithAccessor().context;

    /// <summary>Seeds data using real context's SaveChangesAsync (Finbuckle sets TenantId).</summary>
    public async Task<WorkItemsDbContext> SeedAsync(WorkItemsDbContext context, Guid? projectId = null)
    {
        var pid = projectId ?? Guid.NewGuid();
        var backlog = State.Create("Backlog", "#cccccc", StateGroup.Backlog, pid, true);
        var started = State.Create("Started", "#3b82f6", StateGroup.Started, pid, false);
        var completed = State.Create("Completed", "#22c55e", StateGroup.Completed, pid, false);
        var cancelled = State.Create("Cancelled", "#ef4444", StateGroup.Cancelled, pid, false);
        context.States.AddRange(backlog, started, completed, cancelled);
        await context.SaveChangesAsync();
        context.Issues.Add(Issue.Create("Backlog Issue", pid, stateId: backlog.Id));
        context.Issues.Add(Issue.Create("Started Issue", pid, stateId: started.Id));
        context.Issues.Add(Issue.Create("Completed Issue", pid, stateId: completed.Id));
        context.Cycles.Add(Cycle.Create("Cycle 1", pid));
        context.Modules.Add(WorkItemModule.Create("Module 1", pid));
        await context.SaveChangesAsync();
        return context;
    }

    /// <summary>Seeds multi-project data.</summary>
    public async Task<WorkItemsDbContext> SeedMultiProjectAsync(WorkItemsDbContext context)
    {
        var projectA = Guid.NewGuid(); var projectB = Guid.NewGuid();
        var stateA = State.Create("Todo", "#cccccc", StateGroup.Backlog, projectA, true);
        var doneA = State.Create("Done", "#22c55e", StateGroup.Completed, projectA, false);
        var stateB = State.Create("Todo", "#cccccc", StateGroup.Backlog, projectB, true);
        var doneB = State.Create("Done", "#22c55e", StateGroup.Completed, projectB, false);
        context.States.AddRange(stateA, doneA, stateB, doneB);
        await context.SaveChangesAsync();
        context.Issues.Add(Issue.Create("A-1", projectA, stateId: stateA.Id));
        context.Issues.Add(Issue.Create("A-2", projectA, stateId: doneA.Id));
        context.Issues.Add(Issue.Create("B-1", projectB, stateId: stateB.Id));
        context.Issues.Add(Issue.Create("B-2", projectB, stateId: stateB.Id));
        context.Issues.Add(Issue.Create("B-3", projectB, stateId: doneB.Id));
        context.Cycles.Add(Cycle.Create("Cycle A", projectA));
        context.Cycles.Add(Cycle.Create("Cycle B", projectB));
        await context.SaveChangesAsync();
        return context;
    }

    internal static IMultiTenantContextAccessor<AppTenantInfo> CreateTenantAccessor()
    {
        var accessor = Substitute.For<IMultiTenantContextAccessor<AppTenantInfo>>();
        var tenant = new AppTenantInfo(DefaultTenantIdString, DefaultSlug, "Test Workspace");
        var tenantContext = new MultiTenantContext<AppTenantInfo>(tenant);
        accessor.MultiTenantContext.Returns(tenantContext);
        return accessor;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
