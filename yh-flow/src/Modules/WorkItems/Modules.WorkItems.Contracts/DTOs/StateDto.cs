using System.Text.Json.Serialization;

namespace YH.Modules.WorkItems.Contracts.DTOs;

/// <summary>
/// State response DTO.
/// Field set mirrors Plane <c>serializers/state.py</c> StateSerializer.
/// JsonPropertyName attributes follow Plane JSON naming convention (snake_case).
/// </summary>
public class StateDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public string? Color { get; set; }

    /// <summary>StateGroup enum integer value (Backlog=0/Unstarted=1/Started=2/Completed=3/Cancelled=4).</summary>
    public int Group { get; set; }

    /// <summary>Project that owns this state (scalar Guid, no cross-module FK).</summary>
    public Guid ProjectId { get; set; }

    /// <summary>Whether this state is the default for its Group within the project.</summary>
    public bool IsDefault { get; set; }

    /// <summary>Sort order for state listing UI (default 65535.0 per Plane convention).</summary>
    public double SortOrder { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
