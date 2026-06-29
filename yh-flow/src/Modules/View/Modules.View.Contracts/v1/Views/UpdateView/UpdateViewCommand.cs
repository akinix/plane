using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.View.Contracts.DTOs;

namespace YH.Modules.View.Contracts.v1.Views.UpdateView;

public class UpdateViewCommand : ICommand<ViewDetailDto>
{
    [JsonIgnore]
    public Guid ViewId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Filters { get; set; }

    public string? DisplayFilters { get; set; }

    public string? DisplayProperties { get; set; }

    public string? RichFilters { get; set; }

    public int? Access { get; set; }

    public double? SortOrder { get; set; }

    public string? LogoProps { get; set; }
}