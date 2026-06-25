using YH.Modules.View.Contracts.v1.Views.AddFavorite;
using YH.Modules.View.Contracts.v1.Views.RemoveFavorite;
using YH.Modules.View.Features.v1.Views.AddFavorite;
using YH.Modules.View.Features.v1.Views.RemoveFavorite;

namespace YH.Tests.View.Features;

/// <summary>
/// Handler integration tests for View Favorite (Add/Remove).
/// Uses InMemory ViewDbContext via <see cref="ViewTestFixture"/>.
/// </summary>
public sealed class ViewFavoriteTests : IClassFixture<ViewTestFixture>
{
    private readonly ViewTestFixture _fixture;

    public ViewFavoriteTests(ViewTestFixture fixture)
    {
        _fixture = fixture;
    }

    private static CancellationToken Ct => CancellationToken.None;

    [Fact]
    public async Task AddFavorite_CreatesFavorite()
    {
        using var db = _fixture.CreateDbContext();
        var view = TestViewFactory.CreateValid();
        db.Views.Add(view);
        await db.SaveChangesAsync(Ct);

        var handler = new AddFavoriteCommandHandler(db);
        var command = new AddFavoriteCommand
        {
            ViewId = view.Id,
            UserId = "user-1",
        };

        var result = await handler.Handle(command, Ct);

        result.ShouldBeTrue();
        (await db.ViewFavorites.CountAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task AddFavorite_Twice_IsIdempotent()
    {
        using var db = _fixture.CreateDbContext();
        var view = TestViewFactory.CreateValid();
        db.Views.Add(view);
        await db.SaveChangesAsync(Ct);

        var handler = new AddFavoriteCommandHandler(db);

        await handler.Handle(new AddFavoriteCommand { ViewId = view.Id, UserId = "user-1" }, Ct);
        await handler.Handle(new AddFavoriteCommand { ViewId = view.Id, UserId = "user-1" }, Ct);

        (await db.ViewFavorites.CountAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task RemoveFavorite_RemovesExisting()
    {
        using var db = _fixture.CreateDbContext();
        var view = TestViewFactory.CreateValid();
        db.Views.Add(view);
        await db.SaveChangesAsync(Ct);

        // Add favorite first
        var addHandler = new AddFavoriteCommandHandler(db);
        await addHandler.Handle(new AddFavoriteCommand { ViewId = view.Id, UserId = "user-1" }, Ct);

        // Now remove
        var removeHandler = new RemoveFavoriteCommandHandler(db);
        var result = await removeHandler.Handle(
            new RemoveFavoriteCommand { ViewId = view.Id, UserId = "user-1" }, Ct);

        result.ShouldBeTrue();
        (await db.ViewFavorites.IgnoreQueryFilters().CountAsync(f => f.IsDeleted)).ShouldBe(1);
    }

    [Fact]
    public async Task RemoveFavorite_NonExistent_IsIdempotent()
    {
        using var db = _fixture.CreateDbContext();
        var handler = new RemoveFavoriteCommandHandler(db);
        var command = new RemoveFavoriteCommand
        {
            ViewId = Guid.NewGuid(),
            UserId = "user-1",
        };

        // Should not throw
        var result = await handler.Handle(command, Ct);
        result.ShouldBeTrue();
    }
}
