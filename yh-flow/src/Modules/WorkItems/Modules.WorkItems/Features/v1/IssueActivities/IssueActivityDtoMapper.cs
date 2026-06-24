using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.IssueActivities;

/// <summary>
/// Maps <see cref="IssueActivity"/> domain entities to <see cref="IssueActivityDto"/>.
/// </summary>
internal static class IssueActivityDtoMapper
{
    internal static IssueActivityDto ToDto(IssueActivity a) =>
        new()
        {
            Id = a.Id,
            IssueId = a.IssueId,
            Verb = a.Verb,
            Field = a.Field,
            OldValue = a.OldValue,
            NewValue = a.NewValue,
            Comment = a.Comment,
            ActorId = a.ActorId,
            IssueCommentId = a.IssueCommentId,
            Epoch = a.Epoch,
            CreatedAt = a.CreatedOnUtc,
        };

    internal static List<IssueActivityDto> ToDtoList(IEnumerable<IssueActivity> activities) =>
        activities.Select(ToDto).ToList();
}
