using System.Reflection;
using Finbuckle.MultiTenant.Abstractions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Framework.Shared.Multitenancy;
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
/// <b>Tenant scoping (CR-01 / 02-REVIEW.md §CR-01):</b> the invitation's <c>WorkspaceId</c> may
/// live in a tenant that the DbContext is NOT currently scoped to — the accept endpoint is
/// top-level (<c>POST /api/v1/workspaces/invitations/{token}/accept/</c>), not workspace-scoped,
/// so Finbuckle's auto-applied <c>TenantId</c> filter would silently drop every invitation row
/// belonging to a DIFFERENT tenant than the caller's current tenant. The fix has two parts:
/// <list type="bullet">
///   <item><description>Look up the invitation by hash via <see cref="IInvitationTokenService.ValidateAsync"/>
///   AND re-attach by id using <c>.IgnoreQueryFilters()</c> — the hash is globally unique (256-bit
///   CSPRNG, T-2-token) and the id lookup is tenant-agnostic by construction.</description></item>
///   <item><description>Stamp the new <c>WorkspaceMember</c> row's <c>TenantId</c> shadow property
///   correctly during <c>SaveChangesAsync</c>. <see cref="YH.Framework.Persistence.Context.BaseDbContext"/>
///   runs in <c>TenantNotSetMode=Overwrite</c>, which uses the current
///   <c>MultiTenantContext.TenantInfo</c> to stamp <c>TenantId</c>. The handler therefore injects
///   <see cref="IMultiTenantContextSetter"/> (DI-registered, same resolution path as
///   <c>FshJobActivator.cs:40</c>) and wraps the SaveChanges call in a try/finally that temporarily
///   switches <c>MultiTenantContext.TenantInfo</c> to the invitation's workspace tenant, then
///   restores the previous context in <c>finally</c>.</description></item>
/// </list>
/// The setter is injected explicitly (NOT obtained by casting the accessor to the setter
/// interface) to avoid a single-point-of-failure NullReferenceException if the accessor
/// implementation does not also implement the setter interface — BLOCKER 2 mitigation.
/// </para>
/// </remarks>
public sealed class AcceptInvitationCommandHandler : ICommandHandler<AcceptInvitationCommand, AcceptInvitationResponse>
{
    private readonly IInvitationTokenService _tokenService;
    private readonly WorkspaceDbContext _db;
    private readonly IMultiTenantContextSetter _multiTenantContextSetter;
    private readonly IMultiTenantContextAccessor<AppTenantInfo> _multiTenantContextAccessor;

