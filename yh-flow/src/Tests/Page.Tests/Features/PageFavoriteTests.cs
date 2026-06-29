using YH.Modules.Page.Contracts.v1.Pages.AddFavorite;
using YH.Modules.Page.Contracts.v1.Pages.RemoveFavorite;
using YH.Modules.Page.Features.v1.Pages.AddFavorite;
using YH.Modules.Page.Features.v1.Pages.RemoveFavorite;

namespace YH.Tests.Page.Features;

/// <summary>
/// Handler integration tests for Page Favorite (Add/Remove).
/// Uses InMemory PageDbContext via <see cref="PageTestFixture"/>.
/// </summary>
public sealed class PageFavoriteTests : IClassFixture<PageTestFixture>
{
    private readonly PageTestFixture _fixture;

    public PageFavoriteTests(PageTestFixture fixture)
    {
        _fixture = fixture;
    }

    private static CancellationToken Ct => CancellationToken.None;

    [Fact]
    public async Task AddFavorite_CreatesFavorite()
    {
        using var db = _fixture.CreateDbContext();
        var page = TestPageFactory.CreateValid();
        db.Pages.Add(page);
        await db.SaveChangesAsync(Ct);

        var handler = new AddFavoriteCommandHandler(db);
        var command = new AddFavoriteCommand
        {
            PageId = page.Id,
            UserId = "user-1",
        };

        var result = await handler.Handle(command, Ct);

        result.ShouldBeTrue();
        (await db.PageFavorites.CountAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task AddFavorite_Twice_IsIdempotent()
    {
        using var db = _fixture.CreateDbContext();
        var page = TestPageFactory.CreateValid();
        db.Pages.Add(page);
        await db.SaveChangesAsync(Ct);

        var handler = new AddFavoriteCommandHandler(db);

        await handler.Handle(new AddFavoriteCommand { PageId = page.Id, UserId = "user-1" }, Ct);
        await handler.Handle(new AddFavoriteCommand { PageId = page.Id, UserId = "user-1" }, Ct);

        (await db.PageFavorites.CountAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task RemoveFavorite_RemovesExisting()
    {
        using var db = _fixture.CreateDbContext();
        var page = TestPageFactory.CreateValid();
        db.Pages.Add(page);
        await db.SaveChangesAsync(Ct);

        // Add favorite first
        var addHandler = new AddFavoriteCommandHandler(db);
        await addHandler.Handle(new AddFavoriteCommand { PageId = page.Id, UserId = "user-1" }, Ct);

        // Now remove
        var removeHandler = new RemoveFavoriteCommandHandler(db);
        var result = await removeHandler.Handle(
            new RemoveFavoriteCommand { PageId = page.Id, UserId = "user-1" }, Ct);

        result.ShouldBeTrue();
        (await db.PageFavorites.IgnoreQueryFilters().CountAsync(f => f.IsDeleted)).ShouldBe(1);
    }

    [Fact]
    public async Task RemoveFavorite_NonExistent_IsIdempotent()
    {
        using var db = _fixture.CreateDbContext();
        var handler = new RemoveFavoriteCommandHandler(db);
        var command = new RemoveFavoriteCommand
        {
            PageId = Guid.NewGuid(),
            UserId = "user-1",
        };

        // Should not throw
        var result = await handler.Handle(command, Ct);
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task AddFavorite_WithInvalidPageId_Throws()
    {
        using var db = _fixture.CreateDbContext();
        var handler = new AddFavoriteCommandHandler(db);
        var command = new AddFavoriteCommand
        {
            PageId = Guid.Empty,
            UserId = "user-1",
        };

        await Should.ThrowAsync<CustomException>(async () =>
            await handler.Handle(command, Ct));
    }

    [Fact]
    public async Task RemoveFavorite_WithInvalidPageId_Throws()
    {
        using var db = _fixture.CreateDbContext();
        var handler = new RemoveFavoriteCommandHandler(db);
        var command = new RemoveFavoriteCommand
        {
            PageId = Guid.Empty,
            UserId = "user-1",
        };

        await Should.ThrowAsync<CustomException>(async () =>
            await handler.Handle(command, Ct));
    }
}
