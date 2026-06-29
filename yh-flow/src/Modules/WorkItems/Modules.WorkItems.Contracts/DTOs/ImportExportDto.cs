using System.Text.Json.Serialization;

namespace YH.Modules.WorkItems.Contracts.DTOs;

/// <summary>
/// Export result — file content as byte array with content type and file name.
/// </summary>
public class ExportResultDto
{
    [JsonPropertyName("file_content")]
    public byte[] FileContent { get; set; } = [];

    [JsonPropertyName("content_type")]
    public string ContentType { get; set; } = "text/csv";

    [JsonPropertyName("file_name")]
    public string FileName { get; set; } = "export.csv";
}

/// <summary>
/// Import result summary.
/// </summary>
public class ImportResultDto
{
    public int Imported { get; set; }
    public int Skipped { get; set; }

    [JsonPropertyName("errors")]
    public List<string> Errors { get; set; } = [];
}

/// <summary>
/// A single row in the import payload.
/// </summary>
public class IssueImportRowDto
{
    /// <summary>Issue title (required).</summary>
    public string Name { get; set; } = default!;

    /// <summary>HTML description (optional).</summary>
    [JsonPropertyName("description_html")]
    public string? DescriptionHtml { get; set; }

    /// <summary>Priority: "urgent" / "high" / "medium" / "low" / "none".</summary>
    public string? Priority { get; set; }

    /// <summary>State name to map to existing project state (optional, creates if not found).</summary>
    [JsonPropertyName("state_name")]
    public string? StateName { get; set; }

    /// <summary>Comma-separated user emails for assignees (optional).</summary>
    [JsonPropertyName("assignee_emails")]
    public string? AssigneeEmails { get; set; }

    /// <summary>Label names (optional).</summary>
    [JsonPropertyName("label_names")]
    public List<string>? LabelNames { get; set; }
}
