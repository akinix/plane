using YH.Modules.View.Contracts.v1.Views.CreateView;
using YH.Modules.View.Contracts.v1.Views.GetView;
using YH.Modules.View.Contracts.v1.Views.UpdateView;
using YH.Modules.View.Contracts.v1.Views.DeleteView;
using YH.Modules.View.Features.v1.Views.CreateView;
using YH.Modules.View.Features.v1.Views.GetView;
using YH.Modules.View.Features.v1.Views.UpdateView;
using YH.Modules.View.Features.v1.Views.DeleteView;

namespace YH.Tests.View.Features;

/// <summary>
/// Handler integration tests for View CRUD (Create/Get/Update/Delete).
/// Uses InMemory ViewDbContext via <see cref="ViewTestFixture"/>.
/// </summary>
public sealed class ViewCrudTests : IClassFixture<ViewTestFixture>
{
    private readonly ViewTestFixture _fixture;

    public ViewCrudTests(ViewTestFixture fixture)
    {
        _fixture = fixture;
    }

    private static CancellationToken Ct => CancellationToken.None;

    [Fact]
    public async Task CreateView_WithValidData_CreatesView()
    {
        using var db = _fixture.CreateDbContext();
        var handler = new CreateViewCommandHandler(db);
        var command = new CreateViewCommand
        {
            Name = "Test View",
            OwnedBy = Guid.NewGuid(),
            ProjectId = Guid.NewGuid(),
        };

        var response = await handler.Handle(command, Ct);

        response.Id.ShouldNotBe(Guid.Empty);
        (await db.Views.CountAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task CreateView_WithProjectId_CreatesProjectScopedView()
    {
        using var db = _fixture.CreateDbContext();
        var handler = new CreateViewCommandHandler(db);
        var projectId = Guid.NewGuid();
        var command = new CreateViewCommand
        {
            Name = "Project View",
            OwnedBy = Guid.NewGuid(),
            ProjectId = projectId,
        };

        var response = await handler.Handle(command, Ct);

        var view = await db.Views.FirstAsync(v => v.Id == response.Id);
        view.ProjectId.ShouldBe(projectId);
    }

    [Fact]
    public async Task CreateView_WithoutProjectId_CreatesWorkspaceScopedView()
    {
        using var db = _fixture.CreateDbContext();
        var handler = new CreateViewCommandHandler(db);
        var command = new CreateViewCommand
        {
            Name = "Workspace View",
            OwnedBy = Guid.NewGuid(),
            ProjectId = null,
        };

        var response = await handler.Handle(command, Ct);

        var view = await db.Views.FirstAsync(v => v.Id == response.Id);
        view.ProjectId.ShouldBeNull();
    }

    [Fact]
    public async Task CreateView_WithEmptyName_Throws()
    {
        using var db = _fixture.CreateDbContext();
        var handler = new CreateViewCommandHandler(db);
        var command = new CreateViewCommand
        {
            Name = "",
            OwnedBy = Guid.NewGuid(),
        };

        await Should.ThrowAsync<ArgumentException>(async () =>
            await handler.Handle(command, Ct));
    }

    [Fact]
    public async Task GetView_WithExistingId_ReturnsViewDetailDto()
    {
        using var db = _fixture.CreateDbContext();
        var view = TestViewFactory.CreateValid();
        db.Views.Add(view);
        await db.SaveChangesAsync(Ct);

        var handler = new GetViewQueryHandler(db);
        var query = new GetViewQuery { ViewId = view.Id };

        var result = await handler.Handle(query, Ct);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(view.Id);
        result.Name.ShouldBe("Test View");
    }

    [Fact]
    public async Task GetView_WithNonExistentId_ThrowsNotFoundException()
    {
        using var db = _fixture.CreateDbContext();
        var handler = new GetViewQueryHandler(db);
        var query = new GetViewQuery { ViewId = Guid.NewGuid() };

        await Should.ThrowAsync<NotFoundException>(async () =>
            await handler.Handle(query, Ct));
    }

    [Fact]
    public async Task UpdateView_WithValidData_UpdatesFields()
    {
        using var db = _fixture.CreateDbContext();
        var view = TestViewFactory.CreateValid(name: "Original");
        db.Views.Add(view);
        await db.SaveChangesAsync(Ct);

        var handler = new UpdateViewCommandHandler(db);
        var command = new UpdateViewCommand
        {
            ViewId = view.Id,
            Name = "Updated Name",
        };

        var result = await handler.Handle(command, Ct);

        result.Name.ShouldBe("Updated Name");
    }

    [Fact]
    public async Task UpdateView_WithNonExistentId_ThrowsNotFoundException()
    {
        using var db = _fixture.CreateDbContext();
        var handler = new UpdateViewCommandHandler(db);
        var command = new UpdateViewCommand
        {
            ViewId = Guid.NewGuid(),
            Name = "Nope",
        };

        await Should.ThrowAsync<NotFoundException>(async () =>
            await handler.Handle(command, Ct));
    }

    [Fact]
    public async Task DeleteView_WithExistingId_SoftDeletes()
    {
        using var db = _fixture.CreateDbContext();
        var view = TestViewFactory.CreateValid();
        db.Views.Add(view);
        await db.SaveChangesAsync(Ct);

        var handler = new DeleteViewCommandHandler(db);
        var command = new DeleteViewCommand { ViewId = view.Id };

        var result = await handler.Handle(command, Ct);

        result.ShouldBeTrue();
        (await db.Views.IgnoreQueryFilters().CountAsync(v => v.IsDeleted)).ShouldBe(1);
    }

    [Fact]
    public async Task DeleteView_WithNonExistentId_ThrowsNotFoundException()
    {
        using var db = _fixture.CreateDbContext();
        var handler = new DeleteViewCommandHandler(db);
        var command = new DeleteViewCommand { ViewId = Guid.NewGuid() };

        await Should.ThrowAsync<NotFoundException>(async () =>
            await handler.Handle(command, Ct));
    }
}
