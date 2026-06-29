namespace YH.Tests.WorkItems.Integration;

/// <summary>
/// Integration tests for <see cref="Label"/> CRUD via InMemory DbContext (plan 04-05 Task 2).
/// </summary>
[Collection("WorkItemsTest")]
public sealed class LabelCrudTests : IDisposable
{
    private readonly WorkItemsDbContext _db;
    private static readonly Guid ProjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public LabelCrudTests()
    {
        _db = WorkItemsTestFixture.CreateInMemoryContext();
    }

    [Fact]
    public async Task CreateLabel_PersistsToDatabase()
    {
        var label = TestLabelFactory.CreateValid("Bug", "#F59E0B", ProjectId);
        _db.Labels.Add(label);
        await _db.SaveChangesAsync();

        var saved = await _db.Labels.FindAsync(label.Id);
        saved.ShouldNotBeNull();
        saved.Name.ShouldBe("Bug");
    }

    [Fact]
    public async Task UpdateLabel_PersistsChanges()
    {
        var label = TestLabelFactory.CreateValid("Bug", "#F59E0B", ProjectId);
        _db.Labels.Add(label);
        await _db.SaveChangesAsync();

        label.Update(name: "Feature", color: "#00FF00");
        await _db.SaveChangesAsync();

        var reloaded = await _db.Labels.FindAsync(label.Id);
        reloaded.ShouldNotBeNull();
        reloaded.Name.ShouldBe("Feature");
        reloaded.Color.ShouldBe("#00FF00");
    }

    [Fact]
    public async Task CreateLabelWithParent_SetsHierarchy()
    {
        var parent = TestLabelFactory.CreateValid("Parent", "#000", ProjectId);
        _db.Labels.Add(parent);
        await _db.SaveChangesAsync();

        var child = TestLabelFactory.CreateValid("Child", "#FFF", ProjectId, parent.Id);
        _db.Labels.Add(child);
        await _db.SaveChangesAsync();

        var savedChild = await _db.Labels.FindAsync(child.Id);
        savedChild.ShouldNotBeNull();
        savedChild.ParentId.ShouldBe(parent.Id);
    }

    [Fact]
    public async Task SoftDeleteLabel_MarksAsDeleted()
    {
        var label = TestLabelFactory.CreateValid("Bug", "#F59E0B", ProjectId);
        _db.Labels.Add(label);
        await _db.SaveChangesAsync();

        label.SoftDelete(DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync();

        var deleted = await _db.Labels.FindAsync(label.Id);
        deleted.ShouldNotBeNull();
        deleted.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public async Task ListLabels_ReturnsAllForProject()
    {
        _db.Labels.Add(TestLabelFactory.CreateValid("Bug", "#F59E0B", ProjectId));
        _db.Labels.Add(TestLabelFactory.CreateValid("Feature", "#00FF00", ProjectId));
        _db.Labels.Add(TestLabelFactory.CreateValid("Enhancement", "#0000FF", ProjectId));
        await _db.SaveChangesAsync();

        var labels = await _db.Labels.Where(l => l.ProjectId == ProjectId).ToListAsync();
        labels.Count.ShouldBe(3);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
