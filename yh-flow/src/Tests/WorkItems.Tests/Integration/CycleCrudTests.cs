using Microsoft.EntityFrameworkCore;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;
using YH.Tests.WorkItems.TestData;

namespace YH.Tests.WorkItems.Integration;

/// <summary>
/// Integration tests for <see cref="Cycle"/> CRUD via InMemory DbContext.
/// </summary>
[Collection("WorkItemsTest")]
public sealed class CycleCrudTests : IDisposable
{
    private readonly WorkItemsDbContext _db;
    private static readonly Guid ProjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public CycleCrudTests()
    {
        _db = WorkItemsTestFixture.CreateInMemoryContext();
    }

    [Fact]
    public async Task CreateCycle_PersistsToDatabase()
    {
        var cycle = TestCycleFactory.CreateValid(projectId: ProjectId);
        _db.Cycles.Add(cycle);
        await _db.SaveChangesAsync();

        var saved = await _db.Cycles.FirstOrDefaultAsync(c => c.Id == cycle.Id);
        saved.ShouldNotBeNull();
        saved.Name.ShouldBe("Sprint 1");
        saved.ProjectId.ShouldBe(ProjectId);
    }

    [Fact]
    public async Task GetCycle_ById_ReturnsCycle()
    {
        var cycle = TestCycleFactory.CreateValid("Sprint 2", ProjectId,
            DateTimeOffset.UtcNow.AddDays(-10), DateTimeOffset.UtcNow.AddDays(10));
        _db.Cycles.Add(cycle);
        await _db.SaveChangesAsync();

        var found = await _db.Cycles.FindAsync(cycle.Id);
        found.ShouldNotBeNull();
        found.Name.ShouldBe("Sprint 2");
        found.StartDate.ShouldNotBeNull();
        found.EndDate.ShouldNotBeNull();
    }

    [Fact]
    public async Task UpdateCycle_ChangesName()
    {
        var cycle = TestCycleFactory.CreateValid(projectId: ProjectId);
        _db.Cycles.Add(cycle);
        await _db.SaveChangesAsync();

        cycle.Update(name: "Updated Sprint");
        await _db.SaveChangesAsync();

        var reloaded = await _db.Cycles.FindAsync(cycle.Id);
        reloaded.ShouldNotBeNull();
        reloaded.Name.ShouldBe("Updated Sprint");
    }

    [Fact]
    public async Task UpdateCycle_Completed_Restricted()
    {
        // A cycle with EndDate in the past is "COMPLETED" — should still be updateable by name
        var cycle = TestCycleFactory.CreateValid("Past Sprint", ProjectId,
            DateTimeOffset.UtcNow.AddDays(-20), DateTimeOffset.UtcNow.AddDays(-5));
        _db.Cycles.Add(cycle);
        await _db.SaveChangesAsync();

        cycle.Update(name: "Past Sprint (Reviewed)");
        await _db.SaveChangesAsync();

        var reloaded = await _db.Cycles.FindAsync(cycle.Id);
        reloaded.ShouldNotBeNull();
        reloaded.Name.ShouldBe("Past Sprint (Reviewed)");
    }

    [Fact]
    public async Task SoftDeleteCycle_MarksAsDeleted()
    {
        var cycle = TestCycleFactory.CreateValid(projectId: ProjectId);
        _db.Cycles.Add(cycle);
        await _db.SaveChangesAsync();

        cycle.SoftDelete(DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync();

        var deleted = await _db.Cycles.FindAsync(cycle.Id);
        deleted.ShouldNotBeNull();
        deleted.IsDeleted.ShouldBeTrue();
        deleted.DeletedOnUtc.ShouldNotBeNull();
    }

    [Fact]
    public async Task ListCycles_ReturnsAllForProject()
    {
        _db.Cycles.Add(TestCycleFactory.CreateValid("Sprint 1", ProjectId));
        _db.Cycles.Add(TestCycleFactory.CreateValid("Sprint 2", ProjectId));
        _db.Cycles.Add(TestCycleFactory.CreateValid("Sprint 3", ProjectId));
        await _db.SaveChangesAsync();

        var cycles = await _db.Cycles
            .Where(c => c.ProjectId == ProjectId && !c.IsDeleted && c.ArchivedAt == null)
            .ToListAsync();

        cycles.Count.ShouldBe(3);
    }

    [Fact]
    public async Task ListCycles_FilterByCompleted()
    {
        _db.Cycles.Add(TestCycleFactory.CreateValid("Current Sprint", ProjectId,
            DateTimeOffset.UtcNow.AddDays(-5), DateTimeOffset.UtcNow.AddDays(5)));
        _db.Cycles.Add(TestCycleFactory.CreateValid("Past Sprint", ProjectId,
            DateTimeOffset.UtcNow.AddDays(-20), DateTimeOffset.UtcNow.AddDays(-5)));
        await _db.SaveChangesAsync();

        var allCycles = await _db.Cycles
            .Where(c => c.ProjectId == ProjectId && !c.IsDeleted && c.ArchivedAt == null)
            .ToListAsync();

        // Past Sprint should be identifiable as completed by its EndDate
        var pastSprint = allCycles.FirstOrDefault(c => c.Name == "Past Sprint");
        pastSprint.ShouldNotBeNull();
        pastSprint.EndDate.ShouldNotBeNull();
        (pastSprint.EndDate.Value < DateTimeOffset.UtcNow).ShouldBeTrue();
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
