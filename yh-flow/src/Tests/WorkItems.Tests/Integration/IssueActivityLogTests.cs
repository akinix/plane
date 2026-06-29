namespace YH.Tests.WorkItems.Integration;

/// <summary>
/// Integration tests for <see cref="IssueActivity"/> logging via InMemory DbContext (plan 04-05 Task 2).
/// Verifies that activity records are persisted correctly when issue changes are saved.
/// </summary>
[Collection("WorkItemsTest")]
public sealed class IssueActivityLogTests : IDisposable
{
    private readonly WorkItemsDbContext _db;

    public IssueActivityLogTests()
    {
        _db = WorkItemsTestFixture.CreateInMemoryContext();
    }

    [Fact]
    public async Task CreateActivityRecord_PersistsToDatabase()
    {
        var activity = IssueActivity.Create(
            Guid.NewGuid(), "updated", "user-001", 1_000_000_000,
            field: "priority", oldValue: "none", newValue: "high");
        _db.IssueActivities.Add(activity);
        await _db.SaveChangesAsync();

        var saved = await _db.IssueActivities.FindAsync(activity.Id);
        saved.ShouldNotBeNull();
        saved.Verb.ShouldBe("updated");
        saved.Field.ShouldBe("priority");
        saved.ActorId.ShouldBe("user-001");
    }

    [Fact]
    public async Task CreateActivityRecord_WithComment_SavesComment()
    {
        var commentId = Guid.NewGuid();
        var activity = IssueActivity.Create(
            Guid.NewGuid(), "updated", "user-001", 1_000_000_000,
            field: "name", oldValue: "Old", newValue: "New",
            comment: "Updated by reviewer", issueCommentId: commentId);
        _db.IssueActivities.Add(activity);
        await _db.SaveChangesAsync();

        var saved = await _db.IssueActivities.FindAsync(activity.Id);
        saved.ShouldNotBeNull();
        saved.Comment.ShouldBe("Updated by reviewer");
        saved.IssueCommentId.ShouldBe(commentId);
    }

    [Fact]
    public async Task CreatedVerbActivity_HasNoFieldValues()
    {
        var activity = IssueActivity.Create(
            Guid.NewGuid(), "created", "user-001", 1_000_000_000);
        _db.IssueActivities.Add(activity);
        await _db.SaveChangesAsync();

        var saved = await _db.IssueActivities.FindAsync(activity.Id);
        saved.ShouldNotBeNull();
        saved.Verb.ShouldBe("created");
        saved.Field.ShouldBeNull();
        saved.OldValue.ShouldBeNull();
        saved.NewValue.ShouldBeNull();
    }

    [Fact]
    public async Task ActivityRecords_AreOrderedByEpoch()
    {
        var issueId = Guid.NewGuid();
        _db.IssueActivities.Add(IssueActivity.Create(issueId, "created", "user-001", 100));
        _db.IssueActivities.Add(IssueActivity.Create(issueId, "updated", "user-001", 200, field: "name", oldValue: "A", newValue: "B"));
        _db.IssueActivities.Add(IssueActivity.Create(issueId, "updated", "user-001", 300, field: "priority", oldValue: "none", newValue: "high"));
        await _db.SaveChangesAsync();

        var activities = await _db.IssueActivities
            .Where(a => a.IssueId == issueId)
            .OrderBy(a => a.Epoch)
            .ToListAsync();

        activities.Count.ShouldBe(3);
        activities[0].Verb.ShouldBe("created");
        activities[2].Verb.ShouldBe("updated");
        activities[2].Field.ShouldBe("priority");
    }

    [Fact]
    public async Task MultipleFieldChanges_CreateSeparateActivityRecords()
    {
        var issueId = Guid.NewGuid();
        var epoch = 1_000_000_000;

        // Simulate what IssueActivityHandler does: one activity per changed field
        _db.IssueActivities.Add(IssueActivity.Create(issueId, "updated", "user-001", epoch, field: "name", oldValue: "Old", newValue: "New"));
        _db.IssueActivities.Add(IssueActivity.Create(issueId, "updated", "user-001", epoch, field: "priority", oldValue: "none", newValue: "urgent"));
        await _db.SaveChangesAsync();

        var activities = await _db.IssueActivities
            .Where(a => a.IssueId == issueId)
            .ToListAsync();
        activities.Count.ShouldBe(2);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
