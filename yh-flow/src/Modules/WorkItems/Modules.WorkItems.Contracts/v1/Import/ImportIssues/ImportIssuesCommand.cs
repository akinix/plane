using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.Import.ImportIssues;

/// <summary>
/// Import issues from CSV or JSON format.
/// <see cref="ProjectId"/> is populated from the route by the endpoint.
/// </summary>
public sealed class ImportIssuesCommand : ICommand<ImportResultDto>
{
    /// <summary>Import format: "csv" or "json".</summary>
    public string Format { get; set; } = "csv";

    /// <summary>Raw file content (from uploaded file or base64 body).</summary>
    public byte[] FileContent { get; set; } = [];

    /// <summary>Route segment {projectId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }
}
