---
phase: 02-workspace
reviewed: 2026-06-18T00:00:00Z
depth: deep
files_reviewed: 19
files_reviewed_list:
  - src/Modules/Workspace/Modules.Workspace/Services/InvitationTokenService.cs
  - src/Modules/Workspace/Modules.Workspace/Services/SlugGenerator.cs
  - src/Modules/Workspace/Modules.Workspace/Services/WorkspaceMembershipService.cs
  - src/Modules/Workspace/Modules.Workspace/MultiTenancy/WorkspaceSlugStrategy.cs
  - src/Modules/Workspace/Modules.Workspace/MultiTenancy/WorkspaceTenantStore.cs
  - src/Modules/Workspace/Modules.Workspace/Middleware/WorkspaceMembershipMiddleware.cs
  - src/Modules/Workspace/Modules.Workspace/Authorization/RequireWorkspaceRoleAttribute.cs
  - src/Modules/Workspace/Modules.Workspace/Authorization/RequireWorkspaceRoleAuthorizationHandler.cs
  - src/Modules/Workspace/Modules.Workspace/Authorization/RequireWorkspaceRoleExtensions.cs
  - src/Modules/Workspace/Modules.Workspace/CurrentWorkspaceContext.cs
  - src/Modules/Workspace/Modules.Workspace/WorkspaceModule.cs
  - src/Modules/Workspace/Modules.Workspace/Data/WorkspaceDbContext.cs
  - src/Modules/Workspace/Modules.Workspace/Features/v1/Invitations/AcceptInvitation/AcceptInvitationCommandHandler.cs
  - src/Modules/Workspace/Modules.Workspace/Features/v1/Invitations/AcceptInvitation/AcceptInvitationEndpoint.cs
  - src/Modules/Workspace/Modules.Workspace/Features/v1/Workspaces/ListUserWorkspaces/ListUserWorkspacesQueryHandler.cs
  - src/Modules/Workspace/Modules.Workspace/Features/v1/Members/UpdateMemberRole/UpdateMemberRoleCommandHandler.cs
  - src/Modules/Workspace/Modules.Workspace/Domain/WorkspaceInvitation.cs
  - src/Modules/Identity/Modules.Identity/Services/UserIdentityService.cs
  - src/Tests/Workspace.Tests/Integration/WorkspaceLifecycleSmokeTests.cs
  - src/Tests/Workspace.Tests/Integration/WorkspaceRoleCapabilityTests.cs
findings:
  critical: 3
  high: 3
  medium: 3
  low: 2
  total: 11
status: issues
---

# Phase 02 (Workspace) — Code Review Report

**Reviewed:** 2026-06-18
**Depth:** deep (cross-file, security-sensitive code prioritized)
**Files Reviewed:** 19 (+ cross-referenced Domain/Data/Contracts)
**Status:** issues_found

## Summary

The Workspace module is well-structured and the crypto path (CSPRNG token + SHA-256 hash, raw-never-persisted) is correctly implemented. The role-authorization handler is correctly default-deny. However, **the deepest review surfaced three BLOCKER-class defects** clustered around tenant scoping of EF Core queries on top-level endpoints. The integration test suite is structurally incapable of catching these because the InMemory provider does not execute Finbuckle's `IsMultiTenant().AdjustUniqueIndexes()` shadow-property pipeline (the tests say so themselves — see `MembershipMiddlewareTests.cs:43-49` and `TenantIsolationTests.cs:34-52`). The smoke test that is supposed to be "load-bearing" runs entirely against a single fixed mock tenant and therefore exercises a code path that **does not occur in production**.

The single most important finding is **CR-01**: the accept-invitation flow cannot work in production. The handler explicitly claims (in source comments) that it disables the tenant filter when looking up the invitation by hash, but the implementation never calls `IgnoreQueryFilters` / `TenantNotSetMode`. The DbContext on the top-level accept endpoint is scoped to the user's _current_ resolved tenant (platform/root), which is never the invitation's workspace tenant, so the invitation is never found and every accept returns 404.

## Critical Issues

### CR-01: Accept-Invitation lookup is silently tenant-filtered — accept flow is broken in production

**File:** `src/Modules/Workspace/Modules.Workspace/Services/InvitationTokenService.cs:134-140`
**Also:** `src/Modules/Workspace/Modules.Workspace/Features/v1/Invitations/AcceptInvitation/AcceptInvitationCommandHandler.cs:80-90`

