using YH.Modules.View.Domain;
using YH.Tests.View.TestData;
using ViewEntity = YH.Modules.View.Domain.View;

namespace YH.Tests.View.Domain;

/// <summary>
/// Domain unit tests for <see cref="ViewEntity"/> entity (plan 08-01 Task 3).
/// </summary>
public sealed class ViewDomainTests
{
    [Fact]
    public void Create_WithValidArgs_SetsProperties()
    {
        var ownedBy = Guid.NewGuid();
        var view = ViewEntity.Create("Test View", ownedBy);

        view.Name.ShouldBe("Test View");
        view.OwnedBy.ShouldBe(ownedBy);
        view.SortOrder.ShouldBe(65535.0);
        view.Access.ShouldBe(ViewAccess.Public);
        view.IsDeleted.ShouldBeFalse();
        view.IsLocked.ShouldBeFalse();
        view.ProjectId.ShouldBeNull();
        view.ArchivedAt.ShouldBeNull();
    }

    [Fact]
    public void Create_WithProjectId_SetsProjectScope()
    {
        var projectId = Guid.NewGuid();
        var view = TestViewFactory.CreateValid(projectId: projectId);

        view.ProjectId.ShouldBe(projectId);
    }

    [Fact]
    public void Create_WithoutProjectId_SetsWorkspaceScope()
    {
        var view = TestViewFactory.CreateWorkspaceView();

        view.ProjectId.ShouldBeNull();
    }

    [Fact]
    public void Create_WithFilters_SetsFiltersAndQuery()
    {
        var filters = "{\"priority\":[\"urgent\",\"high\"]}";
        var view = TestViewFactory.CreateWithFilters("Filtered View", filters);

        view.Filters.ShouldBe(filters);
        view.Query.ShouldBe(filters);
    }

    [Fact]
    public void Create_WithEmptyName_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            ViewEntity.Create("", Guid.NewGuid()));
    }

    [Fact]
    public void Create_WithEmptyOwnedBy_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            ViewEntity.Create("Test", Guid.Empty));
    }

    [Fact]
    public void Archive_SetsArchivedAt()
    {
        var view = TestViewFactory.CreateValid();

        view.Archive();

        view.ArchivedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Unarchive_ClearsArchivedAt()
    {
        var view = TestViewFactory.CreateValid();
        view.Archive();

        view.Unarchive();

        view.ArchivedAt.ShouldBeNull();
    }

    [Fact]
    public void Archive_IsIdempotent()
    {
        var view = TestViewFactory.CreateValid();
        view.Archive();
        var firstArchivedAt = view.ArchivedAt;

        view.Archive(); // second call

        view.ArchivedAt.ShouldBe(firstArchivedAt);
    }

    [Fact]
    public void Lock_SetsIsLocked()
    {
        var view = TestViewFactory.CreateValid();

        view.Lock();

        view.IsLocked.ShouldBeTrue();
    }

    [Fact]
    public void Unlock_ClearsIsLocked()
    {
        var view = TestViewFactory.CreateValid();
        view.Lock();

        view.Unlock();

        view.IsLocked.ShouldBeFalse();
    }

    [Fact]
    public void Update_WithName_UpdatesOnlyName()
    {
        var view = TestViewFactory.CreateValid(name: "Original");

        view.Update(name: "Updated");

        view.Name.ShouldBe("Updated");
        view.Access.ShouldBe(ViewAccess.Public); // unchanged
    }

    [Fact]
    public void Update_WithNullParams_DoesNotChange()
    {
        var view = TestViewFactory.CreateValid(name: "Original");

        view.Update(name: null, description: null, sortOrder: null);

        view.Name.ShouldBe("Original");
        view.SortOrder.ShouldBe(65535.0);
    }

    [Fact]
    public void SetSortOrder_UpdatesValue()
    {
        var view = TestViewFactory.CreateValid();

        view.SetSortOrder(1.0);

        view.SortOrder.ShouldBe(1.0);
    }

    [Fact]
    public void SoftDelete_SetsDeletedFlag()
    {
        var view = TestViewFactory.CreateValid();
        var now = DateTimeOffset.UtcNow;

        view.SoftDelete(now);

        view.IsDeleted.ShouldBeTrue();
        view.DeletedOnUtc.ShouldBe(now);
    }

    [Fact]
    public void SoftDelete_IsIdempotent()
    {
        var view = TestViewFactory.CreateValid();
        var now = DateTimeOffset.UtcNow;
        view.SoftDelete(now);

        view.SoftDelete(DateTimeOffset.UtcNow.AddDays(1));

        view.DeletedOnUtc.ShouldBe(now); // unchanged — first delete wins
    }
}