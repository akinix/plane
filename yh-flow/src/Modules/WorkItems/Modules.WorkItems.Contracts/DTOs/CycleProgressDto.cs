namespace YH.Modules.WorkItems.Contracts.DTOs;

/// <summary>
/// Cycle progress snapshot DTO.
/// Contains the completion chart data for a cycle's burndown/overview visualization.
/// </summary>
public class CycleProgressDto
{
    public Dictionary<string, int?> CompletionChart { get; init; } = [];
}
