using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.Cycles;

/// <summary>
/// Maps a <see cref="Cycle"/> domain entity to a <see cref="CycleDto"/> contract DTO.
/// </summary>
internal static class CycleDtoMapper
{
    internal static CycleDto ToDto(Cycle c, string status, int totalIssues, int completedIssues,
        int cancelledIssues, int startedIssues, int unstartedIssues, int backlogIssues) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Description = c.Description,
        StartDate = c.StartDate,
        EndDate = c.EndDate,
        SortOrder = c.SortOrder,
        ExternalSource = c.ExternalSource,
        ExternalId = c.ExternalId,
        ProgressSnapshot = c.ProgressSnapshot,
        ArchivedAt = c.ArchivedAt,
        LogoProps = c.LogoProps,
        Timezone = c.Timezone,
        Version = c.Version,
        Status = status,
        TotalIssues = totalIssues,
        CompletedIssues = completedIssues,
        CancelledIssues = cancelledIssues,
        StartedIssues = startedIssues,
        UnstartedIssues = unstartedIssues,
        BacklogIssues = backlogIssues,
        IsFavorite = false,
        AssigneeIds = [],
        CreatedAt = c.CreatedOnUtc,
        UpdatedAt = c.LastModifiedOnUtc,
    };

    /// <summary>
    /// Computes the cycle status based on its dates (Plane behavior).
    /// </summary>
    internal static string ComputeStatus(Cycle c) => (c.StartDate, c.EndDate) switch
    {
        (null, null) => "DRAFT",
        (_, null) or (null, _) => "DRAFT",
        _ when c.StartDate > DateTimeOffset.UtcNow => "UPCOMING",
        _ when c.EndDate < DateTimeOffset.UtcNow => "COMPLETED",
        _ => "CURRENT"
    };
}
