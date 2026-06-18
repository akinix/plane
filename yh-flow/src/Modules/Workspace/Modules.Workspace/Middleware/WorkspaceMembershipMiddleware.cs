using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Context;
using YH.Framework.Shared.Multitenancy;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Data;

namespace YH.Modules.Workspace.Middleware;

/// <summary>
/// D-02 membership-populating middleware — the SINGLE writer of
/// <see cref="ICurrentWorkspaceContext"/> per request (threat T-2-memberskip).
/// </summary>
/// <remarks>
/// <para>
/// Runs AFTER Finbuckle's <c>UseMultiTenant</c> (so <see cref="IMultiTenantContextAccessor{T}"/>
/// already carries the resolved <c>AppTenantInfo</c> — populated by <c>WorkspaceSlugStrategy</c> +
/// <c>WorkspaceTenantStore</c>) and AFTER authentication (so <see cref="ICurrentUser"/> is populated).
/// <see cref="WorkspaceModule"/> is <c>[FshModule(Order=200)]</c>, which guarantees its
/// <c>ConfigureMiddleware</c> runs AFTER Identity/Multitenancy's auth and tenant-resolution
/// middleware, but BEFORE Auditing (300). The slug strategy's <c>Priority</c> = -100 additionally
/// ensures the slug strategy outranks Phase 1's claim/header strategies at request-resolution time.
/// </para>
/// <para>
/// <b>Per-request flow (D-02):</b>
/// <list type="number">
///   <item>Read <c>tenantInfo</c> from the Finbuckle accessor. If null (top-level endpoint without
///   <c>{slug}</c>), SKIP <c>SetContext</c> — the request is user-scoped, not workspace-scoped.</item>
///   <item>Parse <c>tenantInfo.Id</c> as the workspace Guid (the WorkspaceTenantStore stamps
///   <c>AppTenantInfo.Id</c> = workspace row's Guid string).</item>
///   <item>Resolve the current user via <see cref="ICurrentUser"/>. If unauthenticated, SKIP
///   (anonymous request cannot be a member).</item>
///   <item>Query <see cref="WorkspaceDbContext.Members"/> for the user's active membership.
///   <see cref="WorkspaceDbContext.Members"/> is tenant-filtered by
///   <c>ApplyTenantIsolationByDefault</c> (Pitfall 6) so the query is doubly protected against
///   cross-workspace leakage — the filter scopes it to the resolved tenant even if the
///   <c>userId</c> predicate is somehow wrong.</item>
///   <item>Call <see cref="ICurrentWorkspaceContext.SetContext"/> with workspaceId, slug, and role
///   (or <c>null</c> for non-members). Downstream
///   <c>RequireWorkspaceRoleAuthorizationHandler</c> reads the populated role WITHOUT a DB hit.</item>
/// </list>
/// </para>
/// <para>
/// <b>Cache invalidation note (T-2-cacheinvalid, plan 02-04):</b> this middleware only READS
/// the slug→tenant cache via <c>WorkspaceTenantStore</c>. Workspace CRUD handlers in 02-04 MUST
/// call <c>IMultiTenantStore&lt;AppTenantInfo&gt;.RemoveAsync(slug)</c> after any rename or delete
/// to invalidate the cache entry — otherwise this middleware resolves a stale workspace.
/// </para>
/// </remarks>
public sealed class WorkspaceMembershipMiddleware(
    IMultiTenantContextAccessor<AppTenantInfo> tenantAccessor,
    ICurrentUser currentUser,
    WorkspaceDbContext db,
    ICurrentWorkspaceContext workspaceContext) : IMiddleware
{
    /// <summary>
    /// Populates <see cref="ICurrentWorkspaceContext"/> based on the resolved tenant + authenticated
    /// user, then continues the pipeline.
    /// </summary>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var tenantInfo = tenantAccessor.MultiTenantContext?.TenantInfo;

        // Top-level endpoint (no {slug} → tenantInfo null) OR the slug did not resolve to a
        // workspace (store returned null) → request is user-scoped, NOT workspace-scoped.
        // Leave ICurrentWorkspaceContext null-valued; downstream [RequireWorkspaceRole] must fail.
        // ICurrentUser.GetUserId() returns Guid.Empty for anonymous — guard with IsAuthenticated
        // to avoid spurious SetContext calls on anonymous requests.
        if (tenantInfo is not null
            && Guid.TryParse(tenantInfo.Id, out var workspaceId)
            && currentUser.IsAuthenticated())
        {
            var userId = currentUser.GetUserId();
            // Tenant-filtered query (ApplyTenantIsolationByDefault) — doubly protected:
            // even if userId predicate is wrong, the filter scopes to the resolved tenant.
            var member = await db.Members.AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == userId.ToString() && m.IsActive, context.RequestAborted)
                .ConfigureAwait(false);

            workspaceContext.SetContext(
                workspaceId,
                tenantInfo.Identifier,
                member is null ? null : (WorkspaceRole)member.Role);
        }

        await next(context).ConfigureAwait(false);
    }
}
