using YH.Modules.Identity.Contracts.DTOs;
using Mediator;

namespace YH.Modules.Identity.Contracts.v1.Auth;

public sealed record PlaneSignUpCommand(
    string Email,
    string Password,
    string? FirstName,
    string? LastName) : ICommand<PlaneAuthResponse>;
