using YH.Modules.Multitenancy.Contracts.Dtos;
using Mediator;

namespace YH.Modules.Multitenancy.Contracts.v1.GetTenantStatus;

public sealed record GetTenantStatusQuery(string TenantId) : IQuery<TenantStatusDto>;