using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YH.Modules.WorkItems.Contracts;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="State"/>.
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> <see cref="State"/> does NOT implement <see cref="YH.Framework.Core.Domain.IGlobalEntity"/>.
/// <c>ApplyTenantIsolationByDefault()</c> auto-applies <c>IsMultiTenant().AdjustUniqueIndexes()</c>,
/// which injects the Finbuckle <c>TenantId</c> column AND widens every unique index to include it.
/// <para>
/// <b>Uniqueness invariants:</b>
/// <list type="bullet">
///   <item><b>(TenantId, ProjectId, Name) — conditional unique index</b> with
///     <c>HasFilter("[DeletedOnUtc] IS NULL")</c>: soft-deleted rows do not block name reuse.
///     Widened to (TenantId, ProjectId, Name) by AdjustUniqueIndexes.</item>
///   <item><b>(ProjectId, Group) — query index</b> for state listing grouped by group.</item>
///   <item><b>(ProjectId, IsDefault) — query index</b> for default state lookup.</item>
/// </list>
/// </para>
/// </remarks>
public sealed class StateConfiguration : IEntityTypeConfiguration<State>
{
    public void Configure(EntityTypeBuilder<State> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("States", WorkItemsModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        // Core fields
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(WorkItemsConstants.NameMaxLength); // 255

        builder.Property(x => x.Color)
            .HasMaxLength(WorkItemsConstants.ColorMaxLength); // 7

        builder.Property(x => x.Group)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.ProjectId)
            .IsRequired();

        builder.Property(x => x.IsDefault)
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .IsRequired()
            .HasDefaultValue(WorkItemsConstants.DefaultSortOrder); // 65535.0

        // T-4-scaffold-02: conditional unique index on (TenantId, ProjectId, Name).
        // AdjustUniqueIndexes() widens to (TenantId, ProjectId, Name); HasFilter releases on delete.
        builder.HasIndex(x => new { x.TenantId, x.ProjectId, x.Name })
            .IsUnique()
            .HasDatabaseName("IX_States_Tenant_Project_Name")
            .HasFilter("[DeletedOnUtc] IS NULL");

        // Query index on (ProjectId, Group) for state listing by group.
        builder.HasIndex(x => new { x.ProjectId, x.Group })
            .HasDatabaseName("IX_States_Project_Group");

        // Query index on (ProjectId, IsDefault) for default state lookup.
        builder.HasIndex(x => new { x.ProjectId, x.IsDefault })
            .HasDatabaseName("IX_States_Project_IsDefault");

        // Audit + soft-delete columns populated by AuditableEntitySaveChangesInterceptor.
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(450);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(450);
        builder.Property(x => x.DeletedBy).HasMaxLength(450);
    }
}
