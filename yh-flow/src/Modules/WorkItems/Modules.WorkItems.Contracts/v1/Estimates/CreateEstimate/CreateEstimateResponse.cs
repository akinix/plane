namespace YH.Modules.WorkItems.Contracts.v1.Estimates.CreateEstimate;

/// <summary>
/// Response from creating an estimate system.
/// </summary>
public sealed class CreateEstimateResponse
{
    public Guid Id { get; set; }

    public CreateEstimateResponse(Guid id)
    {
        Id = id;
    }
}
