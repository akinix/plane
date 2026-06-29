using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.View.Contracts.DTOs;

namespace YH.Modules.View.Contracts.v1.Views.ListViews;

public class ListViewsQuery : IQuery<List<ViewDto>>
{
    [JsonIgnore]
    public Guid? ProjectId { get; set; }

    public bool WorkspaceScope { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 30;
}