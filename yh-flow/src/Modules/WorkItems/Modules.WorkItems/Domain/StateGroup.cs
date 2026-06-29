namespace YH.Modules.WorkItems.Domain;

/// <summary>
/// State group classification (Plane: <c>api/plane/db/models/state.py</c> StateGroup enum).
/// Fixed 5 values, matching Plane STR_CHOICES for state groups.
/// </summary>
public enum StateGroup
{
    /// <summary>Backlog /待办 — default state for new issues not yet scheduled.</summary>
    Backlog = 0,

    /// <summary>Unstarted /未开始 — issues ready to work on but not yet in progress.</summary>
    Unstarted = 1,

    /// <summary>Started /进行中 — issues currently being worked on.</summary>
    Started = 2,

    /// <summary>Completed /已完成 — issues that have been finished (closed group).</summary>
    Completed = 3,

    /// <summary>Cancelled /已取消 — issues that were abandoned (closed group).</summary>
    Cancelled = 4,
}
