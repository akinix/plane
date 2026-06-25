using YH.Modules.View.Domain;
using ViewEntity = YH.Modules.View.Domain.View;

namespace YH.Tests.View.TestData;

/// <summary>Factory helpers for creating <see cref="ViewEntity"/> instances in tests.</summary>
internal static class TestViewFactory
{
    private static readonly Guid DefaultOwnedBy = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public static ViewEntity CreateValid(
        string name = "Test View",
        Guid? projectId = null,
        Guid? ownedBy = null)
    {
        return ViewEntity.Create(name,
            ownedBy ?? DefaultOwnedBy,
            projectId: projectId);
    }

    public static ViewEntity CreateWorkspaceView(
        string name = "Workspace View")
    {
        return ViewEntity.Create(name,
            DefaultOwnedBy,
            projectId: null);
    }

    public static ViewEntity CreatePrivate(
        string name = "Private View")
    {
        return ViewEntity.Create(name,
            DefaultOwnedBy,
            access: ViewAccess.Private);
    }

    public static ViewEntity CreateArchived(
        string name = "Archived View")
    {
        var view = ViewEntity.Create(name, DefaultOwnedBy);
        view.Archive();
        return view;
    }

    public static ViewEntity CreateWithFilters(
        string name,
        string filters)
    {
        return ViewEntity.Create(name,
            DefaultOwnedBy,
            filters: filters);
    }
}

/// <summary>Factory helpers for creating <see cref="ViewFavorite"/> instances in tests.</summary>
internal static class TestViewFavoriteFactory
{
    public static ViewFavorite CreateValid(Guid viewId, string userId = "user-1")
    {
        return ViewFavorite.Create(viewId, userId);
    }
}