namespace YH.Tests.WorkItems.Integration;

/// <summary>
/// Integration tests for <see cref="IssueLink"/> CRUD via InMemory DbContext (plan 04-05 Task 2).
/// Tests combined external-link + internal-relation pattern.
/// </summary>
[Collection("WorkItemsTest")]
public sealed class IssueLinkCrudTests : IDisposable
{
    private readonly WorkItemsDbContext _db;
    private static readonly Guid IssueId = Guid.NewGuid();

    public IssueLinkCrudTests()
    {
        _db = WorkItemsTestFixture.CreateInMemoryContext();
    }

    [Fact]
    public async Task CreateIssueRelation_PersistsToDatabase()
    {
        var link = IssueLink.Create(IssueId, LinkType.Blocks, relatedIssueId: Guid.NewGuid());
        _db.IssueLinks.Add(link);
        await _db.SaveChangesAsync();

        var saved = await _db.IssueLinks.FindAsync(link.Id);
        saved.ShouldNotBeNull();
        saved.LinkType.ShouldBe(LinkType.Blocks);
        saved.RelatedIssueId.ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateExternalLink_PersistsToDatabase()
    {
        var link = IssueLink.Create(IssueId, LinkType.RelatesTo, url: "https://example.com", title: "Example");
        _db.IssueLinks.Add(link);
        await _db.SaveChangesAsync();

        var saved = await _db.IssueLinks.FindAsync(link.Id);
        saved.ShouldNotBeNull();
        saved.Url.ShouldBe("https://example.com");
        saved.Title.ShouldBe("Example");
    }

    [Fact]
    public async Task CreateAllLinkTypes_AllPersist()
    {
        foreach (LinkType linkType in Enum.GetValues<LinkType>())
        {
            var link = IssueLink.Create(IssueId, linkType, url: $"https://example.com/{linkType}");
            _db.IssueLinks.Add(link);
        }
        await _db.SaveChangesAsync();

        var count = await _db.IssueLinks.Where(l => l.IssueId == IssueId).CountAsync();
        count.ShouldBe(4);
    }

    [Fact]
    public async Task ListLinksByIssue_ReturnsAll()
    {
        _db.IssueLinks.Add(IssueLink.Create(IssueId, LinkType.Blocks, relatedIssueId: Guid.NewGuid()));
        _db.IssueLinks.Add(IssueLink.Create(IssueId, LinkType.RelatesTo, url: "https://example.com"));
        await _db.SaveChangesAsync();

        var links = await _db.IssueLinks.Where(l => l.IssueId == IssueId).ToListAsync();
        links.Count.ShouldBe(2);
    }

    [Fact]
    public async Task HardDeleteLink_RemovesFromDatabase()
    {
        var link = IssueLink.Create(IssueId, LinkType.RelatesTo, url: "https://example.com");
        _db.IssueLinks.Add(link);
        await _db.SaveChangesAsync();

        _db.IssueLinks.Remove(link);
        await _db.SaveChangesAsync();

        var deleted = await _db.IssueLinks.FindAsync(link.Id);
        deleted.ShouldBeNull();
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
