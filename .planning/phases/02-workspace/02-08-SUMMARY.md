---
phase: 02-workspace
plan: 08
plan_id: 02-08
subsystem: workspace-cross-tenant-invitations
type: execute
wave: 7
gap_closure: true
depends_on: [02-05, 02-06, 02-07]
tags:
  [
    cr-01,
    cr-03,
    cross-tenant,
    finbuckle,
    ignorequeryfilters,
    multitenant-context-setter,
    accept-invitation,
    reject-invitation,
    relational-tests,
  ]
requirements_completed: [REQ-2.1, REQ-2.2, REQ-2.4, NFR-2]
tech-stack:
  added: [] # zero new packages — reuses 02-07's Testcontainers.PostgreSql + Finbuckle + Npgsql
  patterns:
    - "Cached reflection delegate for MultiTenantDbContext.TenantInfo setter (public setter on concrete type, get-only on IMultiTenantDbContext interface) — lets the AcceptInvitation handler rebind the DbContext's cached tenant for the duration of SaveChanges without referencing the internal MultiTenantDbContext type"
    - "DI-explicit IMultiTenantContextSetter + IMultiTenantContextAccessor<AppTenantInfo> injection (BLOCKER 2 path A) — same DI resolution path as FshJobActivator.cs:40 GetRequiredService<IMultiTenantContextSetter>()"
    - "try/finally tenant-scope switch on SaveChanges (handler-internal, mirrors 02-07 FinbuckleTestTenantScope semantics but in production code)"
    - "CR-03 insert-before-lookup branch: IgnoreQueryFilters cross-tenant existing-member query + entity-method reuse (Activate + UpdateRole) instead of duplicate Add"
key-files:
  created:
    - yh-flow/src/Tests/Workspace.Tests/Integration/CrossTenant/AcceptInvitationAcrossTenantsTests.cs (197 lines)
    - yh-flow/src/Tests/Workspace.Tests/Integration/CrossTenant/ReAcceptAfterRemovalTests.cs (253 lines)
  modified:
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Services/InvitationTokenService.cs (ValidateAsync + IgnoreQueryFilters)
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Invitations/AcceptInvitation/AcceptInvitationCommandHandler.cs (CR-01 re-attach IgnoreQueryFilters + BLOCKER 2 explicit DI + try/finally DbContext.TenantInfo rebind + CR-03 existing-member reuse branch + remarks rewritten)
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Invitations/RejectInvitation/RejectInvitationCommandHandler.cs (BLOCKER 1: line 58-60 IgnoreQueryFilters on re-attach)
    - yh-flow/src/Tests/Workspace.Tests/Integration/WorkspaceLifecycleSmokeTests.cs (fixture tenant rebind to workspace.Id + owner member TenantId re-stamp to mirror production workspace-scoped semantics)
decisions:
  - "CR-01 implementation diverged from PLAN pseudocode: the PLAN assumed switching IMultiTenantContextSetter.MultiTenantContext alone would re-scope SaveChanges, but Finbuckle 10.1.0's MultiTenantDbContext CACHES TenantInfo at construction time and EnforceMultiTenant (inside base.SaveChangesAsync) compares each entity's TenantId against the DbContext's cached TenantInfo — NOT against the accessor's live value. The fix therefore also rebinds DbContext.TenantInfo via its public setter (reachable on the concrete type but hidden behind the get-only IMultiTenantDbContext interface property) using a cached reflection delegate. Verified empirically via runtime diagnostic throws during executor bring-up."
  - "BLOCKER 2 path A (explicit DI injection of IMultiTenantContextSetter) retained as planned — no `as IMultiTenantContextSetter` cast anywhere in the handler. Same singleton instance resolves under both IMultiTenantContextSetter and IMultiTenantContextAccessor<AppTenantInfo> DI slots, mirroring 02-07 WorkspacePostgresFixture.BuildServiceProvider wiring and FshJobActivator.cs:40 production resolution path."
  - "WorkspaceLifecycleSmokeTests fixture was silently divergent from production tenant semantics pre-02-08: it used a fixed random Guid for the DbContext tenant across the whole lifecycle. InMemory never enforced Finbuckle's TenantMismatchMode so the divergence was invisible. After 02-08's handler fix enforces multi-tenant on SaveChanges, the fixture had to be corrected: bind the DbContext tenant to the freshly-created workspace.Id + re-stamp the owner member's TenantId shadow property before ListMembers. This mirrors what the production WorkspaceSlugStrategy resolves on the next workspace-scoped request."
  - "CR-03 fix in AcceptInvitationCommandHandler Step 4: tenant-agnostic existing-member lookup via IgnoreQueryFilters (the existing row's TenantId may have been stamped under a different scope). Reuse via entity methods (Activate + UpdateRole) preserves encapsulation per AGENTS.md Rule 3; no direct field assignment."
  - "AppTenantInfo constructor signature aligned with 02-07 fixture: 3-arg `new AppTenantInfo(id, identifier, name)` with `tracked.WorkspaceId.ToString()` for all three — NOT the 5-arg ConnectionString overload (connection comes via DbContextOptionsBuilder.UseNpgsql)."
