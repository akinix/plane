namespace YH.Modules.Workspace.Configuration;

/// <summary>
/// Workspace token configuration (CONTEXT D-12, threat T-2-ttl). Bound from the
/// <c>Workspace</c> configuration section in <c>WorkspaceModule.ConfigureServices</c>.
/// </summary>
/// <remarks>
/// <para><b>TTL default (RESEARCH A3):</b> <see cref="InvitationTokenTtlDays"/> defaults to 7 days
/// — matches Plane's invitation window. Configurable via <c>Workspace:InvitationTokenTtlDays</c>
/// so deployments can shorten/lengthen without code changes.</para>
/// </remarks>
public sealed class WorkspaceTokenOptions
{
    /// <summary>
    /// Invitation token lifetime in days. Used by <c>InvitationTokenService.CreateAsync</c> when
    /// the caller does not specify a TTL. Default 7 days per RESEARCH A3.
    /// </summary>
    public int InvitationTokenTtlDays { get; init; } = 7;
}
