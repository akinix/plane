using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="IssueLabel"/>.
/// </summary>
/// <remarks>
/// <b>Unique index:</b> <c>(TenantId, IssueId, LabelId)</c> prevents duplicate label entries.
/// Widened to include TenantId by <c>AdjustUniqueIndexes()</c> in <c>BaseDbContext</c>.
/// </remarks>
public sealed class IssueLabelConfiguration : IEntityTypeConfiguration<IssueLabel>
{
    public void Configure(EntityTypeBuilder<IssueLabel> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("IssueLabels", WorkItemsModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        builder.Property(x => x.IssueId)
            .IsRequired();

        builder.Property(x => x.LabelId)
            .IsRequired();

        builder.Property(x => x.CreatedOnUtc)
            .IsRequired();

        // Unique index to prevent duplicate labels per issue.
        builder.HasIndex(x => new { x.TenantId, x.IssueId, x.LabelId })
            .IsUnique()
            .HasDatabaseName("IX_IssueLabels_Tenant_Issue_Label");
    }
}