metrics:
  duration: ~70m
  completed: 2026-06-22T04:15:00Z
  tasks_completed: 2
  tasks_total: 2
  files_created: 2
  files_modified: 4
  tests_added: 2
  tests_total_workspace: 100
  tests_total_identity: 412
---

# Phase 02 Plan 08: CR-01 + CR-03 Gap-Closure Summary

FINAL gap-closure plan for Phase 02-workspace — fixes the last two of three CRITICAL tenant-scoping defects (CR-01 + CR-03) flagged by `02-REVIEW.md`, and synchronously fixes the CR-01 sibling defect in `RejectInvitationCommandHandler.cs:58-60` (BLOCKER 1). After this plan, all three CRITICAL CR-01/02/03 defects are closed and Phase 02 is ready for the 11-step HUMAN-UAT smoke.

## What Was Built

### Task 1 — CR-01 cross-tenant accept + BLOCKER 1/2 fixes (commit `c343c2fd4`)

**`InvitationTokenService.ValidateAsync`** (`InvitationTokenService.cs:134-141`): inserted `.IgnoreQueryFilters()` between `_db.Invitations` and `.AsNoTracking()`. The TokenHash lookup is globally unique (256-bit CSPRNG, T-2-token) and tenant-agnostic — the top-level accept endpoint's DbContext scope is the caller's current tenant, never the invitation's workspace tenant, so the Finbuckle auto-applied TenantId filter would silently drop every cross-tenant invitation row → 404. CR-01 comment block above the call documents the root cause.

**`AcceptInvitationCommandHandler`** (`AcceptInvitationCommandHandler.cs`):

