using YH.Modules.Identity.Contracts.DTOs;
using Mediator;

namespace YH.Modules.Identity.Contracts.v1.Auth;

public sealed record PlaneSignInCommand(string Email, string Password) : ICommand<PlaneAuthResponse>;
