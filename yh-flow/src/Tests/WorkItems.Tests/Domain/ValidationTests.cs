namespace YH.Tests.WorkItems.Domain;

/// <summary>
/// Cross-cutting validation tests for WorkItems domain entities (plan 04-05 Task 1).
/// Covers IssuePriorityValidation, StateGroupValidation, IssueParentDepth, IssueCompletedAtSync.
/// </summary>
public sealed class ValidationTests
{
    private static readonly Guid ProjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    // ─────────────────────────────────────────────────────────────
    // Issue Priority Validation
    // ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("urgent")]
    [InlineData("high")]
    [InlineData("medium")]
    [InlineData("low")]
    [InlineData("none")]
    public void Issue_Create_WithValidPriority_Succeeds(string priority)
    {
        var issue = Issue.Create("Test", ProjectId, priority: priority);
        issue.Priority.ShouldBe(priority);
    }

    [Theory]
    [InlineData("")]
    [InlineData("critical")]
    [InlineData("CRITICAL")]
    [InlineData("p0")]
    public void Issue_Create_WithInvalidPriority_Throws(string priority)
    {
        Should.Throw<ArgumentException>(() =>
            Issue.Create("Test", ProjectId, priority: priority));
    }

    [Fact]
    public void Issue_UpdateDetails_WithInvalidPriority_Throws()
    {
        var issue = Issue.Create("Test", ProjectId);
        Should.Throw<ArgumentException>(() =>
            issue.UpdateDetails(priority: "invalid"));
    }

    // ─────────────────────────────────────────────────────────────
    // State Group Validation
    // ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(StateGroup.Backlog)]
    [InlineData(StateGroup.Unstarted)]
    [InlineData(StateGroup.Started)]
    [InlineData(StateGroup.Completed)]
    [InlineData(StateGroup.Cancelled)]
    public void State_Create_WithValidGroup_Succeeds(StateGroup group)
    {
        var state = State.Create($"State-{group}", null, group, ProjectId, true);
        state.Group.ShouldBe(group);
    }

    [Fact]
    public void State_Create_WithInvalidGroup_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            State.Create("Invalid", null, (StateGroup)99, ProjectId, false));
    }

    // ─────────────────────────────────────────────────────────────
    // Issue Parent Depth (1-level hierarchy per CONTEXT)
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public void Issue_Create_WithoutParent_Succeeds()
    {
        var issue = Issue.Create("Root", ProjectId);
        issue.ParentId.ShouldBeNull();
    }

    [Fact]
    public void Issue_Create_WithParent_SetsParentId()
    {
        var parentId = Guid.NewGuid();
        var issue = Issue.Create("Child", ProjectId, parentId: parentId);
        issue.ParentId.ShouldBe(parentId);
    }

    // ─────────────────────────────────────────────────────────────
    // Issue CompletedAt Sync (Plane behavior)
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public void CompletedAt_IsNull_WhenIssueCreated()
    {
        var issue = Issue.Create("New", ProjectId);
        issue.CompletedAt.ShouldBeNull();
    }

    [Fact]
    public void CompletedAt_IsSet_WhenStateChangesToCompleted()
    {
        var issue = Issue.Create("Task", ProjectId);
        var completedStateId = Guid.NewGuid();

        issue.UpdateState(completedStateId, isCompletedGroup: true, isCancelledGroup: false);

        issue.CompletedAt.ShouldNotBeNull();
    }

    [Fact]
    public void CompletedAt_IsSet_WhenStateChangesToCancelled()
    {
        var issue = Issue.Create("Task", ProjectId);

        issue.UpdateState(Guid.NewGuid(), isCompletedGroup: false, isCancelledGroup: true);

        issue.CompletedAt.ShouldNotBeNull();
    }

    [Fact]
    public void CompletedAt_IsCleared_WhenMovingAwayFromCompleted()
    {
        var issue = Issue.Create("Task", ProjectId);
        issue.UpdateState(Guid.NewGuid(), isCompletedGroup: true, isCancelledGroup: false);
        issue.CompletedAt.ShouldNotBeNull();

        issue.UpdateState(Guid.NewGuid(), isCompletedGroup: false, isCancelledGroup: false);

        issue.CompletedAt.ShouldBeNull();
    }

    [Fact]
    public void CompletedAt_RemainsSet_WhenMovingBetweenCompletedStates()
    {
        var issue = Issue.Create("Task", ProjectId);
        issue.UpdateState(Guid.NewGuid(), isCompletedGroup: true, isCancelledGroup: false);
        var firstCompletedAt = issue.CompletedAt;

        issue.UpdateState(Guid.NewGuid(), isCompletedGroup: true, isCancelledGroup: false);

        issue.CompletedAt.ShouldBe(firstCompletedAt); // already set, no change
    }

    // ─────────────────────────────────────────────────────────────
    // Description HTML sanitization
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public void Issue_SanitizeHtml_StripsJavaScriptUrls()
    {
        var html = "<a href=\"javascript:alert('xss')\">Click</a>";
        var issue = Issue.Create("Safe", ProjectId, descriptionHtml: html);

        issue.DescriptionHtml.ShouldNotContain("javascript:");
        issue.DescriptionHtml.ShouldNotContain("alert");
    }

    [Fact]
    public void Issue_DefaultDescriptionHtml_IsEmptyParagraph()
    {
        var issue = Issue.Create("Test", ProjectId);
        issue.DescriptionHtml.ShouldBe("<p></p>");
    }

    [Fact]
    public void Issue_UpdateDetails_WithDescriptionHtml_StripsScriptTags()
    {
        var issue = Issue.Create("Test", ProjectId);
        issue.UpdateDetails(descriptionHtml: "<p>Content</p><script>bad</script>");

        issue.DescriptionHtml.ShouldNotContain("script");
        issue.DescriptionStripped.ShouldBe("Content");
    }
}
