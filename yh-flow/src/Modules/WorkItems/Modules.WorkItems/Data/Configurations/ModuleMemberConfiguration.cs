using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Data.Configurations;

/// <summary>
/// EF Core configuration for <see cref="ModuleMember"/>.
/// </summary>
/// <remarks>
/// <b>Unique index:</b> <c>(TenantId, ModuleId, MemberId)</c> with HasFilter prevents duplicate
/// member membership in a module.
/// </remarks>
public sealed class ModuleMemberConfiguration : IEntityTypeConfiguration<ModuleMember>
{
    public void Configure(EntityTypeBuilder<ModuleMember> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("ModuleMembers", WorkItemsModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        builder.Property(x => x.MemberId)
            .IsRequired();

        builder.Property(x => x.ModuleId)
            .IsRequired();

        builder.Property(x => x.CreatedOnUtc)
            .IsRequired();

        // Unique index to prevent duplicate member-module membership.
        builder.HasIndex(x => new { x.TenantId, x.ModuleId, x.MemberId })
            .IsUnique()
            .HasDatabaseName("IX_ModuleMembers_Tenant_Module_Member")
            .HasFilter("[DeletedOnUtc] IS NULL");
    }
}
