using YH.Modules.Identity.Contracts.DTOs;
using Mediator;

namespace YH.Modules.Identity.Contracts.v1.Sessions.GetMySessions;

public sealed record GetMySessionsQuery : IQuery<List<UserSessionDto>>;