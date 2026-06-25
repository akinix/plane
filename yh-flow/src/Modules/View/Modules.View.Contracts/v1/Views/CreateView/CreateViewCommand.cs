using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.View.Contracts.v1.Views.CreateView;

/// <summary>
/// Command to create a new view.
/// ProjectId is extracted from route, OwnedBy from ClaimsPrincipal via [JsonIgnore].
/// </summary>
public class CreateViewCommand : ICommand<CreateViewResponse>
{
    public string Name { get; set; } = default!;

    public string? Description { get; set; }

    [JsonIgnore]
    public Guid OwnedBy { get; set; }

    public string? Filters { get; set; }

    public string? DisplayFilters { get; set; }

    public string? DisplayProperties { get; set; }

    public string? RichFilters { get; set; }

    public int Access { get; set; } = 1;

    public double SortOrder { get; set; } = 65535.0;

    public string? LogoProps { get; set; }

    [JsonIgnore]
    public Guid? ProjectId { get; set; }
}

public record CreateViewResponse(Guid Id);