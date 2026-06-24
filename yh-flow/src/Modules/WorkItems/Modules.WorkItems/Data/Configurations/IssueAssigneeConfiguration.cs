using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="IssueAssignee"/>.
/// </summary>
/// <remarks>
/// <b>Unique index:</b> <c>(TenantId, IssueId, AssigneeId)</c> prevents duplicate assignee entries.
/// Widened to include TenantId by <c>AdjustUniqueIndexes()</c> in <c>BaseDbContext</c>.
/// </remarks>
public sealed class IssueAssigneeConfiguration : IEntityTypeConfiguration<IssueAssignee>
{
    public void Configure(EntityTypeBuilder<IssueAssignee> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("IssueAssignees", WorkItemsModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        builder.Property(x => x.IssueId)
            .IsRequired();

        builder.Property(x => x.AssigneeId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.CreatedOnUtc)
            .IsRequired();

        // Unique index to prevent duplicate assignees per issue.
        builder.HasIndex(x => new { x.TenantId, x.IssueId, x.AssigneeId })
            .IsUnique()
            .HasDatabaseName("IX_IssueAssignees_Tenant_Issue_Assignee");
    }
}
