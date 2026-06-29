namespace YH.Modules.WorkItems.Contracts.v1.Issues.CreateIssue;

/// <summary>
/// Response from creating an issue.
/// Returns the issue id and auto-generated sequence id.
/// </summary>
public sealed class CreateIssueResponse
{
    public Guid Id { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("sequence_id")]
    public int SequenceId { get; set; }

    public CreateIssueResponse(Guid id, int sequenceId)
    {
        Id = id;
        SequenceId = sequenceId;
    }
}
