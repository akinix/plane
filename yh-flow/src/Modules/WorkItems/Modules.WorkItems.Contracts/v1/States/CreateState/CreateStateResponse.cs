namespace YH.Modules.WorkItems.Contracts.v1.States.CreateState;

/// <summary>
/// Response for POST /states/ — the created state's id.
/// </summary>
/// <param name="Id">Created state Guid.</param>
public sealed record CreateStateResponse(Guid Id);
