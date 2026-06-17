using Mediator;

namespace YH.Modules.Identity.Contracts.v1.ApiTokens;

public sealed record RevokeApiTokenCommand(Guid TokenId) : ICommand<bool>;
