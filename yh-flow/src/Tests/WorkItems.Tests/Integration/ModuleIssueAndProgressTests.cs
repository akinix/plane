using Microsoft.EntityFrameworkCore;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;
using YH.Tests.WorkItems.TestData;

namespace YH.Tests.WorkItems.Integration;

/// <summary>
/// Integration tests for Module-Issue association, ModuleLink, and ModuleProgress operations.
/// </summary>
[Collection("WorkItemsTest")]
public sealed class ModuleIssueAndProgressTests : IDisposable
{
    private readonly WorkItemsDbContext _db;
    private static readonly Guid ProjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public ModuleIssueAndProgressTests()
    {
        _db = WorkItemsTestFixture.CreateInMemoryContext();
    }

    [Fact]
    public async Task AddIssueToModule_CreatesModuleIssue()
    {
        var module = TestModuleFactory.CreateValid("Module", ProjectId);
        _db.Modules.Add(module);

        var issue = TestIssueFactory.CreateValid(projectId: ProjectId);
        _db.Issues.Add(issue);
        await _db.SaveChangesAsync();

        var moduleIssue = TestModuleIssueFactory.CreateValid(issue.Id, module.Id);
        _db.Set<ModuleIssue>().Add(moduleIssue);
        await _db.SaveChangesAsync();

        var saved = await _db.Set<ModuleIssue>()
            .FirstOrDefaultAsync(mi => mi.ModuleId == module.Id && mi.IssueId == issue.Id && !mi.IsDeleted);
        saved.ShouldNotBeNull();
        saved.IssueId.ShouldBe(issue.Id);
        saved.ModuleId.ShouldBe(module.Id);
    }

    [Fact]
    public async Task AddIssueToModule_CompletedModule_Allowed()
    {
        // Module has no COMPLETED restriction — unlike Cycle, even completed modules accept issues
        var module = TestModuleFactory.CreateCompleted("Completed Module", ProjectId);
        _db.Modules.Add(module);

        var issue = TestIssueFactory.CreateValid(projectId: ProjectId);
        _db.Issues.Add(issue);
        await _db.SaveChangesAsync();

        var moduleIssue = TestModuleIssueFactory.CreateValid(issue.Id, module.Id);
        _db.Set<ModuleIssue>().Add(moduleIssue);
        await _db.SaveChangesAsync();

        var saved = await _db.Set<ModuleIssue>()
            .FirstOrDefaultAsync(mi => mi.ModuleId == module.Id && mi.IssueId == issue.Id && !mi.IsDeleted);
        saved.ShouldNotBeNull();
        saved.IssueId.ShouldBe(issue.Id);
    }

