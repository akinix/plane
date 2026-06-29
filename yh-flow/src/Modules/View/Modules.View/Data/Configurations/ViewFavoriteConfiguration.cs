using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YH.Modules.View.Domain;

namespace YH.Modules.View.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="ViewFavorite"/> entity.
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> <see cref="ViewFavorite"/> implements <see cref="YH.Framework.Core.Domain.IHasTenant"/>.
/// <c>ApplyTenantIsolationByDefault()</c> auto-applies <c>IsMultiTenant().AdjustUniqueIndexes()</c>.
/// <para>
/// <b>Uniqueness invariant:</b> the composite <c>(TenantId, ViewId, UserId)</c>
/// unique index with <c>HasFilter("[DeletedOnUtc] IS NULL")</c> prevents duplicate
/// favorites for the same (view, user). Soft-deleted rows release the constraint.
/// </para>
/// </remarks>
public sealed class ViewFavoriteConfiguration : IEntityTypeConfiguration<ViewFavorite>
{
    public void Configure(EntityTypeBuilder<ViewFavorite> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("ViewFavorites", ViewModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        builder.Property(x => x.ViewId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(255); // scalar cross-module userId, NO FK to Identity

        builder.Property(x => x.CreatedOnUtc)
            .IsRequired();

        // Conditional unique index: one active favorite per (tenant, view, user)
        builder.HasIndex(x => new { x.TenantId, x.ViewId, x.UserId })
            .IsUnique()
            .HasDatabaseName("IX_ViewFavorites_Tenant_View_User")
            .HasFilter("[DeletedOnUtc] IS NULL");

        // Performance index for "get all favorites for a view"
        builder.HasIndex(x => x.ViewId)
            .HasDatabaseName("IX_ViewFavorites_ViewId");

        // Soft-delete columns
        builder.Property(x => x.DeletedBy).HasMaxLength(450);
    }
}