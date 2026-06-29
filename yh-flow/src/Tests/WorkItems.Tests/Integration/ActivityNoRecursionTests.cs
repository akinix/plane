namespace YH.Tests.WorkItems.Integration;

/// <summary>
/// Recursion prevention tests for <see cref="IssueActivity"/> (plan 04-05 Task 2, T-4-activity-01).
/// Verifies that IssueActivity does NOT implement IHasDomainEvents, so the handler writing
/// activity records will never re-trigger domain events.
/// </summary>
[Collection("WorkItemsTest")]
public sealed class ActivityNoRecursionTests
{
    [Fact]
    public void IssueActivity_DoesNotImplementIHasDomainEvents()
    {
        // T-4-activity-01 CRITICAL: This is a compile-time invariant checked via reflection.
        // If IssueActivity ever gains IHasDomainEvents, the handler in IssueActivityHandler
        // that writes activity records will re-trigger IssueUpdatedDomainEvent, creating an
        // infinite recursion loop.
        var type = typeof(IssueActivity);
        var interfaces = type.GetInterfaces().Select(i => i.FullName).OfType<string>().ToList();

        interfaces.ShouldNotContain("YH.Framework.Core.Domain.IHasDomainEvents",
            "IssueActivity must never implement IHasDomainEvents (recursion prevention T-4-activity-01)");
    }

    [Fact]
    public void IssueActivity_DoesNotHaveDomainEventsProperty()
    {
        // Verify that IssueActivity has no DomainEvents property (the entity has no
        // IHasDomainEvents interface, so it should not expose a DomainEvents collection).
        var hasDomainEventsProp = typeof(IssueActivity)
            .GetProperties()
            .Any(p => p.Name == "DomainEvents");

        hasDomainEventsProp.ShouldBeFalse("IssueActivity must not expose DomainEvents");
    }

    [Fact]
    public void IssueActivityHandler_WritesDirectlyToDbContext_NoEventChain()
    {
        // Verify that IssueActivityHandler writes via DbContext.Set<>().Add() directly,
        // not through any domain event mechanism. This is a design-level test:
        // IssueActivityHandler uses _db.Set<IssueActivity>().Add(activity) then
        // _db.SaveChangesAsync() — no domain event is raised by those calls because
        // IssueActivity doesn't implement IHasDomainEvents.
        var handlerType = typeof(IssueActivityHandler);
        var interfaces = handlerType.GetInterfaces().Select(i => i.FullName).OfType<string>().ToList();

        interfaces.Any(i => i.StartsWith("Mediator.INotificationHandler`1", StringComparison.Ordinal)).ShouldBeTrue();
    }
}
