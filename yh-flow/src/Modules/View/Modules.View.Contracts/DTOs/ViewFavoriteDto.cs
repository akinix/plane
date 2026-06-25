using System.Text.Json.Serialization;

namespace YH.Modules.View.Contracts.DTOs;

/// <summary>
/// View favorite DTO.
/// </summary>
public class ViewFavoriteDto
{
    public Guid Id { get; set; }

    [JsonPropertyName("view_id")]
    public Guid ViewId { get; set; }

    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = default!;
}