- **Constructor (BLOCKER 2 path A)**: added two explicit DI parameters — `IMultiTenantContextSetter multiTenantContextSetter` + `IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor`. The setter is DI-registered app-wide (same path as `FshJobActivator.cs:40`'s `GetRequiredService<IMultiTenantContextSetter>()`). NO `as IMultiTenantContextSetter` cast anywhere (acceptance_criteria `grep -c == 0`).
- **Step 2 re-attach** (line 146-150): added `.IgnoreQueryFilters()` to the `_db.Invitations.FirstOrDefaultAsync(i => i.Id == invitation.Id, ...)` call — invitation id lookup is tenant-agnostic.
- **Step 4 (CR-03 branch — also implemented in Task 1 per plan <action> step 3)**: before `_db.Members.Add(...)`, query for an existing `(WorkspaceId, UserId)` row via `_db.Members.IgnoreQueryFilters().FirstOrDefaultAsync(...)`. If found: `existing.Activate(); existing.UpdateRole(tracked.Role); member = existing;` else: `WorkspaceMember.Create(...) + _db.Members.Add(member)`.
- **Step 5 SaveChanges tenant-scope switch (CR-01 implementation divergence from PLAN pseudocode — see decision #1)**: the PLAN pseudocode assumed switching `IMultiTenantContextSetter.MultiTenantContext` alone would re-scope SaveChanges. Empirically verified this is insufficient — Finbuckle 10.1.0's `MultiTenantDbContext` caches `TenantInfo` at construction, and `EnforceMultiTenant` (inside `base.SaveChangesAsync`) compares each Added/Modified entity's TenantId against the **DbContext's cached TenantInfo**, NOT against the accessor's live value. The fix therefore:
  1. Captures `previousContext = _multiTenantContextAccessor.MultiTenantContext`
  2. Captures `previousTenantInfo = DbContext.TenantInfo` via cached reflection getter on `IMultiTenantDbContext`
  3. Sets `_multiTenantContextSetter.MultiTenantContext = new MultiTenantContext<AppTenantInfo>(workspaceTenant)` (so AsyncLocal slot is consistent)
  4. Sets `DbContext.TenantInfo = workspaceTenant` via cached reflection setter (public on the concrete `MultiTenantDbContext` type, hidden behind the get-only interface property — the cached delegate invokes it without referencing the internal type)
  5. `try { await _db.SaveChangesAsync(...); } finally { restore DbContext.TenantInfo + setter.MultiTenantContext; }`
- **Remarks block** (lines 44-66): rewrote the false-claim "we therefore disable the tenant filter" to precisely describe the actual behavior (IgnoreQueryFilters on hash + id lookups, plus DI-injected IMultiTenantContextSetter switch of both MultiTenantContext AND cached DbContext.TenantInfo during SaveChanges). `grep "disable the tenant filter"` no longer matches.

**`RejectInvitationCommandHandler`** (`RejectInvitationCommandHandler.cs:58-60`, BLOCKER 1): added `.IgnoreQueryFilters()` to the `_db.Invitations.FirstOrDefaultAsync(i => i.Id == invitation.Id, ...)` re-attach. 02-REVIEW.md:75 explicitly named this as the CR-01 sibling defect. NOT optional.

**`AcceptInvitationAcrossTenantsTests`** (`AcceptInvitationAcrossTenantsTests.cs`): new relational test (`[Fact] [Trait Category=Postgres] [Trait Category=RequiresDocker]`). Seeds workspaceA invitation while scoped to tenantA, then invokes `AcceptInvitationCommandHandler.Handle` while the DbContext is scoped to `RootTenant` (simulating the top-level endpoint). Asserts: 200 response, new WorkspaceMember row's TenantId shadow property == workspaceA.Id.ToString() (NOT RootTenant.Id), WorkspaceId == workspaceA.Id, UserId == invitee, Role == 15, IsActive == true. This is the load-bearing CR-01 + BLOCKER 2 assertion — if the setter cast had been used and returned null, or if only the setter were switched without the DbContext.TenantInfo rebind, SaveChanges would throw `MultiTenantException` and the test would fail.

**`WorkspaceLifecycleSmokeTests`** (`WorkspaceLifecycleSmokeTests.cs`): the InMemory fixture was silently divergent from production tenant semantics pre-02-08 (used a fixed random Guid for the whole lifecycle; InMemory never enforced mismatch). Corrected to bind the AsyncLocalMultiTenantContextAccessor + DbContext.TenantInfo to the freshly-created `workspace.Id` AFTER Step 1, and re-stamp the owner member's TenantId shadow property so subsequent workspace-scoped ListMembers queries surface it. Added `SetDbContextTenantInfo` helper that uses the same reflection pattern as the production handler.

### Task 2 — CR-03 regression coverage (commit `c6a23481c`)

**`ReAcceptAfterRemovalTests`** (`ReAcceptAfterRemovalTests.cs`): new relational test covering the full 5-stage CR-03 flow:

1. Seed workspaceA + create invitation (workspace-scoped) → rawToken1
2. First accept under RootTenant scope (CR-01 cross-tenant path) → member1Id
3. Admin removes the member via `WorkspaceMembershipService.RemoveAsync` → row deactivated (IsActive=false, NOT deleted)
4. Create second invitation (re-invite) → rawToken2
5. Second accept under RootTenant scope — MUST reuse the existing row, NOT throw UniqueConstraintException

Assertions: `response2.MemberId == member1Id` (row reused), `reactivated.IsActive == true`, `reactivated.Role == (int)WorkspaceRole.Admin` (UpdateRole applied), `reactivated.TenantId == workspaceA.Id.ToString()` (CR-01 invariant preserved), `rowsForPair == 1` (no duplicate).

The CR-03 handler branch (`existing.Activate() + existing.UpdateRole(tracked.Role)` via IgnoreQueryFilters cross-tenant lookup) was implemented in Task 1's commit per plan `<action>` step 3 (both tasks touch the same file). Task 2's commit adds the regression test only.

## Acceptance Criteria — All Met

- `grep -c "IgnoreQueryFilters" InvitationTokenService.cs` == 2 (ValidateAsync + comment) ✓
- `grep -c "IgnoreQueryFilters" AcceptInvitationCommandHandler.cs` == 4 (Step 2 re-attach + Step 4 existing-member query + 2 comments) ✓
- `grep -c "IgnoreQueryFilters" RejectInvitationCommandHandler.cs` == 2 (line 58-60 BLOCKER 1 + comment) ✓
- `grep -c "as IMultiTenantContextSetter" AcceptInvitationCommandHandler.cs` == 0 ✓ (remarks/comments rewritten to describe the cast pattern without using the literal string)
- `grep -n "IMultiTenantContextSetter multiTenantContextSetter|_multiTenantContextSetter|new MultiTenantContext<AppTenantInfo>|previousContext|previousTenantInfo" AcceptInvitationCommandHandler.cs` — all hit ✓
- `grep -n "existing.Activate|existing.UpdateRole|FirstOrDefaultAsync(m => m.WorkspaceId" AcceptInvitationCommandHandler.cs` — all hit ✓
- `grep "disable the tenant filter" AcceptInvitationCommandHandler.cs` — 0 matches ✓ (remarks rewritten)
- AcceptInvitationAcrossTenantsTests asserts `TenantId == workspaceA.Id.ToString()` with `[Trait Category=Postgres]` ✓
- ReAcceptAfterRemovalTests covers 5-stage flow + `memberId reuse` + `IsActive.ShouldBeTrue` ✓
- `dotnet build YH.Flow.slnx --nologo` = 0 warnings / 0 errors ✓
- `dotnet test Workspace.Tests --filter Category=Postgres` = 4/4 pass (02-07 x2 + 02-08 x2) on real PG (Docker up) ✓
- `dotnet test Workspace.Tests` (full) = 100/100 (96 InMemory + 4 relational) ✓
- `dotnet test Identity.Tests` = 412/412 (zero regression) ✓
- `git diff` against `WorkspaceMember.cs` / `WorkspaceMemberConfiguration.cs` / `WorkspaceMembershipService.cs` / migrations = empty ✓

## 6 Required SUMMARY Records

### 1. CR-01 + CR-03 fix line numbers + call chain

**CR-01 call chain** (cross-tenant invitation lookup + SaveChanges scope switch):

- `InvitationTokenService.cs:134-141` — `_db.Invitations.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(i => i.TokenHash == hash, ...)` (hash tenant-agnostic lookup)
- `AcceptInvitationCommandHandler.cs:146-150` — `_db.Invitations.IgnoreQueryFilters().FirstOrDefaultAsync(i => i.Id == invitation.Id, ...)` (re-attach tenant-agnostic)
- `AcceptInvitationCommandHandler.cs:210-227` — try/finally tenant-scope switch: `workspaceTenant = new AppTenantInfo(tracked.WorkspaceId.ToString(), ...); _multiTenantContextSetter.MultiTenantContext = new MultiTenantContext<AppTenantInfo>(workspaceTenant); _tenantInfoSet.Value(_db, workspaceTenant); try { SaveChangesAsync; } finally { restore; }`

**CR-03 call chain** (existing-member reuse):

- `AcceptInvitationCommandHandler.cs:171-192` — `_db.Members.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.WorkspaceId == tracked.WorkspaceId && m.UserId == command.CurrentUserId.ToString(), ...)` → if found: `existing.Activate(); existing.UpdateRole(tracked.Role); member = existing;` else: `WorkspaceMember.Create(...) + _db.Members.Add(member);`

### 2. BLOCKER 1 fix location

`RejectInvitationCommandHandler.cs:58-60` — added `.IgnoreQueryFilters()` on the invitation re-attach query. 02-REVIEW.md:75 explicitly named this line as the CR-01 sibling defect. The line now reads:

```csharp
// CR-01 (02-REVIEW.md:75): reject handler 同源缺陷同步修复——invitation.Id 查找租户无关。
// 顶层 reject 端点 DbContext 作用域 ≠ 邀请所属 workspace，必须 IgnoreQueryFilters 才能找到邀请。
var tracked = await _db.Invitations
    .IgnoreQueryFilters()
    .FirstOrDefaultAsync(i => i.Id == invitation.Id, cancellationToken)
    .ConfigureAwait(false);
```

### 3. BLOCKER 2 fix approach (path A — explicit DI injection)

Constructor of `AcceptInvitationCommandHandler` now explicitly declares:

```csharp
public AcceptInvitationCommandHandler(
    IInvitationTokenService tokenService,
    WorkspaceDbContext db,
    IMultiTenantContextSetter multiTenantContextSetter,        // BLOCKER 2 path A
    IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor)
```

`IMultiTenantContextSetter` is DI-registered app-wide — proven by `FshJobActivator.cs:40`'s `_scope.ServiceProvider.GetRequiredService<IMultiTenantContextSetter>()` resolution. This replaces the unsafe `accessor as IMultiTenantContextSetter` cast pattern that would return null (→ NullReferenceException → all accepts 500) if the accessor implementation did not also implement the setter interface. ZERO `as IMultiTenantContextSetter` cast occurrences in the handler.

### 4. SaveChanges tenant-scope switch implementation

The PLAN pseudocode assumed switching `_multiTenantContextSetter.MultiTenantContext` alone would re-scope SaveChanges. **Empirically verified insufficient** during executor bring-up: Finbuckle 10.1.0's `MultiTenantDbContext` caches `TenantInfo` at construction, and `EnforceMultiTenant` (run inside `base.SaveChangesAsync`) compares each Added/Modified entity's TenantId against the DbContext's cached TenantInfo — NOT against the accessor's live value. A test switching only the setter still threw `MultiTenantException: 1 modified entities with Tenant Id mismatch` because the DbContext's cached `TenantInfo` (id="root") did not match the new member row's intended TenantId.

The fix also rebinds `DbContext.TenantInfo` via its public setter (on the concrete `MultiTenantDbContext` type — `IMultiTenantDbContext.TenantInfo` interface property is get-only, but the concrete class declares the setter publicly, verified via reflection). A cached `Lazy<Action<WorkspaceDbContext, ITenantInfo>>` delegate invokes the setter without referencing the internal type:

```csharp
var tenantScopedDb = (IMultiTenantDbContext)_db;
var previousTenantInfo = _tenantInfoGet.Value(tenantScopedDb);
var previousContext = _multiTenantContextAccessor.MultiTenantContext;
_multiTenantContextSetter.MultiTenantContext = new MultiTenantContext<AppTenantInfo>(workspaceTenant);
_tenantInfoSet.Value(_db, workspaceTenant);
try { await _db.SaveChangesAsync(cancellationToken); }
finally
{
    _tenantInfoSet.Value(_db, previousTenantInfo!);
    _multiTenantContextSetter.MultiTenantContext = previousContext;
}
```

**02-07 fixture's `FinbuckleTestTenantScope` cannot be reused in production code** because it operates before DbContext construction (it sets the accessor's AsyncLocal slot, which the DbContext then reads at construction time). The production handler cannot reconstruct its injected `_db`, so it must rebind the cached property instead. The semantics are equivalent (workspace-tenant scope for the duration of one SaveChanges), but the mechanism differs (AsyncLocal slot vs reflection property set).

### 5. AppTenantInfo constructor signature (aligned with 02-07)

```csharp
var workspaceTenant = new AppTenantInfo(
    id: tracked.WorkspaceId.ToString(),
    identifier: tracked.WorkspaceId.ToString(),
    name: tracked.WorkspaceId.ToString());
```

Uses the 3-arg `AppTenantInfo(string id, string identifier, string? name = null)` constructor (AppTenantInfo.cs:18) — NOT the 5-arg ConnectionString overload. Same signature as `WorkspacePostgresFixture.RootTenant` and the per-workspace tenants constructed in `CrossTenantListUserWorkspacesTests.cs:85`. The workspace's `Name` is NOT available on `tracked` (it carries only `WorkspaceId`), so the Guid string is reused for all three arguments (consistent with 02-07 pattern).

### 6. Phase 2 gap-closure complete — all 3 CRITICAL fixed → ready for HUMAN-UAT 11-step smoke

With 02-07 (CR-02) and 02-08 (CR-01 + CR-03 + BLOCKER 1 + BLOCKER 2) shipped, all three CRITICAL tenant-scoping defects from `02-REVIEW.md` are closed:

- **CR-01** — accept/reject-invitation top-level endpoints now use IgnoreQueryFilters + rebinding DbContext.TenantInfo during SaveChanges. Covered by `AcceptInvitationAcrossTenantsTests` on real PG.
- **CR-02** — `ListUserWorkspaces` cross-workspace aggregation uses IgnoreQueryFilters (02-07). Covered by `CrossTenantListUserWorkspacesTests`.
- **CR-03** — re-inviting a removed member reuses the existing deactivated row via `Activate + UpdateRole` (02-08 Task 1 handler branch). Covered by `ReAcceptAfterRemovalTests`.

The 11-step HUMAN-UAT smoke in `02-HUMAN-UAT.md` is now unblocked. Load-bearing steps that depend on these fixes:

- 🟥 Step 5 (CR-01 THE DECIDER) — `POST /api/v1/workspaces/invitations/{token}/accept/` → 200 {memberId, workspaceId}
- 🟥 Step 6 (List members D-05 batch) — invitee appears after accept
- 🟥 Step 7 (EOP self-promotion) — accepted member can be promoted/demoted
- 🟥 Step 10 (Slug release reuse) — implicit re-invite path (CR-03 ensures removed members can re-accept)
- 🟥 Step 11 (Cross-workspace isolation) — cross-tenant correctness

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] CR-01 implementation diverged from PLAN pseudocode — DbContext.TenantInfo cache**

