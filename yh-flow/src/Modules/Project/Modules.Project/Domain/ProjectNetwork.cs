namespace YH.Modules.Project.Domain;

/// <summary>
/// Project visibility (Plane <c>models/project.py</c> <c>Network</c> field).
/// Values mirror Plane's network choices: Secret (0) = only members can view,
/// Public (2) = visible to all workspace members. Value 1 is deliberately unused
/// per Plane's schema.
/// </summary>
public enum ProjectNetwork
{
    /// <summary>Private project — only project members can view (Plane: 0).</summary>
    Secret = 0,

    /// <summary>Public project — visible to all workspace members (Plane: 2).</summary>
    Public = 2,
}
