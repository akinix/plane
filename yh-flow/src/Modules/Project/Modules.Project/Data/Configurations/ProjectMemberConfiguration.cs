using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YH.Modules.Project.Domain;

namespace YH.Modules.Project.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="ProjectMember"/>.
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> <see cref="ProjectMember"/> deliberately does NOT implement
/// <see cref="YH.Framework.Core.Domain.IGlobalEntity"/>. <c>ApplyTenantIsolationByDefault()</c>
/// auto-applies <c>IsMultiTenant().AdjustUniqueIndexes()</c>, which injects the Finbuckle
/// <c>TenantId</c> column AND widens every unique index to include it. NO explicit
/// <c>IsMultiTenant()</c> call appears here.
/// <para>
/// <b>Uniqueness invariant:</b> the composite <c>(TenantId, ProjectId, UserId)</c>
/// unique index guarantees one membership row per user per project. AdjustUniqueIndexes widens
/// any single-column unique to <c>(TenantId, col)</c>, but the explicit composite here is the
/// load-bearing declaration — it survives even if the Finbuckle widening behaviour regresses.
/// </para>
/// </remarks>
public sealed class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("ProjectMembers", ProjectModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        builder.Property(x => x.ProjectId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450); // scalar cross-module userId, NO FK to Identity (D-04)

        builder.Property(x => x.Role)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.SortOrder)
            .IsRequired()
            .HasDefaultValue(65535.0);

        // CRITICAL: one active membership per (project, user). TenantId is
        // auto-added by ApplyTenantIsolationByDefault; the composite here is the load-bearing
        // invariant. AdjustUniqueIndexes() widens it consistently.
        builder.HasIndex(x => new { x.TenantId, x.ProjectId, x.UserId })
            .IsUnique()
            .HasDatabaseName("IX_ProjectMembers_Tenant_Project_User");

        // "Who is in this project?" — the hot list path.
        builder.HasIndex(x => new { x.ProjectId, x.IsActive })
            .HasDatabaseName("IX_ProjectMembers_Project_Active");

        // "What projects does this user belong to?" — cross-project membership query.
        builder.HasIndex(x => new { x.TenantId, x.UserId, x.IsActive })
            .HasDatabaseName("IX_ProjectMembers_Tenant_User_Active");

        // Audit + soft-delete columns populated by AuditableEntitySaveChangesInterceptor.
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(450);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(450);
        builder.Property(x => x.DeletedBy).HasMaxLength(450);
    }
}
