namespace YH.Modules.WorkItems.Contracts.v1.Cycles.DateCheckCycle;

/// <summary>
/// Response for date overlap check.
/// </summary>
public sealed record DateCheckCycleResponse(bool IsDateAvailable, List<string>? OverlappingCycleNames);
