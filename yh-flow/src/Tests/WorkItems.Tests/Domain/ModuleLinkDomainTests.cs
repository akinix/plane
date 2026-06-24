namespace YH.Tests.WorkItems.Domain;

/// <summary>
/// Domain unit tests for <see cref="ModuleLink"/> entity.
/// Covers creation, update, and soft delete behaviors.
/// </summary>
public sealed class ModuleLinkDomainTests
{
    private static readonly Guid ModuleId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidArgs_SetsProperties()
    {
        var link = ModuleLink.Create("Figma Design", "https://figma.com/file/abc", null, ModuleId);

        link.Title.ShouldBe("Figma Design");
        link.Url.ShouldBe("https://figma.com/file/abc");
        link.ModuleId.ShouldBe(ModuleId);
        link.Metadata.ShouldBeNull();
    }

    [Fact]
    public void Create_WithEmptyTitle_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            ModuleLink.Create("", "https://figma.com/file/abc", null, ModuleId));
    }

    [Fact]
    public void Create_WithEmptyUrl_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            ModuleLink.Create("Figma Design", "", null, ModuleId));
    }

    [Fact]
    public void Update_ChangesTitleAndUrl()
    {
        var link = ModuleLink.Create("Figma Design", "https://figma.com/file/abc", null, ModuleId);

        link.Update(title: "Updated Design", url: "https://figma.com/file/def", metadata: "{}");

        link.Title.ShouldBe("Updated Design");
        link.Url.ShouldBe("https://figma.com/file/def");
        link.Metadata.ShouldBe("{}");
    }

    [Fact]
    public void Update_WithNullParams_DoesNotChange()
    {
        var link = ModuleLink.Create("Figma Design", "https://figma.com/file/abc", "{}", ModuleId);

        link.Update(title: null, url: null, metadata: null);

        link.Title.ShouldBe("Figma Design");
        link.Url.ShouldBe("https://figma.com/file/abc");
        link.Metadata.ShouldBe("{}");
    }

    [Fact]
    public void SoftDelete_SetsDeletedFlag()
    {
        var link = ModuleLink.Create("Figma Design", "https://figma.com/file/abc", null, ModuleId);
        var now = DateTimeOffset.UtcNow;

        link.SoftDelete(now);

        link.IsDeleted.ShouldBeTrue();
    }
}
