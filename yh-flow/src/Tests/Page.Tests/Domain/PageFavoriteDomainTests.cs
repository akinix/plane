using YH.Modules.Page.Domain;

namespace YH.Tests.Page.Domain;

/// <summary>
/// Domain unit tests for <see cref="PageFavorite"/> entity (plan 07-01 Task 3).
/// </summary>
public sealed class PageFavoriteDomainTests
{
    [Fact]
    public void Create_WithValidArgs_SetsProperties()
    {
        var pageId = Guid.NewGuid();

        var favorite = PageFavorite.Create(pageId, "user-1");

        favorite.PageId.ShouldBe(pageId);
        favorite.UserId.ShouldBe("user-1");
    }

    [Fact]
    public void Create_WithEmptyPageId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            PageFavorite.Create(Guid.Empty, "user-1"));
    }

    [Fact]
    public void Create_WithEmptyUserId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            PageFavorite.Create(Guid.NewGuid(), ""));
    }

    [Fact]
    public void SoftDelete_SetsDeletedFlag()
    {
        var favorite = TestPageFavoriteFactory.CreateValid(Guid.NewGuid());
        var now = DateTimeOffset.UtcNow;

        favorite.SoftDelete(now);

        favorite.IsDeleted.ShouldBeTrue();
        favorite.DeletedOnUtc.ShouldBe(now);
    }
}