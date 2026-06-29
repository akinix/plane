using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YH.Modules.WorkItems.Contracts;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="EstimatePoint"/>.
/// </summary>
/// <remarks>
/// <b>No ISoftDeletable:</b> <c>EstimatePoint</c> is a child entity of <see cref="Estimate"/> and
/// does not implement <see cref="YH.Framework.Core.Domain.ISoftDeletable"/>. Deletion is handled
/// by the parent <c>EstimateConfiguration</c> cascade.
/// <para>
/// <b>Unique index on (EstimateId, Key):</b> Ensures no duplicate sort keys within an estimate system.
/// </para>
/// </remarks>
public sealed class EstimatePointConfiguration : IEntityTypeConfiguration<EstimatePoint>
{
    public void Configure(EntityTypeBuilder<EstimatePoint> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("EstimatePoints", WorkItemsModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        // Core fields
        builder.Property(x => x.EstimateId)
            .IsRequired();

        builder.Property(x => x.Key)
            .IsRequired();

        builder.Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.SortOrder)
            .IsRequired()
            .HasDefaultValue(WorkItemsConstants.DefaultSortOrder); // 65535.0

        // Unique index on (EstimateId, Key) — no duplicate keys per estimate.
        builder.HasIndex(x => new { x.EstimateId, x.Key })
            .IsUnique()
            .HasDatabaseName("IX_EstimatePoints_Estimate_Key");

        // Query index on (EstimateId, SortOrder) for ordered point listing.
        builder.HasIndex(x => new { x.EstimateId, x.SortOrder })
            .HasDatabaseName("IX_EstimatePoints_Estimate_SortOrder");

        // Audit columns populated by AuditableEntitySaveChangesInterceptor.
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(450);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(450);
    }
}
