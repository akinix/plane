using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.Workspace.Contracts.v1.Invitations.RejectInvitation;
using YH.Modules.Workspace.Data;
using YH.Modules.Workspace.Services;

namespace YH.Modules.Workspace.Features.v1.Invitations.RejectInvitation;

/// <summary>
/// Handles <see cref="RejectInvitationCommand"/> — invitee rejects a workspace invitation by
/// presenting the raw token from the URL path (REQ-2.4, threat T-2-replay [BLOCKING]).
/// </summary>
/// <remarks>
/// <para>
/// Stamps <see cref="YH.Modules.Workspace.Domain.WorkspaceInvitation.RespondedAt"/>; subsequent
/// validation of the same token returns null. No member row is created.
/// </para>
/// <para>
/// Returns <c>Success=false</c> for an unknown/invalid token rather than throwing — the invitee
/// clicking a stale invitation link should see a clean "no longer available" message, not a 404
/// stack trace. The handler therefore treats token-not-found as a soft failure.
/// </para>
/// </remarks>
public sealed class RejectInvitationCommandHandler : ICommandHandler<RejectInvitationCommand, RejectInvitationResponse>
{
    private readonly IInvitationTokenService _tokenService;
    private readonly WorkspaceDbContext _db;

    public RejectInvitationCommandHandler(
        IInvitationTokenService tokenService,
        WorkspaceDbContext db)
    {
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<RejectInvitationResponse> Handle(
        RejectInvitationCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (string.IsNullOrWhiteSpace(command.Token))
        {
            return new RejectInvitationResponse(Success: false);
        }

        var invitation = await _tokenService
            .ValidateAsync(command.Token, cancellationToken)
            .ConfigureAwait(false);

        if (invitation is null)
        {
            // Stale / already-responded / expired — soft fail.
            return new RejectInvitationResponse(Success: false);
        }

        // CR-01 (02-REVIEW.md:75): reject handler 同源缺陷同步修复——invitation.Id 查找租户无关。
        // 顶层 reject 端点 DbContext 作用域 ≠ 邀请所属 workspace，必须 IgnoreQueryFilters 才能找到邀请。
        var tracked = await _db.Invitations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(i => i.Id == invitation.Id, cancellationToken)
            .ConfigureAwait(false);

        if (tracked is null)
        {
            return new RejectInvitationResponse(Success: false);
        }

        tracked.Reject();
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new RejectInvitationResponse(Success: true);
    }
}
