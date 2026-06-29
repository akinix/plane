using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YH.Modules.WorkItems.Contracts;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="Estimate"/>.
/// </summary>
/// <remarks>
/// <b>EstimatePoint cascade:</b> <c>HasMany(e => e.EstimatePoints).WithOne(ep => ep.Estimate)</c>
/// with <c>OnDelete(DeleteBehavior.Cascade)</c>. When an Estimate is deleted (soft or hard), all
/// its EstimatePoints are cascade-deleted. EstimatePoint itself does NOT implement ISoftDeletable,
/// so cascade is the only deletion path.
/// </remarks>
public sealed class EstimateConfiguration : IEntityTypeConfiguration<Estimate>
{
    public void Configure(EntityTypeBuilder<Estimate> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("Estimates", WorkItemsModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        // Core fields
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(WorkItemsConstants.NameMaxLength); // 255

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(20); // "points" or "categories"

        builder.Property(x => x.ProjectId)
            .IsRequired();

        builder.Property(x => x.IsLastUsed)
            .IsRequired();

        // Navigation: Estimate has many EstimatePoints cascade deleted.
        builder.HasMany(e => e.EstimatePoints)
            .WithOne(ep => ep.Estimate)
            .HasForeignKey(ep => ep.EstimateId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.TenantId, x.ProjectId })
            .HasDatabaseName("IX_Estimates_Tenant_Project");

        builder.HasIndex(x => new { x.ProjectId, x.IsLastUsed })
            .HasDatabaseName("IX_Estimates_Project_IsLastUsed");

        // Audit + soft-delete columns populated by AuditableEntitySaveChangesInterceptor.
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(450);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(450);
        builder.Property(x => x.DeletedBy).HasMaxLength(450);
    }
}
