using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YH.Modules.Workspace.Domain;

namespace YH.Modules.Workspace.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="WorkspaceInvitation"/> (CONTEXT D-12 / threat T-2-tenantleak).
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> same auto-<c>IsMultiTenant()</c> pattern as
/// <see cref="WorkspaceMemberConfiguration"/> — Finbuckle injects the <c>TenantId</c> column.
/// <para>
/// <b>Token hash unique (D-12):</b> accept endpoint queries by SHA-256 hash; the unique index
/// enforces collision-free resolution. The raw token never reaches the DB.
/// </para>
/// <para>
/// <b>Pending-list index:</b> the (TenantId, Accepted) composite supports the
/// "list pending invitations for this workspace" admin/query path
/// (plan 02-05 ListInvitations handler).
/// </para>
/// <para>
/// <b>Soft-delete filter (T-2-tenantleak mitigation):</b> <c>BaseDbContext</c>'s global
/// <c>AppendGlobalQueryFilter&lt;ISoftDeletable&gt;</c> hides deleted invitations — the slug
/// strategy store additionally filters <c>!IsDeleted</c> on the Workspace table itself, so a
/// deleted workspace cannot resolve to a tenant (Pitfall 6).
/// </para>
/// </remarks>
public sealed class WorkspaceInvitationConfiguration : IEntityTypeConfiguration<WorkspaceInvitation>
{
    public void Configure(EntityTypeBuilder<WorkspaceInvitation> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("WorkspaceInvitations", WorkspaceModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        builder.Property(x => x.WorkspaceId).IsRequired();

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(254); // RFC 5321 max

        builder.Property(x => x.TokenHash)
            .IsRequired()
            .HasMaxLength(64); // SHA-256 hex = 64 chars

        // D-12: hash-lookup unique index. The accept endpoint hashes the raw token and probes here.
        builder.HasIndex(x => x.TokenHash)
            .IsUnique()
            .HasDatabaseName("IX_WorkspaceInvitations_TokenHash");

        builder.Property(x => x.Role)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Accepted).IsRequired();

        // Pending-list path: "show me unresolved invitations for this workspace".
        // Accepted=false + RespondedAt=null ⇒ pending; the index lets PostgreSQL serve the
        // list from an index-only walk under the tenant filter.
        builder.HasIndex(x => new { x.TenantId, x.Accepted })
            .HasDatabaseName("IX_WorkspaceInvitations_Tenant_Accepted");

        builder.Property(x => x.Message).HasMaxLength(1000);

        builder.Property(x => x.ExpiresAt).IsRequired();

        // Audit + soft-delete columns populated by AuditableEntitySaveChangesInterceptor.
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(450);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(450);
        builder.Property(x => x.DeletedBy).HasMaxLength(450);
    }
}
