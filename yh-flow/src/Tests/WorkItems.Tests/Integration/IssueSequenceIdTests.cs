namespace YH.Tests.WorkItems.Integration;

/// <summary>
/// Integration tests for Issue SequenceId auto-increment (plan 04-05 Task 2).
/// Note: InMemory provider does not support SERIALIZABLE transactions or raw SQL MAX(SequenceId)+1.
/// These tests verify the data model invariants that the production <c>IssueSequenceService</c>
/// relies on: SequenceId is mutable (internal set), project-scoped, and stored correctly.
/// </summary>
[Collection("WorkItemsTest")]
public sealed class IssueSequenceIdTests : IDisposable
{
    private readonly WorkItemsDbContext _db;
    private static readonly Guid ProjectA = Guid.Parse("00000000-0000-0000-0000-aaaaaaaa0001");
    private static readonly Guid ProjectB = Guid.Parse("00000000-0000-0000-0000-bbbbbb000001");

    public IssueSequenceIdTests()
    {
        _db = WorkItemsTestFixture.CreateInMemoryContext();
    }

    [Fact]
    public async Task SequenceId_IsSettableViaInternalSet()
    {
        var issue = Issue.Create("Test", ProjectA);
        // Simulate what IssueSequenceService does — set SequenceId before save
        var prop = issue.GetType().GetProperty("SequenceId");
        prop.ShouldNotBeNull();
        prop.SetValue(issue, 1);
        _db.Issues.Add(issue);
        await _db.SaveChangesAsync();

        var saved = await _db.Issues.FindAsync(issue.Id);
        saved.ShouldNotBeNull();
        saved.SequenceId.ShouldBe(1);
    }

    [Fact]
    public async Task SequenceId_CanBeAssignedAfterCreate_BeforeSave()
    {
        var issue = Issue.Create("Task", ProjectA);
        var prop = issue.GetType().GetProperty("SequenceId");
        prop.ShouldNotBeNull();
        prop.SetValue(issue, 42);
        _db.Issues.Add(issue);
        await _db.SaveChangesAsync();

        var saved = await _db.Issues.FindAsync(issue.Id);
        saved.ShouldNotBeNull();
        saved.SequenceId.ShouldBe(42);
    }

    [Fact]
    public async Task IssuesInDifferentProjects_CanHaveSameSequenceId()
    {
        var issueA = Issue.Create("A-1", ProjectA);
        var propA = issueA.GetType().GetProperty("SequenceId");
        propA.ShouldNotBeNull();
        propA.SetValue(issueA, 1);
        _db.Issues.Add(issueA);

        var issueB = Issue.Create("B-1", ProjectB);
        var propB = issueB.GetType().GetProperty("SequenceId");
        propB.ShouldNotBeNull();
        propB.SetValue(issueB, 1);
        _db.Issues.Add(issueB);

        await _db.SaveChangesAsync();

        var idsA = await _db.Issues.Where(i => i.ProjectId == ProjectA).Select(i => i.SequenceId).ToListAsync();
        var idsB = await _db.Issues.Where(i => i.ProjectId == ProjectB).Select(i => i.SequenceId).ToListAsync();

        idsA.ShouldBe(new[] { 1 });
        idsB.ShouldBe(new[] { 1 });
    }

    [Fact]
    public async Task SequenceId_DefaultsToZero_WhenNotSet()
    {
        // Without IssueSequenceService invocation, SequenceId defaults to 0 (int default).
        var issue = Issue.Create("Unsequenced", ProjectA);
        issue.SequenceId.ShouldBe(0);

        _db.Issues.Add(issue);
        await _db.SaveChangesAsync();

        var saved = await _db.Issues.FindAsync(issue.Id);
        saved.ShouldNotBeNull();
        saved.SequenceId.ShouldBe(0);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
