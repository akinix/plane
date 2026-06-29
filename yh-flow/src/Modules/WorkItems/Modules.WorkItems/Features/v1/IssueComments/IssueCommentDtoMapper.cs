using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.IssueComments;

/// <summary>
/// Maps <see cref="IssueComment"/> domain entities to <see cref="IssueCommentDto"/>.
/// </summary>
internal static class IssueCommentDtoMapper
{
    internal static IssueCommentDto ToDto(IssueComment c) =>
        new()
        {
            Id = c.Id,
            IssueId = c.IssueId,
            CommentHtml = c.CommentHtml,
            CommentJson = c.CommentJson,
            CommentStripped = c.CommentStripped,
            ActorId = c.ActorId,
            ParentId = c.ParentId,
            EditedAt = c.EditedAt,
            CreatedAt = c.CreatedOnUtc,
            UpdatedAt = c.LastModifiedOnUtc,
            DeletedAt = c.DeletedOnUtc,
        };

    internal static List<IssueCommentDto> ToDtoList(IEnumerable<IssueComment> comments) =>
        comments.Select(ToDto).ToList();
}
