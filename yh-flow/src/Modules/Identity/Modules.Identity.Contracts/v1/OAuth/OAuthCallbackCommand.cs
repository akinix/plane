using Mediator;
using YH.Modules.Identity.Contracts.DTOs;

namespace YH.Modules.Identity.Contracts.v1.OAuth;

public sealed record OAuthCallbackCommand(string Code, string? State) : ICommand<PlaneAuthResponse>;
