using YH.Modules.Page.Contracts.v1.Pages.CreatePage;
using YH.Modules.Page.Contracts.v1.Pages.GetPage;
using YH.Modules.Page.Contracts.v1.Pages.UpdatePage;
using YH.Modules.Page.Contracts.v1.Pages.DeletePage;
using YH.Modules.Page.Features.v1.Pages.CreatePage;
using YH.Modules.Page.Features.v1.Pages.GetPage;
using YH.Modules.Page.Features.v1.Pages.UpdatePage;
using YH.Modules.Page.Features.v1.Pages.DeletePage;

namespace YH.Tests.Page.Features;

/// <summary>
/// Handler integration tests for Page CRUD (Create/Get/Update/Delete).
/// Uses InMemory PageDbContext via <see cref="PageTestFixture"/>.
/// </summary>
public sealed class PageCrudTests : IClassFixture<PageTestFixture>
{
    private readonly PageTestFixture _fixture;

    public PageCrudTests(PageTestFixture fixture)
    {
        _fixture = fixture;
    }

    private static CancellationToken Ct => CancellationToken.None;

    [Fact]
    public async Task CreatePage_WithValidData_CreatesPageAndProjectPage()
    {
        using var db = _fixture.CreateDbContext();
        var handler = new CreatePageCommandHandler(db);
        var command = new CreatePageCommand
        {
            Name = "Test Page",
            ProjectId = Guid.NewGuid(),
            OwnedBy = Guid.NewGuid(),
        };

        var response = await handler.Handle(command, Ct);

        response.Id.ShouldNotBe(Guid.Empty);
        (await db.Pages.CountAsync()).ShouldBe(1);
        (await db.ProjectPages.CountAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task CreatePage_WithEmptyName_Throws()
    {
        using var db = _fixture.CreateDbContext();
        var handler = new CreatePageCommandHandler(db);
        var command = new CreatePageCommand
        {
            Name = "",
            ProjectId = Guid.NewGuid(),
            OwnedBy = Guid.NewGuid(),
        };

        await Should.ThrowAsync<ArgumentException>(async () =>
            await handler.Handle(command, Ct));
    }

    [Fact]
    public async Task GetPage_WithExistingId_ReturnsPageDetailDto()
    {
        using var db = _fixture.CreateDbContext();
        var page = TestPageFactory.CreateValid();
        db.Pages.Add(page);
        await db.SaveChangesAsync(Ct);

        var handler = new GetPageQueryHandler(db);
        var query = new GetPageQuery { PageId = page.Id };

        var result = await handler.Handle(query, Ct);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(page.Id);
        result.Name.ShouldBe("Test Page");
    }

    [Fact]
    public async Task GetPage_WithNonExistentId_ThrowsNotFoundException()
    {
        using var db = _fixture.CreateDbContext();
        var handler = new GetPageQueryHandler(db);
        var query = new GetPageQuery { PageId = Guid.NewGuid() };

        await Should.ThrowAsync<NotFoundException>(async () =>
            await handler.Handle(query, Ct));
    }

    [Fact]
    public async Task UpdatePage_WithValidData_UpdatesFields()
    {
        using var db = _fixture.CreateDbContext();
        var page = TestPageFactory.CreateValid(name: "Original");
        db.Pages.Add(page);
        await db.SaveChangesAsync(Ct);

        var handler = new UpdatePageCommandHandler(db);
        var command = new UpdatePageCommand
        {
            PageId = page.Id,
            Name = "Updated Name",
        };

        var result = await handler.Handle(command, Ct);

        result.Name.ShouldBe("Updated Name");
    }

    [Fact]
    public async Task UpdatePage_WithNonExistentId_ThrowsNotFoundException()
    {
        using var db = _fixture.CreateDbContext();
        var handler = new UpdatePageCommandHandler(db);
        var command = new UpdatePageCommand
        {
            PageId = Guid.NewGuid(),
            Name = "Nope",
        };

        await Should.ThrowAsync<NotFoundException>(async () =>
            await handler.Handle(command, Ct));
    }

    [Fact]
    public async Task DeletePage_WithExistingId_SoftDeletes()
    {
        using var db = _fixture.CreateDbContext();
        var page = TestPageFactory.CreateValid();
        db.Pages.Add(page);
        await db.SaveChangesAsync(Ct);

        var handler = new DeletePageCommandHandler(db);
        var command = new DeletePageCommand { PageId = page.Id };

        var result = await handler.Handle(command, Ct);

        result.ShouldBeTrue();
        (await db.Pages.IgnoreQueryFilters().CountAsync(p => p.IsDeleted)).ShouldBe(1);
    }

    [Fact]
    public async Task DeletePage_WithNonExistentId_ThrowsNotFoundException()
    {
        using var db = _fixture.CreateDbContext();
        var handler = new DeletePageCommandHandler(db);
        var command = new DeletePageCommand { PageId = Guid.NewGuid() };

        await Should.ThrowAsync<NotFoundException>(async () =>
            await handler.Handle(command, Ct));
    }
}
