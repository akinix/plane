using Finbuckle.MultiTenant.Abstractions;
using YH.Framework.Persistence.Context;
using YH.Framework.Shared.Multitenancy;
using YH.Framework.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using YH.Modules.View.Domain;

// Namespace/type collision: the root namespace `YH.Modules.View` and the entity
// `YH.Modules.View.Domain.View` share the "View" identifier. Alias the entity so
// DbSet property types resolve unambiguously.
using ViewEntity = YH.Modules.View.Domain.View;

namespace YH.Modules.View.Data;

/// <summary>
/// View module DbContext.
/// </summary>
/// <remarks>
/// <b>OnModelCreating order — RESEARCH Pitfall 6 (CRITICAL):</b>
/// <see cref="OnModelCreating"/> MUST apply per-entity configurations BEFORE delegating to
/// <c>base.OnModelCreating</c>. <c>BaseDbContext.OnModelCreating</c> runs
/// <c>ApplyTenantIsolationByDefault()</c> which iterates the model and calls
/// <c>IsMultiTenant().AdjustUniqueIndexes()</c> on every non-<c>IGlobalEntity</c>. If configs are
/// not yet applied, Finbuckle's <c>AdjustUniqueIndexes</c> has nothing to widen and the composite
/// unique indexes silently end up scoped to the wrong columns.
/// <para>
/// See <c>ProjectDbContext.cs:53-65</c> for the authoritative pattern.
/// </para>
/// </remarks>
public sealed class ViewDbContext : BaseDbContext
{
    public ViewDbContext(
        IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor,
        DbContextOptions<ViewDbContext> options,
        IOptions<DatabaseOptions> settings,
        IHostEnvironment environment)
        : base(multiTenantContextAccessor, options, settings, environment) { }

    /// <summary>Views table (global entity — no TenantId column, tenant resolved via route slug).</summary>
    public DbSet<ViewEntity> Views => Set<ViewEntity>();

    /// <summary>View favorites (tenant-scoped).</summary>
    public DbSet<ViewFavorite> ViewFavorites => Set<ViewFavorite>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        // 1) ApplyConfigurationsFromAssembly FIRST so per-entity configs (unique indexes, owned
        //    types, HasMaxLength) are in place before BaseDbContext calls
        //    ApplyTenantIsolationByDefault() (Pitfall 6 — AdjustUniqueIndexes needs the configs).
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ViewDbContext).Assembly);

        // 2) base.OnModelCreating LAST → AppendGlobalQueryFilter<ISoftDeletable> + auto
        //    IsMultiTenant() on every non-IGlobalEntity + AdjustUniqueIndexes() widens indexes.
        base.OnModelCreating(modelBuilder);
    }
}