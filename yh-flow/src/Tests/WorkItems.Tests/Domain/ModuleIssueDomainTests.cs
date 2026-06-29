namespace YH.Tests.WorkItems.Domain;

/// <summary>
/// Domain unit tests for <see cref="ModuleIssue"/> entity.
/// Covers creation and validation of the module-issue bridge entity.
/// </summary>
public sealed class ModuleIssueDomainTests
{
    private static readonly Guid IssueId = Guid.NewGuid();
    private static readonly Guid ModuleId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidArgs_SetsProperties()
    {
        var moduleIssue = ModuleIssue.Create(IssueId, ModuleId);

        moduleIssue.IssueId.ShouldBe(IssueId);
        moduleIssue.ModuleId.ShouldBe(ModuleId);
        moduleIssue.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void Create_WithEmptyIssueId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            ModuleIssue.Create(Guid.Empty, ModuleId));
    }

    [Fact]
    public void Create_WithEmptyModuleId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            ModuleIssue.Create(IssueId, Guid.Empty));
    }

    [Fact]
    public void SoftDelete_SetsDeletedFlag()
    {
        var moduleIssue = ModuleIssue.Create(IssueId, ModuleId);
        var now = DateTimeOffset.UtcNow;

        moduleIssue.SoftDelete(now);

        moduleIssue.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public void IsDeleted_DefaultIsFalse()
    {
        var moduleIssue = ModuleIssue.Create(IssueId, ModuleId);

        moduleIssue.IsDeleted.ShouldBeFalse();
    }
}
