using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.Labels;

/// <summary>
/// Maps a <see cref="Label"/> domain entity to a <see cref="LabelDto"/> contract DTO.
/// </summary>
internal static class LabelDtoMapper
{
    internal static LabelDto ToDto(Label l) =>
        new()
        {
            Id = l.Id,
            Name = l.Name,
            Color = l.Color,
            ParentId = l.ParentId,
            ProjectId = l.ProjectId,
            SortOrder = l.SortOrder,
            CreatedAt = l.CreatedOnUtc,
            UpdatedAt = l.LastModifiedOnUtc,
        };

    internal static List<LabelDto> ToDtoList(IEnumerable<Label> labels) =>
        labels.Select(ToDto).ToList();
}
