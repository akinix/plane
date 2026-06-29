using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.IssueLinks;

/// <summary>
/// Maps <see cref="IssueLink"/> domain entities to <see cref="IssueLinkDto"/> contract DTOs.
/// </summary>
internal static class IssueLinkDtoMapper
{
    internal static IssueLinkDto ToDto(IssueLink l) =>
        new()
        {
            Id = l.Id,
            IssueId = l.IssueId,
            RelatedIssueId = l.RelatedIssueId,
            Url = l.Url,
            Title = l.Title,
            LinkType = (int)l.LinkType,
            Metadata = l.Metadata,
            CreatedAt = l.CreatedOnUtc,
        };

    internal static List<IssueLinkDto> ToDtoList(IEnumerable<IssueLink> links) =>
        links.Select(ToDto).ToList();
}
