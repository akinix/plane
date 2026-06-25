using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YH.Modules.Page.Contracts;
using YH.Modules.Page.Domain;

// Namespace/type collision: alias the entity type to disambiguate from the YH.Modules.Page namespace.
using PageEntity = YH.Modules.Page.Domain.Page;

namespace YH.Modules.Page.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="PageEntity"/>.
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> <see cref="PageEntity"/> implements <see cref="YH.Framework.Core.Domain.IGlobalEntity"/>.
/// <c>ApplyTenantIsolationByDefault()</c> skips entities implementing IGlobalEntity, so
/// NO <c>IsMultiTenant()</c> is applied — Page has no TenantId column.
/// <para>
/// <b>Self-referencing FK:</b> <see cref="PageEntity.ParentId"/> references <see cref="PageEntity.Id"/>
/// with <c>DeleteBehavior.SetNull</c> to avoid circular cascade paths.
/// </para>
/// </remarks>
public sealed class PageConfiguration : IEntityTypeConfiguration<PageEntity>
{
    public void Configure(EntityTypeBuilder<PageEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("Pages", PageModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        // Core fields
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(PageConstants.NameMaxLength); // 255

        builder.Property(x => x.DescriptionHtml)
            .HasColumnType("text");

        builder.Property(x => x.DescriptionStripped)
            .HasColumnType("text");

        builder.Property(x => x.DescriptionJson)
            .HasColumnType("text");

        builder.Property(x => x.Access)
            .HasConversion<int>()
            .IsRequired()
            .HasDefaultValue(PageAccess.Public);

        builder.Property(x => x.Color)
            .HasMaxLength(PageConstants.ColorMaxLength); // 50

        builder.Property(x => x.SortOrder)
            .IsRequired()
            .HasDefaultValue(65535.0);

        builder.Property(x => x.IsLocked)
            .HasDefaultValue(false);

        builder.Property(x => x.OwnedBy)
            .IsRequired();

        builder.Property(x => x.ProjectId)
            .IsRequired();

        builder.Property(x => x.ArchivedAt);

        // View/logo props — nvarchar(max)
        builder.Property(x => x.ViewProps)
            .HasColumnType("text");

        builder.Property(x => x.LogoProps)
            .HasColumnType("text");

        builder.Property(x => x.IsGlobal)
            .HasDefaultValue(false);

        // External source / id (Plane compatibility)
        builder.Property(x => x.ExternalSource)
            .HasMaxLength(PageConstants.ExternalSourceMaxLength);

        builder.Property(x => x.ExternalId)
            .HasMaxLength(PageConstants.ExternalIdMaxLength);

        // Self-referencing FK: ParentId → Id with SetNull to avoid circular cascade
        builder.HasOne<PageEntity>()
            .WithMany()
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(x => new { x.ProjectId, x.SortOrder })
            .HasDatabaseName("IX_Pages_Project_SortOrder");

        builder.HasIndex(x => x.ParentId)
            .HasDatabaseName("IX_Pages_ParentId");

        builder.HasIndex(x => x.ArchivedAt)
            .HasDatabaseName("IX_Pages_ArchivedAt");

        // Audit + soft-delete columns
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(450);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(450);
        builder.Property(x => x.DeletedBy).HasMaxLength(450);
    }
}