using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.Intake;

/// <summary>
/// Maps <see cref="IntakeIssue"/> domain entities to <see cref="IntakeIssueDto"/>.
/// </summary>
internal static class IntakeIssueDtoMapper
{
    internal static IntakeIssueDto ToDto(IntakeIssue i) =>
        new()
        {
            Id = i.Id,
            IssueId = i.IssueId,
            ProjectId = i.ProjectId,
            Status = (int)i.Status,
            SnoozedTill = i.SnoozedTill,
            DuplicateToIssueId = i.DuplicateToIssueId,
            Source = i.Source,
            CreatedAt = i.CreatedOnUtc,
            UpdatedAt = i.LastModifiedOnUtc,
        };

    internal static List<IntakeIssueDto> ToDtoList(IEnumerable<IntakeIssue> items) =>
        items.Select(ToDto).ToList();
}
