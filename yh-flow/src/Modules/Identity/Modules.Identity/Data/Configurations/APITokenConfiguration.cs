using YH.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace YH.Modules.Identity.Data.Configurations;

public class APITokenConfiguration : IEntityTypeConfiguration<APIToken>
{
    public void Configure(EntityTypeBuilder<APIToken> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("ApiTokens", IdentityModuleConstants.SchemaName)
            .HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(t => t.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(t => t.TokenHash)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(t => t.Prefix)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(t => t.TenantId)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(t => t.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique hash for key lookup
        builder.HasIndex(t => t.TokenHash).IsUnique();

        // By user
        builder.HasIndex(t => t.UserId);

        // Prefix display lookup
        builder.HasIndex(t => t.Prefix);

        // Active keys per user
        builder.HasIndex(t => new { t.UserId, t.IsActive });
    }
}
