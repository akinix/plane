using Finbuckle.MultiTenant.Abstractions;
using YH.Framework.Persistence.Context;
using YH.Framework.Shared.Multitenancy;
using YH.Framework.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using YH.Modules.Project.Domain;

// Namespace/type collision: the root namespace `YH.Modules.Project` and the entity
// `YH.Modules.Project.Domain.Project` share the "Project" identifier. Alias the entity so
// DbSet property types resolve unambiguously.
using ProjectEntity = YH.Modules.Project.Domain.Project;

namespace YH.Modules.Project.Data;

/// <summary>
/// Project module DbContext.
/// </summary>
/// <remarks>
/// <b>OnModelCreating order — RESEARCH Pitfall 6 (CRITICAL):</b>
/// <see cref="OnModelCreating"/> MUST apply per-entity configurations BEFORE delegating to
/// <c>base.OnModelCreating</c>. <c>BaseDbContext.OnModelCreating</c> runs
/// <c>ApplyTenantIsolationByDefault()</c> which iterates the model and calls
/// <c>IsMultiTenant().AdjustUniqueIndexes()</c> on every non-<c>IGlobalEntity</c>. If configs are
/// not yet applied, Finbuckle's <c>AdjustUniqueIndexes</c> has nothing to widen and the composite
/// unique indexes (e.g. <c>(TenantId, Slug)</c>) silently end up scoped to the wrong columns.
/// <para>
/// See <c>WorkspaceDbContext.cs:53-65</c> for the authoritative pattern.
/// </para>
/// </remarks>
public sealed class ProjectDbContext : BaseDbContext
{
    public ProjectDbContext(
        IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor,
        DbContextOptions<ProjectDbContext> options,
        IOptions<DatabaseOptions> settings,
        IHostEnvironment environment)
        : base(multiTenantContextAccessor, options, settings, environment) { }

    /// <summary>Projects table (tenant-scoped — each project belongs to a workspace/tenant).</summary>
    public DbSet<ProjectEntity> Projects => Set<ProjectEntity>();

    /// <summary>Project memberships (tenant-scoped).</summary>
    public DbSet<ProjectMember> Members => Set<ProjectMember>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        // 1) ApplyConfigurationsFromAssembly FIRST so per-entity configs (unique indexes, owned
        //    types, HasMaxLength) are in place before BaseDbContext calls
        //    ApplyTenantIsolationByDefault() (Pitfall 6 — AdjustUniqueIndexes needs the configs).
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProjectDbContext).Assembly);

        // 2) base.OnModelCreating LAST → AppendGlobalQueryFilter<ISoftDeletable> + auto
        //    IsMultiTenant() on every non-IGlobalEntity + AdjustUniqueIndexes() widens indexes.
        base.OnModelCreating(modelBuilder);
    }
}
