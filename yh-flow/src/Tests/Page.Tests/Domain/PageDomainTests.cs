using YH.Modules.Page.Domain;
using PageEntity = YH.Modules.Page.Domain.Page;

namespace YH.Tests.Page.Domain;

/// <summary>
/// Domain unit tests for <see cref="PageEntity"/> entity (plan 07-01 Task 3).
/// </summary>
public sealed class PageDomainTests
{
    [Fact]
    public void Create_WithValidArgs_SetsProperties()
    {
        var projectId = Guid.NewGuid();
        var ownedBy = Guid.NewGuid();
        var page = PageEntity.Create("Test Page", projectId, ownedBy);

        page.Name.ShouldBe("Test Page");
        page.ProjectId.ShouldBe(projectId);
        page.OwnedBy.ShouldBe(ownedBy);
        page.SortOrder.ShouldBe(65535.0);
        page.Access.ShouldBe(PageAccess.Public);
        page.IsDeleted.ShouldBeFalse();
        page.IsLocked.ShouldBeFalse();
        page.IsGlobal.ShouldBeFalse();
        page.ParentId.ShouldBeNull();
        page.ArchivedAt.ShouldBeNull();
    }

    [Fact]
    public void Create_WithParent_SetsParentId()
    {
        var parentId = Guid.NewGuid();
        var page = TestPageFactory.CreateWithParent(parentId: parentId);

        page.ParentId.ShouldBe(parentId);
    }

    [Fact]
    public void Create_WithEmptyName_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            PageEntity.Create("", Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public void Create_WithEmptyProjectId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            PageEntity.Create("Test", Guid.Empty, Guid.NewGuid()));
    }

    [Fact]
    public void Create_WithEmptyOwnedBy_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            PageEntity.Create("Test", Guid.NewGuid(), Guid.Empty));
    }

    [Fact]
    public void Create_WithPrivateAccess_SetsAccessToOne()
    {
        var page = TestPageFactory.CreatePrivate();

        ((int)page.Access).ShouldBe(1);
    }

    [Fact]
    public void Archive_SetsArchivedAt()
    {
        var page = TestPageFactory.CreateValid();

        page.Archive();

        page.ArchivedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Unarchive_ClearsArchivedAt()
    {
        var page = TestPageFactory.CreateValid();
        page.Archive();

        page.Unarchive();

        page.ArchivedAt.ShouldBeNull();
    }

    [Fact]
    public void Archive_IsIdempotent()
    {
        var page = TestPageFactory.CreateValid();
        page.Archive();
        var firstArchivedAt = page.ArchivedAt;

        page.Archive(); // second call

        page.ArchivedAt.ShouldBe(firstArchivedAt);
    }

    [Fact]
    public void Update_WithName_UpdatesOnlyName()
    {
        var page = TestPageFactory.CreateValid(name: "Original");

        page.Update(name: "Updated");

        page.Name.ShouldBe("Updated");
        page.Access.ShouldBe(PageAccess.Public); // unchanged
    }

    [Fact]
    public void Update_WithNullParams_DoesNotChange()
    {
        var page = TestPageFactory.CreateValid(name: "Original");

        page.Update(name: null, color: null, sortOrder: null);

        page.Name.ShouldBe("Original");
        page.SortOrder.ShouldBe(65535.0);
    }

    [Fact]
    public void UpdateDescription_SetsContentFields()
    {
        var page = TestPageFactory.CreateValid();

        page.UpdateDescription("<p>Hello</p>", "Hello", "{}");

        page.DescriptionHtml.ShouldBe("<p>Hello</p>");
        page.DescriptionStripped.ShouldBe("Hello");
        page.DescriptionJson.ShouldBe("{}");
    }

    [Fact]
    public void SetSortOrder_UpdatesValue()
    {
        var page = TestPageFactory.CreateValid();

        page.SetSortOrder(1.0);

        page.SortOrder.ShouldBe(1.0);
    }

    [Fact]
    public void SoftDelete_SetsDeletedFlag()
    {
        var page = TestPageFactory.CreateValid();
        var now = DateTimeOffset.UtcNow;

        page.SoftDelete(now);

        page.IsDeleted.ShouldBeTrue();
        page.DeletedOnUtc.ShouldBe(now);
    }

    [Fact]
    public void SoftDelete_IsIdempotent()
    {
        var page = TestPageFactory.CreateValid();
        var now = DateTimeOffset.UtcNow;
        page.SoftDelete(now);

        page.SoftDelete(DateTimeOffset.UtcNow.AddDays(1));

        page.DeletedOnUtc.ShouldBe(now); // unchanged — first delete wins
    }
}