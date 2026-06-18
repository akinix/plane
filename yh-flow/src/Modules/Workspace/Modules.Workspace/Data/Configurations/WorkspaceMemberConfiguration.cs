using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YH.Modules.Workspace.Domain;

namespace YH.Modules.Workspace.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="WorkspaceMember"/> (CONTEXT D-04 / D-11 / threat T-2-eop).
/// </summary>
/// <remarks>
/// <b>Tenant isolation (RESEARCH §Pitfall 6):</b> <see cref="WorkspaceMember"/> deliberately does
/// NOT implement <see cref="YH.Framework.Core.Domain.IGlobalEntity"/>. <c>ApplyTenantIsolationByDefault()</c>
/// auto-applies <c>IsMultiTenant().AdjustUniqueIndexes()</c>, which injects the Finbuckle
/// <c>TenantId</c> column AND widens every unique index to include it. NO explicit
/// <c>IsMultiTenant()</c> call appears here (contrast <c>AuditRecordConfiguration.cs:13</c> —
/// Auditing predates the auto-default and keeps its explicit call for legacy reasons).
/// <para>
/// <b>Uniqueness invariant (T-2-eop mitigation):</b> the composite <c>(TenantId, UserId)</c>
/// unique index guarantees one membership row per user per workspace. AdjustUniqueIndexes widens
/// any single-column unique to <c>(TenantId, col)</c>, but the explicit composite here is the
/// load-bearing declaration — it survives even if the Finbuckle widening behaviour regresses.
/// </para>
/// </remarks>
public sealed class WorkspaceMemberConfiguration : IEntityTypeConfiguration<WorkspaceMember>
{
    public void Configure(EntityTypeBuilder<WorkspaceMember> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("WorkspaceMembers", WorkspaceModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        builder.Property(x => x.WorkspaceId)
            .IsRequired(); // scalar copy of Finbuckle TenantId (Guid) for cross-workspace queries

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450); // scalar cross-module userId, NO FK to Identity (D-04)

        builder.Property(x => x.Role)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        // CRITICAL (D-04 / T-2-eop): one active membership per (workspace, user). TenantId is
        // auto-added by ApplyTenantIsolationByDefault; the composite here is the load-bearing
        // invariant. AdjustUniqueIndexes() widens it consistently.
        builder.HasIndex(x => new { x.TenantId, x.UserId })
            .IsUnique()
            .HasDatabaseName("IX_WorkspaceMembers_Tenant_User");

        // "Who is in this workspace?" is the hot list path. TenantId is filter-driven; WorkspaceId
        // adds a secondary dimension for the cross-workspace admin view.
        builder.HasIndex(x => new { x.TenantId, x.WorkspaceId, x.IsActive })
            .HasDatabaseName("IX_WorkspaceMembers_Tenant_Workspace_Active");

        // Audit + soft-delete columns populated by AuditableEntitySaveChangesInterceptor.
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(450);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(450);
        builder.Property(x => x.DeletedBy).HasMaxLength(450);
    }
}
