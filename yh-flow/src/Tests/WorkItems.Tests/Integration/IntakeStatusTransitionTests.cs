namespace YH.Tests.WorkItems.Integration;

/// <summary>
/// Integration tests for <see cref="IntakeIssue"/> status transitions via InMemory DbContext (plan 04-05 Task 2,
/// T-4-intake-02).
/// </summary>
[Collection("WorkItemsTest")]
public sealed class IntakeStatusTransitionTests : IDisposable
{
    private readonly WorkItemsDbContext _db;
    private static readonly Guid ProjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public IntakeStatusTransitionTests()
    {
        _db = WorkItemsTestFixture.CreateInMemoryContext();
    }

    [Fact]
    public async Task CreateIntake_PersistsToDatabase()
    {
        var issueId = Guid.NewGuid();
        var intake = IntakeIssue.Create(issueId, ProjectId);
        _db.IntakeIssues.Add(intake);
        await _db.SaveChangesAsync();

        var saved = await _db.IntakeIssues.FindAsync(intake.Id);
        saved.ShouldNotBeNull();
        saved.IssueId.ShouldBe(issueId);
        saved.Status.ShouldBe(IntakeIssueStatus.Pending);
    }

    [Fact]
    public async Task AcceptIntake_UpdatesStatusToAccepted()
    {
        var intake = IntakeIssue.Create(Guid.NewGuid(), ProjectId);
        _db.IntakeIssues.Add(intake);
        await _db.SaveChangesAsync();

        intake.UpdateStatus(IntakeIssueStatus.Accepted);
        await _db.SaveChangesAsync();

        var saved = await _db.IntakeIssues.FindAsync(intake.Id);
        saved.ShouldNotBeNull();
        saved.Status.ShouldBe(IntakeIssueStatus.Accepted);
    }

    [Fact]
    public async Task RejectIntake_UpdatesStatusToRejected()
    {
        var intake = IntakeIssue.Create(Guid.NewGuid(), ProjectId);
        _db.IntakeIssues.Add(intake);
        await _db.SaveChangesAsync();

        intake.UpdateStatus(IntakeIssueStatus.Rejected);
        await _db.SaveChangesAsync();

        var saved = await _db.IntakeIssues.FindAsync(intake.Id);
        saved.ShouldNotBeNull();
        saved.Status.ShouldBe(IntakeIssueStatus.Rejected);
    }

    [Fact]
    public async Task SnoozeIntake_SetsSnoozedTill()
    {
        var intake = IntakeIssue.Create(Guid.NewGuid(), ProjectId);
        _db.IntakeIssues.Add(intake);
        await _db.SaveChangesAsync();

        var snoozedTill = DateTime.UtcNow.AddDays(3);
        intake.UpdateStatus(IntakeIssueStatus.Snoozed, snoozedTill: snoozedTill);
        await _db.SaveChangesAsync();

        var saved = await _db.IntakeIssues.FindAsync(intake.Id);
        saved.ShouldNotBeNull();
        saved.Status.ShouldBe(IntakeIssueStatus.Snoozed);
        saved.SnoozedTill.ShouldBe(snoozedTill);
    }

    [Fact]
    public async Task MarkDuplicate_SetsDuplicateToIssueId()
    {
        var intake = IntakeIssue.Create(Guid.NewGuid(), ProjectId);
        _db.IntakeIssues.Add(intake);
        await _db.SaveChangesAsync();

        var duplicateToId = Guid.NewGuid();
        intake.UpdateStatus(IntakeIssueStatus.Duplicate, duplicateToIssueId: duplicateToId);
        await _db.SaveChangesAsync();

        var saved = await _db.IntakeIssues.FindAsync(intake.Id);
        saved.ShouldNotBeNull();
        saved.Status.ShouldBe(IntakeIssueStatus.Duplicate);
        saved.DuplicateToIssueId.ShouldBe(duplicateToId);
    }

    [Fact]
    public async Task AlreadyAccepted_CannotTransitionAgain()
    {
        var intake = IntakeIssue.Create(Guid.NewGuid(), ProjectId);
        _db.IntakeIssues.Add(intake);
        await _db.SaveChangesAsync();

        intake.UpdateStatus(IntakeIssueStatus.Accepted);
        await _db.SaveChangesAsync();

        Should.Throw<InvalidOperationException>(() =>
            intake.UpdateStatus(IntakeIssueStatus.Rejected));
    }

    [Fact]
    public async Task ListPendingIntakes_ReturnsOnlyPending()
    {
        _db.IntakeIssues.Add(IntakeIssue.Create(Guid.NewGuid(), ProjectId)); // Pending
        _db.IntakeIssues.Add(IntakeIssue.Create(Guid.NewGuid(), ProjectId)); // Pending
        var accepted = IntakeIssue.Create(Guid.NewGuid(), ProjectId);
        accepted.UpdateStatus(IntakeIssueStatus.Accepted);
        _db.IntakeIssues.Add(accepted);
        await _db.SaveChangesAsync();

        var pending = await _db.IntakeIssues
            .Where(i => i.ProjectId == ProjectId && i.Status == IntakeIssueStatus.Pending)
            .ToListAsync();
        pending.Count.ShouldBe(2);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
