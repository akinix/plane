using YH.Modules.Auditing.Contracts.Dtos;
using Mediator;

namespace YH.Modules.Auditing.Contracts.v1.GetAuditById;

public sealed record GetAuditByIdQuery(Guid Id) : IQuery<AuditDetailDto>;