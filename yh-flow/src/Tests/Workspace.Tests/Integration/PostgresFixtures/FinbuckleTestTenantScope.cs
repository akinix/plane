using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using YH.Framework.Shared.Multitenancy;

namespace YH.Tests.Workspace.Integration.PostgresFixtures;

/// <summary>
/// RAII helper that temporarily switches the Finbuckle <c>MultiTenantContext</c> for a block of
/// test code, then restores the previous context on dispose (Wave 6 / plan 02-07).
/// </summary>
/// <remarks>
/// <para>
/// <b>Purpose:</b> simulates the runtime state transitions of a top-level endpoint (e.g.
/// <c>GET /api/v1/users/me/workspaces/</c> or <c>POST /api/v1/workspaces/invitations/{token}/accept/</c>),
/// where Finbuckle's <c>MultiTenantContext.TenantInfo</c> changes between the caller's current
/// tenant and a workspace tenant resolved from the request body/path.
/// </para>
/// <para>
/// <b>Design note (Finbuckle 10.1.0 API):</b>
/// <c>Finbuckle.MultiTenant.Abstractions.IMultiTenantContextSetter.MultiTenantContext</c> is a
/// set-only property by design (per the library's XML doc "implementation detail, not intended for
/// general use"). To read the previous value we pair it with the concrete
/// <see cref="AsyncLocalMultiTenantContextAccessor{AppTenantInfo}"/> registered in
/// <see cref="WorkspacePostgresFixture"/>, whose <c>MultiTenantContext</c> is read/write (it backs
/// onto an <c>AsyncLocal&lt;T&gt;</c> field). The same instance is registered under all three Finbuckle
/// accessor interfaces in <see cref="WorkspacePostgresFixture.BuildServiceProvider"/> so either DI
/// slot resolves to the shared AsyncLocal slot.
/// </para>
/// <para>
/// <b>Usage (02-08 reuse):</b>
/// <code>
/// var setter = fixture.Services.GetRequiredService&lt;IMultiTenantContextSetter&gt;();
/// var accessor = fixture.Services.GetRequiredService&lt;AsyncLocalMultiTenantContextAccessor&lt;AppTenantInfo&gt;&gt;();
/// using (new FinbuckleTestTenantScope(accessor, setter, workspaceA))
/// {
///     await db.Members.AddAsync(WorkspaceMember.Create(workspaceA.Id, userId, role: 20));
///     await db.SaveChangesAsync();
/// }
/// </code>
/// </para>
/// </remarks>
internal sealed class FinbuckleTestTenantScope : IDisposable
{
    private readonly AsyncLocalMultiTenantContextAccessor<AppTenantInfo> _accessor;
    private readonly IMultiTenantContextSetter _setter;
    private readonly IMultiTenantContext? _previous;

    /// <summary>
    /// Captures the current <c>MultiTenantContext</c> from <paramref name="accessor"/> and replaces
    /// it (via <paramref name="setter"/>) with a context bound to <paramref name="newTenant"/>.
    /// </summary>
    /// <param name="accessor">Shared AsyncLocal accessor — get path (read previous).</param>
    /// <param name="setter">Shared setter — set path (write new + restore previous on dispose).</param>
    /// <param name="newTenant">Tenant to activate for the scope of this block.</param>
    public FinbuckleTestTenantScope(
        AsyncLocalMultiTenantContextAccessor<AppTenantInfo> accessor,
        IMultiTenantContextSetter setter,
        AppTenantInfo newTenant)
    {
        _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        _setter = setter ?? throw new ArgumentNullException(nameof(setter));
        ArgumentNullException.ThrowIfNull(newTenant);
        _previous = _accessor.MultiTenantContext;
        _setter.MultiTenantContext = new MultiTenantContext<AppTenantInfo>(newTenant);
    }

    public void Dispose()
    {
        _setter.MultiTenantContext = _previous!; // _previous may be null only on first-ever scope; restoring null is the correct baseline.
        GC.SuppressFinalize(this);
    }
}
