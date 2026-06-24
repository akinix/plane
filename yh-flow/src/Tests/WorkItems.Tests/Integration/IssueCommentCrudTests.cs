namespace YH.Tests.WorkItems.Integration;

/// <summary>
/// Integration tests for <see cref="IssueComment"/> CRUD via InMemory DbContext (plan 04-05 Task 2).
/// </summary>
[Collection("WorkItemsTest")]
public sealed class IssueCommentCrudTests : IDisposable
{
    private readonly WorkItemsDbContext _db;
    private static readonly Guid IssueId = Guid.NewGuid();

    public IssueCommentCrudTests()
    {
        _db = WorkItemsTestFixture.CreateInMemoryContext();
    }

    [Fact]
    public async Task CreateComment_PersistsToDatabase()
    {
        var comment = IssueComment.Create(IssueId, "<p>First comment</p>", "user-001");
        _db.IssueComments.Add(comment);
        await _db.SaveChangesAsync();

        var saved = await _db.IssueComments.FindAsync(comment.Id);
        saved.ShouldNotBeNull();
        saved.CommentHtml.ShouldBe("<p>First comment</p>");
        saved.ActorId.ShouldBe("user-001");
    }

    [Fact]
    public async Task UpdateComment_ChangesBodyAndSetsEditedAt()
    {
        var comment = IssueComment.Create(IssueId, "<p>Original</p>", "user-001");
        _db.IssueComments.Add(comment);
        await _db.SaveChangesAsync();

        comment.Update("<p>Edited</p>");
        await _db.SaveChangesAsync();

        var reloaded = await _db.IssueComments.FindAsync(comment.Id);
        reloaded.ShouldNotBeNull();
        reloaded.CommentHtml.ShouldBe("<p>Edited</p>");
        reloaded.EditedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task SoftDeleteComment_MarksAsDeleted()
    {
        var comment = IssueComment.Create(IssueId, "<p>To delete</p>", "user-001");
        _db.IssueComments.Add(comment);
        await _db.SaveChangesAsync();

        comment.SoftDelete(DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync();

        var deleted = await _db.IssueComments.FindAsync(comment.Id);
        deleted.ShouldNotBeNull();
        deleted.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public async Task CreateNestedReply_SetsParentId()
    {
        var parent = IssueComment.Create(IssueId, "<p>Parent</p>", "user-001");
        _db.IssueComments.Add(parent);
        await _db.SaveChangesAsync();

        var reply = IssueComment.Create(IssueId, "<p>Reply</p>", "user-002", parent.Id);
        _db.IssueComments.Add(reply);
        await _db.SaveChangesAsync();

        var savedReply = await _db.IssueComments.FindAsync(reply.Id);
        savedReply.ShouldNotBeNull();
        savedReply.ParentId.ShouldBe(parent.Id);
    }

    [Fact]
    public async Task ListCommentsByIssue_ReturnsAll()
    {
        _db.IssueComments.Add(IssueComment.Create(IssueId, "<p>C1</p>", "user-001"));
        _db.IssueComments.Add(IssueComment.Create(IssueId, "<p>C2</p>", "user-002"));
        await _db.SaveChangesAsync();

        var comments = await _db.IssueComments
            .Where(c => c.IssueId == IssueId)
            .OrderBy(c => c.CreatedOnUtc)
            .ToListAsync();
        comments.Count.ShouldBe(2);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
