using YH.Modules.Page.Contracts.v1.Pages.ArchivePage;
using YH.Modules.Page.Features.v1.Pages.ArchivePage;

namespace YH.Tests.Page.Features;

/// <summary>
/// Handler integration tests for Archive/Unarchive.
/// Uses InMemory PageDbContext via <see cref="PageTestFixture"/>.
/// </summary>
public sealed class PageArchiveTests : IClassFixture<PageTestFixture>
{
    private readonly PageTestFixture _fixture;

    public PageArchiveTests(PageTestFixture fixture)
    {
        _fixture = fixture;
    }

    private static CancellationToken Ct => CancellationToken.None;

    [Fact]
    public async Task ArchivePage_SetsArchivedAt()
    {
        using var db = _fixture.CreateDbContext();
        var page = TestPageFactory.CreateValid();
        db.Pages.Add(page);
        await db.SaveChangesAsync(Ct);

        var handler = new ArchivePageCommandHandler(db);
        var command = new ArchivePageCommand { PageId = page.Id };

        var result = await handler.Handle(command, Ct);

        result.ShouldBeTrue();
        var reloaded = await db.Pages.FindAsync([page.Id], Ct);
        reloaded.ShouldNotBeNull();
        reloaded.ArchivedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task UnarchivePage_ClearsArchivedAt()
    {
        using var db = _fixture.CreateDbContext();
        var page = TestPageFactory.CreateArchived();
        db.Pages.Add(page);
        await db.SaveChangesAsync(Ct);

        var handler = new UnarchivePageCommandHandler(db);
        var command = new UnarchivePageCommand { PageId = page.Id };

        var result = await handler.Handle(command, Ct);

        result.ShouldBeTrue();
        var reloaded = await db.Pages.FindAsync([page.Id], Ct);
        reloaded.ShouldNotBeNull();
        reloaded.ArchivedAt.ShouldBeNull();
    }

    [Fact]
    public async Task ArchivePage_WithNonExistentId_Throws()
    {
        using var db = _fixture.CreateDbContext();
        var handler = new ArchivePageCommandHandler(db);
        var command = new ArchivePageCommand { PageId = Guid.NewGuid() };

        await Should.ThrowAsync<NotFoundException>(async () =>
            await handler.Handle(command, Ct));
    }

    [Fact]
    public async Task ArchivePage_AlreadyArchived_IsIdempotent()
    {
        using var db = _fixture.CreateDbContext();
        var page = TestPageFactory.CreateArchived();
        db.Pages.Add(page);
        await db.SaveChangesAsync(Ct);

        var handler = new ArchivePageCommandHandler(db);
        var command = new ArchivePageCommand { PageId = page.Id };

        var result = await handler.Handle(command, Ct);

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task UnarchivePage_WithNonExistentId_Throws()
    {
        using var db = _fixture.CreateDbContext();
        var handler = new UnarchivePageCommandHandler(db);
        var command = new UnarchivePageCommand { PageId = Guid.NewGuid() };

        await Should.ThrowAsync<NotFoundException>(async () =>
            await handler.Handle(command, Ct));
    }

    [Fact]
    public async Task UnarchivePage_ActivePage_IsIdempotent()
    {
        using var db = _fixture.CreateDbContext();
        var page = TestPageFactory.CreateValid();
        db.Pages.Add(page);
        await db.SaveChangesAsync(Ct);

        var handler = new UnarchivePageCommandHandler(db);
        var command = new UnarchivePageCommand { PageId = page.Id };

        var result = await handler.Handle(command, Ct);

        result.ShouldBeTrue();
    }
}