**Issue:**
`WorkspaceInvitation` is `IHasTenant` (NOT `IGlobalEntity`), so `BaseDbContext.ApplyTenantIsolationByDefault` auto-applies the Finbuckle tenant filter to `db.Invitations`. The accept endpoint is **top-level** (`POST /api/v1/workspaces/invitations/{token}/accept/`, no `{slug}` — see `WorkspaceModule.cs:200` and `AcceptInvitationEndpoint.cs:26`). On that route `WorkspaceSlugStrategy.GetIdentifierAsync` returns null (`WorkspaceSlugStrategy.cs:87-90`), so Finbuckle falls through to the Phase 1 claim/header strategy and the DbContext is scoped to the invitee's _current_ tenant (platform/root or their last workspace) — **never the invitation's workspace tenant**.

`ValidateAsync` then runs:

```csharp
var invitation = await _db.Invitations
    .AsNoTracking()
    .FirstOrDefaultAsync(i => i.TokenHash == hash, cancellationToken)  // tenant-filtered!
```

The tenant filter discards every invitation whose `TenantId` (= invitation's `WorkspaceId`) does not match the current request tenant. **No invitation is ever found.** The accept handler returns 404 for a valid token.

The handler's own comment claims mitigation (`AcceptInvitationCommandHandler.cs:42-48`):

> "we therefore disable the tenant filter when looking up the invitation by hash via `IInvitationTokenService.ValidateAsync` (which queries by hash, not by tenant)"

This claim is **false** — `ValidateAsync` does not disable the filter anywhere. `grep IgnoreQueryFilters|TenantNotSetMode` against the Workspace module returns **zero** matches. The `Step 4` assertion in `WorkspaceLifecycleSmokeTests` (the "load-bearing" 02-06 test) passes only because the test stubs the tenant accessor with a fixed workspace Guid (`WorkspaceLifecycleSmokeTests.cs:288-293`) — the same tenant the invitation was created under. In production the tenant is the invitee's default, not the invitation's workspace.

The same defect affects the re-attach query at `AcceptInvitationCommandHandler.cs:87-89` (`_db.Invitations.FirstOrDefaultAsync(i => i.Id == invitation.Id, ...)`) and `RejectInvitationCommandHandler.cs:58-60`.

**Fix:**
Look up the invitation by hash **with the tenant filter disabled**, and write the new `WorkspaceMember` row using a context explicitly scoped to the invitation's workspace tenant. Minimal fix:

```csharp
// In InvitationTokenService.ValidateAsync:
var invitation = await _db.Invitations
    .IgnoreQueryFilters()                 // CR-01: hash is globally unique; tenant-agnostic lookup
    .AsNoTracking()
    .FirstOrDefaultAsync(i => i.TokenHash == hash, cancellationToken)
    .ConfigureAwait(false);
```

And in `AcceptInvitationCommandHandler`, after re-attaching, you must additionally ensure the new `WorkspaceMember` row is saved under the invitation's workspace tenant (either by overriding `TenantNotSetMode` or by switching the Finbuckle `MultiTenantContext.TenantInfo` for the duration of `SaveChangesAsync`). Without that, the membership row's `TenantId` will be stamped with the invitee's current tenant, not the workspace's — breaking the `(TenantId, UserId)` uniqueness invariant across all of the user's workspaces and corrupting the `ListMembers`/`ListUserWorkspaces` results.

A relational integration test (PostgreSQL via Testcontainers — the project already has the harness per `AGENTS.md`) is required; the InMemory suite cannot exercise this path.

---

### CR-02: `ListUserWorkspaces` is tenant-scoped — returns only the user's CURRENT workspace's membership

**File:** `src/Modules/Workspace/Modules.Workspace/Features/v1/Workspaces/ListUserWorkspaces/ListUserWorkspacesQueryHandler.cs:54-60`

**Issue:**
The handler is the top-level `GET /api/v1/users/me/workspaces/` endpoint (REQ-2.1). It issues:

```csharp
var workspaceIds = await _db.Members
    .AsNoTracking()
    .Where(m => m.UserId == query.UserId && m.IsActive && !m.IsDeleted)
    .Select(m => m.WorkspaceId)
    .Distinct()
    .ToListAsync(cancellationToken);
```

`WorkspaceMember` is `IHasTenant` (auto-filtered). On a top-level endpoint the DbContext is scoped to the user's current/last-resolved tenant, so this query returns only the membership rows whose `TenantId` equals that single tenant — typically **a single workspace** (or zero, if the user has never visited a workspace in this session). A user who is a member of workspaces A, B and C will see at most one of them. REQ-2.1 is not satisfied.

The handler's remarks claim the cross-workspace scalar `WorkspaceId` lets it "read without disabling the tenant filter" (`WorkspaceMember.cs:36-39`), but Finbuckle's filter applies at the row level via the `TenantId` shadow property, not the explicit `WorkspaceId` column — the scalar copy does not help.

**Fix:**
Disable the tenant filter for this query (cross-workspace aggregation is the entire point of the endpoint):

```csharp
var workspaceIds = await _db.Members
    .IgnoreQueryFilters()
    .AsNoTracking()
    .Where(m => m.UserId == query.UserId && m.IsActive && !m.IsDeleted)
    .Select(m => m.WorkspaceId)
    .Distinct()
    .ToListAsync(cancellationToken);
```

Add a relational test that seeds memberships in two distinct tenants and asserts both workspace ids surface.

---

### CR-03: Accepting an invitation for a previously-removed member throws a unique-constraint violation

**File:** `src/Modules/Workspace/Modules.Workspace/Features/v1/Invitations/AcceptInvitation/AcceptInvitationCommandHandler.cs:102-107`
**Also:** `src/Modules/Workspace/Modules.Workspace/Domain/WorkspaceMember.cs:118-124` and `Data/Configurations/WorkspaceMemberConfiguration.cs:51-53`

**Issue:**
When an admin removes a member, `WorkspaceMembershipService.RemoveAsync` calls `member.Deactivate()` (`WorkspaceMembershipService.cs:114-123`), which sets `IsActive = false` but leaves `IsDeleted = false`. The composite unique index `IX_WorkspaceMembers_Tenant_User` is on `(TenantId, UserId)` — **neither `IsActive` nor `IsDeleted` is part of the key**. So the deactivated row still occupies the uniqueness slot.

If the same user is later re-invited and accepts, the accept handler unconditionally inserts a brand-new `WorkspaceMember` row (`AcceptInvitationCommandHandler.cs:102-107`):

```csharp
var member = WorkspaceMember.Create(
    workspaceId: tracked.WorkspaceId,
    userId: command.CurrentUserId.ToString(),
    role: tracked.Role,
    isActive: true);
_db.Members.Add(member);
```

`SaveChangesAsync` then throws a `UniqueConstraintException` from PostgreSQL — surfaced as an unhandled 500. The user is permanently locked out of re-joining the workspace until an admin manually clears the deactivated row.

`WorkspaceMember.Activate()` exists (`WorkspaceMember.cs:127-132`) precisely for this scenario but is never called from the accept path.

**Fix:**
Before inserting, look up an existing (possibly-deactivated) membership for `(workspaceId, userId)` — with tenant filter handled per CR-01 — and either reactivate it or branch:

```csharp
var existing = await _db.Members
    .IgnoreQueryFilters()                       // cross-tenant per CR-01
    .FirstOrDefaultAsync(m => m.WorkspaceId == tracked.WorkspaceId
                           && m.UserId == command.CurrentUserId.ToString(),
                         cancellationToken);
if (existing is not null)
{
    existing.Activate();
    existing.UpdateRole(tracked.Role);
}
else
{
    _db.Members.Add(WorkspaceMember.Create(
        workspaceId: tracked.WorkspaceId,
        userId: command.CurrentUserId.ToString(),
        role: tracked.Role,
        isActive: true));
}
```

Cover with a test: invite → accept → admin removes → re-invite → re-accept should succeed without a 500.

---

## High Severity

### HI-01: Top-level invitation endpoints call `Guid.Parse(userId)` without a fallback — malformed claims yield 500, not 401

**File:** `src/Modules/Workspace/Modules.Workspace/Features/v1/Invitations/AcceptInvitation/AcceptInvitationEndpoint.cs:41`
**Also:** `src/Modules/Workspace/Modules.Workspace/Features/v1/Members/RemoveMember/RemoveMemberEndpoint.cs:43` and every other endpoint that does `Guid.Parse(user.GetUserId())`

**Issue:**
The endpoint already gates on `string.IsNullOrEmpty(userId)` (line 33), but `user.GetUserId()` returning a non-Guid string (legacy claim, corrupted token, alt identity provider) will throw `FormatException` at line 41 → unhandled 500. Under load, this is also a cheap DoS surface (crafted JWTs with a non-Guid sub). The other workspace endpoints (RemoveMember, UpdateMemberRole, Leave) repeat the pattern.

**Fix:**
Use `Guid.TryParse` and treat failure as unauthorized:

```csharp
if (!Guid.TryParse(userId, out var userIdGuid))
{
    throw new UnauthorizedException();
}
```

Apply consistently across all workspace endpoints.

---

### HI-02: `UpdateMemberRole` self-promotion guard is bypassable via the membership-service path and only covers the Admin target

**File:** `src/Modules/Workspace/Modules.Workspace/Features/v1/Members/UpdateMemberRole/UpdateMemberRoleCommandHandler.cs:57-62`

**Issue:**
The T-2-eop-self guard only fires when the _target_ role is `Admin`. But the realistic privilege-escalation scenarios it claims to mitigate include:

1. A **Guest or Member** somehow passing the endpoint's `RequireWorkspaceRole(Admin)` decoration (decoration regression or test misconfiguration). The decoration is the primary gate; the in-handler check is described as "belt-and-braces." If the decoration regresses, the in-handler guard does NOT catch a Guest/Member promoting themselves to Member (no Admin target).
2. An Admin **demoting themselves to Guest** to test something, then re-promoting via a colleague's session — the documented scenario. The check fires only on `Role == Admin`, so a self-demotion to Guest is permitted (intended), but there is no symmetric guard preventing the same Admin from re-promoting themselves by re-using their own session through a different code path (e.g. `WorkspaceMembershipService.UpdateRoleAsync` directly).

More importantly, the handler performs the lookup with `AsNoTracking()` then calls `_membership.UpdateRoleAsync` which does its own lookup _without_ the self-check (`WorkspaceMembershipService.cs:90-108`). If a caller bypasses the handler and calls the service directly (future Phase 3 internal call, Hangfire job, etc.), the self-guard is gone. The mitigation lives at the wrong layer.

**Fix:**
Move the self-promotion guard into `WorkspaceMembershipService.UpdateRoleAsync` so it cannot be bypassed by any caller. Keep the handler check as defense-in-depth but document the service as the authoritative gate.

---

### HI-03: Slug uniqueness is enforced by application-level probe + ignore-soft-deleted assumption — concurrent creates race, and the assumption is fragile

**File:** `src/Modules/Workspace/Modules.Workspace/Services/SlugGenerator.cs:124-147`
**Also:** `src/Modules/Workspace/Modules.Workspace/Domain/Workspace.cs:140-148`

**Issue:**
`GenerateUniqueSlugAsync` does an `AnyAsync` existence probe and returns the candidate — there is no transaction, no `SELECT ... FOR UPDATE`, and the Workspaces table does not appear to have a unique index on `Slug` enforced at the DB level (no `HasIndex(x => x.Slug).IsUnique()` in `WorkspaceConfiguration`). Two concurrent `CreateWorkspace` requests with the same name can both pass the probe and both insert. Since the suffixed-collision fallback also relies on the probe (no DB-side constraint), the same race exists for the suffixed candidates.

The remarks at `SlugGenerator.cs:25-29` also claim soft-deleted rows "already had their slug suffixed" — true only if `SoftDelete` was invoked. A workspace deleted via a future bulk path, a DBA script, or a bug in the soft-delete interceptor would leave the original slug on an `IsDeleted=true` row; the collision probe (which does NOT filter `IsDeleted`) would then false-positive a fresh create as a collision and silently append a suffix — surprising UX. The handler comment justifies not filtering as "could miss the edge case where a row's slug has not been suffixed yet," but that very edge case is what produces the false positive.

**Fix:**

1. Add `builder.HasIndex(x => x.Slug).IsUnique()` in `WorkspaceConfiguration` (PostgreSQL will treat `acme-smoke` and `acme-smoke__1700000000` as distinct, so D-08 still works). Catch the resulting `UniqueConstraintException` in `CreateWorkspaceCommandHandler` and retry with a fresh suffix.
2. In the collision probe, filter `!w.IsDeleted` — soft-deleted rows are suffixed and will not collide; a future non-suffixed delete is a bug to fix at the source, not paper over here.

---

## Medium Severity

### MD-01: `InvitationTokenService.CreateAsync` is documented as deduping invitations for the same mailbox but does not — duplicate invitations accumulate

**File:** `src/Modules/Workspace/Modules.Workspace/Services/InvitationTokenService.cs:106-117`

**Issue:**
The remarks at line 106-107 say D-12 mandates lowercased emails "so a case-difference in the invitee address cannot produce duplicate invitations for the same mailbox." Lowercasing normalizes case, but the service never checks whether an existing _pending_ invitation for `(workspaceId, normalizedEmail)` already exists. An admin who clicks "Invite" twice (or a flaky retry) produces two pending invitations, two emails (Phase 11), two token hashes — and either can be accepted. Plane's reference UI prevents this server-side. There is no unique index on `(TenantId, Email, Accepted)` to catch it at the DB layer either.

**Fix:** Before `CreateAsync` inserts, query for an existing pending invitation (`!Accepted && RespondedAt == null && !IsExpired`) for the same workspace + normalized email, and either return the existing one or throw 409. Add a partial unique index `WHERE Accepted = false AND RespondedAt IS NULL` on `(TenantId, Email)` to make the constraint server-side.

---

### MD-02: `WorkspaceMembershipMiddleware` runs a membership DB query on every workspace-scoped request with no caching — and on anonymous skip, but no rate-limit guard

**File:** `src/Modules/Workspace/Modules.Workspace/Middleware/WorkspaceMembershipMiddleware.cs:80-82`

**Issue:**
Every workspace-scoped authenticated request issues `SELECT TOP 1 ... FROM WorkspaceMembers WHERE UserId = @u AND IsActive = 1` under the tenant filter. With the `(TenantId, WorkspaceId, IsActive)` index (`WorkspaceMemberConfiguration.cs:57-58`) this is fast, but there is no per-request cache and no short-circuit when `ICurrentWorkspaceContext` was already populated earlier in the pipeline. Combined with the slug strategy's Redis round-trip (`WorkspaceTenantStore.cs:106-110`), every workspace-scoped request currently costs at least one Redis hit + one SQL hit just to authorize. At NFR-1 target volume this is a measurable overhead and a single-postgres-row hotspot for active workspaces. (Out of strict scope per the brief — flagged because the brief asks about N+1/expense.) More urgently, there is no circuit-breaker if the DB is slow: the middleware awaits unconditionally and the request hangs.

**Fix:** Consider populating the role claim into the principal at login time (or a short-TTL HybridCache keyed on `(userId, workspaceId)`). Add a CancellationToken-aware timeout on the membership probe. (Severity kept at medium because correctness is fine — this is robustness/SLO.)

---

### MD-03: Slug release on delete uses `(int)now.ToUnixTimeSeconds()` — collision on rapid delete/re-create loop and integer truncation of dates beyond 2038-01-19

**File:** `src/Modules/Workspace/Modules.Workspace/Domain/Workspace.cs:147`

**Issue:**
`Slug = $"{Slug}__{(int)now.ToUnixTimeSeconds()}"` casts a `long` epoch to `int`. Unix seconds exceed `int.MaxValue` on 2038-01-19 03:14:07 UTC — the cast then silently wraps to a negative number, producing a slug like `acme__-2139734652` (technically still unique, but ugly and surprising). Additionally, two delete/re-create cycles of the same base slug within the same second produce the SAME suffix — the soft-delete unique-suffix claim (`SoftDeleteSlugReleaseTests.cs:144` asserts only 2 underscores total) breaks if a workspace is deleted, recreated, and deleted again within one second (rare but possible in tests/automation). The smoke test (`WorkspaceLifecycleSmokeTests.cs:244-256`) gets away with it because `Guid.NewGuid()` differs each time, but two deletes of literally `"Acme Smoke"` in a tight loop collide.

**Fix:** Use `long` (no cast): `$"{Slug}__{now.ToUnixTimeSeconds()}"`. For real collision resistance append the workspace's own `Id` (already unique) instead of a timestamp: `$"{Slug}__{Id:N}"`. This is deterministic, collision-free, and avoids the 2038 problem.

---

## Low Severity

### LO-01: `RequireWorkspaceRoleAuthorizationHandler` reads `ICurrentWorkspaceContext` without verifying `CurrentWorkspaceId` is non-null

**File:** `src/Modules/Workspace/Modules.Workspace/Authorization/RequireWorkspaceRoleAuthorizationHandler.cs:49-54`

**Issue:**
The handler only checks `CurrentUserRole`. If a future pipeline regression populates `CurrentUserRole` without setting `CurrentWorkspaceId` (or vice versa), the handler would authorize against a role from an unset workspace. Today this cannot happen because `WorkspaceMembershipMiddleware` sets both atomically, but the authorization decision should not trust an unrelated field for a _workspace-scoped_ check. Defense in depth.

**Fix:** Add `if (_workspaceContext.CurrentWorkspaceId is null) { context.Fail(); return; }` before the role check.

---

### LO-02: `WorkspaceSlugStrategy.GetIdentifierAsync` returns the raw route slug without normalization — case or trailing-space variations bypass cache + tenant-store lookup

**File:** `src/Modules/Workspace/Modules.Workspace/MultiTenancy/WorkspaceSlugStrategy.cs:87-97`

**Issue:**
The strategy returns the slug exactly as the router binds it. ASP.NET route binding does not lowercase, so a request to `/api/v1/workspaces/AcMe/` resolves the slug `AcMe`. The `WorkspaceTenantStore` queries `Workspaces.FirstOrDefaultAsync(w => w.Slug == identifier ...)` with default ordinal case-sensitive comparison (`WorkspaceTenantStore.cs:114-117`), so `AcMe` does not match the stored `acme` → tenant resolution fails → middleware sees no tenant → all `[RequireWorkspaceRole]` endpoints 403 the user from a workspace they are genuinely a member of. The cache key is also case-sensitive (`ws:slug:AcMe` vs `ws:slug:acme`), so even if the store were case-insensitive the cache would miss every time.

Production routing should normally lowercase slugs at the URL layer, but nothing in the module enforces this. Plane lowercases slugs at generation time and rejects mixed-case routes; this module relies on the client to behave.

**Fix:** In `WorkspaceSlugStrategy.GetIdentifierAsync`, after confirming `slug` is a non-empty string, also validate it matches the strict slug format regex from `SlugGenerator` (or at minimum `ToLowerInvariant` it) and return null if it does not — Finbuckle then falls through and the request 404s cleanly instead of authorizing against a stale state.

---

## Notes on the 02-06 closeout tests

`WorkspaceLifecycleSmokeTests.cs` and `WorkspaceRoleCapabilityTests.cs` are well-written and exercise the documented D-06/D-08/D-11/D-12 invariants — **as the invariants manifest under the test fixture**. Three structural blind spots reduce their ability to catch production defects:

1. **Single-tenant fixture hides CR-01/CR-02/CR-03.** Every test creates a single InMemory context with a fixed mock tenant (`WorkspaceLifecycleSmokeTests.cs:286-303`). The accept flow, the list-mine flow, and the re-invite flow all execute under that one tenant. The InMemory provider does not apply Finbuckle's tenant filter (the test files document this at `MembershipMiddlewareTests.cs:43-49` and `TenantIsolationTests.cs:34-52`), so the tenant-scoping defects above are not just uncaught — they are structurally uncatchable by this suite. **Add at least one PostgreSQL-via-Testcontainers integration test that drives the accept flow over HTTP with a real Finbuckle pipeline**, exercising the actual tenant resolution. Without it, CR-01 ships.

2. **`WorkspaceRoleCapabilityTests` does not cover the `ICurrentWorkspaceContext.CurrentWorkspaceId is null` case at the handler level** (LO-01). The matrix covers role-vs-roles, not context-corruption.

3. **`WorkspaceLifecycleSmokeTests` Step 7 "self-promotion guard" mis-describes what it tests.** The comment at lines 199-203 says "an Admin cannot promote THEMSELVES to Admin via this endpoint" — but the handler's guard checks "caller == target", and the test calls it with `MemberId = ownerMember.Id` and `CurrentUserId = OwnerUserId`. That works because owner == caller == target, but it does not actually exercise the _Admin_ path (the OwnerId is also an Admin, but the guard fires regardless of caller role). The test would still pass if the guard were weakened to "any user" — making it a weaker guard than the comment implies. Tighten the comment or split into a Guest-caller variant.

The crypto tests (`InvitationHashTests`, `InvitationTtlTests`, `InvitationInvalidateTests`) are correct and load-bearing for T-2-token / T-2-ttl / T-2-replay — the SHA-256 storage and state-machine invalidation are genuinely verified. No issues there.

---

_Reviewed: 2026-06-18_
_Reviewer: Claude (gsd-code-reviewer)_
_Depth: deep_
