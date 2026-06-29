using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="ModuleIssue"/>.
/// </summary>
/// <remarks>
/// <b>Unique index:</b> <c>(TenantId, ModuleId, IssueId)</c> with HasFilter prevents duplicate
/// issue membership in a module.
/// </remarks>
public sealed class ModuleIssueConfiguration : IEntityTypeConfiguration<ModuleIssue>
{
    public void Configure(EntityTypeBuilder<ModuleIssue> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("ModuleIssues", WorkItemsModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        builder.Property(x => x.IssueId)
            .IsRequired();

        builder.Property(x => x.ModuleId)
            .IsRequired();

        builder.Property(x => x.CreatedOnUtc)
            .IsRequired();

        // Unique index to prevent duplicate issue-module membership.
        builder.HasIndex(x => new { x.TenantId, x.ModuleId, x.IssueId })
            .IsUnique()
            .HasDatabaseName("IX_ModuleIssues_Tenant_Module_Issue")
            .HasFilter("[DeletedOnUtc] IS NULL");
    }
}
