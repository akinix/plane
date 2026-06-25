using System.Text.Json.Serialization;

namespace YH.Modules.Page.Contracts.DTOs;

/// <summary>
/// Project-Page association DTO.
/// </summary>
public class ProjectPageDto
{
    public Guid Id { get; set; }

    [JsonPropertyName("page_id")]
    public Guid PageId { get; set; }

    [JsonPropertyName("project_id")]
    public Guid ProjectId { get; set; }
}