namespace YH.Tests.WorkItems.Integration;

/// <summary>
/// Integration tests for <see cref="Estimate"/> and <see cref="EstimatePoint"/> CRUD via InMemory DbContext (plan 04-05 Task 2).
/// </summary>
[Collection("WorkItemsTest")]
public sealed class EstimateCrudTests : IDisposable
{
    private readonly WorkItemsDbContext _db;
    private static readonly Guid ProjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public EstimateCrudTests()
    {
        _db = WorkItemsTestFixture.CreateInMemoryContext();
    }

    [Fact]
    public async Task CreateEstimate_PersistsToDatabase()
    {
        var estimate = TestEstimateFactory.CreateValid(projectId: ProjectId);
        _db.Estimates.Add(estimate);
        await _db.SaveChangesAsync();

        var saved = await _db.Estimates.FindAsync(estimate.Id);
        saved.ShouldNotBeNull();
        saved.Name.ShouldBe("Fibonacci");
        saved.Type.ShouldBe("points");
    }

    [Fact]
    public async Task CreateEstimateWithPoints_PersistsPoints()
    {
        var estimate = TestEstimateFactory.CreateValid(projectId: ProjectId);
        estimate.AddPoint(TestEstimateFactory.CreatePoint(estimate.Id, 0, "1"));
        estimate.AddPoint(TestEstimateFactory.CreatePoint(estimate.Id, 1, "2"));
        estimate.AddPoint(TestEstimateFactory.CreatePoint(estimate.Id, 2, "3"));
        _db.Estimates.Add(estimate);
        await _db.SaveChangesAsync();

        var saved = await _db.Estimates
            .Include(e => e.EstimatePoints)
            .FirstAsync(e => e.Id == estimate.Id);
        saved.EstimatePoints.Count.ShouldBe(3);
    }

    [Fact]
    public async Task UpdateEstimate_PersistsChanges()
    {
        var estimate = TestEstimateFactory.CreateValid(projectId: ProjectId);
        _db.Estimates.Add(estimate);
        await _db.SaveChangesAsync();

        estimate.Update(name: "T-shirt sizes", type: "categories");
        await _db.SaveChangesAsync();

        var reloaded = await _db.Estimates.FindAsync(estimate.Id);
        reloaded.ShouldNotBeNull();
        reloaded.Name.ShouldBe("T-shirt sizes");
        reloaded.Type.ShouldBe("categories");
    }

    [Fact]
    public async Task MarkEstimateAsLastUsed_PersistsFlag()
    {
        var estimate = TestEstimateFactory.CreateValid(projectId: ProjectId);
        _db.Estimates.Add(estimate);
        await _db.SaveChangesAsync();

        estimate.MarkAsLastUsed();
        await _db.SaveChangesAsync();

        var reloaded = await _db.Estimates.FindAsync(estimate.Id);
        reloaded.ShouldNotBeNull();
        reloaded.IsLastUsed.ShouldBeTrue();
    }

    [Fact]
    public async Task ListEstimates_ReturnsAllForProject()
    {
        _db.Estimates.Add(TestEstimateFactory.CreateValid("Fibonacci", "points", ProjectId));
        _db.Estimates.Add(TestEstimateFactory.CreateValid("T-shirt", "categories", ProjectId));
        await _db.SaveChangesAsync();

        var estimates = await _db.Estimates.Where(e => e.ProjectId == ProjectId).ToListAsync();
        estimates.Count.ShouldBe(2);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
