namespace YH.Modules.Project.Contracts;

/// <summary>
/// Project module constants: validation limits and forbidden patterns.
/// Mirrors Plane <c>apps/api/plane/db/models/project.py</c> FORBIDDEN_IDENTIFIER_CHARS_PATTERN
/// and field max-length constraints.
/// </summary>
public static class ProjectConstants
{
    /// <summary>
    /// Regex pattern matching forbidden identifier characters.
    /// Source: Plane <c>project.py:143</c> — <c>FORBIDDEN_IDENTIFIER_CHARS_PATTERN</c>.
    /// </summary>
    public const string ForbiddenIdentifierCharsPattern = @"[&+,:;$^*}{=?@#|'<>.()%!\-]";

    /// <summary>Maximum length for project identifier (Plane: 12 chars).</summary>
    public const int IdentifierMaxLength = 12;

    /// <summary>Maximum length for project name.</summary>
    public const int NameMaxLength = 255;

    /// <summary>Maximum length for project description.</summary>
    public const int DescriptionMaxLength = 5000;

    /// <summary>Maximum length for project slug.</summary>
    public const int SlugMaxLength = 100;
}
