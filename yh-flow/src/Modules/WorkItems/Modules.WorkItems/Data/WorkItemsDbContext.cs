using Finbuckle.MultiTenant.Abstractions;
using YH.Framework.Persistence.Context;
using YH.Framework.Shared.Multitenancy;
using YH.Framework.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Data;

/// <summary>
/// WorkItems module DbContext.
/// </summary>
/// <remarks>
/// <b>OnModelCreating order — RESEARCH Pitfall 8 (CRITICAL):</b>
/// <see cref="OnModelCreating"/> MUST apply per-entity configurations BEFORE delegating to
/// <c>base.OnModelCreating</c>. <c>BaseDbContext.OnModelCreating</c> runs
/// <c>ApplyTenantIsolationByDefault()</c> which iterates the model and calls
/// <c>IsMultiTenant().AdjustUniqueIndexes()</c> on every non-<c>IGlobalEntity</c>. If configs are
/// not yet applied, Finbuckle's <c>AdjustUniqueIndexes</c> has nothing to widen and the composite
/// unique indexes (e.g. <c>(TenantId, ProjectId, Name)</c>) silently end up scoped to the wrong columns.
/// <para>
/// See <c>ProjectDbContext.cs</c> for the authoritative pattern (verified in Phase 3).
/// </para>
/// </remarks>
public sealed class WorkItemsDbContext : BaseDbContext
{
    public WorkItemsDbContext(
        IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor,
        DbContextOptions<WorkItemsDbContext> options,
        IOptions<DatabaseOptions> settings,
        IHostEnvironment environment)
        : base(multiTenantContextAccessor, options, settings, environment) { }

    /// <summary>States table.</summary>
    public DbSet<State> States => Set<State>();

    /// <summary>Labels table.</summary>
    public DbSet<Label> Labels => Set<Label>();

    /// <summary>Estimates table.</summary>
    public DbSet<Estimate> Estimates => Set<Estimate>();

    /// <summary>EstimatePoints table.</summary>
    public DbSet<EstimatePoint> EstimatePoints => Set<EstimatePoint>();

    /// <summary>Issues table (Wave 2).</summary>
    public DbSet<Issue> Issues => Set<Issue>();

    /// <summary>Issue-Author M2M through table (Wave 2).</summary>
    public DbSet<IssueAssignee> IssueAssignees => Set<IssueAssignee>();

    /// <summary>Issue-Label M2M through table (Wave 2).</summary>
    public DbSet<IssueLabel> IssueLabels => Set<IssueLabel>();

    /// <summary>IssueLinks table (Wave 3).</summary>
    public DbSet<IssueLink> IssueLinks => Set<IssueLink>();

    /// <summary>IssueComments table (Wave 4).</summary>
    public DbSet<IssueComment> IssueComments => Set<IssueComment>();

    /// <summary>IssueActivities table (Wave 4).</summary>
    public DbSet<IssueActivity> IssueActivities => Set<IssueActivity>();

    // TODO(Wave 4): IntakeIssues

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        // 1) ApplyConfigurationsFromAssembly FIRST so per-entity configs (unique indexes, owned
        //    types, HasMaxLength) are in place before BaseDbContext calls
        //    ApplyTenantIsolationByDefault() (Pitfall 8 — AdjustUniqueIndexes needs the configs).
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkItemsDbContext).Assembly);

        // 2) base.OnModelCreating LAST → AppendGlobalQueryFilter<ISoftDeletable> + auto
        //    IsMultiTenant() on every non-IGlobalEntity + AdjustUniqueIndexes() widens indexes.
        base.OnModelCreating(modelBuilder);
    }
}
