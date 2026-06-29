using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="IssueActivity"/>.
/// </summary>
/// <remarks>
/// <b>Index design (T-4-activity-02):</b>
/// <list type="bullet">
///   <item><b>(IssueId, Epoch) — query index</b> for listing activities by issue in chronological order.</item>
///   <item><b>(IssueId, Verb) — query index</b> for filtering by verb (e.g. only "updated" records).</item>
///   <item><b>(TenantId, IssueId) — tenant + issue filter</b> for multi-tenant isolation.</item>
/// </list>
/// </remarks>
public sealed class IssueActivityConfiguration : IEntityTypeConfiguration<IssueActivity>
{
    public void Configure(EntityTypeBuilder<IssueActivity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("IssueActivities", WorkItemsModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        // Core fields
        builder.Property(x => x.IssueId)
            .IsRequired();

        builder.Property(x => x.Verb)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Field)
            .HasMaxLength(100);

        builder.Property(x => x.OldValue);

        builder.Property(x => x.NewValue);

        builder.Property(x => x.Comment);

        builder.Property(x => x.ActorId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.IssueCommentId)
            .IsRequired(false);

        builder.Property(x => x.Epoch)
            .IsRequired();

        // Indexes
        builder.HasIndex(x => new { x.IssueId, x.Epoch })
            .HasDatabaseName("IX_IssueActivities_Issue_Epoch");

        builder.HasIndex(x => new { x.IssueId, x.Verb })
            .HasDatabaseName("IX_IssueActivities_Issue_Verb");

        builder.HasIndex(x => new { x.TenantId, x.IssueId })
            .HasDatabaseName("IX_IssueActivities_Tenant_Issue");

        // Audit columns
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(450);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(450);
    }
}
