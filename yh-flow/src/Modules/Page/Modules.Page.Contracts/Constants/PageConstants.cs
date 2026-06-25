namespace YH.Modules.Page.Contracts;

/// <summary>
/// Page module constants: validation limits.
/// Mirrors Plane <c>apps/api/plane/db/models/page.py</c> field max-length constraints.
/// </summary>
public static class PageConstants
{
    /// <summary>Maximum length for page name (Plane: 255 chars).</summary>
    public const int NameMaxLength = 255;

    /// <summary>Maximum length for description fields (Plane: unrestricted, nvarchar(max)). Use -1 to signal max.</summary>
    public const int DescriptionMaxLength = -1;

    /// <summary>Maximum length for page color hex value.</summary>
    public const int ColorMaxLength = 50;

    /// <summary>Default sort order for pages (Plane convention).</summary>
    public const double DefaultSortOrder = 65535.0;

    /// <summary>Maximum length for external source identifier.</summary>
    public const int ExternalSourceMaxLength = 255;

    /// <summary>Maximum length for external record id.</summary>
    public const int ExternalIdMaxLength = 255;
}