namespace YH.Tests.WorkItems.Integration;

/// <summary>
/// Integration tests for issue list filtering via InMemory DbContext (plan 04-05 Task 2).
/// Tests filtering by state, priority, assignee, label, and draft status.
/// </summary>
[Collection("WorkItemsTest")]
public sealed class IssueListFilterTests : IDisposable
{
    private readonly WorkItemsDbContext _db;
    private static readonly Guid ProjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid StateId = Guid.NewGuid();

    public IssueListFilterTests()
    {
        _db = WorkItemsTestFixture.CreateInMemoryContext();
    }

    [Fact]
    public async Task FilterByPriority_ReturnsMatchingIssues()
    {
        _db.Issues.Add(Issue.Create("Urgent", ProjectId, stateId: StateId, priority: "high"));
        _db.Issues.Add(Issue.Create("Trivial", ProjectId, priority: "low"));
        _db.Issues.Add(Issue.Create("Normal", ProjectId, priority: "none"));
        await _db.SaveChangesAsync();

        var highIssues = await _db.Issues
            .Where(i => i.Priority == "high" && i.ProjectId == ProjectId)
            .ToListAsync();

        highIssues.Count.ShouldBe(1);
        highIssues[0].Name.ShouldBe("Urgent");
    }

    [Fact]
    public async Task FilterByDraft_ReturnsDraftIssues()
    {
        _db.Issues.Add(Issue.Create("Regular", ProjectId));
        _db.Issues.Add(Issue.Create("Draft", ProjectId, isDraft: true));
        await _db.SaveChangesAsync();

        var drafts = await _db.Issues
            .Where(i => i.IsDraft && i.ProjectId == ProjectId)
            .ToListAsync();

        drafts.Count.ShouldBe(1);
        drafts[0].Name.ShouldBe("Draft");
    }

    [Fact]
    public async Task FilterByState_ReturnsIssuesInState()
    {
        _db.Issues.Add(Issue.Create("In State", ProjectId, stateId: StateId));
        _db.Issues.Add(Issue.Create("No State", ProjectId));
        await _db.SaveChangesAsync();

        var stateIssues = await _db.Issues
            .Where(i => i.StateId == StateId && i.ProjectId == ProjectId)
            .ToListAsync();

        stateIssues.Count.ShouldBe(1);
        stateIssues[0].Name.ShouldBe("In State");
    }

    [Fact]
    public async Task FilterByAssignee_ReturnsIssuesForUser()
    {
        var issue = Issue.Create("Assigned", ProjectId);
        _db.Issues.Add(issue);
        await _db.SaveChangesAsync();

        // Add assignee directly to the join table (bypassing the Issue.UpdateAssigneeList which
        // has InMemory concurrency issues when called after SaveChanges).
        var assignee = IssueAssignee.Create(issue.Id, "user-001");
        _db.IssueAssignees.Add(assignee);
        await _db.SaveChangesAsync();

        var assignedIssues = await _db.Issues
            .IgnoreQueryFilters()
            .Where(i => i.Assignees.Any(a => a.AssigneeId == "user-001"))
            .ToListAsync();

        assignedIssues.Count.ShouldBe(1);
        assignedIssues[0].Id.ShouldBe(issue.Id);
    }

    [Fact]
    public async Task FilterByLabel_ReturnsIssuesWithLabel()
    {
        var labelId = Guid.NewGuid();
        var issue = Issue.Create("Labeled", ProjectId);
        _db.Issues.Add(issue);
        await _db.SaveChangesAsync();

        var issueLabel = IssueLabel.Create(issue.Id, labelId);
        _db.IssueLabels.Add(issueLabel);
        await _db.SaveChangesAsync();

        var labeledIssues = await _db.Issues
            .IgnoreQueryFilters()
            .Where(i => i.Labels.Any(l => l.LabelId == labelId))
            .ToListAsync();

        labeledIssues.Count.ShouldBe(1);
        labeledIssues[0].Id.ShouldBe(issue.Id);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
