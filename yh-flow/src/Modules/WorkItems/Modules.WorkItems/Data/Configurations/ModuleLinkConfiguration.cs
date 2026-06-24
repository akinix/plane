using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="ModuleLink"/>.
/// </summary>
/// <remarks>
/// <b>Index design:</b>
/// <list type="bullet">
///   <item><b>(TenantId, IsDeleted, ModuleId) — query index</b> for module link queries.</item>
/// </list>
/// </remarks>
public sealed class ModuleLinkConfiguration : IEntityTypeConfiguration<ModuleLink>
{
    public void Configure(EntityTypeBuilder<ModuleLink> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("ModuleLinks", WorkItemsModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Url)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(x => x.Metadata)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.ModuleId)
            .IsRequired();

        builder.Property(x => x.CreatedOnUtc)
            .IsRequired();

        // Query index for module link queries.
        builder.HasIndex(x => new { x.TenantId, x.IsDeleted, x.ModuleId })
            .HasDatabaseName("IX_ModuleLinks_Tenant_Deleted_Module");
    }
}
