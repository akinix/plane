using System.Text.Json.Serialization;

namespace YH.Modules.WorkItems.Contracts.DTOs;

/// <summary>
/// Issue detail DTO — extends <see cref="IssueDto"/> with navigation fields.
/// </summary>
public class IssueDetailDto : IssueDto
{
    /// <summary>Issue assignees (M2M through IssueAssignee table).</summary>
    public List<IssueAssigneeDto>? Assignees { get; set; }

    /// <summary>Issue labels (M2M through IssueLabel table).</summary>
    public List<IssueLabelDto>? Labels { get; set; }

    [JsonPropertyName("state_name")]
    public string? StateName { get; set; }

    [JsonPropertyName("state_group")]
    public int? StateGroup { get; set; }

    [JsonPropertyName("parent_issue_name")]
    public string? ParentIssueName { get; set; }

    /// <summary>Formatted display string like "PROJ-1" — built from Project.Identifier + SequenceId.</summary>
    [JsonPropertyName("sequence_id_display")]
    public string? SequenceIdDisplay { get; set; }
}
