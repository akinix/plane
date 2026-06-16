using YH.Modules.Multitenancy.Contracts.Dtos;
using Mediator;

namespace YH.Modules.Multitenancy.Contracts.v1.TenantProvisioning;

public sealed record GetTenantProvisioningStatusQuery(string TenantId) : IQuery<TenantProvisioningStatusDto>;