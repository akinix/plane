namespace YH.Modules.Workspace.Contracts;

/// <summary>
/// Outbound notification abstraction for workspace invitations (CONTEXT D-10).
/// <b>Phase 11 placeholder</b> — Phase 2 only persists invitation records and returns invitation
/// links to the caller; actual email dispatch is deferred to Phase 11 (Hangfire + MailKit).
/// The <c>CreateInvitation</c> handler will resolve this service but short-circuit when the
/// implementation is null/unavailable, so Phase 2 functionality is not blocked by the absence of a
/// notification backend. Phase 11 will register a concrete implementation that delivers emails via
/// the Notifications module.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Notify an invitee of a pending workspace invitation. Phase 11 implementation will send the
    /// email via Hangfire-scheduled MailKit job; Phase 2 callers should treat this as fire-and-forget.
    /// </summary>
    /// <param name="email">Invitee email address.</param>
    /// <param name="workspaceId">Owning workspace id.</param>
    /// <param name="slug">Workspace slug (used to build the invitation URL on the frontend).</param>
    /// <param name="token">Raw invitation token (frontend appends to URL, e.g. <c>/{slug}/invitation?token=...</c>).</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendInvitationNotificationAsync(string email, Guid workspaceId, string slug, string token, CancellationToken ct);
}
