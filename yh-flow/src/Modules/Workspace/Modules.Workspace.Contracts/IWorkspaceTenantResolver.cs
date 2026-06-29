namespace YH.Modules.Workspace.Contracts;

/// <summary>
/// Optional facade for downstream modules to query the currently resolved workspace id without
/// depending on Finbuckle's <c>IMultiTenantContextAccessor</c> directly (RESEARCH §No Analog).
/// <b>Not implemented in Phase 2</b> — surfaced here as a contract for future consumers (Phase 3
/// Project module may need it). YAGNI: do not implement until a real consumer arrives; modules that
/// need workspace id should read <see cref="ICurrentWorkspaceContext.CurrentWorkspaceId"/> instead.
/// </summary>
public interface IWorkspaceTenantResolver
{
    /// <summary>
    /// Returns the currently resolved workspace id for the executing request, or null when the
    /// request is on a top-level (no-slug) endpoint.
    /// </summary>
    Guid? ResolveWorkspaceId();
}
