using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YH.Modules.WorkItems.Contracts.Constants;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="Module"/>.
/// </summary>
/// <remarks>
/// <b>Index design:</b>
/// <list type="bullet">
///   <item><b>(TenantId, ProjectId, Name) — unique index</b> with HasFilter for soft-delete isolation.</item>
///   <item><b>(TenantId, IsDeleted, ProjectId) — tenant + soft-delete filter</b> for global queries.</item>
///   <item><b>(TenantId, ProjectId, SortOrder) — sort order index</b> for module listing.</item>
///   <item><b>(TenantId, ProjectId, ArchivedAt) — archive query index</b> for archived module queries.</item>
/// </list>
/// </remarks>
public sealed class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("Modules", WorkItemsModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        // Core fields
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(ModuleConstants.NameMaxLength); // 255

        builder.Property(x => x.Description)
            .HasMaxLength(ModuleConstants.DescriptionMaxLength); // 10000

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(ModuleConstants.StatusMaxLength) // 20
            .HasDefaultValue(ModuleConstants.DefaultStatus); // "planned"

        builder.Property(x => x.SortOrder)
            .IsRequired()
            .HasDefaultValue(ModuleConstants.DefaultSortOrder); // 65535.0

        builder.Property(x => x.ProjectId)
            .IsRequired();

        builder.Property(x => x.Version)
            .IsRequired()
            .HasDefaultValue(1);

        // Optional fields
        builder.Property(x => x.StartDate)
            .IsRequired(false);

        builder.Property(x => x.TargetDate)
            .IsRequired(false);

        builder.Property(x => x.LeadId)
            .IsRequired(false);

        builder.Property(x => x.ProgressSnapshot);

        builder.Property(x => x.LogoProps);

        builder.Property(x => x.ArchivedAt)
            .IsRequired(false);

        // Indexes ---

        // Unique index: prevents duplicate module names per project (soft-delete aware).
        builder.HasIndex(x => new { x.TenantId, x.ProjectId, x.Name })
            .IsUnique()
            .HasDatabaseName("IX_Modules_Tenant_Project_Name")
            .HasFilter("[DeletedOnUtc] IS NULL");

        // Tenant + soft-delete query index.
        builder.HasIndex(x => new { x.TenantId, x.IsDeleted, x.ProjectId })
            .HasDatabaseName("IX_Modules_Tenant_Deleted_Project");

        // Sort order index for module listing.
        builder.HasIndex(x => new { x.TenantId, x.ProjectId, x.SortOrder })
            .HasDatabaseName("IX_Modules_Tenant_Project_SortOrder");

        // Archive query index for archived module queries.
        builder.HasIndex(x => new { x.TenantId, x.ProjectId, x.ArchivedAt })
            .HasDatabaseName("IX_Modules_Tenant_Project_Archived");

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
