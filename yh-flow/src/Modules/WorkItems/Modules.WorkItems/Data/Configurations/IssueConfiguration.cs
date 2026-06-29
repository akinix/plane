using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YH.Modules.WorkItems.Contracts;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="Issue"/>.
/// </summary>
/// <remarks>
/// <b>Index design (Plane issue.py Meta):</b>
/// <list type="bullet">
///   <item><b>(ProjectId, SequenceId) — conditional unique index</b> ensures sequence IDs are unique
///     per project. HasFilter excludes soft-deleted rows so a deleted Issue's sequence_id is reusable
///     by the service (aligned with Plane behavior).</item>
///   <item><b>(ProjectId, StateId) — query index</b> for state-listing performance.</item>
///   <item><b>(ProjectId, Priority) — query index</b> for priority-based filtering.</item>
///   <item><b>(ProjectId, ParentId) — conditional query index</b> for parent-child queries.
///     HasFilter excludes null ParentId values (most issues are root-level).</item>
///   <item><b>(TenantId, IsDeleted, ProjectId) — tenant + soft-delete filter</b> for global queries.</item>
/// </list>
/// </remarks>
public sealed class IssueConfiguration : IEntityTypeConfiguration<Issue>
{
    public void Configure(EntityTypeBuilder<Issue> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("Issues", WorkItemsModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        // Backing fields for private navigation lists
        builder.Navigation(x => x.Assignees)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Labels)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Core fields
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(WorkItemsConstants.NameMaxLength); // 255

        builder.Property(x => x.Priority)
            .IsRequired()
            .HasMaxLength(WorkItemsConstants.PriorityMaxLength) // 10
            .HasDefaultValue("none");

        builder.Property(x => x.SequenceId)
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .IsRequired()
            .HasDefaultValue(WorkItemsConstants.DefaultSortOrder); // 65535.0

        builder.Property(x => x.ProjectId)
            .IsRequired();

        builder.Property(x => x.DescriptionHtml)
            .HasMaxLength(WorkItemsConstants.DescriptionMaxLength); // 10000

        builder.Property(x => x.DescriptionStripped)
            .HasMaxLength(WorkItemsConstants.DescriptionMaxLength); // 10000

        // Optional FKs
        builder.Property(x => x.ParentId)
            .IsRequired(false);

        builder.Property(x => x.StateId)
            .IsRequired(false);

        builder.Property(x => x.EstimatePointId)
            .IsRequired(false);

        // Date mappings
        builder.Property(x => x.StartDate);
        builder.Property(x => x.TargetDate);
        builder.Property(x => x.CompletedAt);
        builder.Property(x => x.ArchivedAt);

        // Flags
        builder.Property(x => x.IsDraft)
            .IsRequired()
            .HasDefaultValue(false);

        // Self-referencing ParentId FK (depth 1 per CONTEXT)
        builder.HasOne<Issue>()
            .WithMany()
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Navigation: Issue has many IssueAssignees (cascade delete)
        builder.HasMany(x => x.Assignees)
            .WithOne()
            .HasForeignKey(a => a.IssueId)
            .OnDelete(DeleteBehavior.Cascade);

        // Navigation: Issue has many IssueLabels (cascade delete)
        builder.HasMany(x => x.Labels)
            .WithOne()
            .HasForeignKey(l => l.IssueId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes ---

        // T-4-issue-04: Unique index on (ProjectId, SequenceId) — sequence_id unique per project.
        // HasFilter excludes soft-deleted rows so sequence_id can be reused.
        builder.HasIndex(x => new { x.ProjectId, x.SequenceId })
            .IsUnique()
            .HasDatabaseName("IX_Issues_Project_Sequence")
            .HasFilter("[DeletedOnUtc] IS NULL");

        // Query index on (ProjectId, StateId) for state listing performance.
        builder.HasIndex(x => new { x.ProjectId, x.StateId })
            .HasDatabaseName("IX_Issues_Project_State");

        // Query index on (ProjectId, Priority) for priority filtering.
        builder.HasIndex(x => new { x.ProjectId, x.Priority })
            .HasDatabaseName("IX_Issues_Project_Priority");

        // Conditional query index on (ProjectId, ParentId) for parent-child queries.
        builder.HasIndex(x => new { x.ProjectId, x.ParentId })
            .HasDatabaseName("IX_Issues_Project_Parent")
            .HasFilter("[ParentId] IS NOT NULL");

        // Tenant + soft-delete query index.
        builder.HasIndex(x => new { x.TenantId, x.IsDeleted, x.ProjectId })
            .HasDatabaseName("IX_Issues_Tenant_Deleted_Project");

        // Analytics query index on (TenantId, ProjectId, CreatedOnUtc, StateId)
        // Covers the most common analytics query pattern: filter by tenant + project,
        // filter by creation date range, group by state.
        builder.HasIndex(x => new { x.TenantId, x.ProjectId, x.CreatedOnUtc, x.StateId })
            .HasDatabaseName("IX_Issues_Tenant_Project_CreatedAt_StateId")
            .HasFilter("[DeletedOnUtc] IS NULL");

        // Audit + soft-delete columns populated by AuditableEntitySaveChangesInterceptor.
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(450);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(450);
        builder.Property(x => x.DeletedBy).HasMaxLength(450);
    }
}
