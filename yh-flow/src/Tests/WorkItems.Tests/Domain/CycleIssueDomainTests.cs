namespace YH.Tests.WorkItems.Domain;

/// <summary>
/// Domain unit tests for <see cref="CycleIssue"/> entity.
/// Covers creation and validation of the cycle-issue bridge entity.
/// </summary>
public sealed class CycleIssueDomainTests
{
    private static readonly Guid IssueId = Guid.NewGuid();
    private static readonly Guid CycleId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidArgs_SetsProperties()
    {
        var cycleIssue = CycleIssue.Create(IssueId, CycleId);

        cycleIssue.IssueId.ShouldBe(IssueId);
        cycleIssue.CycleId.ShouldBe(CycleId);
        cycleIssue.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void Create_SetsCreatedOnUtc()
    {
        var before = DateTimeOffset.UtcNow;

        var cycleIssue = CycleIssue.Create(IssueId, CycleId);

        cycleIssue.CreatedOnUtc.ShouldBeGreaterThanOrEqualTo(before);
        cycleIssue.CreatedOnUtc.ShouldBeLessThanOrEqualTo(DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Create_WithEmptyIssueId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            CycleIssue.Create(Guid.Empty, CycleId));
    }

    [Fact]
    public void Create_WithEmptyCycleId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            CycleIssue.Create(IssueId, Guid.Empty));
    }

    [Fact]
    public void Create_WithBothValid_AssociatesCorrectly()
    {
        var cycleIssue = CycleIssue.Create(IssueId, CycleId);

        cycleIssue.IssueId.ShouldBe(IssueId);
        cycleIssue.CycleId.ShouldBe(CycleId);
    }
}
