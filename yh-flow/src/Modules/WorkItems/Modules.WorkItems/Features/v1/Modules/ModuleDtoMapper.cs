using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.Modules;

/// <summary>
/// Maps a <see cref="Module"/> domain entity to a <see cref="ModuleDto"/> contract DTO.
/// </summary>
internal static class ModuleDtoMapper
{
    internal static ModuleDto ToDto(Module m, int totalIssues, int completedIssues,
        int cancelledIssues, int startedIssues, int unstartedIssues, int backlogIssues) => new()
    {
        Id = m.Id,
        Name = m.Name,
        Description = m.Description,
        Status = m.Status,
        StartDate = m.StartDate,
        TargetDate = m.TargetDate,
        SortOrder = m.SortOrder,
        LeadId = m.LeadId,
        ProgressSnapshot = m.ProgressSnapshot,
        ArchivedAt = m.ArchivedAt,
        LogoProps = m.LogoProps,
        Version = m.Version,
        ProjectId = m.ProjectId,
        TotalIssues = totalIssues,
        CompletedIssues = completedIssues,
        CancelledIssues = cancelledIssues,
        StartedIssues = startedIssues,
        UnstartedIssues = unstartedIssues,
        BacklogIssues = backlogIssues,
        CreatedAt = m.CreatedOnUtc,
        UpdatedAt = m.LastModifiedOnUtc,
    };
}
