using Mediator;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using YH.Modules.Workspace.Configuration;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Contracts.v1.Invitations.CreateInvitation;
using YH.Modules.Workspace.Services;

namespace YH.Modules.Workspace.Features.v1.Invitations.CreateInvitation;

/// <summary>
/// Handles <see cref="CreateInvitationCommand"/> — generates the invitation token (CSPRNG raw +
/// SHA-256 hash, D-12), persists ONLY the hash, and returns the raw token exactly once.
/// </summary>
/// <remarks>
/// <para>
/// <b>Token lifecycle (D-12):</b> the raw token exists ONLY in this handler's response. The
/// persistence layer (<see cref="IInvitationTokenService.CreateAsync"/>) writes only the
/// <c>TokenHash</c> to <see cref="YH.Modules.Workspace.Domain.WorkspaceInvitation.TokenHash"/>.
/// Threats T-2-token [BLOCKING] + T-2-tokenleak mitigated.
/// </para>
/// <para>
/// <b>Notification placeholder (D-10):</b> Phase 11 will register a concrete
/// <c>INotificationService</c> implementation that emails the invitee. Phase 2 leaves it
/// unregistered (null) — the admin receives the invitation link in the HTTP response and is
/// responsible for delivering it. Threat T-2-notifier: accept (Phase 11 audit).
/// </para>
/// </remarks>
public sealed class CreateInvitationCommandHandler : ICommandHandler<CreateInvitationCommand, CreateInvitationResponse>
{
    private readonly IInvitationTokenService _tokenService;
    private readonly WorkspaceTokenOptions _options;

    public CreateInvitationCommandHandler(
        IInvitationTokenService tokenService,
        IOptions<WorkspaceTokenOptions> options)
    {
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
    }

    public async ValueTask<CreateInvitationResponse> Handle(
        CreateInvitationCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.WorkspaceId == Guid.Empty)
        {
            throw new ArgumentException("Workspace id is required.", nameof(command));
        }

        if (string.IsNullOrWhiteSpace(command.Slug))
        {
            throw new ArgumentException("Workspace slug is required.", nameof(command));
        }

        if (command.Role is WorkspaceRole.None)
        {
            command.Role = WorkspaceRole.Member;
        }

        var ttlDays = _options.InvitationTokenTtlDays > 0 ? _options.InvitationTokenTtlDays : 7;
        var (invitation, rawToken) = await _tokenService
            .CreateAsync(
                workspaceId: command.WorkspaceId,
                email: command.Email,
                role: command.Role,
                ttlDays: ttlDays,
                message: command.Message,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        // D-10 INotificationService placeholder — Phase 11 wires the concrete email dispatcher.
        // Phase 2 does not register INotificationService; the admin delivers the link manually.
        return new CreateInvitationResponse(invitation.Id, rawToken, command.Slug);
    }
}
