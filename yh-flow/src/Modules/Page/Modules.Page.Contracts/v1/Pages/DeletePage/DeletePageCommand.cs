using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.Page.Contracts.v1.Pages.DeletePage;

/// <summary>
/// Soft-delete a page by id. PageId is populated from the route by the endpoint.
/// </summary>
public sealed class DeletePageCommand : ICommand<bool>
{
    /// <summary>Route segment {pageId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid PageId { get; set; }
}