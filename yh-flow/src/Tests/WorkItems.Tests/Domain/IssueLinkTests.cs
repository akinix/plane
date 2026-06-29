namespace YH.Tests.WorkItems.Domain;

/// <summary>
/// Domain unit tests for <see cref="IssueLink"/> entity (plan 04-05 Task 1).
/// Covers combined external-link + internal-relation pattern (CONTEXT §灰色区域 1).
/// </summary>
public sealed class IssueLinkTests
{
    private static readonly Guid IssueId = Guid.NewGuid();

    [Fact]
    public void CreateRelation_WithValidArgs_SetsProperties()
    {
        var relatedId = Guid.NewGuid();
        var link = IssueLink.Create(IssueId, LinkType.Blocks, relatedIssueId: relatedId);

        link.IssueId.ShouldBe(IssueId);
        link.LinkType.ShouldBe(LinkType.Blocks);
        link.RelatedIssueId.ShouldBe(relatedId);
        link.Url.ShouldBeNull();
        link.Title.ShouldBeNull();
    }

    [Fact]
    public void CreateExternalLink_WithValidArgs_SetsProperties()
    {
        var link = IssueLink.Create(IssueId, LinkType.RelatesTo, url: "https://example.com", title: "Example");

        link.Url.ShouldBe("https://example.com");
        link.Title.ShouldBe("Example");
        link.RelatedIssueId.ShouldBeNull();
    }

    [Fact]
    public void Create_WithEmptyIssueId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            IssueLink.Create(Guid.Empty, LinkType.RelatesTo, relatedIssueId: Guid.NewGuid()));
    }

    [Fact]
    public void Create_WithoutRelatedIssueIdOrUrl_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            IssueLink.Create(IssueId, LinkType.RelatesTo));
    }

    [Fact]
    public void Create_AllFourLinkTypes_AreValid()
    {
        foreach (LinkType linkType in Enum.GetValues<LinkType>())
        {
            var link = IssueLink.Create(IssueId, linkType, url: $"https://example.com/{linkType}");
            link.LinkType.ShouldBe(linkType);
        }
    }

    [Fact]
    public void Create_WithMetadata_SetsMetadata()
    {
        var link = IssueLink.Create(IssueId, LinkType.RelatesTo,
            relatedIssueId: Guid.NewGuid(),
            metadata: "{\"source\": \"manual\"}");

        link.Metadata.ShouldBe("{\"source\": \"manual\"}");
    }

    // IssueLink is NOT soft-deletable per Plane pattern (hard-delete).
    // This is a compile-time invariant verified by the class declaration — no runtime test needed.
}
