namespace YH.Tests.WorkItems.Domain;

/// <summary>
/// Domain unit tests for <see cref="IssueComment"/> entity (plan 04-05 Task 1).
/// </summary>
public sealed class IssueCommentTests
{
    private static readonly Guid IssueId = Guid.NewGuid();
    private const string ActorId = "user-001";

    [Fact]
    public void Create_WithValidArgs_SetsProperties()
    {
        var comment = IssueComment.Create(IssueId, "<p>Hello</p>", ActorId);

        comment.IssueId.ShouldBe(IssueId);
        comment.CommentHtml.ShouldBe("<p>Hello</p>");
        comment.ActorId.ShouldBe(ActorId);
        comment.ParentId.ShouldBeNull();
        comment.EditedAt.ShouldBeNull();
        comment.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void Create_WithEmptyIssueId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            IssueComment.Create(Guid.Empty, "<p>Hi</p>", ActorId));
    }

    [Fact]
    public void Create_WithEmptyHtml_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            IssueComment.Create(IssueId, "", ActorId));
    }

    [Fact]
    public void Create_WithEmptyActorId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            IssueComment.Create(IssueId, "<p>Hi</p>", ""));
    }

    [Fact]
    public void Create_WithParentId_SetsNestedReply()
    {
        var parentId = Guid.NewGuid();
        var comment = IssueComment.Create(IssueId, "<p>Reply</p>", ActorId, parentId);

        comment.ParentId.ShouldBe(parentId);
    }

    [Fact]
    public void Create_SanitizesHtml()
    {
        var comment = IssueComment.Create(IssueId, "<p>Safe</p><script>alert(1)</script>", ActorId);

        comment.CommentHtml.ShouldNotContain("script");
        comment.CommentStripped.ShouldBe("Safe");
    }

    [Fact]
    public void Update_ChangesBodyAndSetsEditedAt()
    {
        var comment = IssueComment.Create(IssueId, "<p>Original</p>", ActorId);

        comment.Update("<p>Updated</p>");

        comment.CommentHtml.ShouldBe("<p>Updated</p>");
        comment.EditedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Update_WithEmptyHtml_Throws()
    {
        var comment = IssueComment.Create(IssueId, "<p>Original</p>", ActorId);
        Should.Throw<ArgumentException>(() => comment.Update(""));
    }

    [Fact]
    public void SoftDelete_SetsDeletedFlag()
    {
        var comment = IssueComment.Create(IssueId, "<p>Hi</p>", ActorId);
        var now = DateTimeOffset.UtcNow;

        comment.SoftDelete(now);

        comment.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public void SoftDelete_IsIdempotent()
    {
        var comment = IssueComment.Create(IssueId, "<p>Hi</p>", ActorId);
        var now = DateTimeOffset.UtcNow;
        comment.SoftDelete(now);

        comment.SoftDelete(DateTimeOffset.UtcNow.AddDays(1));

        comment.DeletedOnUtc.ShouldBe(now);
    }
}
