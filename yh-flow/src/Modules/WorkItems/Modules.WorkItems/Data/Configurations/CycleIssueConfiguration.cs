using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="CycleIssue"/>.
/// </summary>
/// <remarks>
/// <b>Unique index:</b> <c>(TenantId, CycleId, IssueId)</c> with HasFilter prevents duplicate
/// issue membership in a cycle. Widened to include TenantId by <c>AdjustUniqueIndexes()</c>.
/// </remarks>
public sealed class CycleIssueConfiguration : IEntityTypeConfiguration<CycleIssue>
{
    public void Configure(EntityTypeBuilder<CycleIssue> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("CycleIssues", WorkItemsModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        builder.Property(x => x.IssueId)
            .IsRequired();

        builder.Property(x => x.CycleId)
            .IsRequired();

        builder.Property(x => x.CreatedOnUtc)
            .IsRequired();

        // Unique index to prevent duplicate issue-cycle membership.
        builder.HasIndex(x => new { x.TenantId, x.CycleId, x.IssueId })
            .IsUnique()
            .HasDatabaseName("IX_CycleIssues_Tenant_Cycle_Issue")
            .HasFilter("[DeletedOnUtc] IS NULL");
    }
}
