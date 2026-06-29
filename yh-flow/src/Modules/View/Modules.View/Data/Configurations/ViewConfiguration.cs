using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YH.Modules.View.Contracts.Constants;
using YH.Modules.View.Domain;

// Namespace/type collision: alias the entity type to disambiguate from the YH.Modules.View namespace.
using ViewEntity = YH.Modules.View.Domain.View;

namespace YH.Modules.View.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="ViewEntity"/>.
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> <see cref="ViewEntity"/> implements <see cref="YH.Framework.Core.Domain.IGlobalEntity"/>.
/// <c>ApplyTenantIsolationByDefault()</c> skips entities implementing IGlobalEntity, so
/// NO <c>IsMultiTenant()</c> is applied — View has no TenantId column.
/// <para>
/// <b>JSON columns:</b> Query, Filters, DisplayFilters, DisplayProperties, RichFilters, LogoProps
/// are stored as PostgreSQL jsonb columns for flexible schema-less storage.
/// </para>
/// </remarks>
public sealed class ViewConfiguration : IEntityTypeConfiguration<ViewEntity>
{
    public void Configure(EntityTypeBuilder<ViewEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("Views", ViewModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        // Core fields
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(ViewConstants.NameMaxLength); // 255

        builder.Property(x => x.Description)
            .HasColumnType("text"); // nvarchar(max)

        // JSONB columns
        builder.Property(x => x.Query)
            .HasColumnType("jsonb");

        builder.Property(x => x.Filters)
            .HasColumnType("jsonb");

        builder.Property(x => x.DisplayFilters)
            .HasColumnType("jsonb");

        builder.Property(x => x.DisplayProperties)
            .HasColumnType("jsonb");

        builder.Property(x => x.RichFilters)
            .HasColumnType("jsonb");

        builder.Property(x => x.LogoProps)
            .HasColumnType("jsonb");

        // Enum to int conversion
        builder.Property(x => x.Access)
            .HasConversion<int>()
            .IsRequired()
            .HasDefaultValue(ViewAccess.Public);

        builder.Property(x => x.SortOrder)
            .IsRequired()
            .HasDefaultValue(65535.0);

        builder.Property(x => x.IsLocked)
            .HasDefaultValue(false);

        builder.Property(x => x.OwnedBy)
            .IsRequired();

        builder.Property(x => x.ProjectId)
            .IsRequired(false); // nullable = workspace-level view

        builder.Property(x => x.ArchivedAt);

        // Indexes
        builder.HasIndex(x => new { x.ProjectId, x.SortOrder })
            .HasDatabaseName("IX_Views_Project_SortOrder");

        builder.HasIndex(x => x.ProjectId)
            .HasDatabaseName("IX_Views_ProjectId");

        builder.HasIndex(x => x.ArchivedAt)
            .HasDatabaseName("IX_Views_ArchivedAt");

        builder.HasIndex(x => x.OwnedBy)
            .HasDatabaseName("IX_Views_OwnedBy");

        // Audit + soft-delete columns
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(450);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(450);
        builder.Property(x => x.DeletedBy).HasMaxLength(450);
    }
}