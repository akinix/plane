using YH.Modules.Page.Contracts.v1.Pages.GetPageDescription;
using YH.Modules.Page.Contracts.v1.Pages.UpdatePageDescription;
using YH.Modules.Page.Features.v1.Pages.GetPageDescription;
using YH.Modules.Page.Features.v1.Pages.UpdatePageDescription;

namespace YH.Tests.Page.Features;

/// <summary>
/// Handler integration tests for Page Description CRUD (Get/Update).
/// Uses InMemory PageDbContext via <see cref="PageTestFixture"/>.
/// </summary>
public sealed class PageDescriptionTests : IClassFixture<PageTestFixture>
{
    private readonly PageTestFixture _fixture;

    public PageDescriptionTests(PageTestFixture fixture)
    {
        _fixture = fixture;
    }

    private static CancellationToken Ct => CancellationToken.None;

    [Fact]
    public async Task GetDescription_WithExistingId_ReturnsDescriptionFields()
    {
        using var db = _fixture.CreateDbContext();
        var page = TestPageFactory.CreateWithDescription(
            html: "<p>Hello World</p>",
            stripped: "Hello World");
        db.Pages.Add(page);
        await db.SaveChangesAsync(Ct);

        var handler = new GetPageDescriptionQueryHandler(db);
        var query = new GetPageDescriptionQuery { PageId = page.Id };

        var result = await handler.Handle(query, Ct);

        result.ShouldNotBeNull();
        result.DescriptionHtml.ShouldBe("<p>Hello World</p>");
        result.DescriptionStripped.ShouldBe("Hello World");
    }

    [Fact]
    public async Task GetDescription_WithNullDescription_ReturnsNullFields()
    {
        using var db = _fixture.CreateDbContext();
        var page = TestPageFactory.CreateValid("No Description");
        db.Pages.Add(page);
        await db.SaveChangesAsync(Ct);

        var handler = new GetPageDescriptionQueryHandler(db);
        var query = new GetPageDescriptionQuery { PageId = page.Id };

        var result = await handler.Handle(query, Ct);

        result.ShouldNotBeNull();
        result.DescriptionHtml.ShouldBeNull();
        result.DescriptionStripped.ShouldBeNull();
    }

    [Fact]
    public async Task GetDescription_WithNonExistentId_Throws()
    {
        using var db = _fixture.CreateDbContext();
        var handler = new GetPageDescriptionQueryHandler(db);
        var query = new GetPageDescriptionQuery { PageId = Guid.NewGuid() };

        await Should.ThrowAsync<NotFoundException>(async () =>
            await handler.Handle(query, Ct));
    }

    [Fact]
    public async Task UpdateDescription_SetsNewContent()
    {
        using var db = _fixture.CreateDbContext();
        var page = TestPageFactory.CreateValid("Page for Description");
        db.Pages.Add(page);
        await db.SaveChangesAsync(Ct);

        var handler = new UpdatePageDescriptionCommandHandler(db);
        var command = new UpdatePageDescriptionCommand
        {
            PageId = page.Id,
            DescriptionHtml = "<p>Updated</p>",
            DescriptionStripped = "Updated",
        };

        var result = await handler.Handle(command, Ct);

        result.ShouldNotBeNull();
        var reloaded = await db.Pages.FindAsync([page.Id], Ct);
        reloaded.ShouldNotBeNull();
        reloaded.DescriptionHtml.ShouldBe("<p>Updated</p>");
        reloaded.DescriptionStripped.ShouldBe("Updated");
    }

    [Fact]
    public async Task UpdateDescription_PartialUpdate_OnlyUpdatesGivenFields()
    {
        using var db = _fixture.CreateDbContext();
        var page = TestPageFactory.CreateWithDescription(
            html: "<p>Original</p>",
            stripped: "Original",
            json: "{}");
        db.Pages.Add(page);
        await db.SaveChangesAsync(Ct);

        var handler = new UpdatePageDescriptionCommandHandler(db);
        // Only update stripped, leave html and json unchanged
        var command = new UpdatePageDescriptionCommand
        {
            PageId = page.Id,
            DescriptionStripped = "Updated Only Stripped",
        };

        var result = await handler.Handle(command, Ct);

        result.ShouldNotBeNull();
        var reloaded = await db.Pages.FindAsync([page.Id], Ct);
        reloaded.ShouldNotBeNull();
        reloaded.DescriptionHtml.ShouldBe("<p>Original</p>"); // unchanged
        reloaded.DescriptionStripped.ShouldBe("Updated Only Stripped"); // updated
        reloaded.DescriptionJson.ShouldBe("{}"); // unchanged
    }

    [Fact]
    public async Task UpdateDescription_WithNonExistentId_Throws()
    {
        using var db = _fixture.CreateDbContext();
        var handler = new UpdatePageDescriptionCommandHandler(db);
        var command = new UpdatePageDescriptionCommand
        {
            PageId = Guid.NewGuid(),
            DescriptionHtml = "<p>Nope</p>",
        };

        await Should.ThrowAsync<NotFoundException>(async () =>
            await handler.Handle(command, Ct));
    }
}
