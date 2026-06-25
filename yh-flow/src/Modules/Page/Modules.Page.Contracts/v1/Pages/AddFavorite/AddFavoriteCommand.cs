using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.Page.Contracts.v1.Pages.AddFavorite;

/// <summary>
/// Add a page to the current user's favorites. UserId is resolved from ClaimsPrincipal.
/// </summary>
public sealed class AddFavoriteCommand : ICommand<bool>
{
    /// <summary>Route segment {pageId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid PageId { get; set; }

    /// <summary>Authenticated user id — set by the endpoint, NOT client-writable.</summary>
    [JsonIgnore]
    public string UserId { get; set; } = default!;
}