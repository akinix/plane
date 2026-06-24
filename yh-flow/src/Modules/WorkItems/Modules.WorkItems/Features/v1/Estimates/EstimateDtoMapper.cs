using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.Estimates;

/// <summary>
/// Maps <see cref="Estimate"/> domain entities to <see cref="EstimateDto"/> contract DTOs,
/// and <see cref="EstimatePoint"/> to <see cref="EstimatePointDto"/>.
/// </summary>
internal static class EstimateDtoMapper
{
    internal static EstimateDto ToDto(Estimate e)
    {
        var dto = new EstimateDto
        {
            Id = e.Id,
            Name = e.Name,
            Type = e.Type,
            ProjectId = e.ProjectId,
            IsLastUsed = e.IsLastUsed,
            CreatedAt = e.CreatedOnUtc,
            UpdatedAt = e.LastModifiedOnUtc,
        };

        if (e.EstimatePoints.Count > 0)
        {
            dto.EstimatePoints = e.EstimatePoints
                .OrderBy(ep => ep.Key)
                .Select(ToPointDto)
                .ToList();
        }

        return dto;
    }

    internal static EstimatePointDto ToPointDto(EstimatePoint ep) =>
        new()
        {
            Id = ep.Id,
            EstimateId = ep.EstimateId,
            Key = ep.Key,
            Value = ep.Value,
            SortOrder = ep.SortOrder,
            CreatedAt = ep.CreatedOnUtc,
            UpdatedAt = ep.LastModifiedOnUtc,
        };

    internal static List<EstimateDto> ToDtoList(IEnumerable<Estimate> estimates) =>
        estimates.Select(ToDto).ToList();
}
