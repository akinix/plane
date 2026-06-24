namespace YH.Tests.WorkItems.TestData;

/// <summary>Factory helpers for creating <see cref="State"/> instances in tests.</summary>
internal static class TestStateFactory
{
    public static State CreateValid(
        string name = "Todo",
        string? color = "#3A85FF",
        StateGroup group = StateGroup.Unstarted,
        Guid? projectId = null,
        bool isDefault = true,
        double sortOrder = 65535.0)
    {
        return State.Create(name, color, group,
            projectId ?? Guid.Parse("00000000-0000-0000-0000-000000000001"),
            isDefault, sortOrder);
    }

    public static State CreateCompleted(
        string name = "Done",
        Guid? projectId = null)
    {
        return State.Create(name, "#00FF00", StateGroup.Completed,
            projectId ?? Guid.Parse("00000000-0000-0000-0000-000000000001"),
            true);
    }

    public static State CreateCancelled(
        string name = "Cancelled",
        Guid? projectId = null)
    {
        return State.Create(name, "#FF0000", StateGroup.Cancelled,
            projectId ?? Guid.Parse("00000000-0000-0000-0000-000000000001"),
            true);
    }
}

/// <summary>Factory helpers for creating <see cref="Label"/> instances in tests.</summary>
internal static class TestLabelFactory
{
    public static Label CreateValid(
        string name = "Bug",
        string? color = "#F59E0B",
        Guid? projectId = null,
        Guid? parentId = null,
        string? description = null,
        double sortOrder = 65535.0)
    {
        return Label.Create(name, color,
            projectId ?? Guid.Parse("00000000-0000-0000-0000-000000000001"),
            parentId, description, sortOrder);
    }
}

/// <summary>Factory helpers for creating <see cref="Estimate"/> instances in tests.</summary>
internal static class TestEstimateFactory
{
    public static Estimate CreateValid(
        string name = "Fibonacci",
        string type = "points",
        Guid? projectId = null)
    {
        return Estimate.Create(name, type,
            projectId ?? Guid.Parse("00000000-0000-0000-0000-000000000001"));
    }

    public static EstimatePoint CreatePoint(
        Guid estimateId,
        int key = 0,
        string value = "1",
        double sortOrder = 65535.0)
    {
        return EstimatePoint.Create(estimateId, key, value, sortOrder);
    }
}

/// <summary>Factory helpers for creating <see cref="Issue"/> instances in tests.</summary>
internal static class TestIssueFactory
{
    private static readonly Guid DefaultProjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public static Issue CreateValid(
        string name = "Test Issue",
        Guid? projectId = null,
        Guid? stateId = null,
        Guid? parentId = null,
        Guid? estimatePointId = null,
        string priority = "none")
    {
        return Issue.Create(name,
            projectId ?? DefaultProjectId,
            stateId, parentId, estimatePointId,
            priority: priority);
    }

    public static Issue CreateDraft(
        string name = "Draft Issue",
        Guid? projectId = null)
    {
        return Issue.Create(name,
            projectId ?? DefaultProjectId,
            isDraft: true);
    }
}

/// <summary>Factory helpers for creating <see cref="IssueComment"/> instances in tests.</summary>
internal static class TestCommentFactory
{
    public static IssueComment CreateValid(
        Guid issueId,
        string commentHtml = "<p>Test comment</p>",
        string actorId = "user-001",
        Guid? parentId = null)
    {
        return IssueComment.Create(issueId, commentHtml, actorId, parentId);
    }
}

/// <summary>Factory helpers for creating <see cref="IssueLink"/> instances in tests.</summary>
internal static class TestLinkFactory
{
    public static IssueLink CreateRelation(
        Guid issueId,
        Guid relatedIssueId,
        LinkType linkType = LinkType.RelatesTo)
    {
        return IssueLink.Create(issueId, linkType, relatedIssueId: relatedIssueId);
    }

    public static IssueLink CreateExternalLink(
        Guid issueId,
        string url = "https://example.com",
        string? title = "Example",
        LinkType linkType = LinkType.RelatesTo)
    {
        return IssueLink.Create(issueId, linkType, url: url, title: title);
    }
}

/// <summary>Factory helpers for creating <see cref="IntakeIssue"/> instances in tests.</summary>
internal static class TestIntakeFactory
{
    public static IntakeIssue CreateValid(
        Guid issueId,
        Guid? projectId = null,
        string source = "IN_APP")
    {
        return IntakeIssue.Create(issueId,
            projectId ?? Guid.Parse("00000000-0000-0000-0000-000000000001"),
            source);
    }
}

/// <summary>Factory helpers for creating <see cref="Cycle"/> instances in tests.</summary>
internal static class TestCycleFactory
{
    public static Cycle CreateValid(
        string name = "Sprint 1",
        Guid? projectId = null,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        string? description = null)
    {
        return Cycle.Create(name,
            projectId ?? Guid.Parse("00000000-0000-0000-0000-000000000001"),
            startDate, endDate, description);
    }

    public static Cycle CreateBacklog(
        string name = "Backlog",
        Guid? projectId = null)
    {
        return Cycle.Create(name,
            projectId ?? Guid.Parse("00000000-0000-0000-0000-000000000001"));
    }
}

/// <summary>Factory helpers for creating <see cref="IssueActivity"/> instances in tests.</summary>
internal static class TestActivityFactory
{
    public static IssueActivity CreateValid(
        Guid issueId,
        string verb = "updated",
        string actorId = "user-001",
        long epoch = 1_000_000_000,
        string? field = null,
        string? oldValue = null,
        string? newValue = null)
    {
        return IssueActivity.Create(issueId, verb, actorId, epoch,
            field: field, oldValue: oldValue, newValue: newValue);
    }
}
