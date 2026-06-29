using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YH.Modules.WorkItems.Contracts;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="IssueComment"/>.
/// </summary>
/// <remarks>
/// <b>Index design:</b>
/// <list type="bullet">
///   <item><b>(IssueId, CreatedOnUtc) — query index</b> for listing comments by issue in chronological order.</item>
///   <item><b>(TenantId, IsDeleted, IssueId) — tenant + soft-delete filter</b> for global queries.</item>
/// </list>
/// </remarks>
public sealed class IssueCommentConfiguration : IEntityTypeConfiguration<IssueComment>
{
    public void Configure(EntityTypeBuilder<IssueComment> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("IssueComments", WorkItemsModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        // Core fields
        builder.Property(x => x.IssueId)
            .IsRequired();

        builder.Property(x => x.CommentHtml)
            .IsRequired()
            .HasMaxLength(WorkItemsConstants.DescriptionMaxLength); // 10000

        builder.Property(x => x.CommentJson);

        builder.Property(x => x.CommentStripped)
            .HasMaxLength(WorkItemsConstants.DescriptionMaxLength); // 10000

        builder.Property(x => x.ActorId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.ParentId)
            .IsRequired(false);

        builder.Property(x => x.EditedAt);

        // Self-referencing ParentId FK (depth 1 per CONTEXT)
        builder.HasOne<IssueComment>()
            .WithMany()
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(x => new { x.IssueId, x.CreatedOnUtc })
            .HasDatabaseName("IX_IssueComments_Issue_CreatedOn");

        builder.HasIndex(x => new { x.TenantId, x.IsDeleted, x.IssueId })
            .HasDatabaseName("IX_IssueComments_Tenant_Deleted_Issue");

        // Audit + soft-delete columns
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(450);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(450);
        builder.Property(x => x.DeletedBy).HasMaxLength(450);
    }
}
