using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.Export.ExportIssues;

/// <summary>
/// Export issues as CSV or JSON.
/// <see cref="ProjectId"/> is populated from the route by the endpoint.
/// </summary>
public sealed class ExportIssuesCommand : ICommand<ExportResultDto>
{
    /// <summary>Export format: "csv" or "json" (default "csv").</summary>
    public string Format { get; set; } = "csv";

    /// <summary>Optional filter: only include issues in these states.</summary>
    public List<Guid>? StateIds { get; set; }

    /// <summary>Optional filter: only include issues with this priority.</summary>
    public string? Priority { get; set; }

    /// <summary>Route segment {projectId} — set by the endpoint, not client-writable.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }
}
