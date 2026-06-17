using YH.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace YH.Modules.Identity.Data;

public class OAuthProviderSettingsConfiguration : IEntityTypeConfiguration<OAuthProviderSettings>
{
    public void Configure(EntityTypeBuilder<OAuthProviderSettings> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("OAuthProviderSettings", IdentityModuleConstants.SchemaName);

        // NOT multitenant — global config, IGlobalEntity auto-skipped by ApplyTenantIsolationByDefault

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ProviderName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.ClientId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.ClientSecret)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(s => s.CallbackUrl)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(s => s.Scope)
            .HasMaxLength(512);

        // Provider name must be unique (one config per provider type)
        builder.HasIndex(s => s.ProviderName).IsUnique();
    }
}
