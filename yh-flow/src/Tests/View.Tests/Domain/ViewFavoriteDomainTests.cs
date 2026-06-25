using YH.Modules.View.Domain;
using YH.Tests.View.TestData;

namespace YH.Tests.View.Domain;

/// <summary>
/// Domain unit tests for <see cref="ViewFavorite"/> entity (plan 08-01 Task 3).
/// </summary>
public sealed class ViewFavoriteDomainTests
{
    [Fact]
    public void Create_WithValidArgs_SetsProperties()
    {
        var viewId = Guid.NewGuid();

        var favorite = TestViewFavoriteFactory.CreateValid(viewId, "user-1");

        favorite.ViewId.ShouldBe(viewId);
        favorite.UserId.ShouldBe("user-1");
        favorite.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void Create_WithEmptyViewId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            ViewFavorite.Create(Guid.Empty, "user-1"));
    }

    [Fact]
    public void Create_WithEmptyUserId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            ViewFavorite.Create(Guid.NewGuid(), ""));
    }

    [Fact]
    public void SoftDelete_SetsDeletedFlag()
    {
        var favorite = TestViewFavoriteFactory.CreateValid(Guid.NewGuid());
        var now = DateTimeOffset.UtcNow;

        favorite.SoftDelete(now);

        favorite.IsDeleted.ShouldBeTrue();
        favorite.DeletedOnUtc.ShouldBe(now);
    }
}