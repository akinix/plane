namespace YH.Modules.WorkItems.Domain;

/// <summary>
/// Issue link type — combined external-link + internal-relation pattern per CONTEXT decision.
/// Maps to Plane's IssueRelation types (RelatesTo/Duplicate/Blocks/BlockedBy).
/// </summary>
public enum LinkType
{
    /// <summary>Related to (default).</summary>
    RelatesTo = 0,

    /// <summary>Duplicate of another issue.</summary>
    Duplicate = 1,

    /// <summary>Blocks another issue.</summary>
    Blocks = 2,

    /// <summary>Blocked by another issue.</summary>
    BlockedBy = 3,
}
