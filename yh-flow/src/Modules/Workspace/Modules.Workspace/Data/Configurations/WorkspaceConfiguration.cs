using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YH.Modules.Workspace.Domain;

// Namespace/type collision: the root namespace `YH.Modules.Workspace` and the entity
// `YH.Modules.Workspace.Domain.Workspace` share the "Workspace" identifier. Alias the entity.
using WorkspaceEntity = YH.Modules.Workspace.Domain.Workspace;

namespace YH.Modules.Workspace.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="WorkspaceEntity"/> (CONTEXT D-08 / threat T-2-isolation [BLOCKING]).
/// </summary>
/// <remarks>
/// <b>IGlobalEntity guard (CRITICAL — RESEARCH Pitfall 6 / T-2-isolation):</b>
/// <see cref="WorkspaceEntity"/> is <c>IGlobalEntity</c> — its <c>ClrType</c> satisfies
/// <c>typeof(IGlobalEntity).IsAssignableFrom(...)</c> at <c>TenantIsolationExtensions.cs:41</c>,
/// so <c>ApplyTenantIsolationByDefault()</c> skips it. NO <c>IsMultiTenant()</c> call appears
/// here. <b>Adding <c>IsMultiTenant()</c> to this configuration would re-introduce a TenantId
/// column and break the slug-strategy resolution chain (resolve-tenant-by-querying-tenant-filtered-table = cycle).</b>
/// </remarks>
public sealed class WorkspaceConfiguration : IEntityTypeConfiguration<WorkspaceEntity>
{
    public void Configure(EntityTypeBuilder<WorkspaceEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("Workspaces", WorkspaceModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(80); // Plane name max 80

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(WorkspaceModuleConstants.SlugMaxLength); // D-09: 48

        // D-08: unique on the ACTIVE slug. SoftDelete(now) rewrites Slug with __{epoch} so the
        // original value is released for reuse — combined with this index, two rows can never hold
        // the same active slug simultaneously (threat T-2-softdelete mitigation).
        builder.HasIndex(x => x.Slug)
            .IsUnique()
            .HasDatabaseName("IX_Workspaces_Slug");

        builder.Property(x => x.OwnerId)
            .IsRequired(); // scalar Guid, NO FK to Identity (D-06)

        builder.Property(x => x.Logo)
            .HasMaxLength(2048);

        builder.Property(x => x.OrganizationSize)
            .HasMaxLength(20); // Plane max 20

        builder.Property(x => x.TimeZone)
            .IsRequired()
            .HasMaxLength(64)
            .HasDefaultValue("UTC");

        builder.Property(x => x.BackgroundColor)
            .IsRequired()
            .HasMaxLength(32)
            .HasDefaultValue("#000000");

        // Audit + soft-delete columns are populated by AuditableEntitySaveChangesInterceptor.
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(450);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(450);
        builder.Property(x => x.DeletedBy).HasMaxLength(450);
    }
}
