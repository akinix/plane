namespace YH.Modules.Page.Contracts.v1.Pages.CreatePage;

/// <summary>
/// Response for creating a page — the created page's id.
/// </summary>
/// <param name="Id">Created page Guid.</param>
public sealed record CreatePageResponse(Guid Id);