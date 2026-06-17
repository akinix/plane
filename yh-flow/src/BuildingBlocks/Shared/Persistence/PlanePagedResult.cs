using System.Text.Json.Serialization;

namespace YH.Framework.Shared.Persistence;

/// <summary>
/// Plane-compatible paginated response format. Uses cursor/offset pagination
/// with next/previous URL links instead of page numbers.
/// </summary>
public sealed class PlanePagedResult<T>
{
    [JsonPropertyName("count")]
    public long Count { get; init; }

    [JsonPropertyName("next")]
    public string? Next { get; init; }

    [JsonPropertyName("previous")]
    public string? Previous { get; init; }

    [JsonPropertyName("results")]
    public IReadOnlyCollection<T> Results { get; init; } = Array.Empty<T>();
}

/// <summary>
/// Factory for creating <see cref="PlanePagedResult{T}"/> instances from
/// existing <see cref="PagedResponse{T}"/> objects.
/// </summary>
public static class PlanePagedResultFactory
{
    /// <summary>
    /// Creates a PlanePagedResult from an existing PagedResponse, building
    /// next/previous URLs that preserve the original query parameters.
    /// </summary>
    public static PlanePagedResult<T> FromPagedResponse<T>(
        PagedResponse<T> paged,
        string baseUrl,
        string? additionalQueryParams = null)
    {
        ArgumentNullException.ThrowIfNull(paged);
        ArgumentNullException.ThrowIfNull(baseUrl);

        var separator = baseUrl.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        var extraParams = string.IsNullOrWhiteSpace(additionalQueryParams)
            ? string.Empty
            : $"&{additionalQueryParams}";

        string? next = null;
        string? previous = null;

        if (paged.HasNext)
        {
            next = $"{baseUrl}{separator}cursor={paged.PageNumber + 1}&limit={paged.PageSize}{extraParams}";
        }

        if (paged.HasPrevious)
        {
            previous = $"{baseUrl}{separator}cursor={paged.PageNumber - 1}&limit={paged.PageSize}{extraParams}";
        }

        return new PlanePagedResult<T>
        {
            Count = paged.TotalCount,
            Next = next,
            Previous = previous,
            Results = paged.Items,
        };
    }
}