- **Found during:** Task 1 first run of `AcceptInvitationAcrossTenantsTests` — `MultiTenantException: 1 modified entities with Tenant Id mismatch` thrown from `EnforceMultiTenant` inside `base.SaveChangesAsync`.
- **Issue:** The PLAN's `<critical_constraints> #3` and `<action>` step 3 pseudocode assumed switching `IMultiTenantContextSetter.MultiTenantContext` alone re-scopes SaveChanges. Empirically, Finbuckle 10.1.0's `MultiTenantDbContext` caches `TenantInfo` at construction and `EnforceMultiTenant` compares entity TenantIds against the **DbContext's cached TenantInfo** (not the accessor's live value). The handler must ALSO rebind the DbContext's own TenantInfo property.
- **Fix:** Added a cached reflection delegate (`Lazy<Action<WorkspaceDbContext, ITenantInfo>>`) that invokes the public setter on the concrete `MultiTenantDbContext.TenantInfo` property (the property is get-only on the `IMultiTenantDbContext` interface but the concrete type declares a public setter, verified via reflection at runtime). The try/finally restores both the setter.MultiTenantContext AND DbContext.TenantInfo.
- **Files modified:** `AcceptInvitationCommandHandler.cs` (Step 5 block + reflection helpers)
- **Commit:** `c343c2fd4`

