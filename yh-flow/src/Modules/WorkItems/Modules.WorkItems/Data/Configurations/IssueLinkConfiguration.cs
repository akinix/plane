using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YH.Modules.WorkItems.Contracts;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="IssueLink"/>.
/// </summary>
/// <remarks>
/// <b>Combined external-link + internal-relation:</b> <c>RelatedIssueId</c> and <c>Url</c> are both
/// optional (handler validates at least one is set). Indexes support both lookup patterns.
/// <para>
/// <b>No ISoftDeletable:</b> IssueLink is hard-deleted per Plane pattern. Cascade delete from parent Issue.
/// </para>
/// </remarks>
public sealed class IssueLinkConfiguration : IEntityTypeConfiguration<IssueLink>
{
    public void Configure(EntityTypeBuilder<IssueLink> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("IssueLinks", WorkItemsModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        // Core fields
        builder.Property(x => x.IssueId)
            .IsRequired();

        builder.Property(x => x.RelatedIssueId)
            .IsRequired(false);

        builder.Property(x => x.Url)
            .HasMaxLength(2048)
            .IsRequired(false);

        builder.Property(x => x.Title)
            .HasMaxLength(WorkItemsConstants.NameMaxLength) // 255
            .IsRequired(false);

        builder.Property(x => x.LinkType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Metadata)
            .HasMaxLength(4000)
            .IsRequired(false);

        // Cascade delete from parent Issue
        builder.HasOne<Issue>()
            .WithMany()
            .HasForeignKey(x => x.IssueId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes

        // Index on (IssueId, LinkType) for listing links by issue
        builder.HasIndex(x => new { x.IssueId, x.LinkType })
            .HasDatabaseName("IX_IssueLinks_Issue_LinkType");

        // Index on (RelatedIssueId) for reverse-link lookups
        builder.HasIndex(x => x.RelatedIssueId)
            .HasDatabaseName("IX_IssueLinks_RelatedIssue");

        // Audit columns populated by AuditableEntitySaveChangesInterceptor.
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(450);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(450);
    }
}
