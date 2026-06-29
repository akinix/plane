using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.View.Contracts.v1.Views.RemoveFavorite;

public class RemoveFavoriteCommand : ICommand<bool>
{
    public Guid ViewId { get; set; }

    [JsonIgnore]
    public string UserId { get; set; } = default!;
}