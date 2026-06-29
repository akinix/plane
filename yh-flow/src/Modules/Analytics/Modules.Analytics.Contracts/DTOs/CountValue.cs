using System.Text.Json.Serialization;

namespace YH.Modules.Analytics.Contracts.DTOs;

public sealed class CountValue
{
    [JsonPropertyName("count")]
    public int Count { get; set; }
}
