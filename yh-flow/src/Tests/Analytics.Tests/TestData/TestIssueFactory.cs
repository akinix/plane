using System.Reflection;
using WorkItemModule = YH.Modules.WorkItems.Domain.Module;

namespace YH.Tests.Analytics.TestData;

/// <summary>
/// Factory helpers for creating WorkItems domain entities in Analytics integration tests.
/// Uses reflection to set private-set properties (CreatedOnUtc, CompletedAt) for
/// date-sensitive test scenarios.
/// </summary>
internal static class TestIssueFactory
{
    private static readonly PropertyInfo CreatedOnUtcProp = typeof(Issue)
        .GetProperty("CreatedOnUtc", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

    private static readonly PropertyInfo CompletedAtProp = typeof(Issue)
        .GetProperty("CompletedAt", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

    /// <summary>
    /// Creates an <see cref="Issue"/> with optional overrides for testing.
    /// Uses <see cref="Issue.Create"/> factory then reflection-sets CreatedOnUtc and CompletedAt.
    /// </summary>
    internal static Issue CreateIssue(
        string name,
        Guid projectId,
        Guid? stateId = null,
        string priority = "none",
        DateTimeOffset? createdAt = null,
        DateTimeOffset? completedAt = null)
    {
        var issue = Issue.Create(name, projectId, stateId: stateId, priority: priority);

        if (createdAt.HasValue)
        {
            CreatedOnUtcProp.SetValue(issue, createdAt.Value);
        }

        if (completedAt.HasValue)
        {
            CompletedAtProp.SetValue(issue, completedAt.Value);
        }

        return issue;
    }

    /// <summary>
    /// Creates a <see cref="State"/> with the given parameters.
    /// </summary>
    internal static State CreateState(
        string name,
        StateGroup group,
        Guid projectId,
        string? color = null,
        bool isDefault = false)
    {
        return State.Create(name, color ?? "#cccccc", group, projectId, isDefault);
    }

    /// <summary>
    /// Creates a <see cref="Cycle"/> with the given parameters.
    /// </summary>
    internal static Cycle CreateCycle(
        Guid projectId,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        string name = "Test Cycle")
    {
        return Cycle.Create(name, projectId, startDate, endDate);
    }

    /// <summary>
    /// Creates a <see cref="WorkItemModule"/> with the given parameters.
    /// </summary>
    internal static WorkItemModule CreateModule(
        Guid projectId,
        string name = "Test Module",
        DateTimeOffset? startDate = null,
        DateTimeOffset? targetDate = null)
    {
        return WorkItemModule.Create(name, projectId, startDate: startDate, targetDate: targetDate);
    }
}
