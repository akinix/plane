namespace YH.Modules.View.Contracts.Constants;

/// <summary>
/// View module constants: validation limits.
/// Mirrors Plane <c>apps/api/plane/db/models/issue_view.py</c> field max-length constraints.
/// </summary>
public static class ViewConstants
{
    /// <summary>Maximum length for view name (Plane: 255 chars).</summary>
    public const int NameMaxLength = 255;

    /// <summary>Maximum length for description fields (Plane: unrestricted, nvarchar(max)). Use -1 to signal max.</summary>
    public const int DescriptionMaxLength = -1;

    /// <summary>Default sort order for views (Plane convention).</summary>
    public const double DefaultSortOrder = 65535.0;
}
