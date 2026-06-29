namespace YH.Tests.WorkItems.Domain;

/// <summary>
/// Domain unit tests for <see cref="Issue"/> entity (plan 04-05 Task 1).
/// Covers creation, update details, CompletedAt sync, assignee/label replacement, draft acceptance, and soft delete.
/// </summary>
public sealed class IssueTests
{
    private static readonly Guid ProjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid StateId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidArgs_SetsProperties()
    {
        var issue = Issue.Create("My Issue", ProjectId, stateId: StateId);

        issue.Name.ShouldBe("My Issue");
        issue.ProjectId.ShouldBe(ProjectId);
        issue.StateId.ShouldBe(StateId);
        issue.Priority.ShouldBe("none");
        issue.SortOrder.ShouldBe(65535.0);
        issue.IsDraft.ShouldBeFalse();
        issue.IsDeleted.ShouldBeFalse();
        issue.CompletedAt.ShouldBeNull();
        issue.DescriptionHtml.ShouldBe("<p></p>");
        issue.DescriptionStripped.ShouldBeNull();
    }

    [Fact]
    public void Create_WithEmptyName_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            Issue.Create("", ProjectId));
    }

    [Fact]
    public void Create_WithPriority_SetsValidValue()
    {
        var issue = Issue.Create("Urgent Issue", ProjectId, priority: "urgent");
        issue.Priority.ShouldBe("urgent");
    }

    [Fact]
    public void Create_WithInvalidPriority_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            Issue.Create("Bad", ProjectId, priority: "critical"));
    }

    [Fact]
    public void Create_AsDraft_SetsIsDraftTrue()
    {
        var issue = Issue.Create("Draft", ProjectId, isDraft: true);
        issue.IsDraft.ShouldBeTrue();
    }

    [Fact]
    public void UpdateDetails_WithName_ChangesName()
    {
        var issue = TestIssueFactory.CreateValid(name: "Original");

        issue.UpdateDetails(name: "Updated");

        issue.Name.ShouldBe("Updated");
    }

    [Fact]
    public void UpdateDetails_WithPriority_ChangesPriority()
    {
        var issue = TestIssueFactory.CreateValid(priority: "none");

        issue.UpdateDetails(priority: "high");

        issue.Priority.ShouldBe("high");
    }

    [Fact]
    public void UpdateDetails_WithInvalidPriority_Throws()
    {
        var issue = TestIssueFactory.CreateValid();

        Should.Throw<ArgumentException>(() =>
            issue.UpdateDetails(priority: "invalid"));
    }

    [Fact]
    public void UpdateDetails_WithNullValues_DoesNotChangeFields()
    {
        var issue = TestIssueFactory.CreateValid(name: "Original", priority: "low");

        issue.UpdateDetails(name: null, priority: null);

        issue.Name.ShouldBe("Original");
        issue.Priority.ShouldBe("low");
    }

    [Fact]
    public void UpdateDetails_EmitsDomainEvent_WhenChangesExist()
    {
        var issue = TestIssueFactory.CreateValid(name: "Original");

        issue.UpdateDetails(name: "Updated", priority: "high");

        issue.DomainEvents.ShouldNotBeEmpty();
        var evt = issue.DomainEvents.OfType<IssueUpdatedDomainEvent>().Single();
        evt.IssueId.ShouldBe(issue.Id);
        evt.Changes.ShouldContain(c => c.Field == "name");
        evt.Changes.ShouldContain(c => c.Field == "priority");
    }

    [Fact]
    public void UpdateDetails_NoDomainEvent_WhenNoChanges()
    {
        var issue = TestIssueFactory.CreateValid();

        issue.UpdateDetails(name: null);

        issue.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void UpdateState_ToCompletedGroup_SetsCompletedAt()
    {
        var issue = TestIssueFactory.CreateValid();

        issue.UpdateState(Guid.NewGuid(), isCompletedGroup: true, isCancelledGroup: false);

        issue.CompletedAt.ShouldNotBeNull();
    }

    [Fact]
    public void UpdateState_ToCancelledGroup_SetsCompletedAt()
    {
        var issue = TestIssueFactory.CreateValid();

        issue.UpdateState(Guid.NewGuid(), isCompletedGroup: false, isCancelledGroup: true);

        issue.CompletedAt.ShouldNotBeNull();
    }

    [Fact]
    public void UpdateState_LeavingCompletedGroup_ClearsCompletedAt()
    {
        var issue = TestIssueFactory.CreateValid();
        issue.UpdateState(Guid.NewGuid(), isCompletedGroup: true, isCancelledGroup: false);
        issue.CompletedAt.ShouldNotBeNull();

        issue.UpdateState(Guid.NewGuid(), isCompletedGroup: false, isCancelledGroup: false);

        issue.CompletedAt.ShouldBeNull();
    }

    [Fact]
    public void UpdateState_EmitsDomainEvent()
    {
        var issue = TestIssueFactory.CreateValid();

        issue.UpdateState(Guid.NewGuid(), isCompletedGroup: true, isCancelledGroup: false);

        issue.DomainEvents.OfType<IssueUpdatedDomainEvent>().ShouldNotBeEmpty();
    }

    [Fact]
    public void UpdateAssigneeList_ReplacesAssignees()
    {
        var issue = TestIssueFactory.CreateValid();
        var userIds = new[] { "user-1", "user-2" };

        issue.UpdateAssigneeList(userIds);

        issue.Assignees.Count.ShouldBe(2);
        issue.Assignees.ShouldAllBe(a => a.IssueId == issue.Id);
    }

    [Fact]
    public void UpdateAssigneeList_WithNull_Throws()
    {
        var issue = TestIssueFactory.CreateValid();
        Should.Throw<ArgumentNullException>(() => issue.UpdateAssigneeList(null!));
    }

    [Fact]
    public void UpdateAssigneeList_ReplacesExistingAssignees()
    {
        var issue = TestIssueFactory.CreateValid();
        issue.UpdateAssigneeList(new[] { "user-1" });
        issue.Assignees.Count.ShouldBe(1);

        issue.UpdateAssigneeList(new[] { "user-2" });
        issue.Assignees.Count.ShouldBe(1);
        issue.Assignees.Single().AssigneeId.ShouldBe("user-2");
    }

    [Fact]
    public void UpdateLabelList_ReplacesLabels()
    {
        var issue = TestIssueFactory.CreateValid();
        var labelIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        issue.UpdateLabelList(labelIds);

        issue.Labels.Count.ShouldBe(2);
        issue.Labels.ShouldAllBe(l => l.IssueId == issue.Id);
    }

    [Fact]
    public void UpdateLabelList_WithNull_Throws()
    {
        var issue = TestIssueFactory.CreateValid();
        Should.Throw<ArgumentNullException>(() => issue.UpdateLabelList(null!));
    }

    [Fact]
    public void MarkAsAccepted_ClearsDraftAndSetsState()
    {
        var issue = Issue.Create("Draft", ProjectId, isDraft: true);
        var defaultStateId = Guid.NewGuid();

        issue.MarkAsAccepted(defaultStateId);

        issue.IsDraft.ShouldBeFalse();
        issue.StateId.ShouldBe(defaultStateId);
    }

    [Fact]
    public void SoftDelete_SetsDeletedFlag()
    {
        var issue = TestIssueFactory.CreateValid();
        var now = DateTimeOffset.UtcNow;

        issue.SoftDelete(now);

        issue.IsDeleted.ShouldBeTrue();
        issue.DeletedOnUtc.ShouldBe(now);
    }

    [Fact]
    public void SoftDelete_IsIdempotent()
    {
        var issue = TestIssueFactory.CreateValid();
        var now = DateTimeOffset.UtcNow;
        issue.SoftDelete(now);

        issue.SoftDelete(DateTimeOffset.UtcNow.AddDays(1));

        issue.DeletedOnUtc.ShouldBe(now);
    }

    [Fact]
    public void SanitizeHtml_RemovesScriptTags()
    {
        var html = "<p>Hello</p><script>alert('xss')</script>";
        var issue = Issue.Create("Safe", ProjectId, descriptionHtml: html);

        issue.DescriptionHtml.ShouldNotContain("script");
        issue.DescriptionHtml.ShouldContain("<p>Hello</p>");
    }

    [Fact]
    public void SanitizeHtml_RemovesEventHandlers()
    {
        var html = "<p onclick=\"alert(1)\">Click</p>";
        var issue = Issue.Create("Safe", ProjectId, descriptionHtml: html);

        issue.DescriptionHtml.ShouldNotContain("onclick");
    }

    [Fact]
    public void Create_WithDescriptionHtml_SetsStrippedText()
    {
        var issue = Issue.Create("Desc", ProjectId, descriptionHtml: "<p>Hello <strong>World</strong></p>");

        issue.DescriptionStripped.ShouldBe("Hello World");
    }
}
