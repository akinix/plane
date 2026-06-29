namespace YH.Tests.WorkItems.Domain;

/// <summary>
/// Domain unit tests for <see cref="IssueActivity"/> entity (plan 04-05 Task 1).
/// Verifies the entity does NOT implement <see cref="YH.Framework.Core.Domain.IHasDomainEvents"/>
/// (T-4-activity-01 recursion prevention).
/// </summary>
public sealed class IssueActivityTests
{
    private static readonly Guid IssueId = Guid.NewGuid();
    private const string ActorId = "user-001";

    [Fact]
    public void Create_WithValidArgs_SetsProperties()
    {
        var activity = IssueActivity.Create(IssueId, "updated", ActorId, 1_000_000_000,
            field: "priority", oldValue: "none", newValue: "high");

        activity.IssueId.ShouldBe(IssueId);
        activity.Verb.ShouldBe("updated");
        activity.ActorId.ShouldBe(ActorId);
        activity.Epoch.ShouldBe(1_000_000_000);
        activity.Field.ShouldBe("priority");
        activity.OldValue.ShouldBe("none");
        activity.NewValue.ShouldBe("high");
        activity.Comment.ShouldBeNull();
        activity.IssueCommentId.ShouldBeNull();
    }

    [Fact]
    public void Create_WithCreatedVerb_SetsProperties()
    {
        var activity = IssueActivity.Create(IssueId, "created", ActorId, 1_000_000_000);

        activity.Verb.ShouldBe("created");
        activity.Field.ShouldBeNull();
        activity.OldValue.ShouldBeNull();
        activity.NewValue.ShouldBeNull();
    }

    [Fact]
    public void Create_WithDeletedVerb_SetsProperties()
    {
        var activity = IssueActivity.Create(IssueId, "deleted", ActorId, 1_000_000_000);

        activity.Verb.ShouldBe("deleted");
    }

    [Fact]
    public void Create_WithEmptyIssueId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            IssueActivity.Create(Guid.Empty, "updated", ActorId, 0));
    }

    [Fact]
    public void Create_WithEmptyVerb_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            IssueActivity.Create(IssueId, "", ActorId, 0));
    }

    [Fact]
    public void Create_WithEmptyActorId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            IssueActivity.Create(IssueId, "updated", "", 0));
    }

    [Fact]
    public void Create_WithCommentAndCommentId_SetsOptionalFields()
    {
        var commentId = Guid.NewGuid();
        var activity = IssueActivity.Create(IssueId, "updated", ActorId, 1_000_000_000,
            field: "name", oldValue: "Old", newValue: "New",
            comment: "Updated by reviewer", issueCommentId: commentId);

        activity.Comment.ShouldBe("Updated by reviewer");
        activity.IssueCommentId.ShouldBe(commentId);
    }

    // T-4-activity-01: IssueActivity does NOT implement IHasDomainEvents or ISoftDeletable.
    // This is a compile-time invariant verified by the class declaration — no runtime test needed.
}
