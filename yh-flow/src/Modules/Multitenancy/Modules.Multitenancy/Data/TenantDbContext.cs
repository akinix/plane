using Finbuckle.MultiTenant.EntityFrameworkCore.Stores;
using YH.Framework.Shared.Multitenancy;
using YH.Modules.Multitenancy.Domain;
using YH.Modules.Multitenancy.Provisioning;
using Microsoft.EntityFrameworkCore;

namespace YH.Modules.Multitenancy.Data;

public class TenantDbContext : EFCoreStoreDbContext<AppTenantInfo>
{
    public const string Schema = "tenant";

    public TenantDbContext(DbContextOptions<TenantDbContext> options)
        : base(options)
    {
    }

    public DbSet<TenantProvisioning> TenantProvisionings => Set<TenantProvisioning>();

    public DbSet<TenantProvisioningStep> TenantProvisioningSteps => Set<TenantProvisioningStep>();

    public DbSet<TenantTheme> TenantThemes => Set<TenantTheme>();

    public DbSet<TenantExpiryNotice> TenantExpiryNotices => Set<TenantExpiryNotice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantDbContext).Assembly);
    }
}