using YH.Modules.View.Contracts.v1.Views.ListViews;
using YH.Modules.View.Features.v1.Views.ListViews;

namespace YH.Tests.View.Features;

/// <summary>
/// Handler integration tests for View list (project scope + workspace scope).
/// Uses InMemory ViewDbContext via <see cref="ViewTestFixture"/>.
/// </summary>
public sealed class ViewListTests : IClassFixture<ViewTestFixture>
{
    private readonly ViewTestFixture _fixture;

    public ViewListTests(ViewTestFixture fixture)
    {
        _fixture = fixture;
    }

    private static CancellationToken Ct => CancellationToken.None;

    [Fact]
    public async Task ListViews_WithProjectId_ReturnsProjectScopedViews()
    {
        using var db = _fixture.CreateDbContext();
        var projectId = Guid.NewGuid();

        // Seed 3 project-level views
        for (int i = 0; i < 3; i++)
        {
            db.Views.Add(TestViewFactory.CreateValid($"Project View {i}", projectId: projectId));
        }
        // Seed 2 workspace-level views
        db.Views.Add(TestViewFactory.CreateWorkspaceView("WS View 1"));
        db.Views.Add(TestViewFactory.CreateWorkspaceView("WS View 2"));
        await db.SaveChangesAsync(Ct);

        var handler = new ListViewsQueryHandler(db);
        var query = new ListViewsQuery
        {
            ProjectId = projectId,
            WorkspaceScope = false,
        };

        var result = await handler.Handle(query, Ct);

        result.Count.ShouldBe(3);
        result.ShouldAllBe(v => v.ProjectId == projectId);
    }

    [Fact]
    public async Task ListViews_WithWorkspaceScope_ReturnsWorkspaceScopedViews()
    {
        using var db = _fixture.CreateDbContext();
        var projectId = Guid.NewGuid();

        // Seed project-level and workspace-level views
        db.Views.Add(TestViewFactory.CreateValid("Project View", projectId: projectId));
        db.Views.Add(TestViewFactory.CreateWorkspaceView("WS View 1"));
        db.Views.Add(TestViewFactory.CreateWorkspaceView("WS View 2"));
        await db.SaveChangesAsync(Ct);

        var handler = new ListViewsQueryHandler(db);
        var query = new ListViewsQuery
        {
            WorkspaceScope = true,
        };

        var result = await handler.Handle(query, Ct);

        result.Count.ShouldBe(2);
        result.ShouldAllBe(v => v.ProjectId == null);
    }

    [Fact]
    public async Task ListViews_SortsByNameThenCreatedAt()
    {
        using var db = _fixture.CreateDbContext();
        var projectId = Guid.NewGuid();

        db.Views.Add(TestViewFactory.CreateValid("Beta", projectId: projectId));
        db.Views.Add(TestViewFactory.CreateValid("Alpha", projectId: projectId));
        db.Views.Add(TestViewFactory.CreateValid("Gamma", projectId: projectId));
        await db.SaveChangesAsync(Ct);

        var handler = new ListViewsQueryHandler(db);
        var query = new ListViewsQuery
        {
            ProjectId = projectId,
            WorkspaceScope = false,
        };

        var result = await handler.Handle(query, Ct);

        result.Count.ShouldBe(3);
        result[0].Name.ShouldBe("Alpha");
        result[1].Name.ShouldBe("Beta");
        result[2].Name.ShouldBe("Gamma");
    }
}
