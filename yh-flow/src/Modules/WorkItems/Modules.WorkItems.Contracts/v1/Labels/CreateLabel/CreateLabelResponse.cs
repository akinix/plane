namespace YH.Modules.WorkItems.Contracts.v1.Labels.CreateLabel;

/// <summary>
/// Response for POST /labels/ — the created label's id.
/// </summary>
/// <param name="Id">Created label Guid.</param>
public sealed record CreateLabelResponse(Guid Id);
