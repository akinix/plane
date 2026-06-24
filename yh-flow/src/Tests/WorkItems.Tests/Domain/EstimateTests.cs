namespace YH.Tests.WorkItems.Domain;

/// <summary>
/// Domain unit tests for <see cref="Estimate"/> and <see cref="EstimatePoint"/> entities (plan 04-05 Task 1).
/// </summary>
public sealed class EstimateTests
{
    private static readonly Guid ProjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    [Fact]
    public void CreateEstimate_WithValidArgs_SetsProperties()
    {
        var estimate = Estimate.Create("Fibonacci", "points", ProjectId);

        estimate.Name.ShouldBe("Fibonacci");
        estimate.Type.ShouldBe("points");
        estimate.ProjectId.ShouldBe(ProjectId);
        estimate.IsLastUsed.ShouldBeFalse();
        estimate.EstimatePoints.ShouldBeEmpty();
    }

    [Fact]
    public void CreateEstimate_WithEmptyName_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            Estimate.Create("", "points", ProjectId));
    }

    [Fact]
    public void CreateEstimate_WithInvalidType_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            Estimate.Create("Bad", "stars", ProjectId));
    }

    [Fact]
    public void CreateEstimate_WithEmptyProjectId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            Estimate.Create("Test", "points", Guid.Empty));
    }

    [Fact]
    public void Update_ChangesNameAndType()
    {
        var estimate = TestEstimateFactory.CreateValid();

        estimate.Update(name: "T-shirt sizes", type: "categories");

        estimate.Name.ShouldBe("T-shirt sizes");
        estimate.Type.ShouldBe("categories");
    }

    [Fact]
    public void Update_WithNull_DoesNotChange()
    {
        var estimate = TestEstimateFactory.CreateValid();

        estimate.Update(name: null, type: null);

        estimate.Name.ShouldBe("Fibonacci");
        estimate.Type.ShouldBe("points");
    }

    [Fact]
    public void Update_WithInvalidType_Throws()
    {
        var estimate = TestEstimateFactory.CreateValid();

        Should.Throw<ArgumentException>(() =>
            estimate.Update(type: "invalid"));
    }

    [Fact]
    public void AddPoint_AddsToCollection()
    {
        var estimate = TestEstimateFactory.CreateValid();
        var point = EstimatePoint.Create(estimate.Id, 0, "1");

        estimate.AddPoint(point);

        estimate.EstimatePoints.Count.ShouldBe(1);
        estimate.EstimatePoints[0].Value.ShouldBe("1");
        estimate.EstimatePoints[0].Key.ShouldBe(0);
    }

    [Fact]
    public void AddPoint_WithNull_Throws()
    {
        var estimate = TestEstimateFactory.CreateValid();
        Should.Throw<ArgumentNullException>(() => estimate.AddPoint(null!));
    }

    [Fact]
    public void RemovePoint_RemovesExistingPoint()
    {
        var estimate = TestEstimateFactory.CreateValid();
        var point = EstimatePoint.Create(estimate.Id, 0, "1");
        estimate.AddPoint(point);
        estimate.EstimatePoints.Count.ShouldBe(1);

        estimate.RemovePoint(point.Id);

        estimate.EstimatePoints.ShouldBeEmpty();
    }

    [Fact]
    public void RemovePoint_WithUnknownId_DoesNothing()
    {
        var estimate = TestEstimateFactory.CreateValid();
        estimate.AddPoint(EstimatePoint.Create(estimate.Id, 0, "1"));

        estimate.RemovePoint(Guid.NewGuid());

        estimate.EstimatePoints.Count.ShouldBe(1);
    }

    [Fact]
    public void MarkAsLastUsed_SetsIsLastUsed()
    {
        var estimate = TestEstimateFactory.CreateValid();

        estimate.MarkAsLastUsed();

        estimate.IsLastUsed.ShouldBeTrue();
    }

    [Fact]
    public void MarkAsLastUsed_IsIdempotent()
    {
        var estimate = TestEstimateFactory.CreateValid();
        estimate.MarkAsLastUsed();

        estimate.MarkAsLastUsed();

        estimate.IsLastUsed.ShouldBeTrue();
    }

    [Fact]
    public void SoftDelete_SetsDeletedFlag()
    {
        var estimate = TestEstimateFactory.CreateValid();
        var now = DateTimeOffset.UtcNow;

        estimate.SoftDelete(now);

        estimate.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public void CreateEstimatePoint_WithValidArgs_SetsProperties()
    {
        var estimateId = Guid.NewGuid();
        var point = EstimatePoint.Create(estimateId, 0, "1", 1.0);

        point.EstimateId.ShouldBe(estimateId);
        point.Key.ShouldBe(0);
        point.Value.ShouldBe("1");
        point.SortOrder.ShouldBe(1.0);
    }

    [Fact]
    public void CreateEstimatePoint_WithEmptyValue_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            EstimatePoint.Create(Guid.NewGuid(), 0, ""));
    }

    [Fact]
    public void CreateEstimatePoint_WithEmptyEstimateId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            EstimatePoint.Create(Guid.Empty, 0, "1"));
    }

    [Fact]
    public void EstimatePoint_UpdateValue_ChangesValue()
    {
        var point = EstimatePoint.Create(Guid.NewGuid(), 0, "1");
        point.UpdateValue("2", key: 1);

        point.Value.ShouldBe("2");
        point.Key.ShouldBe(1);
    }
}
