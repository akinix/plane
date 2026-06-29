namespace YH.Tests.WorkItems.Domain;

/// <summary>
/// Domain unit tests for <see cref="IntakeIssue"/> entity (plan 04-05 Task 1).
/// Covers creation and status transition validation (T-4-intake-02).
/// </summary>
public sealed class IntakeIssueTests
{
    private static readonly Guid IssueId = Guid.NewGuid();
    private static readonly Guid ProjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    [Fact]
    public void Create_WithValidArgs_SetsProperties()
    {
        var intake = IntakeIssue.Create(IssueId, ProjectId, "EMAIL");

        intake.IssueId.ShouldBe(IssueId);
        intake.ProjectId.ShouldBe(ProjectId);
        intake.Status.ShouldBe(IntakeIssueStatus.Pending);
        intake.Source.ShouldBe("EMAIL");
        intake.SnoozedTill.ShouldBeNull();
        intake.DuplicateToIssueId.ShouldBeNull();
    }

    [Fact]
    public void Create_WithDefaultSource_SetsInApp()
    {
        var intake = IntakeIssue.Create(IssueId, ProjectId);
        intake.Source.ShouldBe("IN_APP");
    }

    [Fact]
    public void Create_WithEmptyIssueId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            IntakeIssue.Create(Guid.Empty, ProjectId));
    }

    [Fact]
    public void Create_WithEmptyProjectId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            IntakeIssue.Create(IssueId, Guid.Empty));
    }

    [Fact]
    public void UpdateStatus_FromPendingToAccepted_Succeeds()
    {
        var intake = IntakeIssue.Create(IssueId, ProjectId);

        intake.UpdateStatus(IntakeIssueStatus.Accepted);

        intake.Status.ShouldBe(IntakeIssueStatus.Accepted);
    }

    [Fact]
    public void UpdateStatus_FromPendingToRejected_Succeeds()
    {
        var intake = IntakeIssue.Create(IssueId, ProjectId);

        intake.UpdateStatus(IntakeIssueStatus.Rejected);

        intake.Status.ShouldBe(IntakeIssueStatus.Rejected);
    }

    [Fact]
    public void UpdateStatus_FromPendingToSnoozed_SetsSnoozedTill()
    {
        var intake = IntakeIssue.Create(IssueId, ProjectId);
        var snoozedTill = DateTime.UtcNow.AddDays(1);

        intake.UpdateStatus(IntakeIssueStatus.Snoozed, snoozedTill: snoozedTill);

        intake.Status.ShouldBe(IntakeIssueStatus.Snoozed);
        intake.SnoozedTill.ShouldBe(snoozedTill);
    }

    [Fact]
    public void UpdateStatus_FromPendingToDuplicate_SetsDuplicateToIssueId()
    {
        var intake = IntakeIssue.Create(IssueId, ProjectId);
        var duplicateToId = Guid.NewGuid();

        intake.UpdateStatus(IntakeIssueStatus.Duplicate, duplicateToIssueId: duplicateToId);

        intake.Status.ShouldBe(IntakeIssueStatus.Duplicate);
        intake.DuplicateToIssueId.ShouldBe(duplicateToId);
    }

    [Fact]
    public void UpdateStatus_FromAccepted_Throws()
    {
        var intake = IntakeIssue.Create(IssueId, ProjectId);
        intake.UpdateStatus(IntakeIssueStatus.Accepted);

        Should.Throw<InvalidOperationException>(() =>
            intake.UpdateStatus(IntakeIssueStatus.Rejected));
    }

    [Fact]
    public void UpdateStatus_FromDuplicate_Throws()
    {
        var intake = IntakeIssue.Create(IssueId, ProjectId);
        intake.UpdateStatus(IntakeIssueStatus.Duplicate, duplicateToIssueId: Guid.NewGuid());

        Should.Throw<InvalidOperationException>(() =>
            intake.UpdateStatus(IntakeIssueStatus.Accepted));
    }

    [Fact]
    public void UpdateStatus_ToPending_Throws()
    {
        var intake = IntakeIssue.Create(IssueId, ProjectId);

        Should.Throw<InvalidOperationException>(() =>
            intake.UpdateStatus(IntakeIssueStatus.Pending));
    }
}