    // Cached reflection accessor for MultiTenantDbContext.TenantInfo. The concrete class declares
    // a public setter (verified via reflection at runtime) even though IMultiTenantDbContext only
    // exposes a getter. Cached statically so the per-call reflection cost is a single delegate
    // invocation. Used by the CR-01 SaveChanges-tenant-switch in Handle().
    private static readonly Lazy<Func<Finbuckle.MultiTenant.EntityFrameworkCore.IMultiTenantDbContext, ITenantInfo?>> _tenantInfoGet =
        new(BuildTenantInfoGetter, LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<Action<WorkspaceDbContext, ITenantInfo>> _tenantInfoSet =
        new(BuildTenantInfoSetter, LazyThreadSafetyMode.ExecutionAndPublication);
    private static Func<Finbuckle.MultiTenant.EntityFrameworkCore.IMultiTenantDbContext, ITenantInfo?> BuildTenantInfoGetter()
    {
        var prop = typeof(Finbuckle.MultiTenant.EntityFrameworkCore.IMultiTenantDbContext).GetProperty("TenantInfo")
            ?? throw new InvalidOperationException("IMultiTenantDbContext.TenantInfo getter not found.");
        return prop.GetMethod!.CreateDelegate<Func<Finbuckle.MultiTenant.EntityFrameworkCore.IMultiTenantDbContext, ITenantInfo?>>();
    }
    private static Action<WorkspaceDbContext, ITenantInfo> BuildTenantInfoSetter()
    {
        // The setter lives on the concrete MultiTenantDbContext type, not the interface.
        // Public-only binding avoids accessibility bypass (Sonar S3011).
        var prop = typeof(WorkspaceDbContext)
            .GetProperty("TenantInfo", BindingFlags.Instance | BindingFlags.Public)
            ?? throw new InvalidOperationException("MultiTenantDbContext.TenantInfo property not found via reflection.");
        var method = prop.SetMethod
            ?? throw new InvalidOperationException("MultiTenantDbContext.TenantInfo has no setter.");
        return method.CreateDelegate<Action<WorkspaceDbContext, ITenantInfo>>();
    }

    public AcceptInvitationCommandHandler(
        IInvitationTokenService tokenService,
        WorkspaceDbContext db,
        // BLOCKER 2 (02-REVIEW.md §CR-01 / 02-08 PLAN <critical_constraints> #3): explicit DI
        // injection of IMultiTenantContextSetter (DI-registered app-wide — see FshJobActivator.cs:40
        // GetRequiredService<IMultiTenantContextSetter>()). Replaces the unsafe pattern of casting
        // the accessor to the setter interface.
        IMultiTenantContextSetter multiTenantContextSetter,
        // Used to read the previous MultiTenantContext so the try/finally can restore it after
        // SaveChanges. The setter interface is write-only by Finbuckle design.
        IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor)
    {
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _multiTenantContextSetter = multiTenantContextSetter ?? throw new ArgumentNullException(nameof(multiTenantContextSetter));
        _multiTenantContextAccessor = multiTenantContextAccessor ?? throw new ArgumentNullException(nameof(multiTenantContextAccessor));
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
        // returned an untracked entity by design. CR-01 (02-REVIEW.md §CR-01): the id lookup is
        // tenant-agnostic — the accept endpoint is top-level and the DbContext scope (caller's
        // current tenant) is NOT the invitation's workspace tenant, so the tenant filter MUST be
        // disabled here or the re-attach silently returns null → 404 for every cross-tenant accept.
        var tracked = await _db.Invitations
            .IgnoreQueryFilters()
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
        // CR-03 (02-REVIEW.md §CR-03): if a previously-removed membership row already exists for
        // this (workspaceId, userId) pair (Deactivate sets IsActive=false but keeps the row — and
        // the composite unique index IX_WorkspaceMembers_Tenant_User is on (TenantId, UserId),
        // NOT IsActive), reuse it via Activate() + UpdateRole() instead of inserting a duplicate
        // (which would throw UniqueConstraintException → 500, locking the user out permanently).
        // IgnoreQueryFilters is REQUIRED here: even though the DbContext scope is switched to the
        // workspace tenant below, the existing row's TenantId may have been stamped by an earlier
        // write under a different scope — the only safe lookup is tenant-agnostic by
        // (WorkspaceId, UserId).
        var existing = await _db.Members
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.WorkspaceId == tracked.WorkspaceId
                && m.UserId == command.CurrentUserId.ToString(), cancellationToken)
            .ConfigureAwait(false);

        WorkspaceMember member;
        if (existing is not null)
        {
            existing.Activate();
            existing.UpdateRole(tracked.Role);
            member = existing;
        }
        else
        {
            member = WorkspaceMember.Create(
                workspaceId: tracked.WorkspaceId,
                userId: command.CurrentUserId.ToString(),
                role: tracked.Role,
                isActive: true);
            _db.Members.Add(member);
        }

        // Step 5 — save under a workspace-tenant scope so BaseDbContext.SaveChanges
        // (TenantNotSetMode=Overwrite) stamps the new row's TenantId shadow property with the
        // invitation's workspace id — NOT the caller's current tenant. The previous
        // MultiTenantContext + DbContext.TenantInfo are restored in `finally` to avoid leaking
        // the switch across requests when the DbContext + accessor are reused (scoped lifetime).
        //
        // CR-01 (02-REVIEW.md §CR-01) implementation note: Finbuckle's
        // MultiTenantDbContext caches TenantInfo at construction time AND EnforceMultiTenant
        // (run inside base.SaveChangesAsync) compares each Added/Modified entity's TenantId
        // against the DbContext's cached TenantInfo — NOT against the IMultiTenantContextAccessor's
        // current value. Merely switching the setter (the original plan pseudocode) is therefore
        // insufficient: we must ALSO rebind the DbContext's own TenantInfo property. The
        // IMultiTenantDbContext interface only exposes a getter, but the concrete
        // MultiTenantDbContext declares a setter on the property (verified via reflection at
        // runtime). The cached delegate invokes the underlying setter so we can rebind without
        // referencing the internal MultiTenantDbContext type directly.
        var workspaceTenant = new AppTenantInfo(
            id: tracked.WorkspaceId.ToString(),
            identifier: tracked.WorkspaceId.ToString(),
            name: tracked.WorkspaceId.ToString());
        var tenantScopedDb = (Finbuckle.MultiTenant.EntityFrameworkCore.IMultiTenantDbContext)_db;
        var previousTenantInfo = _tenantInfoGet.Value(tenantScopedDb);
        var previousContext = _multiTenantContextAccessor.MultiTenantContext;
        _multiTenantContextSetter.MultiTenantContext = new MultiTenantContext<AppTenantInfo>(workspaceTenant);
        _tenantInfoSet.Value(_db, workspaceTenant);
        try
        {
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _tenantInfoSet.Value(_db, previousTenantInfo!);
            _multiTenantContextSetter.MultiTenantContext = previousContext;
        }

        return new AcceptInvitationResponse(member.Id, tracked.WorkspaceId);
    }
}
