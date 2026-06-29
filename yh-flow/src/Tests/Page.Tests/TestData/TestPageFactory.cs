using YH.Modules.Page.Domain;
using PageEntity = YH.Modules.Page.Domain.Page;

namespace YH.Tests.Page.TestData;

/// <summary>Factory helpers for creating <see cref="PageEntity"/> instances in tests.</summary>
internal static class TestPageFactory
{
    private static readonly Guid DefaultProjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid DefaultOwnedBy = Guid.Parse("00000000-0000-0000-0000-000000000002");

    public static PageEntity CreateValid(
        string name = "Test Page",
        Guid? projectId = null,
        Guid? ownedBy = null)
    {
        return PageEntity.Create(name,
            projectId ?? DefaultProjectId,
            ownedBy ?? DefaultOwnedBy);
    }

    public static PageEntity CreateWithParent(
        string name = "Child Page",
        Guid? parentId = null,
        Guid? projectId = null)
    {
        return PageEntity.Create(name,
            projectId ?? DefaultProjectId,
            DefaultOwnedBy,
            parentId: parentId ?? Guid.NewGuid());
    }

    public static PageEntity CreatePrivate(
        string name = "Private Page",
        Guid? projectId = null)
    {
        return PageEntity.Create(name,
            projectId ?? DefaultProjectId,
            DefaultOwnedBy,
            access: PageAccess.Private);
    }

    public static PageEntity CreateArchived(
        string name = "Archived Page",
        Guid? projectId = null)
    {
        var page = PageEntity.Create(name,
            projectId ?? DefaultProjectId,
            DefaultOwnedBy);
        page.Archive();
        return page;
    }

    public static PageEntity CreateWithDescription(
        string html = "<p>Hello</p>",
        string stripped = "Hello",
        string? json = null,
        string name = "Page with Description",
        Guid? projectId = null)
    {
        var page = PageEntity.Create(name,
            projectId ?? DefaultProjectId,
            DefaultOwnedBy);
        page.UpdateDescription(html, stripped, json);
        return page;
    }
}

/// <summary>Factory helpers for creating <see cref="ProjectPage"/> instances in tests.</summary>
internal static class TestProjectPageFactory
{
    public static ProjectPage CreateValid(Guid pageId, Guid projectId)
    {
        return ProjectPage.Create(pageId, projectId);
    }
}

/// <summary>Factory helpers for creating <see cref="PageFavorite"/> instances in tests.</summary>
internal static class TestPageFavoriteFactory
{
    public static PageFavorite CreateValid(Guid pageId, string userId = "user-1")
    {
        return PageFavorite.Create(pageId, userId);
    }
}
