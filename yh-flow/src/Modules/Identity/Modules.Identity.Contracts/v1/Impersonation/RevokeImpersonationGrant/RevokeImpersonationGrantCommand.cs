using Mediator;

namespace YH.Modules.Identity.Contracts.v1.Impersonation.RevokeImpersonationGrant;

public sealed record RevokeImpersonationGrantCommand(
    Guid GrantId,
    string? Reason)
    : ICommand<ImpersonationGrantDto>;
