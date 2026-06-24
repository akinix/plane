using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="IntakeIssue"/>.
/// </summary>
/// <remarks>
/// <b>Index design:</b>
/// <list type="bullet">
///   <item><b>(ProjectId, Status) — query index</b> for listing intake issues by status.</item>
///   <item><b>(TenantId, ProjectId) — tenant + project filter.</b></item>
/// </list>
/// </remarks>
public sealed class IntakeIssueConfiguration : IEntityTypeConfiguration<IntakeIssue>
{
    public void Configure(EntityTypeBuilder<IntakeIssue> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("IntakeIssues", WorkItemsModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        // Core fields
        builder.Property(x => x.IssueId)
            .IsRequired();

        builder.Property(x => x.ProjectId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired()
            .HasDefaultValue(IntakeIssueStatus.Pending);

        builder.Property(x => x.SnoozedTill);

        builder.Property(x => x.DuplicateToIssueId);

        builder.Property(x => x.Source)
            .HasMaxLength(50)
            .HasDefaultValue("IN_APP");

        // Navigation: IntakeIssue has one Issue
        builder.HasOne(x => x.Issue)
            .WithMany()
            .HasForeignKey(x => x.IssueId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.ProjectId, x.Status })
            .HasDatabaseName("IX_IntakeIssues_Project_Status");

        builder.HasIndex(x => new { x.TenantId, x.ProjectId })
            .HasDatabaseName("IX_IntakeIssues_Tenant_Project");

        // Audit columns
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(450);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(450);
    }
}
