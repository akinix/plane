namespace YH.Modules.WorkItems.Contracts.DTOs;

/// <summary>
/// Module progress DTO — real-time aggregated issue statistics for a module.
/// Computed at query time from ModuleIssues -> Issues -> States -> StateGroup.
/// </summary>
public class ModuleProgressDto
{
    /// <summary>Total number of issues in this module.</summary>
    public int TotalIssues { get; set; }

    /// <summary>Number of completed issues (StateGroup.Completed).</summary>
    public int CompletedIssues { get; set; }

    /// <summary>Number of cancelled issues (StateGroup.Cancelled).</summary>
    public int CancelledIssues { get; set; }

    /// <summary>Number of started/in-progress issues (StateGroup.Started).</summary>
    public int StartedIssues { get; set; }

    /// <summary>Number of unstarted issues (StateGroup.Unstarted).</summary>
    public int UnstartedIssues { get; set; }

    /// <summary>Number of backlog issues (StateGroup.Backlog).</summary>
    public int BacklogIssues { get; set; }

    /// <summary>Completion percentage (completed / total * 100), rounded to 1 decimal.</summary>
    public double CompletedPercentage { get; set; }
}
