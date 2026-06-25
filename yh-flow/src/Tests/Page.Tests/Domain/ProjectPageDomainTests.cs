using YH.Modules.Page.Domain;

namespace YH.Tests.Page.Domain;

/// <summary>
/// Domain unit tests for <see cref="ProjectPage"/> entity (plan 07-01 Task 3).
/// </summary>
public sealed class ProjectPageDomainTests
{
    [Fact]
    public void Create_WithValidArgs_SetsProperties()
    {
        var pageId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var link = ProjectPage.Create(pageId, projectId);

        link.PageId.ShouldBe(pageId);
        link.ProjectId.ShouldBe(projectId);
        link.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void Create_WithEmptyPageId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            ProjectPage.Create(Guid.Empty, Guid.NewGuid()));
    }

    [Fact]
    public void Create_WithEmptyProjectId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            ProjectPage.Create(Guid.NewGuid(), Guid.Empty));
    }

    [Fact]
    public void SoftDelete_SetsDeletedFlag()
    {
        var link = TestProjectPageFactory.CreateValid(Guid.NewGuid(), Guid.NewGuid());
        var now = DateTimeOffset.UtcNow;

        link.SoftDelete(now);

        link.IsDeleted.ShouldBeTrue();
        link.DeletedOnUtc.ShouldBe(now);
    }
}