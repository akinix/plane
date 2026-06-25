using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YH.Modules.Page.Domain;

namespace YH.Modules.Page.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="PageFavorite"/> entity.
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> <see cref="PageFavorite"/> implements <see cref="YH.Framework.Core.Domain.IHasTenant"/>.
/// <c>ApplyTenantIsolationByDefault()</c> auto-applies <c>IsMultiTenant().AdjustUniqueIndexes()</c>.
/// <para>
/// <b>Uniqueness invariant:</b> the composite <c>(TenantId, PageId, UserId)</c>
/// unique index with <c>HasFilter("[DeletedOnUtc] IS NULL")</c> prevents duplicate
/// favorites for the same (page, user). Soft-deleted rows release the constraint.
/// </para>
/// </remarks>
public sealed class PageFavoriteConfiguration : IEntityTypeConfiguration<PageFavorite>
{
    public void Configure(EntityTypeBuilder<PageFavorite> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("PageFavorites", PageModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        builder.Property(x => x.PageId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(255); // scalar cross-module userId, NO FK to Identity

        builder.Property(x => x.CreatedOnUtc)
            .IsRequired();

        // Conditional unique index: one active favorite per (tenant, page, user)
        builder.HasIndex(x => new { x.TenantId, x.PageId, x.UserId })
            .IsUnique()
            .HasDatabaseName("IX_PageFavorites_Tenant_Page_User")
            .HasFilter("[DeletedOnUtc] IS NULL");

        // Performance index for "get all favorites for a page"
        builder.HasIndex(x => x.PageId)
            .HasDatabaseName("IX_PageFavorites_PageId");

        // Soft-delete columns
        builder.Property(x => x.DeletedBy).HasMaxLength(450);
    }
}