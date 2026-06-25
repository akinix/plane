using YH.Modules.Page.Contracts.v1.Pages.ListPages;
using YH.Modules.Page.Contracts.v1.Pages.GetPageSummary;
using YH.Modules.Page.Features.v1.Pages.ListPages;
using YH.Modules.Page.Features.v1.Pages.GetPageSummary;
using PageEntity = YH.Modules.Page.Domain.Page;

namespace YH.Tests.Page.Features;

/// <summary>
/// Handler integration tests for ListPages + GetPageSummary.
/// Uses InMemory PageDbContext via <see cref="PageTestFixture"/>.
/// </summary>
public sealed class PageListSummaryTests : IClassFixture<PageTestFixture>
{
    private readonly PageTestFixture _fixture;

    public PageListSummaryTests(PageTestFixture fixture)
    {
        _fixture = fixture;
    }

    private static CancellationToken Ct => CancellationToken.None;

    private static readonly Guid DefaultProjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid DefaultOwnedBy = Guid.Parse("00000000-0000-0000-0000-000000000002");

    [Fact]
    public async Task ListPages_Default_ReturnsTopLevelPages()
    {
        using var db = _fixture.CreateDbContext();
        var projectId = Guid.NewGuid();

        // Seed 3 top-level pages
        db.Pages.Add(TestPageFactory.CreateValid("Top 1", projectId));
        db.Pages.Add(TestPageFactory.CreateValid("Top 2", projectId));
        db.Pages.Add(TestPageFactory.CreateValid("Top 3", projectId));
        // Seed 1 parent + 1 child (with parent)
        var parent = TestPageFactory.CreateValid("Parent", projectId);
        db.Pages.Add(parent);
        var child = PageEntity.Create("Child", projectId, DefaultOwnedBy, parentId: parent.Id);
        db.Pages.Add(child);
        await db.SaveChangesAsync(Ct);

        var handler = new ListPagesQueryHandler(db);
        var query = new ListPagesQuery();

        var result = await handler.Handle(query, Ct);

        // 3 top-level + 1 parent = 4 (child excluded because it has ParentId)
        result.Count.ShouldBe(4);
    }

    [Fact]
    public async Task ListPages_WithIsArchivedTrue_ReturnsArchivedPages()
    {
        using var db = _fixture.CreateDbContext();
        var projectId = Guid.NewGuid();

        db.Pages.Add(TestPageFactory.CreateValid("Active", projectId));
        db.Pages.Add(TestPageFactory.CreateArchived("Archived 1", projectId));
        db.Pages.Add(TestPageFactory.CreateArchived("Archived 2", projectId));
        await db.SaveChangesAsync(Ct);

        var handler = new ListPagesQueryHandler(db);
        var query = new ListPagesQuery { IsArchived = true };

        var result = await handler.Handle(query, Ct);

        result.Count.ShouldBe(2);
        result.All(p => p.Name.StartsWith("Archived", StringComparison.Ordinal)).ShouldBeTrue();
    }

    [Fact]
    public async Task ListPages_SortsBySortOrderThenCreatedAt()
    {
        using var db = _fixture.CreateDbContext();

        // Seed pages with explicit sort orders
        var page1 = PageEntity.Create("Third", DefaultProjectId, DefaultOwnedBy, sortOrder: 3.0);
        var page2 = PageEntity.Create("First", DefaultProjectId, DefaultOwnedBy, sortOrder: 1.0);
        var page3 = PageEntity.Create("Second", DefaultProjectId, DefaultOwnedBy, sortOrder: 2.0);
        db.Pages.AddRange(page1, page2, page3);
        await db.SaveChangesAsync(Ct);

        var handler = new ListPagesQueryHandler(db);
        var query = new ListPagesQuery();

        var result = await handler.Handle(query, Ct);

        result.Count.ShouldBe(3);
        result[0].Name.ShouldBe("First");
        result[1].Name.ShouldBe("Second");
        result[2].Name.ShouldBe("Third");
    }

    [Fact]
    public async Task GetPageSummary_ReturnsAggregatedData()
    {
        using var db = _fixture.CreateDbContext();
        var projectId = Guid.NewGuid();

        db.Pages.Add(TestPageFactory.CreateValid("Active 1", projectId));
        db.Pages.Add(TestPageFactory.CreateValid("Active 2", projectId));
        db.Pages.Add(TestPageFactory.CreateValid("Active 3", projectId));
        db.Pages.Add(TestPageFactory.CreateArchived("Archived 1", projectId));
        db.Pages.Add(TestPageFactory.CreateArchived("Archived 2", projectId));
        await db.SaveChangesAsync(Ct);

        var handler = new GetPageSummaryQueryHandler(db);
        var query = new GetPageSummaryQuery();

        var result = await handler.Handle(query, Ct);

        // The handler returns anonymous type via reflection
        var totalPages = (int)result.GetType().GetProperty("total_pages")!.GetValue(result)!;
        var totalArchived = (int)result.GetType().GetProperty("total_archived_pages")!.GetValue(result)!;
        var recent = result.GetType().GetProperty("recently_updated")!.GetValue(result);

        totalPages.ShouldBe(5);
        totalArchived.ShouldBe(2);
        recent.ShouldNotBeNull();
    }
}
