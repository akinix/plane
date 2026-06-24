using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YH.Modules.WorkItems.Contracts.Constants;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="Cycle"/>.
/// </summary>
/// <remarks>
/// <b>Index design:</b>
/// <list type="bullet">
///   <item><b>(ProjectId, SortOrder) — query index</b> for cycle listing sorted by sort order.</item>
///   <item><b>(TenantId, IsDeleted, ProjectId) — tenant + soft-delete filter</b> for global queries.</item>
/// </list>
/// </remarks>
public sealed class CycleConfiguration : IEntityTypeConfiguration<Cycle>
{
    public void Configure(EntityTypeBuilder<Cycle> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("Cycles", WorkItemsModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        // Core fields
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(CycleConstants.NameMaxLength); // 255

        builder.Property(x => x.Description)
            .HasMaxLength(CycleConstants.DescriptionMaxLength); // 10000

        builder.Property(x => x.SortOrder)
            .IsRequired()
            .HasDefaultValue(CycleConstants.DefaultSortOrder); // 65535.0

        builder.Property(x => x.ProjectId)
            .IsRequired();

        builder.Property(x => x.Timezone)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue(CycleConstants.DefaultTimezone); // UTC

        builder.Property(x => x.Version)
            .IsRequired()
            .HasDefaultValue(1);

        // Optional date fields
        builder.Property(x => x.StartDate)
            .IsRequired(false);

        builder.Property(x => x.EndDate)
            .IsRequired(false);

        // Optional string fields
        builder.Property(x => x.ExternalSource)
            .HasMaxLength(500);

        builder.Property(x => x.ExternalId)
            .HasMaxLength(255);

        builder.Property(x => x.ProgressSnapshot);

        builder.Property(x => x.LogoProps);

        // Indexes ---

        // Query index on (ProjectId, SortOrder) for cycle listing.
        builder.HasIndex(x => new { x.ProjectId, x.SortOrder })
            .HasDatabaseName("IX_Cycles_Project_SortOrder");

        // Tenant + soft-delete query index.
        builder.HasIndex(x => new { x.TenantId, x.IsDeleted, x.ProjectId })
            .HasDatabaseName("IX_Cycles_Tenant_Deleted_Project");

        // Audit + soft-delete columns populated by AuditableEntitySaveChangesInterceptor.
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(450);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(450);
        builder.Property(x => x.DeletedBy).HasMaxLength(450);

        // Timestamp for optimistic concurrency
        builder.Property(x => x.Version)
            .IsConcurrencyToken();
    }
}
