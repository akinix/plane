using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YH.Modules.Page.Domain;

namespace YH.Modules.Page.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="ProjectPage"/> bridge entity.
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> <see cref="ProjectPage"/> implements <see cref="YH.Framework.Core.Domain.IHasTenant"/>.
/// <c>ApplyTenantIsolationByDefault()</c> auto-applies <c>IsMultiTenant().AdjustUniqueIndexes()</c>,
/// which injects the Finbuckle <c>TenantId</c> column AND widens every unique index to include it.
/// <para>
/// <b>Uniqueness invariant:</b> the composite <c>(TenantId, ProjectId, PageId)</c>
/// unique index with <c>HasFilter("[DeletedOnUtc] IS NULL")</c> guarantees one active
/// association per (project, page). Soft-deleted rows release the constraint.
/// </para>
/// </remarks>
public sealed class ProjectPageConfiguration : IEntityTypeConfiguration<ProjectPage>
{
    public void Configure(EntityTypeBuilder<ProjectPage> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("ProjectPages", PageModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        builder.Property(x => x.PageId)
            .IsRequired();

        builder.Property(x => x.ProjectId)
            .IsRequired();

        // Conditional unique index: one active association per (tenant, project, page)
        builder.HasIndex(x => new { x.TenantId, x.ProjectId, x.PageId })
            .IsUnique()
            .HasDatabaseName("IX_ProjectPages_Tenant_Project_Page")
            .HasFilter("[DeletedOnUtc] IS NULL");

        // Soft-delete columns
        builder.Property(x => x.DeletedBy).HasMaxLength(450);
    }
}