using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.Page.Contracts.v1.Pages.RemoveFavorite;

/// <summary>
/// Remove a page from the current user's favorites.
/// </summary>
public sealed class RemoveFavoriteCommand : ICommand<bool>
{
    /// <summary>Route segment {pageId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid PageId { get; set; }

    /// <summary>Authenticated user id — set by the endpoint, NOT client-writable.</summary>
    [JsonIgnore]
    public string UserId { get; set; } = default!;
}