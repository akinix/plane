using YH.Modules.View.Contracts.DTOs;
using ViewEntity = YH.Modules.View.Domain.View;

namespace YH.Modules.View.Features.v1.Views;

/// <summary>
/// Maps a <see cref="ViewEntity"/> domain entity to <see cref="ViewDto"/> / <see cref="ViewDetailDto"/> contract DTOs.
/// </summary>
internal static class ViewDtoMapper
{
    internal static ViewDto ToDto(ViewEntity v) =>
        new()
        {
            Id = v.Id,
            Name = v.Name,
            Description = v.Description,
            Access = (int)v.Access,
            SortOrder = v.SortOrder,
            LogoProps = v.LogoProps,
            OwnedBy = v.OwnedBy,
            IsLocked = v.IsLocked,
            ArchivedAt = v.ArchivedAt,
            ProjectId = v.ProjectId,
            CreatedAt = v.CreatedOnUtc,
            UpdatedAt = v.LastModifiedOnUtc,
        };

    internal static ViewDetailDto ToDetailDto(ViewEntity v) =>
        new()
        {
            Id = v.Id,
            Name = v.Name,
            Description = v.Description,
            Access = (int)v.Access,
            SortOrder = v.SortOrder,
            LogoProps = v.LogoProps,
            OwnedBy = v.OwnedBy,
            IsLocked = v.IsLocked,
            ArchivedAt = v.ArchivedAt,
            ProjectId = v.ProjectId,
            CreatedAt = v.CreatedOnUtc,
            UpdatedAt = v.LastModifiedOnUtc,
            Query = v.Query,
            Filters = v.Filters,
            DisplayFilters = v.DisplayFilters,
            DisplayProperties = v.DisplayProperties,
            RichFilters = v.RichFilters,
        };
}
