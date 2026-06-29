namespace YH.Modules.Workspace.Contracts;

/// <summary>
/// Workspace membership role (CONTEXT D-11, RESEARCH §Permission Model).
/// Numerical values mirror Plane <c>app/permissions/workspace.py</c> <c>ROLE_CHOICES</c>:
/// Admin = 20 (full: CRUD workspace/settings/members/invitations),
/// Member = 15 (read/write workspace content; cannot manage members or settings),
/// Guest = 5 (read-only).
/// <see cref="None"/> (=0) is the canonical "no role / not a member" sentinel (CA1008); it is never
/// stored on a <c>WorkspaceMember</c> row. Capability matrix is enforced via
/// <c>[RequireWorkspaceRole]</c> authorization (D-11).
/// </summary>
public enum WorkspaceRole
{
    /// <summary>
    /// Sentinel "no role" value (CA1008). Used when a user is not an active member of the resolved
    /// workspace; never persisted on a membership row.
    /// </summary>
    None = 0,

    /// <summary>Read-only access (Plane numeric value: 5).</summary>
    Guest = 5,

    /// <summary>Read/write workspace content; cannot manage members or settings (Plane numeric value: 15).</summary>
    Member = 15,

    /// <summary>Full workspace control including members/invitations/settings (Plane numeric value: 20).</summary>
    Admin = 20,
}
