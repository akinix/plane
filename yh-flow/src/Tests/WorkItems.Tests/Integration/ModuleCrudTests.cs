using Microsoft.EntityFrameworkCore;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;
using YH.Tests.WorkItems.TestData;

namespace YH.Tests.WorkItems.Integration;

/// <summary>
/// Integration tests for <see cref="Module"/> CRUD and archive operations via InMemory DbContext.
/// </summary>
[Collection("WorkItemsTest")]
public sealed class ModuleCrudTests : IDisposable
{
    private readonly WorkItemsDbContext _db;
    private static readonly Guid ProjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public ModuleCrudTests()
    {
        _db = WorkItemsTestFixture.CreateInMemoryContext();
    }

    [Fact]
    public async Task CreateModule_PersistsToDatabase()
    {
        var module = TestModuleFactory.CreateValid("Feature Module", ProjectId);
        _db.Modules.Add(module);
        await _db.SaveChangesAsync();

        var saved = await _db.Modules.FirstOrDefaultAsync(m => m.Id == module.Id);
        saved.ShouldNotBeNull();
        saved.Name.ShouldBe("Feature Module");
        saved.ProjectId.ShouldBe(ProjectId);
    }

    [Fact]
    public async Task GetModule_ById_ReturnsModule()
    {
        var module = TestModuleFactory.CreateValid("Release 1", ProjectId);
        _db.Modules.Add(module);
        await _db.SaveChangesAsync();

        var found = await _db.Modules.FindAsync(module.Id);
        found.ShouldNotBeNull();
        found.Name.ShouldBe("Release 1");
        found.Status.ShouldBe("planned");
    }

    [Fact]
    public async Task UpdateModule_ChangesName()
    {
        var module = TestModuleFactory.CreateValid("Original Name", ProjectId);
        _db.Modules.Add(module);
        await _db.SaveChangesAsync();

        module.Update(name: "Updated Module");
        await _db.SaveChangesAsync();

        var reloaded = await _db.Modules.FindAsync(module.Id);
        reloaded.ShouldNotBeNull();
        reloaded.Name.ShouldBe("Updated Module");
    }

    [Fact]
    public async Task UpdateModule_ChangesStatus()
    {
        var module = TestModuleFactory.CreateValid("Status Test", ProjectId);
        _db.Modules.Add(module);
        await _db.SaveChangesAsync();

        module.UpdateStatus("in-progress");
        await _db.SaveChangesAsync();

        var reloaded = await _db.Modules.FindAsync(module.Id);
        reloaded.ShouldNotBeNull();
        reloaded.Status.ShouldBe("in-progress");
    }

    [Fact]
    public async Task SoftDeleteModule_MarksAsDeleted()
    {
        var module = TestModuleFactory.CreateValid("Delete Test", ProjectId);
        _db.Modules.Add(module);
        await _db.SaveChangesAsync();

        module.SoftDelete(DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync();

        var deleted = await _db.Modules.FindAsync(module.Id);
        deleted.ShouldNotBeNull();
        deleted.IsDeleted.ShouldBeTrue();
        deleted.DeletedOnUtc.ShouldNotBeNull();
    }

    [Fact]
    public async Task ListModules_ReturnsAllForProject()
    {
        _db.Modules.Add(TestModuleFactory.CreateValid("M1", ProjectId));
        _db.Modules.Add(TestModuleFactory.CreateValid("M2", ProjectId));
        _db.Modules.Add(TestModuleFactory.CreateValid("M3", ProjectId));
        await _db.SaveChangesAsync();

        var modules = await _db.Modules
            .Where(m => m.ProjectId == ProjectId && !m.IsDeleted && m.ArchivedAt == null)
            .ToListAsync();

        modules.Count.ShouldBe(3);
    }

    [Fact]
    public async Task ListModules_FilterByStatus()
    {
        _db.Modules.Add(TestModuleFactory.CreateValid("Planned Module", ProjectId, "planned"));
        _db.Modules.Add(TestModuleFactory.CreateValid("InProgress Module", ProjectId, "in-progress"));
        _db.Modules.Add(TestModuleFactory.CreateValid("Completed Module", ProjectId, "completed"));
        await _db.SaveChangesAsync();

        var completedModules = await _db.Modules
            .Where(m => m.ProjectId == ProjectId && !m.IsDeleted && m.ArchivedAt == null && m.Status == "completed")
            .ToListAsync();

        completedModules.Count.ShouldBe(1);
        completedModules[0].Name.ShouldBe("Completed Module");
    }

    [Fact]
    public async Task ArchiveModule_ArchivesModule()
    {
        var module = TestModuleFactory.CreateValid("Archive Test", ProjectId);
        _db.Modules.Add(module);
        await _db.SaveChangesAsync();

        module.Archive();
        await _db.SaveChangesAsync();

        var archived = await _db.Modules.FindAsync(module.Id);
        archived.ShouldNotBeNull();
        archived.ArchivedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task UnarchiveModule_ClearsArchivedAt()
    {
        var module = TestModuleFactory.CreateValid("Unarchive Test", ProjectId);
        _db.Modules.Add(module);
        module.Archive();
        await _db.SaveChangesAsync();

        // Verify archived
        module.ArchivedAt.ShouldNotBeNull();

        // Unarchive
        module.Unarchive();
        await _db.SaveChangesAsync();

        var restored = await _db.Modules.FindAsync(module.Id);
        restored.ShouldNotBeNull();
        restored.ArchivedAt.ShouldBeNull();
    }

    [Fact]
    public async Task ListArchivedModules_ReturnsOnlyArchived()
    {
        _db.Modules.Add(TestModuleFactory.CreateValid("Active Module", ProjectId));
        var archived1 = TestModuleFactory.CreateValid("Archived 1", ProjectId);
        archived1.Archive();
        _db.Modules.Add(archived1);
        var archived2 = TestModuleFactory.CreateValid("Archived 2", ProjectId);
        archived2.Archive();
        _db.Modules.Add(archived2);
        await _db.SaveChangesAsync();

        var archivedModules = await _db.Modules
            .Where(m => m.ProjectId == ProjectId && !m.IsDeleted && m.ArchivedAt != null)
            .ToListAsync();

        archivedModules.Count.ShouldBe(2);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
