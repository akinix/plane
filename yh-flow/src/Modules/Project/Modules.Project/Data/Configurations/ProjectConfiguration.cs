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
/// <b>Uniqueness invariants:</b> <see cref="ProjectEntity.Slug"/> and <see cref="ProjectEntity.Identifier"/>
/// each have a unique index that is widened to <c>(TenantId, Slug)</c> / <c>(TenantId, Identifier)</c>
/// by <c>AdjustUniqueIndexes()</c>, enforcing per-workspace uniqueness. On soft delete the values
/// are rewritten with <c>__{epoch}</c> (see <see cref="ProjectEntity.SoftDelete"/>), so the unique
/// constraint never blocks reuse of a released slug/identifier (D-08 pattern).
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

        builder.Property(x => x.Network)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Identifier)
            .IsRequired()
            .HasMaxLength(ProjectConstants.IdentifierMaxLength); // 12

        // D-08: unique on active slug. SoftDelete(now) rewrites Slug with __{epoch}.
        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(ProjectConstants.SlugMaxLength); // 100

        builder.HasIndex(x => x.Slug)
            .IsUnique()
            .HasDatabaseName("IX_Projects_Slug");

        // D-08: unique on active identifier. SoftDelete(now) rewrites with __{epoch}.
        builder.HasIndex(x => x.Identifier)
            .IsUnique()
            .HasDatabaseName("IX_Projects_Identifier");

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

        // Sort order index — for listing projects in user-defined order within a tenant.
        builder.Property(x => x.SortOrder);
        builder.HasIndex(x => x.SortOrder)
            .HasDatabaseName("IX_Projects_SortOrder");

        // Active project listing path: "show me active projects for this workspace".
        // ArchivedAt is null for active projects; the index covers the common tenant-scoped query.
        builder.HasIndex(x => new { x.ArchivedAt, x.SortOrder })
            .HasDatabaseName("IX_Projects_ArchivedAt_SortOrder");

        // Audit + soft-delete columns populated by AuditableEntitySaveChangesInterceptor.
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(450);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(450);
        builder.Property(x => x.DeletedBy).HasMaxLength(450);
    }
}
