using YH.Modules.Notifications.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace YH.Modules.Notifications.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="UserNotificationPreference"/> entity.
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> <see cref="UserNotificationPreference"/> implements <see cref="YH.Framework.Core.Domain.IHasTenant"/>.
/// <c>ApplyTenantIsolationByDefault()</c> auto-applies <c>IsMultiTenant().AdjustUniqueIndexes()</c>,
/// which injects the Finbuckle <c>TenantId</c> column AND widens every unique index to include it.
/// </remarks>
public sealed class UserNotificationPreferenceConfiguration : IEntityTypeConfiguration<UserNotificationPreference>
{
    public void Configure(EntityTypeBuilder<UserNotificationPreference> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("UserNotificationPreferences", NotificationsDbContext.Schema)
            .HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.PropertyChanged).IsRequired();
        builder.Property(x => x.StateChanged).IsRequired();
        builder.Property(x => x.Comment).IsRequired();
        builder.Property(x => x.Mention).IsRequired();
        builder.Property(x => x.IssueCompleted).IsRequired();

        // One preference row per (tenant, user, workspace, project) — nullable FK columns
        // allow global, workspace-wide, and project-scoped records to coexist.
        builder.HasIndex(x => new { x.TenantId, x.UserId, x.WorkspaceId, x.ProjectId })
            .IsUnique()
            .HasDatabaseName("IX_UserNotificationPreferences_Tenant_User_Workspace_Project");
    }
}
