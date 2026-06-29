using YH.Modules.View.Contracts.v1.Views.ArchiveView;
using YH.Modules.View.Features.v1.Views.ArchiveView;

namespace YH.Tests.View.Features;

/// <summary>
/// Handler integration tests for View Archive/Unarchive.
/// Uses InMemory ViewDbContext via <see cref="ViewTestFixture"/>.
/// </summary>
public sealed class ViewArchiveTests : IClassFixture<ViewTestFixture>
{
    private readonly ViewTestFixture _fixture;

    public ViewArchiveTests(ViewTestFixture fixture)
    {
        _fixture = fixture;
    }

    private static CancellationToken Ct => CancellationToken.None;

    [Fact]
    public async Task ArchiveView_SetsArchivedAt()
    {
        using var db = _fixture.CreateDbContext();
        var view = TestViewFactory.CreateValid();
        db.Views.Add(view);
        await db.SaveChangesAsync(Ct);

        var handler = new ArchiveViewCommandHandler(db);
        var command = new ArchiveViewCommand { ViewId = view.Id };

        var result = await handler.Handle(command, Ct);

        result.ShouldBeTrue();
        (await db.Views.FirstAsync(v => v.Id == view.Id)).ArchivedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task UnarchiveView_ClearsArchivedAt()
    {
        using var db = _fixture.CreateDbContext();
        var view = TestViewFactory.CreateArchived();
        db.Views.Add(view);
        await db.SaveChangesAsync(Ct);

        var handler = new UnarchiveViewCommandHandler(db);
        var command = new UnarchiveViewCommand { ViewId = view.Id };

        var result = await handler.Handle(command, Ct);

        result.ShouldBeTrue();
        (await db.Views.FirstAsync(v => v.Id == view.Id)).ArchivedAt.ShouldBeNull();
    }

    [Fact]
    public async Task ArchiveView_WithNonExistentId_ThrowsNotFoundException()
    {
        using var db = _fixture.CreateDbContext();
        var handler = new ArchiveViewCommandHandler(db);
        var command = new ArchiveViewCommand { ViewId = Guid.NewGuid() };

        await Should.ThrowAsync<NotFoundException>(async () =>
            await handler.Handle(command, Ct));
    }

    [Fact]
    public async Task ArchiveView_AlreadyArchived_IsIdempotent()
    {
        using var db = _fixture.CreateDbContext();
        var view = TestViewFactory.CreateValid();
        view.Archive(); // pre-archive
        db.Views.Add(view);
        await db.SaveChangesAsync(Ct);

        var handler = new ArchiveViewCommandHandler(db);
        var command = new ArchiveViewCommand { ViewId = view.Id };

        // Should not throw
        var result = await handler.Handle(command, Ct);

        result.ShouldBeTrue();
    }
}
