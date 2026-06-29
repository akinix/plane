namespace YH.Tests.WorkItems.Domain;

/// <summary>
/// Domain unit tests for <see cref="Module"/> entity.
/// Covers creation, update, archive, and soft delete behaviors.
/// </summary>
public sealed class ModuleDomainTests
{
    private static readonly Guid ProjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    [Fact]
    public void Create_WithValidArgs_SetsProperties()
    {
        var module = Module.Create("Sprint Planning", ProjectId);

        module.Name.ShouldBe("Sprint Planning");
        module.ProjectId.ShouldBe(ProjectId);
        module.Status.ShouldBe("planned");
        module.SortOrder.ShouldBe(65535.0);
        module.Version.ShouldBe(1);
        module.ArchivedAt.ShouldBeNull();
        module.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void Create_WithEmptyName_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            Module.Create("", ProjectId));
    }

    [Fact]
    public void Create_WithEmptyProjectId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            Module.Create("Sprint Planning", Guid.Empty));
    }

    [Fact]
    public void Create_DefaultStatusIsPlanned()
    {
        var module = Module.Create("Default Status", ProjectId);

        module.Status.ShouldBe("planned");
    }

    [Fact]
    public void Create_WithCustomStatus_SetsStatus()
    {
        var module = Module.Create("InProgress Module", ProjectId, status: "in-progress");

        module.Status.ShouldBe("in-progress");
    }

    [Fact]
    public void Update_ChangesName()
    {
        var module = TestModuleFactory.CreateValid(name: "Original");

        module.Update(name: "Updated");

        module.Name.ShouldBe("Updated");
        module.LastModifiedOnUtc.ShouldNotBeNull();
    }

    [Fact]
    public void Update_WithNullParams_DoesNotChange()
    {
        var module = TestModuleFactory.CreateValid(name: "Original", status: "planned");

        module.Update(name: null, description: null, status: null);

        module.Name.ShouldBe("Original");
        module.Status.ShouldBe("planned");
    }

    [Fact]
    public void UpdateStatus_ChangesStatus()
    {
        var module = TestModuleFactory.CreateValid();

        module.UpdateStatus("completed");

        module.Status.ShouldBe("completed");
    }

    [Fact]
    public void UpdateStatus_WithNull_DoesNotChange()
    {
        var module = TestModuleFactory.CreateValid();
        var originalStatus = module.Status;

        module.UpdateStatus(null);

        module.Status.ShouldBe(originalStatus);
    }

    [Fact]
    public void Archive_SetsArchivedAt()
    {
        var module = TestModuleFactory.CreateValid();

        module.Archive();

        module.ArchivedAt.ShouldNotBeNull();
        module.LastModifiedOnUtc.ShouldNotBeNull();
    }

    [Fact]
    public void Unarchive_ClearsArchivedAt()
    {
        var module = TestModuleFactory.CreateValid();
        module.Archive();

        module.Unarchive();

        module.ArchivedAt.ShouldBeNull();
    }

    [Fact]
    public void SoftDelete_SetsDeletedFlag()
    {
        var module = TestModuleFactory.CreateValid();
        var now = DateTimeOffset.UtcNow;

        module.SoftDelete(now);

        module.IsDeleted.ShouldBeTrue();
        module.DeletedOnUtc.ShouldBe(now);
    }

    [Fact]
    public void SoftDelete_IsIdempotent()
    {
        var module = TestModuleFactory.CreateValid();
        var now = DateTimeOffset.UtcNow;
        module.SoftDelete(now);

        module.SoftDelete(DateTimeOffset.UtcNow.AddDays(1));

        module.DeletedOnUtc.ShouldBe(now);
    }
}
