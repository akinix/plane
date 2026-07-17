using YH.Modules.Notifications.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace YH.Modules.Notifications.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="EmailNotificationLog"/> entity.
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> <see cref="EmailNotificationLog"/> implements <see cref="YH.Framework.Core.Domain.IHasTenant"/>.
/// <c>ApplyTenantIsolationByDefault()</c> auto-applies <c>IsMultiTenant().AdjustUniqueIndexes()</c>.
/// </remarks>
public sealed class EmailNotificationLogConfiguration : IEntityTypeConfiguration<EmailNotificationLog>
{
    public void Configure(EntityTypeBuilder<EmailNotificationLog> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("EmailNotificationLogs", NotificationsDbContext.Schema)
            .HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.ReceiverId).IsRequired();
        builder.Property(x => x.TriggeredById).IsRequired();
        builder.Property(x => x.EntityName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Data).HasColumnType("jsonb");
        builder.Property(x => x.Entity).HasMaxLength(128).IsRequired();
        builder.Property(x => x.OldValue).HasMaxLength(2048);
        builder.Property(x => x.NewValue).HasMaxLength(2048);

        // Query pattern: "recent log entries for a receiver"
        builder.HasIndex(x => new { x.ReceiverId, x.SentAt })
            .HasDatabaseName("IX_EmailNotificationLogs_Receiver_Sent");
    }
}
