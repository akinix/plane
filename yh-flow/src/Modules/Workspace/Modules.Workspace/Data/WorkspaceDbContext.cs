using Finbuckle.MultiTenant.Abstractions;
using YH.Framework.Persistence.Context;
using YH.Framework.Shared.Multitenancy;
using YH.Framework.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using YH.Modules.Workspace.Domain;

// Namespace/type collision: the root namespace `YH.Modules.Workspace` and the entity
// `YH.Modules.Workspace.Domain.Workspace` share the "Workspace" identifier. Alias the entity so
// DbSet property types resolve unambiguously.
using WorkspaceEntity = YH.Modules.Workspace.Domain.Workspace;

namespace YH.Modules.Workspace.Data;

/// <summary>
/// Workspace module DbContext (CONTEXT core; RESEARCH §Architectural Responsibility Map).
/// </summary>
/// <remarks>
/// <b>OnModelCreating order — RESEARCH §Pitfall 6 (CRITICAL):</b>
/// <see cref="OnModelCreating"/> MUST apply per-entity configurations BEFORE delegating to
/// <c>base.OnModelCreating</c>. <c>BaseDbContext.OnModelCreating</c> runs
/// <c>ApplyTenantIsolationByDefault()</c> which iterates the model and calls
/// <c>IsMultiTenant().AdjustUniqueIndexes()</c> on every non-<c>IGlobalEntity</c>. If configs are
/// not yet applied, Finbuckle's <c>AdjustUniqueIndexes</c> has nothing to widen and the composite
/// unique indexes (e.g. <c>(TenantId, UserId)</c> on <c>WorkspaceMember</c>) silently end up
/// scoped to the wrong columns — the membership uniqueness invariant (D-04) breaks at runtime.
/// See <c>AuditDbContext.cs:48-51</c> warning comment for the precedent.
/// </remarks>
public sealed class WorkspaceDbContext : BaseDbContext
{
    public WorkspaceDbContext(
        IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor,
        DbContextOptions<WorkspaceDbContext> options,
        IOptions<DatabaseOptions> settings,
        IHostEnvironment environment)
        : base(multiTenantContextAccessor, options, settings, environment) { }

    /// <summary>
    /// Workspaces table — the rows that DEFINE tenants (each <c>Workspace.Id</c> becomes a tenant id).
    /// <see cref="WorkspaceEntity"/> is <see cref="YH.Framework.Core.Domain.IGlobalEntity"/>, so this
    /// DbSet is NOT tenant-filtered (the row IS the tenant).
    /// </summary>
    public DbSet<WorkspaceEntity> Workspaces => Set<WorkspaceEntity>();

    /// <summary>Workspace memberships (tenant-scoped).</summary>
    public DbSet<WorkspaceMember> Members => Set<WorkspaceMember>();

    /// <summary>Pending workspace invitations (tenant-scoped).</summary>
    public DbSet<WorkspaceInvitation> Invitations => Set<WorkspaceInvitation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        // 1) ApplyConfigurationsFromAssembly FIRST so per-entity configs (unique indexes, owned
        //    types, HasMaxLength) are in place before BaseDbContext calls
        //    ApplyTenantIsolationByDefault() (Pitfall 6 — AdjustUniqueIndexes needs the configs).
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkspaceDbContext).Assembly);

        // 2) base.OnModelAttribute LAST → AppendGlobalQueryFilter<ISoftDeletable> + auto
        //    IsMultiTenant() on every non-IGlobalEntity + AdjustUniqueIndexes() widens indexes.
        base.OnModelCreating(modelBuilder);
    }
}
