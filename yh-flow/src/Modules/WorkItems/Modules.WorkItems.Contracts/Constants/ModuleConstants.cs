namespace YH.Modules.WorkItems.Contracts.Constants;

/// <summary>
/// Module validation constants — mirrors <see cref="CycleConstants"/> pattern.
/// </summary>
public static class ModuleConstants
{
    /// <summary>Maximum name length (255 chars).</summary>
    public const int NameMaxLength = 255;

    /// <summary>Maximum description length (10000 chars).</summary>
    public const int DescriptionMaxLength = 10000;

    /// <summary>Default sort order (Plane convention).</summary>
    public const double DefaultSortOrder = 65535.0;

    /// <summary>Default module status.</summary>
    public const string DefaultStatus = "planned";

    /// <summary>Maximum status string length (longest: "in-progress" = 11, plus buffer).</summary>
    public const int StatusMaxLength = 20;

    /// <summary>Valid status values.</summary>
    public static readonly string[] ValidStatuses =
        ["backlog", "planned", "in-progress", "paused", "completed", "cancelled"];

    /// <summary>Checks whether <paramref name="status"/> is a valid module status value.</summary>
    public static bool IsValidStatus(string? status) =>
        status is not null && ValidStatuses.Contains(status);
}
