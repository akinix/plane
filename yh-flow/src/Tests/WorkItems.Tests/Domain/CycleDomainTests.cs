namespace YH.Tests.WorkItems.Domain;

/// <summary>
/// Domain unit tests for <see cref="Cycle"/> entity.
/// Covers creation, update, archive, and soft delete behaviors.
/// </summary>
public sealed class CycleDomainTests
{
    private static readonly Guid ProjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    [Fact]
    public void Create_WithValidArgs_SetsProperties()
    {
        var cycle = Cycle.Create("Sprint 1", ProjectId);

        cycle.Name.ShouldBe("Sprint 1");
        cycle.ProjectId.ShouldBe(ProjectId);
        cycle.SortOrder.ShouldBe(65535.0);
        cycle.Version.ShouldBe(1);
        cycle.Timezone.ShouldBe("UTC");
        cycle.StartDate.ShouldBeNull();
        cycle.EndDate.ShouldBeNull();
        cycle.ArchivedAt.ShouldBeNull();
        cycle.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void Create_WithEmptyName_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            Cycle.Create("", ProjectId));
    }

    [Fact]
    public void Create_WithEmptyProjectId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            Cycle.Create("Sprint 1", Guid.Empty));
    }

    [Fact]
    public void Create_WithStartDateOnly_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            Cycle.Create("Sprint 1", ProjectId,
                startDate: DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Create_WithBothDates_SetsProperties()
    {
        var start = DateTimeOffset.UtcNow;
        var end = start.AddDays(14);

        var cycle = Cycle.Create("Sprint 1", ProjectId, start, end);

        cycle.StartDate.ShouldBe(start);
        cycle.EndDate.ShouldBe(end);
    }

    [Fact]
    public void Create_BacklogCycle_WithoutDates_SetsProperties()
    {
        var cycle = Cycle.Create("Backlog", ProjectId);

        cycle.StartDate.ShouldBeNull();
        cycle.EndDate.ShouldBeNull();
        cycle.Name.ShouldBe("Backlog");
    }

    [Fact]
    public void AssignSortOrder_UpdatesSortOrder()
    {
        var cycle = TestCycleFactory.CreateValid();

        cycle.AssignSortOrder(100.0);

        cycle.SortOrder.ShouldBe(100.0);
    }

    [Fact]
    public void AssignSortOrder_WithNull_DoesNotChange()
    {
        var cycle = TestCycleFactory.CreateValid();

        cycle.AssignSortOrder(null);

        cycle.SortOrder.ShouldBe(65535.0);
    }

    [Fact]
    public void Update_ChangesName()
    {
        var cycle = TestCycleFactory.CreateValid(name: "Original");

        cycle.Update(name: "Updated");

        cycle.Name.ShouldBe("Updated");
        cycle.LastModifiedOnUtc.ShouldNotBeNull();
    }

    [Fact]
    public void Update_WithNullValues_DoesNotChangeFields()
    {
        var cycle = TestCycleFactory.CreateValid(name: "Original", description: "desc");

        cycle.Update(name: null, description: null);

        cycle.Name.ShouldBe("Original");
        cycle.Description.ShouldBe("desc");
    }

    [Fact]
    public void Update_WithDateMismatch_Throws()
    {
        var cycle = TestCycleFactory.CreateBacklog();
        var endOnly = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero);

        var ex = Record.Exception(() => cycle.Update(endDate: endOnly));

        ex.ShouldNotBeNull();
        ex.ShouldBeOfType<ArgumentException>();
    }

    [Fact]
    public void UpdateRestricted_ChangesExternalFields()
    {
        var cycle = TestCycleFactory.CreateValid();

        cycle.UpdateRestricted(externalSource: "github", externalId: "milestone-1");

        cycle.ExternalSource.ShouldBe("github");
        cycle.ExternalId.ShouldBe("milestone-1");
        cycle.LastModifiedOnUtc.ShouldNotBeNull();
    }

    [Fact]
    public void Archive_SetsArchivedAt()
    {
        var cycle = TestCycleFactory.CreateValid();

        cycle.Archive();

        cycle.ArchivedAt.ShouldNotBeNull();
        cycle.LastModifiedOnUtc.ShouldNotBeNull();
    }

    [Fact]
    public void Unarchive_ClearsArchivedAt()
    {
        var cycle = TestCycleFactory.CreateValid();
        cycle.Archive();

        cycle.Unarchive();

        cycle.ArchivedAt.ShouldBeNull();
        cycle.ProgressSnapshot.ShouldBeNull();
    }

    [Fact]
    public void SoftDelete_SetsDeletedFlag()
    {
        var cycle = TestCycleFactory.CreateValid();
        var now = DateTimeOffset.UtcNow;

        cycle.SoftDelete(now);

        cycle.IsDeleted.ShouldBeTrue();
        cycle.DeletedOnUtc.ShouldBe(now);
    }

    [Fact]
    public void SoftDelete_IsIdempotent()
    {
        var cycle = TestCycleFactory.CreateValid();
        var now = DateTimeOffset.UtcNow;
        cycle.SoftDelete(now);

        cycle.SoftDelete(DateTimeOffset.UtcNow.AddDays(1));

        cycle.DeletedOnUtc.ShouldBe(now);
    }

    [Fact]
    public void FreezeSnapshot_SetsSnapshot()
    {
        var cycle = TestCycleFactory.CreateValid();
        var snapshot = "{\"total\":10,\"completed\":5}";

        cycle.FreezeSnapshot(snapshot);

        cycle.ProgressSnapshot.ShouldBe(snapshot);
        cycle.LastModifiedOnUtc.ShouldNotBeNull();
    }
}
