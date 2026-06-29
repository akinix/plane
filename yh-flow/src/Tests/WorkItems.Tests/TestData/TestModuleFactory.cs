namespace YH.Tests.WorkItems.TestData;

/// <summary>Factory helpers for creating <see cref="Module"/> instances in tests.</summary>
internal static class TestModuleFactory
{
    private static readonly Guid DefaultProjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public static Module CreateValid(
        string name = "Sprint Planning",
        Guid? projectId = null,
        string? status = null,
        DateTimeOffset? startDate = null,
        DateTimeOffset? targetDate = null)
    {
        return Module.Create(name, projectId ?? DefaultProjectId, status,
            startDate ?? DateTimeOffset.UtcNow,
            targetDate ?? DateTimeOffset.UtcNow.AddDays(14));
    }

    public static Module CreatePlanned(
        string name = "Q2 Features",
        Guid? projectId = null)
    {
        return Module.Create(name, projectId ?? DefaultProjectId,
            status: "planned", startDate: DateTimeOffset.UtcNow.AddDays(7),
            targetDate: DateTimeOffset.UtcNow.AddDays(28));
    }

    public static Module CreateCompleted(
        string name = "Completed Module",
        Guid? projectId = null)
    {
        return Module.Create(name, projectId ?? DefaultProjectId,
            status: "completed",
            startDate: DateTimeOffset.UtcNow.AddDays(-30),
            targetDate: DateTimeOffset.UtcNow.AddDays(-1));
    }
}

/// <summary>Factory helpers for creating <see cref="ModuleIssue"/> instances in tests.</summary>
internal static class TestModuleIssueFactory
{
    public static ModuleIssue CreateValid(Guid issueId, Guid moduleId)
        => ModuleIssue.Create(issueId, moduleId);
}

/// <summary>Factory helpers for creating <see cref="ModuleMember"/> instances in tests.</summary>
internal static class TestModuleMemberFactory
{
    public static ModuleMember CreateValid(Guid memberId, Guid moduleId)
        => ModuleMember.Create(memberId, moduleId);
}

/// <summary>Factory helpers for creating <see cref="ModuleLink"/> instances in tests.</summary>
internal static class TestModuleLinkFactory
{
    public static ModuleLink CreateValid(
        string title = "Figma Design",
        string url = "https://figma.com/file/abc",
        string? metadata = null,
        Guid? moduleId = null)
        => ModuleLink.Create(title, url, metadata,
            moduleId ?? Guid.NewGuid());
}
