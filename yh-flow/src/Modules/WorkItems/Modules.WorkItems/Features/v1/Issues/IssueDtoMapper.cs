using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.Issues;

/// <summary>
/// Maps <see cref="Issue"/> domain entities to <see cref="IssueDto"/> and <see cref="IssueDetailDto"/>.
/// </summary>
internal static class IssueDtoMapper
{
    internal static IssueDto ToDto(Issue i) =>
        new()
        {
            Id = i.Id,
            Name = i.Name,
            DescriptionHtml = i.DescriptionHtml,
            DescriptionJson = i.DescriptionJson,
            DescriptionStripped = i.DescriptionStripped,
            Priority = i.Priority,
            SequenceId = i.SequenceId,
            SortOrder = i.SortOrder,
            ProjectId = i.ProjectId,
            ParentId = i.ParentId,
            StateId = i.StateId,
            EstimatePointId = i.EstimatePointId,
            StartDate = i.StartDate,
            TargetDate = i.TargetDate,
            CompletedAt = i.CompletedAt,
            ArchivedAt = i.ArchivedAt,
            IsDraft = i.IsDraft,
            CreatedAt = i.CreatedOnUtc,
            UpdatedAt = i.LastModifiedOnUtc,
            DeletedAt = i.DeletedOnUtc,
        };

    internal static IssueDetailDto ToDetailDto(Issue i, string? stateName = null, int? stateGroup = null, string? sequenceIdDisplay = null)
    {
        var dto = new IssueDetailDto
        {
            Id = i.Id,
            Name = i.Name,
            DescriptionHtml = i.DescriptionHtml,
            DescriptionJson = i.DescriptionJson,
            DescriptionStripped = i.DescriptionStripped,
            Priority = i.Priority,
            SequenceId = i.SequenceId,
            SortOrder = i.SortOrder,
            ProjectId = i.ProjectId,
            ParentId = i.ParentId,
            StateId = i.StateId,
            EstimatePointId = i.EstimatePointId,
            StartDate = i.StartDate,
            TargetDate = i.TargetDate,
            CompletedAt = i.CompletedAt,
            ArchivedAt = i.ArchivedAt,
            IsDraft = i.IsDraft,
            CreatedAt = i.CreatedOnUtc,
            UpdatedAt = i.LastModifiedOnUtc,
            DeletedAt = i.DeletedOnUtc,
            StateName = stateName,
            StateGroup = stateGroup,
            SequenceIdDisplay = sequenceIdDisplay,
        };

        if (i.Assignees.Count > 0)
        {
            dto.Assignees = i.Assignees
                .Select(a => new IssueAssigneeDto
                {
                    Id = a.Id,
                    IssueId = a.IssueId,
                    AssigneeId = a.AssigneeId,
                    CreatedAt = a.CreatedOnUtc,
                })
                .ToList();
        }

        if (i.Labels.Count > 0)
        {
            dto.Labels = i.Labels
                .Select(l => new IssueLabelDto
                {
                    Id = l.Id,
                    IssueId = l.IssueId,
                    LabelId = l.LabelId,
                    CreatedAt = l.CreatedOnUtc,
                })
                .ToList();
        }

        return dto;
    }

    internal static List<IssueDto> ToDtoList(IEnumerable<Issue> issues) =>
        issues.Select(ToDto).ToList();
}
