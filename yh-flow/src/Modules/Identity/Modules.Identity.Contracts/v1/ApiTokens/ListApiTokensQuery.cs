using Mediator;
using YH.Modules.Identity.Contracts.DTOs;

namespace YH.Modules.Identity.Contracts.v1.ApiTokens;

public sealed record ListApiTokensQuery : IQuery<IReadOnlyList<APITokenDto>>;
