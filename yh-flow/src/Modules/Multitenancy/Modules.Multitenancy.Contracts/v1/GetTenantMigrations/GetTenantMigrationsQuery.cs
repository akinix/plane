using YH.Modules.Multitenancy.Contracts.Dtos;
using Mediator;

namespace YH.Modules.Multitenancy.Contracts.v1.GetTenantMigrations;

public sealed record GetTenantMigrationsQuery : IQuery<IReadOnlyCollection<TenantMigrationStatusDto>>;