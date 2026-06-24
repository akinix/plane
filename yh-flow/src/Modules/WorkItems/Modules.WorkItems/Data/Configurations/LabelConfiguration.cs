using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YH.Modules.WorkItems.Contracts;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="Label"/>.
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> <see cref="Label"/> does NOT implement <see cref="YH.Framework.Core.Domain.IGlobalEntity"/>.
/// <c>ApplyTenantIsolationByDefault()</c> auto-applies multi-tenant widening.
/// <para>
/// <b>Self-referencing ParentId:</b> The FK <c>(ParentId → Id)</c> uses <c>OnDelete(SetNull)</c> to
/// prevent orphan issues when a parent label is deleted. The index on <c>(ProjectId, ParentId)</c>
/// supports hierarchical queries.
/// </para>
/// </remarks>
public sealed class LabelConfiguration : IEntityTypeConfiguration<Label>
{
    public void Configure(EntityTypeBuilder<Label> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("Labels", WorkItemsModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        // Core fields
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(WorkItemsConstants.NameMaxLength); // 255

        builder.Property(x => x.Color)
            .HasMaxLength(WorkItemsConstants.ColorMaxLength); // 7

        builder.Property(x => x.ParentId);

        // Self-referencing FK with SetNull to prevent orphan issues on parent delete.
        builder.HasOne<Label>()
            .WithMany()
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.ProjectId)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.SortOrder)
            .IsRequired()
            .HasDefaultValue(WorkItemsConstants.DefaultSortOrder); // 65535.0

        // T-4-scaffold-03: conditional unique index on (TenantId, ProjectId, Name).
        // AdjustUniqueIndexes() widens to (TenantId, ProjectId, Name); HasFilter releases on delete.
        builder.HasIndex(x => new { x.TenantId, x.ProjectId, x.Name })
            .IsUnique()
            .HasDatabaseName("IX_Labels_Tenant_Project_Name")
            .HasFilter("[DeletedOnUtc] IS NULL");

        // Query index on (ProjectId, ParentId) for hierarchical queries.
        builder.HasIndex(x => new { x.ProjectId, x.ParentId })
            .HasDatabaseName("IX_Labels_Project_Parent");

        // Audit + soft-delete columns populated by AuditableEntitySaveChangesInterceptor.
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(450);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(450);
        builder.Property(x => x.DeletedBy).HasMaxLength(450);
    }
}
