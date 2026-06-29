using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YH.Modules.Project.Contracts;
using YH.Modules.Project.Domain;

// Namespace/type collision: alias the entity type to disambiguate from the YH.Modules.Project namespace.
using ProjectEntity = YH.Modules.Project.Domain.Project;

namespace YH.Modules.Project.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="ProjectEntity"/>.
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> <see cref="ProjectEntity"/> deliberately does NOT implement
/// <see cref="YH.Framework.Core.Domain.IGlobalEntity"/>. <c>ApplyTenantIsolationByDefault()</c>
/// auto-applies <c>IsMultiTenant().AdjustUniqueIndexes()</c>, which injects the Finbuckle
/// <c>TenantId</c> column AND widens every unique index to include it. NO explicit
/// <c>IsMultiTenant()</c> call appears here.
/// <para>
/// <b>Uniqueness invariants:</b>
/// <list type="bullet">
///   <item><b>(TenantId, Identifier) — conditional unique index</b> with
///     <c>HasFilter("[DeletedOnUtc] IS NULL")</c>: soft-deleted rows do not block identifier
///     reuse (plan T-3-domain-03 mitigation, contrast with Slug which is epoch-modified).</item>
///   <item><b>(TenantId, Name) — conditional unique index</b> with
///     <c>HasFilter("[DeletedOnUtc] IS NULL")</c>: same release-on-delete pattern.</item>
///   <item><b>Slug — unconditional unique index</b>: <see cref="ProjectEntity.SoftDelete"/>
///     rewrites Slug with <c>__{epoch}</c> so the unique constraint never blocks reuse.</item>
/// </list>
/// </para>
/// </remarks>
public sealed class ProjectConfiguration : IEntityTypeConfiguration<ProjectEntity>
{
    public void Configure(EntityTypeBuilder<ProjectEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("Projects", ProjectModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        // Core fields
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(ProjectConstants.NameMaxLength); // 255

        builder.Property(x => x.Description)
            .HasMaxLength(ProjectConstants.DescriptionMaxLength); // 5000

        builder.Property(x => x.DescriptionText)
            .HasMaxLength(ProjectConstants.DescriptionMaxLength);

        builder.Property(x => x.DescriptionHtml)
            .HasMaxLength(ProjectConstants.DescriptionMaxLength);

        builder.Property(x => x.Network)
            .HasConversion<int>()
            .IsRequired()
            .HasDefaultValue(ProjectNetwork.Public);

        builder.Property(x => x.Identifier)
            .IsRequired()
            .HasMaxLength(ProjectConstants.IdentifierMaxLength); // 12

        // D-08: unconditional unique index on Slug — SoftDelete rewrites with __{epoch}.
        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(ProjectConstants.SlugMaxLength); // 100

        builder.HasIndex(x => x.Slug)
            .IsUnique()
            .HasDatabaseName("IX_Projects_Slug");

        // T-3-domain-03: conditional unique index on (TenantId, Identifier).
        // AdjustUniqueIndexes() widens to (TenantId, Identifier); HasFilter releases on delete.
        builder.HasIndex(x => new { x.TenantId, x.Identifier })
            .IsUnique()
            .HasDatabaseName("IX_Projects_Tenant_Identifier")
            .HasFilter("[DeletedOnUtc] IS NULL");

        // Conditional unique index on (TenantId, Name) — name release on delete.
        builder.HasIndex(x => new { x.TenantId, x.Name })
            .IsUnique()
            .HasDatabaseName("IX_Projects_Tenant_Name")
            .HasFilter("[DeletedOnUtc] IS NULL");

        // Scalar user references (NO FK to Identity per D-06)
        builder.Property(x => x.OwnerId)
            .IsRequired();

        builder.Property(x => x.ProjectLeadId);

        builder.Property(x => x.DefaultAssigneeId);

        // Optional media / icon fields
        builder.Property(x => x.Emoji)
            .HasMaxLength(64);

        builder.Property(x => x.IconProp)
            .HasMaxLength(1000);

        builder.Property(x => x.CoverImageUrl)
            .HasMaxLength(2048);

        builder.Property(x => x.LogoProps)
            .HasMaxLength(1000);

        // Timezone
        builder.Property(x => x.TimeZone)
            .IsRequired()
            .HasMaxLength(64)
            .HasDefaultValue("UTC");

        // Feature toggles
        builder.Property(x => x.ModuleViewEnabled).IsRequired();
        builder.Property(x => x.CycleViewEnabled).IsRequired();
        builder.Property(x => x.IssueViewsViewEnabled).IsRequired();
        builder.Property(x => x.PageViewEnabled).IsRequired();
        builder.Property(x => x.IntakeViewEnabled).IsRequired();
        builder.Property(x => x.GuestViewAllFeatures).IsRequired();
        builder.Property(x => x.IsTimeTrackingEnabled).IsRequired();
        builder.Property(x => x.IsIssueTypeEnabled).IsRequired();

        // Archive / close settings
        builder.Property(x => x.ArchiveIn).IsRequired();
        builder.Property(x => x.CloseIn).IsRequired();
        builder.Property(x => x.ArchivedAt);

        // External source / id (Plane compatibility)
        builder.Property(x => x.ExternalSource).HasMaxLength(256);
        builder.Property(x => x.ExternalId).HasMaxLength(256);

        // Sort order — default 65535.0 per Plane convention.
        builder.Property(x => x.SortOrder)
            .IsRequired()
            .HasDefaultValue(65535.0);

        // Active project listing index.
        builder.HasIndex(x => new { x.ArchivedAt, x.SortOrder })
            .HasDatabaseName("IX_Projects_ArchivedAt_SortOrder");

        // OwnerId performance index.
        builder.HasIndex(x => x.OwnerId)
            .HasDatabaseName("IX_Projects_Owner");

        // Audit + soft-delete columns populated by AuditableEntitySaveChangesInterceptor.
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(450);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(450);
        builder.Property(x => x.DeletedBy).HasMaxLength(450);
    }
}
