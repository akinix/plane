using System.Text.Json.Serialization;

namespace YH.Modules.WorkItems.Contracts.DTOs;

/// <summary>
/// ModuleLink response DTO — external resource link attached to a module.
/// JsonPropertyName attributes follow Plane JSON naming convention (snake_case).
/// </summary>
public class ModuleLinkDto
{
    public Guid Id { get; set; }

    /// <summary>Link title (max 255).</summary>
    public string Title { get; set; } = default!;

    /// <summary>Link URL (max 2048).</summary>
    public string Url { get; set; } = default!;

    /// <summary>Optional metadata (JSON string).</summary>
    public string? Metadata { get; set; }

    /// <summary>Module that owns this link.</summary>
    public Guid ModuleId { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
}
