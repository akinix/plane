using Microsoft.EntityFrameworkCore;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Data.Seeders;

/// <summary>
/// Default state seed data — 5 states per project, one per group.
/// </summary>
/// <remarks>
/// <b>Idempotency:</b> <see cref="SeedProjectStatesAsync"/> checks if a state with the same
/// Name + ProjectId already exists before creating (T-4-seed-01).
/// <para>
/// <b>Default states (Plane DEFAULT_STATES compatibility):</b>
/// <list type="bullet">
///   <item>Backlog (#60646C, Backlog group, sort 15000, IsDefault=true)</item>
///   <item>Todo (#60646C, Unstarted group, sort 25000)</item>
///   <item>In Progress (#F59E0B, Started group, sort 35000)</item>
///   <item>Done (#46A758, Completed group, sort 45000)</item>
///   <item>Cancelled (#9AA4BC, Cancelled group, sort 55000)</item>
/// </list>
/// </para>
/// </remarks>
public static class StateSeeder
{
    /// <summary>The 5 default states per project.</summary>
    public static readonly IReadOnlyList<StateSeedData> DefaultStates =
    [
        new("Backlog", "#60646C", StateGroup.Backlog, 15000, true),
        new("Todo", "#60646C", StateGroup.Unstarted, 25000, false),
        new("In Progress", "#F59E0B", StateGroup.Started, 35000, false),
        new("Done", "#46A758", StateGroup.Completed, 45000, false),
        new("Cancelled", "#9AA4BC", StateGroup.Cancelled, 55000, false),
    ];

    /// <summary>
    /// Seeds default states for a project. Idempotent — skips existing states.
    /// </summary>
    /// <param name="db">The WorkItems DbContext.</param>
    /// <param name="projectId">The target project.</param>
    /// <param name="createdBy">The user who created the project (for audit trail).</param>
    public static async Task SeedProjectStatesAsync(WorkItemsDbContext db, Guid projectId, Guid createdBy)
    {
        ArgumentNullException.ThrowIfNull(db);

        foreach (var s in DefaultStates)
        {
            var exists = await db.States
                .AnyAsync(x => x.ProjectId == projectId && x.Name == s.Name)
                .ConfigureAwait(false);

            if (!exists)
            {
                var state = State.Create(
                    name: s.Name,
                    color: s.Color,
                    group: s.Group,
                    projectId: projectId,
                    isDefault: s.IsDefault,
                    sortOrder: s.SortOrder);

                // Set created-by for audit trail (private setter for IAuditableEntity)
                // Note: CreatedBy is populated by AuditableEntitySaveChangesInterceptor
                db.States.Add(state);
            }
        }

        await db.SaveChangesAsync().ConfigureAwait(false);
    }
}

/// <summary>
/// Seed data record for a default state.
/// </summary>
public sealed record StateSeedData(
    string Name,
    string Color,
    StateGroup Group,
    double SortOrder,
    bool IsDefault);
