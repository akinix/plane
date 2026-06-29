using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.States;

/// <summary>
/// Maps a <see cref="State"/> domain entity to a <see cref="StateDto"/> contract DTO.
/// </summary>
internal static class StateDtoMapper
{
    internal static StateDto ToDto(State s) =>
        new()
        {
            Id = s.Id,
            Name = s.Name,
            Color = s.Color,
            Group = (int)s.Group,
            ProjectId = s.ProjectId,
            IsDefault = s.IsDefault,
            SortOrder = s.SortOrder,
            CreatedAt = s.CreatedOnUtc,
            UpdatedAt = s.LastModifiedOnUtc,
        };

    internal static List<StateDto> ToDtoList(IEnumerable<State> states) =>
        states.Select(ToDto).ToList();
}
