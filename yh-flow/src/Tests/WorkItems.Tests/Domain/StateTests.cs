namespace YH.Tests.WorkItems.Domain;

/// <summary>
/// Domain unit tests for <see cref="State"/> entity (plan 04-05 Task 1).
/// </summary>
public sealed class StateTests
{
    [Fact]
    public void Create_WithValidArgs_SetsProperties()
    {
        var projectId = Guid.NewGuid();
        var state = State.Create("In Progress", "#F59E0B", StateGroup.Started, projectId, true);

        state.Name.ShouldBe("In Progress");
        state.Color.ShouldBe("#F59E0B");
        state.Group.ShouldBe(StateGroup.Started);
        state.ProjectId.ShouldBe(projectId);
        state.IsDefault.ShouldBeTrue();
        state.SortOrder.ShouldBe(65535.0);
        state.IsDeleted.ShouldBeFalse();
        state.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void Create_WithEmptyName_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            State.Create("", null, StateGroup.Backlog, Guid.NewGuid(), false));
    }

    [Fact]
    public void Create_WithEmptyProjectId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            State.Create("Todo", null, StateGroup.Backlog, Guid.Empty, false));
    }

    [Fact]
    public void Create_WithInvalidGroup_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            State.Create("Invalid", null, (StateGroup)99, Guid.NewGuid(), false));
    }

    [Fact]
    public void Update_ChangesNameAndColor()
    {
        var state = TestStateFactory.CreateValid(name: "Original", color: "#000");

        state.Update(name: "Updated", color: "#FFF");

        state.Name.ShouldBe("Updated");
        state.Color.ShouldBe("#FFF");
        state.LastModifiedOnUtc.ShouldNotBeNull();
    }

    [Fact]
    public void Update_WithNullValues_DoesNotChangeFields()
    {
        var state = TestStateFactory.CreateValid(name: "Original", color: "#000", sortOrder: 1.0);

        state.Update(name: null, color: null, sortOrder: null);

        state.Name.ShouldBe("Original");
        state.Color.ShouldBe("#000");
        state.SortOrder.ShouldBe(1.0);
    }

    [Fact]
    public void SoftDelete_SetsDeletedFlag()
    {
        var state = TestStateFactory.CreateValid();
        var now = DateTimeOffset.UtcNow;

        state.SoftDelete(now);

        state.IsDeleted.ShouldBeTrue();
        state.DeletedOnUtc.ShouldBe(now);
    }

    [Fact]
    public void SoftDelete_IsIdempotent()
    {
        var state = TestStateFactory.CreateValid();
        var now = DateTimeOffset.UtcNow;
        state.SoftDelete(now);

        state.SoftDelete(DateTimeOffset.UtcNow.AddDays(1));

        state.DeletedOnUtc.ShouldBe(now); // unchanged — first delete wins
    }

    [Fact]
    public void AllFiveGroups_CanCreateDefaultState()
    {
        foreach (StateGroup group in Enum.GetValues<StateGroup>())
        {
            var state = State.Create($"State-{group}", null, group, Guid.NewGuid(), true);
            state.Group.ShouldBe(group);
        }
    }
}
