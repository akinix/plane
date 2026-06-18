using Finbuckle.MultiTenant.Abstractions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Framework.Shared.Multitenancy;
using YH.Modules.Workspace.Contracts.v1.Workspaces.DeleteWorkspace;
using YH.Modules.Workspace.Data;

namespace YH.Modules.Workspace.Features.v1.Workspaces.DeleteWorkspace;

/// <summary>
/// Handles <see cref="DeleteWorkspaceCommand"/> — soft-deletes a workspace (D-08) and invalidates
/// the Finbuckle slug→tenant cache.
/// </summary>
/// <remarks>
/// <para><b>Owner-only (D-06, threat T-2-eop-delete [BLOCKING]):</b> the endpoint already gates on
/// <c>RequireWorkspaceRole(Admin)</c>; this handler ADDITIONALLY asserts that the caller is the
/// workspace <c>OwnerId</c>. Belt-and-braces — an Admin who is not the owner still gets 403.</para>
/// <para><b>Soft delete + slug release (D-08):</b> delegates to <c>Workspace.SoftDelete(now)</c>
/// which appends <c>__{epoch}</c> to the slug, releasing the original for reuse while keeping the
/// unique index collision-free.</para>
/// <para><b>Cache invalidation (T-2-cacheinvalid, plan 02-02 wiring note):</b> calls
/// <c>IMultiTenantStore&lt;AppTenantInfo&gt;.RemoveAsync(slug)</c> after SaveChanges so the
/// <c>WorkspaceTenantStore</c> drops the cached entry. Without this, the stale cache would keep
/// resolving the deleted workspace's slug to its old tenant id for up to 30 minutes. This is the
/// load-bearing wiring note called out in 02-02-SUMMARY.md ("Workspace CRUD handlers MUST call
/// IMultiTenantStore&lt;AppTenantInfo&gt;.RemoveAsync(slug)").</para>
/// <para><b>Idempotency (T-2-softdelete-idempotent accept):</b> a second DELETE on the same slug
/// returns 404 — the soft-deleted row's slug is now suffixed with <c>__{epoch}</c>, so the
/// <c>!IsDeleted</c> predicate no longer matches it. The handler never appends a second epoch.</para>
/// </remarks>
public sealed class DeleteWorkspaceCommandHandler : ICommandHandler<DeleteWorkspaceCommand>
{
    private readonly WorkspaceDbContext _db;
    private readonly IMultiTenantStore<AppTenantInfo> _tenantStore;

    public DeleteWorkspaceCommandHandler(
        WorkspaceDbContext db,
        IMultiTenantStore<AppTenantInfo> tenantStore)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _tenantStore = tenantStore ?? throw new ArgumentNullException(nameof(tenantStore));
    }

    public async ValueTask<Unit> Handle(DeleteWorkspaceCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (string.IsNullOrWhiteSpace(command.Slug))
        {
            throw new CustomException("Slug is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        // Workspace is IGlobalEntity (no tenant filter); exclude soft-deleted so a second DELETE
        // surfaces as 404 (idempotent — threat T-2-softdelete-idempotent accept).
        var ws = await _db.Workspaces
            .FirstOrDefaultAsync(w => w.Slug == command.Slug && !w.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (ws is null)
        {
            throw new NotFoundException($"Workspace '{command.Slug}' was not found.");
        }

        // D-06 / T-2-eop-delete: only the owner may delete. Admin-but-not-owner → 403.
        if (ws.OwnerId != command.CurrentUserId)
        {
            throw new ForbiddenException("Only the workspace owner may delete the workspace.");
        }

        var now = DateTimeOffset.UtcNow;
        ws.SoftDelete(now);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // T-2-cacheinvalid + 02-02 wiring note — invalidate the Finbuckle slug→tenant cache so the
        // next resolution of this slug re-queries the Workspaces table (and finds nothing, since the
        // row is now soft-deleted with a suffixed slug). WorkspaceTenantStore.RemoveAsync is a
        // cache-eviction no-op against the Workspaces table itself.
        await _tenantStore.RemoveAsync(command.Slug).ConfigureAwait(false);

        return Unit.Value;
    }
}
