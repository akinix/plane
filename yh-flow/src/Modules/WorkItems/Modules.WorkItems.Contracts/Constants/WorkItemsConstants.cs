namespace YH.Modules.WorkItems.Contracts;

/// <summary>
/// WorkItems module constants: validation limits and default values.
/// Mirrors Plane field max-length constraints across Issue, State, Label, Estimate entities.
/// </summary>
public static class WorkItemsConstants
{
    /// <summary>Maximum length for entity name fields (Plane: 255 chars).</summary>
    public const int NameMaxLength = 255;

    /// <summary>Maximum length for description / comment HTML fields.</summary>
    public const int DescriptionMaxLength = 10000;

    /// <summary>Maximum length for color hex codes (e.g. "#60646C").</summary>
    public const int ColorMaxLength = 7;

    /// <summary>Maximum length for priority strings ("urgent" / "high" / "medium" / "low" / "none").</summary>
    public const int PriorityMaxLength = 10;

    /// <summary>Maximum length for project identifier prefix (Plane: 12 chars).</summary>
    public const int IdentifierMaxLength = 12;

    /// <summary>Default sort order for new entities (Plane convention: 65535.0).</summary>
    public const double DefaultSortOrder = 65535.0;

    /// <summary>
    /// Regex pattern matching characters forbidden in entity identifiers.
    /// Mirrors Plane <c>project.py</c> FORBIDDEN_IDENTIFIER_CHARS_PATTERN.
    /// </summary>
    public const string ForbiddenCharsPattern = @"[&+,:;$^*}{=?@#|'<>.()%!\-]";
}
