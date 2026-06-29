namespace YH.Tests.WorkItems.Integration;

/// <summary>
/// Integration tests for issue bulk update scenarios via InMemory DbContext (plan 04-05 Task 2).
/// Tests batch state changes, priority updates, and assignee/label replacement.
/// </summary>
[Collection("WorkItemsTest")]
public sealed class IssueBulkUpdateTests : IDisposable
{
    private readonly WorkItemsDbContext _db;
    private static readonly Guid ProjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid NewStateId = Guid.NewGuid();

    public IssueBulkUpdateTests()
    {
        _db = WorkItemsTestFixture.CreateInMemoryContext();
    }

    [Fact]
    public async Task BulkStateChange_UpdatesAllIssues()
    {
        var issues = new[]
        {
            Issue.Create("Issue 1", ProjectId),
            Issue.Create("Issue 2", ProjectId),
            Issue.Create("Issue 3", ProjectId),
        };
        _db.Issues.AddRange(issues);
        await _db.SaveChangesAsync();

        foreach (var issue in issues)
        {
            issue.UpdateState(NewStateId, isCompletedGroup: false, isCancelledGroup: false);
        }
        await _db.SaveChangesAsync();

        var allInNewState = await _db.Issues
            .Where(i => i.ProjectId == ProjectId && i.StateId == NewStateId)
            .ToListAsync();
        allInNewState.Count.ShouldBe(3);
    }

    [Fact]
    public async Task BulkPriorityUpdate_ChangesAllPriorities()
    {
        var issues = new[]
        {
            Issue.Create("Issue 1", ProjectId),
            Issue.Create("Issue 2", ProjectId),
            Issue.Create("Issue 3", ProjectId),
        };
        _db.Issues.AddRange(issues);
        await _db.SaveChangesAsync();

        foreach (var issue in issues)
        {
            issue.UpdateDetails(priority: "urgent");
        }
        await _db.SaveChangesAsync();

        var urgentCount = await _db.Issues
            .Where(i => i.ProjectId == ProjectId && i.Priority == "urgent")
            .CountAsync();
        urgentCount.ShouldBe(3);
    }

    [Fact]
    public async Task AssigneeReplacement_ReplacesExistingAssignees()
    {
        var issue = Issue.Create("Test", ProjectId);
        _db.Issues.Add(issue);
        await _db.SaveChangesAsync();

        // First set of assignees — add directly to join table
        _db.IssueAssignees.Add(IssueAssignee.Create(issue.Id, "user-1"));
        _db.IssueAssignees.Add(IssueAssignee.Create(issue.Id, "user-2"));
        await _db.SaveChangesAsync();

        // Replace: remove all existing, add new
        var existing = await _db.IssueAssignees.Where(a => a.IssueId == issue.Id).ToListAsync();
        _db.IssueAssignees.RemoveRange(existing);
        _db.IssueAssignees.Add(IssueAssignee.Create(issue.Id, "user-3"));
        await _db.SaveChangesAsync();

        var assignees = await _db.IssueAssignees
            .Where(a => a.IssueId == issue.Id)
            .ToListAsync();
        assignees.Count.ShouldBe(1);
        assignees[0].AssigneeId.ShouldBe("user-3");
    }

    [Fact]
    public async Task LabelReplacement_ReplacesExistingLabels()
    {
        var issue = Issue.Create("Test", ProjectId);
        _db.Issues.Add(issue);
        await _db.SaveChangesAsync();

        var initialLabelId = Guid.NewGuid();
        _db.IssueLabels.Add(IssueLabel.Create(issue.Id, initialLabelId));
        await _db.SaveChangesAsync();

        var newLabelId = Guid.NewGuid();

        // Replace: remove all existing, add new
        var existing = await _db.IssueLabels.Where(l => l.IssueId == issue.Id).ToListAsync();
        _db.IssueLabels.RemoveRange(existing);
        _db.IssueLabels.Add(IssueLabel.Create(issue.Id, newLabelId));
        await _db.SaveChangesAsync();

        var labels = await _db.IssueLabels
            .Where(l => l.IssueId == issue.Id)
            .ToListAsync();
        labels.Count.ShouldBe(1);
        labels[0].LabelId.ShouldBe(newLabelId);
    }

    [Fact]
    public async Task BulkDraftAcceptance_UpdatesMultipleDrafts()
    {
        var drafts = new[]
        {
            Issue.Create("Draft 1", ProjectId, isDraft: true),
            Issue.Create("Draft 2", ProjectId, isDraft: true),
        };
        _db.Issues.AddRange(drafts);
        await _db.SaveChangesAsync();

        var defaultStateId = Guid.NewGuid();
        foreach (var draft in drafts)
        {
            draft.MarkAsAccepted(defaultStateId);
        }
        await _db.SaveChangesAsync();

        var activeIssues = await _db.Issues
            .Where(i => i.ProjectId == ProjectId && !i.IsDraft && i.StateId == defaultStateId)
            .ToListAsync();
        activeIssues.Count.ShouldBe(2);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
