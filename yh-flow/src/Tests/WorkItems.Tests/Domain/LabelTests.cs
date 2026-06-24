namespace YH.Tests.WorkItems.Domain;

/// <summary>
/// Domain unit tests for <see cref="Label"/> entity (plan 04-05 Task 1).
/// </summary>
public sealed class LabelTests
{
    [Fact]
    public void Create_WithValidArgs_SetsProperties()
    {
        var projectId = Guid.NewGuid();
        var label = Label.Create("Bug", "#F59E0B", projectId);

        label.Name.ShouldBe("Bug");
        label.Color.ShouldBe("#F59E0B");
        label.ProjectId.ShouldBe(projectId);
        label.ParentId.ShouldBeNull();
        label.Description.ShouldBeNull();
        label.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void Create_WithEmptyName_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            Label.Create("", null, Guid.NewGuid()));
    }

    [Fact]
    public void Create_WithEmptyProjectId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            Label.Create("Bug", null, Guid.Empty));
    }

    [Fact]
    public void Create_WithParentId_SetsHierarchy()
    {
        var projectId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var label = Label.Create("Sub-bug", "#FF0000", projectId, parentId, "Nested label");

        label.ParentId.ShouldBe(parentId);
        label.Description.ShouldBe("Nested label");
    }

    [Fact]
    public void Update_ChangesAllFields()
    {
        var label = TestLabelFactory.CreateValid();
        var parentId = Guid.NewGuid();

        label.Update(name: "Feature", color: "#00FF00", parentId: parentId, description: "A feature label", sortOrder: 1.0);

        label.Name.ShouldBe("Feature");
        label.Color.ShouldBe("#00FF00");
        label.ParentId.ShouldBe(parentId);
        label.Description.ShouldBe("A feature label");
        label.SortOrder.ShouldBe(1.0);
    }

    [Fact]
    public void Update_WithNullValues_DoesNotChangeFields()
    {
        var label = TestLabelFactory.CreateValid(name: "Original", color: "#000");
        label.Update(name: null, color: null, parentId: null, description: null, sortOrder: null);

        label.Name.ShouldBe("Original");
        label.Color.ShouldBe("#000");
        label.ParentId.ShouldBeNull();
    }

    [Fact]
    public void SoftDelete_SetsDeletedFlag()
    {
        var label = TestLabelFactory.CreateValid();
        var now = DateTimeOffset.UtcNow;

        label.SoftDelete(now);

        label.IsDeleted.ShouldBeTrue();
        label.DeletedOnUtc.ShouldBe(now);
    }

    [Fact]
    public void SoftDelete_IsIdempotent()
    {
        var label = TestLabelFactory.CreateValid();
        var now = DateTimeOffset.UtcNow;
        label.SoftDelete(now);

        label.SoftDelete(DateTimeOffset.UtcNow.AddDays(1));

        label.DeletedOnUtc.ShouldBe(now);
    }
}
