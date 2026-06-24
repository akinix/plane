namespace YH.Tests.WorkItems.Integration;

/// <summary>
/// Integration tests for <see cref="Issue"/> CRUD via InMemory DbContext (plan 04-05 Task 2).
/// </summary>
[Collection("WorkItemsTest")]
public sealed class IssueCrudTests : IDisposable
{
    private readonly WorkItemsDbContext _db;
    private static readonly Guid ProjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public IssueCrudTests()
    {
        _db = WorkItemsTestFixture.CreateInMemoryContext();
    }

    [Fact]
    public async Task CreateIssue_PersistsToDatabase()
    {
        var issue = TestIssueFactory.CreateValid(projectId: ProjectId);
        _db.Issues.Add(issue);
        await _db.SaveChangesAsync();

        var saved = await _db.Issues.FindAsync(issue.Id);
        saved.ShouldNotBeNull();
        saved.Name.ShouldBe("Test Issue");
        saved.SortOrder.ShouldBe(65535.0);
    }

    [Fact]
    public async Task UpdateIssue_PersistsChanges()
    {
        var issue = TestIssueFactory.CreateValid("Original", ProjectId);
        _db.Issues.Add(issue);
        await _db.SaveChangesAsync();

        issue.UpdateDetails(name: "Updated", priority: "high");
        await _db.SaveChangesAsync();

        var reloaded = await _db.Issues.FindAsync(issue.Id);
        reloaded.ShouldNotBeNull();
        reloaded.Name.ShouldBe("Updated");
        reloaded.Priority.ShouldBe("high");
    }

    [Fact]
    public async Task SoftDeleteIssue_MarksAsDeleted()
    {
        var issue = TestIssueFactory.CreateValid(projectId: ProjectId);
        _db.Issues.Add(issue);
        await _db.SaveChangesAsync();

        issue.SoftDelete(DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync();

        var reloaded = await _db.Issues.FindAsync(issue.Id);
        reloaded.ShouldNotBeNull();
        reloaded.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public async Task CreateIssueWithAssignees_PersistsAssignees()
    {
        var issue = TestIssueFactory.CreateValid(projectId: ProjectId);
        _db.Issues.Add(issue);
        await _db.SaveChangesAsync();

        // Add assignees directly to the join table to avoid InMemory concurrency issue
        // with the UpdateAssigneeList clear+add approach after SaveChanges.
        _db.IssueAssignees.Add(IssueAssignee.Create(issue.Id, "user-1"));
        _db.IssueAssignees.Add(IssueAssignee.Create(issue.Id, "user-2"));
        await _db.SaveChangesAsync();

        var saved = await _db.Issues
            .IgnoreQueryFilters()
            .Include(i => i.Assignees)
            .FirstAsync(i => i.Id == issue.Id);
        saved.Assignees.Count.ShouldBe(2);
    }

    [Fact]
    public async Task CreateIssueWithLabels_PersistsLabels()
    {
        var issue = TestIssueFactory.CreateValid(projectId: ProjectId);
        _db.Issues.Add(issue);
        await _db.SaveChangesAsync();

        // Add labels directly to the join table to avoid InMemory concurrency issue.
        _db.IssueLabels.Add(IssueLabel.Create(issue.Id, Guid.NewGuid()));
        _db.IssueLabels.Add(IssueLabel.Create(issue.Id, Guid.NewGuid()));
        await _db.SaveChangesAsync();

        var saved = await _db.Issues
            .IgnoreQueryFilters()
            .Include(i => i.Labels)
            .FirstAsync(i => i.Id == issue.Id);
        saved.Labels.Count.ShouldBe(2);
    }

    [Fact]
    public async Task ListIssues_ReturnsAllForProject()
    {
        _db.Issues.Add(TestIssueFactory.CreateValid("Issue 1", ProjectId));
        _db.Issues.Add(TestIssueFactory.CreateValid("Issue 2", ProjectId));
        _db.Issues.Add(TestIssueFactory.CreateValid("Issue 3", ProjectId));
        await _db.SaveChangesAsync();

        var issues = await _db.Issues.Where(i => i.ProjectId == ProjectId).ToListAsync();
        issues.Count.ShouldBe(3);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
