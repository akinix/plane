namespace YH.Tests.WorkItems.Integration;

/// <summary>
/// Integration tests for <see cref="State"/> CRUD via InMemory DbContext (plan 04-05 Task 2).
/// </summary>
[Collection("WorkItemsTest")]
public sealed class StateCrudTests : IDisposable
{
    private readonly WorkItemsDbContext _db;
    private static readonly Guid ProjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public StateCrudTests()
    {
        _db = WorkItemsTestFixture.CreateInMemoryContext();
    }

    [Fact]
    public async Task CreateState_PersistsToDatabase()
    {
        var state = TestStateFactory.CreateValid(projectId: ProjectId);
        _db.States.Add(state);
        await _db.SaveChangesAsync();

        var saved = await _db.States.FirstOrDefaultAsync(s => s.Id == state.Id);
        saved.ShouldNotBeNull();
        saved.Name.ShouldBe("Todo");
        saved.Group.ShouldBe(StateGroup.Unstarted);
    }

    [Fact]
    public async Task ReadState_ById_ReturnsState()
    {
        var state = TestStateFactory.CreateValid("In Progress", "#F59E0B", StateGroup.Started, ProjectId);
        _db.States.Add(state);
        await _db.SaveChangesAsync();

        var found = await _db.States.FindAsync(state.Id);
        found.ShouldNotBeNull();
        found.Name.ShouldBe("In Progress");
        found.Color.ShouldBe("#F59E0B");
    }

    [Fact]
    public async Task UpdateState_PersistsChanges()
    {
        var state = TestStateFactory.CreateValid(projectId: ProjectId);
        _db.States.Add(state);
        await _db.SaveChangesAsync();

        state.Update(name: "Updated State", color: "#FF0000");
        await _db.SaveChangesAsync();

        var reloaded = await _db.States.FindAsync(state.Id);
        reloaded.ShouldNotBeNull();
        reloaded.Name.ShouldBe("Updated State");
        reloaded.Color.ShouldBe("#FF0000");
    }

    [Fact]
    public async Task SoftDeleteState_MarksAsDeleted()
    {
        var state = TestStateFactory.CreateValid(projectId: ProjectId);
        _db.States.Add(state);
        await _db.SaveChangesAsync();

        state.SoftDelete(DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync();

        var deleted = await _db.States.FindAsync(state.Id);
        deleted.ShouldNotBeNull();
        deleted.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public async Task ListStates_ReturnsAllForProject()
    {
        _db.States.Add(TestStateFactory.CreateValid("Todo", group: StateGroup.Unstarted, projectId: ProjectId));
        _db.States.Add(TestStateFactory.CreateValid("In Progress", group: StateGroup.Started, projectId: ProjectId));
        _db.States.Add(TestStateFactory.CreateValid("Done", group: StateGroup.Completed, projectId: ProjectId));
        await _db.SaveChangesAsync();

        var states = await _db.States.Where(s => s.ProjectId == ProjectId).ToListAsync();

        states.Count.ShouldBe(3);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
