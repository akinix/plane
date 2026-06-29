using YH.Modules.Page.Contracts.DTOs;
using PageEntity = YH.Modules.Page.Domain.Page;

namespace YH.Modules.Page.Features.v1.Pages;

/// <summary>
/// Maps a <see cref="PageEntity"/> domain entity to <see cref="PageDto"/> / <see cref="PageDetailDto"/> contract DTOs.
/// </summary>
internal static class PageDtoMapper
{
    internal static PageDto ToDto(PageEntity p) =>
        new()
        {
            Id = p.Id,
            Name = p.Name,
            Access = (int)p.Access,
            Color = p.Color,
            IsLocked = p.IsLocked,
            IsGlobal = p.IsGlobal,
            SortOrder = p.SortOrder,
            OwnedBy = p.OwnedBy,
            ParentId = p.ParentId,
            ArchivedAt = p.ArchivedAt,
            ViewProps = p.ViewProps,
            LogoProps = p.LogoProps,
            ExternalSource = p.ExternalSource,
            ExternalId = p.ExternalId,
            CreatedAt = p.CreatedOnUtc,
            UpdatedAt = p.LastModifiedOnUtc,
        };

    internal static PageDetailDto ToDetailDto(PageEntity p) =>
        new()
        {
            Id = p.Id,
            Name = p.Name,
            Access = (int)p.Access,
            Color = p.Color,
            IsLocked = p.IsLocked,
            IsGlobal = p.IsGlobal,
            SortOrder = p.SortOrder,
            OwnedBy = p.OwnedBy,
            ParentId = p.ParentId,
            ArchivedAt = p.ArchivedAt,
            ViewProps = p.ViewProps,
            LogoProps = p.LogoProps,
            ExternalSource = p.ExternalSource,
            ExternalId = p.ExternalId,
            CreatedAt = p.CreatedOnUtc,
            UpdatedAt = p.LastModifiedOnUtc,
            DescriptionHtml = p.DescriptionHtml,
            DescriptionStripped = p.DescriptionStripped,
            DescriptionJson = p.DescriptionJson,
        };
}