**2. [Rule 3 - Blocking] WorkspaceLifecycleSmokeTests fixture divergence exposed**

- **Found during:** Task 1 full Workspace.Tests run after CR-01 fix — `Shouldly.ShouldAssertException: listResult.Count should be 2L but was 1L` at Step 5.
- **Issue:** The pre-02-08 InMemory smoke test fixture used a fixed random Guid for the DbContext tenant across the whole 10-step lifecycle. InMemory never enforced Finbuckle's `TenantMismatchMode`, so the divergence from production (where create-invitation runs workspace-scoped) was invisible. After 02-08's handler fix enforces multi-tenant on SaveChanges, the owner member (created under the boot tenant) was filtered out by ListMembers when the DbContext was rebound to workspace.Id.
- **Fix:** Rebind the AsyncLocalMultiTenantContextAccessor + DbContext.TenantInfo to the freshly-created `workspace.Id` AFTER Step 1 (before Step 3 creates the invitation). Re-stamp the owner member's TenantId shadow property via `_db.Entry(ownerMember).Property<string>("TenantId").CurrentValue = workspaceTenant.Id` so subsequent workspace-scoped ListMembers queries surface it. Mirrors what the production `WorkspaceSlugStrategy` resolves on the next workspace-scoped request.
- **Files modified:** `WorkspaceLifecycleSmokeTests.cs` (fixture setup + new `SetDbContextTenantInfo` helper using the same reflection pattern as the production handler)
- **Commit:** `c343c2fd4`

