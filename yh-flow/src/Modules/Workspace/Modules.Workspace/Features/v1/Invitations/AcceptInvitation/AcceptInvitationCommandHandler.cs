using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.Workspace.Contracts.v1.Invitations.AcceptInvitation;
using YH.Modules.Workspace.Data;
using YH.Modules.Workspace.Domain;
using YH.Modules.Workspace.Services;

namespace YH.Modules.Workspace.Features.v1.Invitations.AcceptInvitation;

/// <summary>
/// Handles <see cref="AcceptInvitationCommand"/> — invitee accepts a workspace invitation by
/// presenting the raw token from the URL path (REQ-2.4, threats T-2-replay [BLOCKING] +
/// T-2-acceptdouble + T-2-acceptpublic).
/// </summary>
/// <remarks>
/// <para>
/// <b>Validation flow (D-12 state machine):</b>
/// <list type="number">
///   <item><description>Hash the inbound raw token and look the invitation up by
///   <c>TokenHash</c>.</description></item>
///   <item><description>If no match OR the invitation is no longer
///   <see cref="WorkspaceInvitation.IsValid"/> (already accepted / revoked / rejected / expired)
///   → throw <see cref="NotFoundException"/> so attackers cannot distinguish these states (no
///   enumeration surface).</description></item>
///   <item><description>Transition the invitation to Accepted + create the WorkspaceMember row
///   in the SAME SaveChanges unit of work.</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Double-accept (T-2-acceptdouble):</b> a second accept attempt on the same token fails
/// the <see cref="WorkspaceInvitation.IsValid"/> check (the first accept stamped
/// <c>RespondedAt</c>) → <see cref="NotFoundException"/>. No duplicate member row is created.
/// </para>
/// <para>
/// <b>Public-access (T-2-acceptpublic):</b> the endpoint applies plain <c>.RequireAuthorization()</c>
/// (any authenticated user). The token hash + IsValid check is the load-bearing gate — an
/// attacker without the raw token cannot enumerate invitations.
/// </para>
/// <para>
/// <b>Tenant scoping:</b> the invitation's <c>WorkspaceId</c> may live in a tenant that the
/// DbContext is not currently scoped to (the accept endpoint is top-level, not workspace-scoped).
/// We therefore disable the tenant filter when looking up the invitation by hash via
/// <see cref="IInvitationTokenService.ValidateAsync"/> (which queries by hash, not by tenant) —
/// and we materialise the workspace by id without tenant filter. The new WorkspaceMember row is
/// saved through the same DbContext; Finbuckle stamps its TenantId from the invitation's
/// WorkspaceId during SaveChanges.
/// </para>
/// </remarks>
public sealed class AcceptInvitationCommandHandler : ICommandHandler<AcceptInvitationCommand, AcceptInvitationResponse>
{
    private readonly IInvitationTokenService _tokenService;
    private readonly WorkspaceDbContext _db;

    public AcceptInvitationCommandHandler(
        IInvitationTokenService tokenService,
        WorkspaceDbContext db)
    {
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<AcceptInvitationResponse> Handle(
        AcceptInvitationCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (string.IsNullOrWhiteSpace(command.Token))
        {
            throw new NotFoundException("Invalid or expired invitation token.");
        }

        if (command.CurrentUserId == Guid.Empty)
        {
            throw new UnauthorizedException();
        }

        // Step 1 — hash the inbound raw token and look up by hash. ValidateAsync enforces
        // IsValid (rejects accepted / revoked / rejected / expired).
        var invitation = await _tokenService
            .ValidateAsync(command.Token, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Invalid or expired invitation token.");

        // Step 2 — re-attach as tracked so the Accept() transition persists. ValidateAsync
        // returned an untracked entity by design.
        var tracked = await _db.Invitations
            .FirstOrDefaultAsync(i => i.Id == invitation.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Invalid or expired invitation token.");

        // Step 3 — terminal transition (idempotent-guarded by the entity: returns false if
        // already terminal, which would mean the token was accepted in a race between steps
        // 1 and 3 — surface as NotFound to avoid leaking state).
        if (!tracked.Accept())
        {
            throw new NotFoundException("Invalid or expired invitation token.");
        }

        // Step 4 — create the new WorkspaceMember row in the SAME SaveChanges unit so the
        // membership and the invitation transition commit atomically (or roll back together).
        var member = WorkspaceMember.Create(
            workspaceId: tracked.WorkspaceId,
            userId: command.CurrentUserId.ToString(),
            role: tracked.Role,
            isActive: true);
        _db.Members.Add(member);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new AcceptInvitationResponse(member.Id, tracked.WorkspaceId);
    }
}