    [Fact]
    public async Task RemoveIssueFromModule_SoftDeletes()
    {
        var module = TestModuleFactory.CreateValid("Module", ProjectId);
        _db.Modules.Add(module);

        var issue = TestIssueFactory.CreateValid(projectId: ProjectId);
        _db.Issues.Add(issue);
        await _db.SaveChangesAsync();

        var moduleIssue = TestModuleIssueFactory.CreateValid(issue.Id, module.Id);
        _db.Set<ModuleIssue>().Add(moduleIssue);
        await _db.SaveChangesAsync();

        // Soft-delete the ModuleIssue
        moduleIssue.SoftDelete(DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync();

        var deleted = await _db.Set<ModuleIssue>().FindAsync(moduleIssue.Id);
        deleted.ShouldNotBeNull();
        deleted.IsDeleted.ShouldBeTrue();
        deleted.DeletedOnUtc.ShouldNotBeNull();
    }

    [Fact]
    public async Task ListModuleIssues_ReturnsIssues()
    {
        var module = TestModuleFactory.CreateValid("Module", ProjectId);
        _db.Modules.Add(module);

        var issue1 = TestIssueFactory.CreateValid("Module Issue 1", projectId: ProjectId);
        _db.Issues.Add(issue1);
        var issue2 = TestIssueFactory.CreateValid("Module Issue 2", projectId: ProjectId);
        _db.Issues.Add(issue2);
        await _db.SaveChangesAsync();

        _db.Set<ModuleIssue>().Add(TestModuleIssueFactory.CreateValid(issue1.Id, module.Id));
        _db.Set<ModuleIssue>().Add(TestModuleIssueFactory.CreateValid(issue2.Id, module.Id));
        await _db.SaveChangesAsync();

        // Query issues in the module
        var issueIds = await _db.Set<ModuleIssue>()
            .Where(mi => mi.ModuleId == module.Id && !mi.IsDeleted)
            .Select(mi => mi.IssueId)
            .ToListAsync();

        var issues = await _db.Issues
            .Where(i => issueIds.Contains(i.Id) && !i.IsDeleted)
            .ToListAsync();

        issues.Count.ShouldBe(2);
    }

    [Fact]
    public async Task AddLinkToModule_CreatesModuleLink()
    {
        var module = TestModuleFactory.CreateValid("Module with Links", ProjectId);
        _db.Modules.Add(module);
        await _db.SaveChangesAsync();

        var link = TestModuleLinkFactory.CreateValid(moduleId: module.Id);
        _db.Set<ModuleLink>().Add(link);
        await _db.SaveChangesAsync();

        var saved = await _db.Set<ModuleLink>()
            .FirstOrDefaultAsync(l => l.ModuleId == module.Id && !l.IsDeleted);
        saved.ShouldNotBeNull();
        saved.Title.ShouldBe("Figma Design");
        saved.Url.ShouldBe("https://figma.com/file/abc");
    }

    [Fact]
    public async Task RemoveLinkFromModule_SoftDeletes()
    {
        var module = TestModuleFactory.CreateValid("Module", ProjectId);
        _db.Modules.Add(module);
        await _db.SaveChangesAsync();

        var link = TestModuleLinkFactory.CreateValid(moduleId: module.Id);
        _db.Set<ModuleLink>().Add(link);
        await _db.SaveChangesAsync();

        // Soft-delete the ModuleLink
        link.SoftDelete(DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync();

        var deleted = await _db.Set<ModuleLink>().FindAsync(link.Id);
        deleted.ShouldNotBeNull();
        deleted.IsDeleted.ShouldBeTrue();
        deleted.DeletedOnUtc.ShouldNotBeNull();
    }

    [Fact]
    public async Task ListModuleLinks_ReturnsLinks()
    {
        var module = TestModuleFactory.CreateValid("Module with Links", ProjectId);
        _db.Modules.Add(module);
        await _db.SaveChangesAsync();

        _db.Set<ModuleLink>().Add(TestModuleLinkFactory.CreateValid("Link 1", "https://example.com/1", moduleId: module.Id));
        _db.Set<ModuleLink>().Add(TestModuleLinkFactory.CreateValid("Link 2", "https://example.com/2", moduleId: module.Id));
        await _db.SaveChangesAsync();

        var links = await _db.Set<ModuleLink>()
            .Where(l => l.ModuleId == module.Id && !l.IsDeleted)
            .OrderBy(l => l.CreatedOnUtc)
            .ToListAsync();

        links.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetModuleProgress_ReturnsAggregatedData()
    {
        var module = TestModuleFactory.CreateValid("Progress Module", ProjectId);
        _db.Modules.Add(module);

        // Create states for different state groups
        var completedState = TestStateFactory.CreateCompleted(projectId: ProjectId);
        _db.States.Add(completedState);
        var startedState = TestStateFactory.CreateValid("In Progress", "#FF0000", StateGroup.Started, ProjectId);
        _db.States.Add(startedState);
        var unstartedState = TestStateFactory.CreateValid("Todo", "#3A85FF", StateGroup.Unstarted, ProjectId);
        _db.States.Add(unstartedState);
        await _db.SaveChangesAsync();

        // Create issues with different states
        var completedIssue = TestIssueFactory.CreateValid("Done Issue", projectId: ProjectId, stateId: completedState.Id);
        _db.Issues.Add(completedIssue);
        var startedIssue = TestIssueFactory.CreateValid("In Progress Issue", projectId: ProjectId, stateId: startedState.Id);
        _db.Issues.Add(startedIssue);
        var unstartedIssue = TestIssueFactory.CreateValid("Todo Issue", projectId: ProjectId, stateId: unstartedState.Id);
        _db.Issues.Add(unstartedIssue);
        await _db.SaveChangesAsync();

        // Associate issues with module
        _db.Set<ModuleIssue>().Add(TestModuleIssueFactory.CreateValid(completedIssue.Id, module.Id));
        _db.Set<ModuleIssue>().Add(TestModuleIssueFactory.CreateValid(startedIssue.Id, module.Id));
        _db.Set<ModuleIssue>().Add(TestModuleIssueFactory.CreateValid(unstartedIssue.Id, module.Id));
        await _db.SaveChangesAsync();

        // Aggregate progress
        var progress = await (
            from mi in _db.Set<ModuleIssue>().AsNoTracking()
            join i in _db.Issues.AsNoTracking() on mi.IssueId equals i.Id
            join s in _db.States.AsNoTracking() on i.StateId equals s.Id
            where mi.ModuleId == module.Id && !mi.IsDeleted && !i.IsDeleted
            group s by s.Group into g
            select new { Group = g.Key, Count = g.Count() }
        ).ToListAsync();

        var total = progress.Sum(x => x.Count);
        var completed = progress.Where(x => x.Group == StateGroup.Completed).Sum(x => x.Count);
        var started = progress.Where(x => x.Group == StateGroup.Started).Sum(x => x.Count);
        var unstarted = progress.Where(x => x.Group == StateGroup.Unstarted).Sum(x => x.Count);

        total.ShouldBe(3);
        completed.ShouldBe(1);
        started.ShouldBe(1);
        unstarted.ShouldBe(1);

        var completedPercentage = total > 0 ? Math.Round((double)completed / total * 100, 1) : 0;
        completedPercentage.ShouldBe(33.3);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