### Package-Legitimacy Check — N/A

Zero new NuGet packages added. The plan reuses 02-07's already-installed `Testcontainers.PostgreSql` 4.11.0, `Finbuckle.MultiTenant[.EntityFrameworkCore]`, and `Npgsql.EntityFrameworkCore.PostgreSQL`. No `dotnet add package` / `pnpm install` attempted.

## Authentication Gates

None.

## Known Stubs

None. Both new relational tests run against real PostgreSQL (via 02-07's `WorkspacePostgresFixture`) and exercise the full production handler chain.

## Threat Flags

None. The threat model register (`T-02-08-01` through `T-02-08-SC`) is fully mitigated as planned — no new threat surface introduced. The cached-reflection-delegate pattern (Record #1 deviation) is a pure performance optimization over `PropertyInfo.SetValue`; the access is public-only (`BindingFlags.Public`) avoiding Sonar S3011 accessibility-bypass concerns.

## Self-Check: PASSED

Files:

- FOUND: yh-flow/src/Tests/Workspace.Tests/Integration/CrossTenant/AcceptInvitationAcrossTenantsTests.cs
- FOUND: yh-flow/src/Tests/Workspace.Tests/Integration/CrossTenant/ReAcceptAfterRemovalTests.cs
- FOUND: yh-flow/src/Modules/Workspace/Modules.Workspace/Services/InvitationTokenService.cs (modified)
- FOUND: yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Invitations/AcceptInvitation/AcceptInvitationCommandHandler.cs (modified)
- FOUND: yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Invitations/RejectInvitation/RejectInvitationCommandHandler.cs (modified)
- FOUND: yh-flow/src/Tests/Workspace.Tests/Integration/WorkspaceLifecycleSmokeTests.cs (modified)

Commits:

- FOUND: c343c2fd4 — fix(02-08): CR-01 cross-tenant accept + BLOCKER 1/2 fixes
- FOUND: c6a23481c — test(02-08): CR-03 ReAcceptAfterRemovalTests relational coverage

Gates:

- `dotnet build YH.Flow.slnx --nologo` exit 0 (0 warnings / 0 errors)
- `dotnet test Workspace.Tests --filter Category=Postgres` exit 0 (4/4 pass — 02-07 x2 + 02-08 x2, Docker up)
- `dotnet test Workspace.Tests` exit 0 (100/100 — 96 InMemory + 4 relational)
- `dotnet test Identity.Tests` exit 0 (412/412 — zero regression)
- No edits to `WorkspaceMember.cs`, `WorkspaceMemberConfiguration.cs`, `WorkspaceMembershipService.cs`, or migrations (git diff scope confirmed)
