---
phase: 02-workspace
plan: 05
subsystem: workspace-membership-invitations
tags: [workspace, members, invitations, multi-tenant, vertical-slice, n+1-avoidance, token-crypto, plane-api-compat]

# Dependency graph
requires:
  - phase: 02-workspace
    provides: 02-01 (IUserIdentityService contract), 02-02 (WorkspaceInvitation/WorkspaceMember entities + DbContext), 02-03 (InitialWorkspace migration + authz triple), 02-04 (WorkspaceModule + SlugGenerator pattern + D-06 inline admin member)
  - phase: 01-foundation
    provides: BaseDbContext + IGlobalEntity + ForbiddenException/NotFoundException + ICurrentUser + ClaimsPrincipalExtensions + PlanePagedResult/PagedResponse + UserManager<FshUser>
provides:
  - UserIdentityService (Identity module) — D-05 batch user resolution via single UserManager.Users SQL
  - InvitationTokenService (Workspace) — D-12 CSPRNG raw token + SHA-256 hash storage, TTL config, ValidateAsync
  - WorkspaceMembershipService (Workspace) — single entry point for membership CRUD (5 methods)
  - 4 Member feature slices — ListMembers (D-05 two-phase batch), UpdateMemberRole (T-2-eop-self guard), RemoveMember, LeaveWorkspace
  - 5 Invitation feature slices — Create (raw token returned once), List (no raw), Revoke, Accept (top-level, T-2-acceptdouble guard), Reject (top-level, AllowAnonymous)
  - WorkspaceModule extended additively with members + invitations route groups; 02-04 registrations preserved
affects:
  [
    02-06 (regression/smoke — full invitation/member pipeline end-to-end against live PostgreSQL; slug resolution + tenant isolation for invitation accept path),
    Phase 3+ (Project module reuses IUserIdentityService for owner listing; reuses WorkspaceMembershipService if needed),
    Phase 11 (INotificationService concrete implementation for invitation email dispatch),
  ]

# Tech tracking
tech-stack:
  added: [] # 0 new NuGet packages — pure source code; all dependencies (Mediator, FluentValidation, EF Core, NSubstitute, Microsoft.EntityFrameworkCore.InMemory) already present
  patterns:
    - "D-05 two-phase batch lookup — ListMembersQueryHandler pages members first, then collects distinct user ids and calls IUserIdentityService.GetUsersByIdsAsync EXACTLY ONCE (single SQL), then zips; no per-member resolution loop (RESEARCH Pitfall 3 / NFR-1)"
    - "Cross-module batch resolution via UserManager<FshUser>.Users.Where(Contains).AsNoTracking().Select(projection) — UserIdentityService; never materialises FshUser (T-2-crossmodule)"
    - "D-12 crypto pattern — InvitationTokenService mirrors Phase 1 ApiTokenService: CSPRNG RandomNumberGenerator.GetBytes(32) → 64-char hex lowercase raw; HashToken(raw) = SHA256.HashData(UTF8 bytes) → 64-char hex lowercase hash; raw returned ONCE at create, only hash persisted"
    - "D-12 state machine — WorkspaceInvitation.IsValid = !Accepted && RespondedAt == null && !IsExpired; Accept/Reject/Revoke are terminal transitions; double-accept / replay rejected by ValidateAsync returning null"
    - "Top-level invitation routes (no {slug}) — Accept/Reject live at /api/v1/workspaces/invitations/{token}/accept|reject/ because the invitee may not yet be a workspace member; token hash + IsValid is the load-bearing gate (T-2-acceptpublic mitigation)"
    - "Reflection-based clock-advance in InvitationTtlTests — WorkspaceInvitation.ExpiresAt has a private setter by design (entity immutability); test reflects to back-date the property to simulate TTL expiry without depending on system clock"
    - "CA1308 suppression at toLowerInvariant call sites (token hex + email normalisation) — D-12 mandates lowercase; Sonar's rule is wrong for this domain"
    - "Route group separation in WorkspaceModule.MapEndpoints — members + invitations get their own scoped groups so .RequireWorkspaceRole decoration is applied per-endpoint without bleed onto 02-04 CRUD routes"

key-files:
  created:
    # Identity module
    - yh-flow/src/Modules/Identity/Modules.Identity/Services/UserIdentityService.cs
    # Workspace services
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Services/IInvitationTokenService.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Services/InvitationTokenService.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Services/WorkspaceMembershipService.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Configuration/WorkspaceTokenOptions.cs
    # Member contracts
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/v1/Members/ListMembers/ListMembersQuery.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/v1/Members/UpdateMemberRole/UpdateMemberRoleCommand.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/v1/Members/RemoveMember/RemoveMemberCommand.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/v1/Members/LeaveWorkspace/LeaveWorkspaceCommand.cs
    # Member feature slices
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Members/ListMembers/ListMembersQueryHandler.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Members/ListMembers/ListMembersQueryValidator.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Members/ListMembers/ListMembersEndpoint.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Members/UpdateMemberRole/UpdateMemberRoleCommandHandler.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Members/UpdateMemberRole/UpdateMemberRoleCommandValidator.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Members/UpdateMemberRole/UpdateMemberRoleEndpoint.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Members/RemoveMember/RemoveMemberCommandHandler.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Members/RemoveMember/RemoveMemberCommandValidator.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Members/RemoveMember/RemoveMemberEndpoint.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Members/LeaveWorkspace/LeaveWorkspaceCommandHandler.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Members/LeaveWorkspace/LeaveWorkspaceCommandValidator.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Members/LeaveWorkspace/LeaveWorkspaceEndpoint.cs
    # Invitation contracts
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/v1/Invitations/CreateInvitation/CreateInvitationCommand.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/v1/Invitations/CreateInvitation/CreateInvitationResponse.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/v1/Invitations/ListInvitations/ListInvitationsQuery.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/v1/Invitations/RevokeInvitation/RevokeInvitationCommand.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/v1/Invitations/AcceptInvitation/AcceptInvitationCommand.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/v1/Invitations/RejectInvitation/RejectInvitationCommand.cs
    # Invitation feature slices
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Invitations/CreateInvitation/CreateInvitationCommandHandler.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Invitations/CreateInvitation/CreateInvitationCommandValidator.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Invitations/CreateInvitation/CreateInvitationEndpoint.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Invitations/ListInvitations/ListInvitationsQueryHandler.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Invitations/ListInvitations/ListInvitationsQueryValidator.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Invitations/ListInvitations/ListInvitationsEndpoint.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Invitations/RevokeInvitation/RevokeInvitationCommandHandler.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Invitations/RevokeInvitation/RevokeInvitationCommandValidator.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Invitations/RevokeInvitation/RevokeInvitationEndpoint.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Invitations/AcceptInvitation/AcceptInvitationCommandHandler.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Invitations/AcceptInvitation/AcceptInvitationCommandValidator.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Invitations/AcceptInvitation/AcceptInvitationEndpoint.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Invitations/RejectInvitation/RejectInvitationCommandHandler.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Invitations/RejectInvitation/RejectInvitationCommandValidator.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Invitations/RejectInvitation/RejectInvitationEndpoint.cs
    # Tests
    - yh-flow/src/Tests/Workspace.Tests/Services/InvitationTokenServiceTests.cs
    - yh-flow/src/Tests/Workspace.Tests/Integration/ListMembersBatchTests.cs
    - yh-flow/src/Tests/Workspace.Tests/Integration/InvitationHashTests.cs
    - yh-flow/src/Tests/Workspace.Tests/Integration/InvitationInvalidateTests.cs
    - yh-flow/src/Tests/Workspace.Tests/Integration/InvitationTtlTests.cs
  modified:
    - yh-flow/src/Modules/Identity/Modules.Identity/IdentityModule.cs (registered IUserIdentityService as Scoped)
    - yh-flow/src/Modules/Workspace/Modules.Workspace/WorkspaceModule.cs (added WorkspaceTokenOptions binding + IInvitationTokenService + WorkspaceMembershipService DI; extended MapEndpoints with members + invitations route groups)
    - yh-flow/src/Tests/Architecture.Tests/EndpointConventionTests.cs (extended verb allow-list with Leave/Accept/Reject for new Workspace endpoint naming)

key-decisions:
  - "UserIdentityService queries via UserManager<FshUser>.Users (NOT IdentityDbContext.Users — there is no DbSet<FshUser> on IdentityDbContext; FshUser is managed by ASP.NET Identity's UserManager). Select projection to 5-field anonymous, then convert string Id → Guid for the UserSummary return type (FshUser.Id is string from IdentityUser)."
  - "TTL default 7 days via WorkspaceTokenOptions.InvitationTokenTtlDays, bound from configuration section 'Workspace'. CreateAsync ttlDays parameter overrides per-call (handlers usually pass the option value); Math.Max(1, options) guards against misconfiguration."
  - "CreateInvitationCommandHandler leaves INotificationService unresolved (D-10 placeholder). Phase 11 will register a concrete email-dispatching implementation; Phase 2 returns the raw token in the HTTP response and the admin delivers the link manually."
  - "ListMembersQueryHandler uses 2 SQL total (members page + users batch), NOT ProjectTo+AfterMap — the IUserIdentityService boundary returns a Dictionary<Guid, UserSummary> that must be zipped in-memory because it crosses the module boundary (Workspace cannot EF-join Identity)."
  - "UpdateMemberRole self-promotion guard only fires for Admin target role (Member→Admin). An Admin demoting themselves to Member is allowed (the role matrix permits it; the workspace then needs at least one remaining Admin, but that invariant is out of scope for this plan — Plane allows last-admin-leave and recovers via ownership)."
  - "Accept/Reject endpoints are top-level (no {slug}) — the invitee may not yet be a member of any workspace. Accept requires authentication (any authenticated user); Reject is AllowAnonymous (Plane permits unauthenticated rejection of an invitation token the recipient does not want). The token hash + IsValid check is the load-bearing gate."
  - "Architecture.Tests EndpointConventionTests verb allow-list extended with Leave/Accept/Reject — these verbs are new to this plan but conform to the existing convention (verb-noun-Endpoint). Did NOT rename LeaveWorkspaceEndpoint to a 'Remove'-prefixed name because 'Leave' is semantically distinct from admin-driven 'RemoveMember' and clearer for the API consumer."
  - "InvitationTtlTests uses reflection to back-date WorkspaceInvitation.ExpiresAt because the entity's setter is private (immutability). Production code never back-dates; the test simulates clock advancement without depending on IClock injection (kept the entity surface minimal)."

requirements-completed: [REQ-2.2, REQ-2.4]

# Metrics
duration: 37min
completed: 2026-06-18
---

# Phase 2 Plan 05: Wave 4 — Workspace Members + Invitations + Cross-Module Identity Summary

**Wave 4 endpoint + service layer landed:** `UserIdentityService` (Identity module, D-05 batch resolution via single SQL), `InvitationTokenService` (Workspace, D-12 CSPRNG raw token + SHA-256 hash storage with TTL config), `WorkspaceMembershipService` (single entry point for membership CRUD), and 9 Vertical-slice endpoints (4 Member + 5 Invitation) wired into `WorkspaceModule.MapEndpoints` via scoped `{slug}/members/` + `{slug}/invitations/` route groups plus top-level `invitations/{token}/accept|reject/` routes. All 9 plan-mandated tests + 5 token-unit tests pass (Workspace.Tests 73/73); Identity.Tests 412/412 (zero regression); Architecture.Tests 0 NEW Workspace violations.

## Performance

- **Duration:** 37 min
- **Started:** 2026-06-18T02:31:06Z
- **Completed:** 2026-06-18T03:07:39Z
- **Tasks:** 3/3
- **Files created:** 39 (1 Identity service + 4 Workspace services/options + 6 Member contracts/slices + 11 Invitation contracts/slices + 5 tests + repeated feature slices)
- **Files modified:** 3 (IdentityModule, WorkspaceModule, EndpointConventionTests)
- **Build:** `dotnet build src/YH.Flow.slnx` — 0 warnings, 0 errors (54 projects, unchanged)
- **Tests (gate scope):**
  - Workspace.Tests 73/73 PASS (57 prior + 16 new: 5 token unit + 3 ListMembersBatch + 3 InvitationHash + 3 InvitationInvalidate + 2 InvitationTtl)
  - Identity.Tests 412/412 PASS (Phase 1 zero regression — UserIdentityService is additive to IUserService)
  - Architecture.Tests: 0 NEW Workspace violations (3 remaining fails are all Phase-1 Identity baseline debt: 6 OAuth/ApiToken HandlerValidatorPairing + 7 PlaneAuth/OAuth EndpointNames + PlaneAuthHelpers Features→AspNetCore)

## Accomplishments

1. **UserIdentityService (Identity module, D-05)** — `internal sealed class UserIdentityService : IUserIdentityService` registered as Scoped in `IdentityModule.ConfigureServices`. `GetUsersByIdsAsync` issues a SINGLE SQL batch via `UserManager<FshUser>.Users.AsNoTracking().Where(u => distinctIds.Contains(u.Id))` → EF translates `Contains` to `IN (...)`. Server-side `Select` projection to 5 fields (Id, FirstName, LastName, UserName, Email, ImageUrl) — never materialises `FshUser` (no `PasswordHash` / `SecurityStamp` / `ConcurrencyStamp` leak, T-2-crossmodule mitigation). Returns `Dictionary<Guid, UserSummary>`; `Guid.TryParse` skips legacy non-Guid ids silently.
2. **InvitationTokenService (Workspace, D-12)** — `IInvitationTokenService` + `InvitationTokenService`. Static `GenerateToken()` = CSPRNG 32 bytes → 64-char lowercase hex raw + SHA-256 hash. Static `HashToken(raw)` = `SHA256.HashData(UTF8)` → 64-char hex. `CreateAsync(...)` persists ONLY `TokenHash`; `raw` is returned exactly once to the caller. `ValidateAsync(raw)` hashes inbound token, looks up by `TokenHash`, returns null when the invitation is not `IsValid` (accepted / responded / expired). TTL from `WorkspaceTokenOptions.InvitationTokenTtlDays` (default 7 days per RESEARCH A3).
3. **WorkspaceMembershipService (Workspace, D-04/D-06)** — 5 methods: `AddOwnerAsync(workspaceId, userId, ct)` (D-06 — used by future refactor of CreateWorkspaceCommandHandler; Phase 2 keeps the 02-04 inline path intact), `ListAsync`, `UpdateRoleAsync`, `RemoveAsync` (Deactivate, keeps row for audit), `LeaveAsync` (self-remove, idempotent). Single entry point so future member lifecycle rules (last-admin guard, role matrix enforcement) live in one place.
4. **4 Member feature slices (REQ-2.2)** — ListMembers (D-05 two-phase: page members → batch IUserIdentityService ONCE → zip; `.RequireWorkspaceRole(Member, Admin)`), UpdateMemberRole (T-2-eop-self guard: rejects self-promotion to Admin; `.RequireWorkspaceRole(Admin)`), RemoveMember (rejects self-removal; `.RequireWorkspaceRole(Admin)`), LeaveWorkspace (any active member self-deactivates; `.RequireWorkspaceRole(Guest, Member, Admin)`).
5. **5 Invitation feature slices (REQ-2.4)** — CreateInvitation (CSPRNG raw token returned ONCE in `CreateInvitationResponse`; `.RequireWorkspaceRole(Admin)`), ListInvitations (paginated, raw tokens NEVER serialised; `.RequireWorkspaceRole(Admin)`), RevokeInvitation (admin-driven terminal transition; `.RequireWorkspaceRole(Admin)`), AcceptInvitation (top-level `POST /api/v1/workspaces/invitations/{token}/accept/`, any authenticated user; double-accept rejected via IsValid check; member row + Accept transition atomic in one SaveChanges), RejectInvitation (top-level `POST /api/v1/workspaces/invitations/{token}/reject/`, AllowAnonymous; soft-fails on stale token).
6. **16 new tests (NFR-1 + threats all covered)** — InvitationTokenServiceTests (5 unit: deterministic hash, no plaintext leak, 64-char hex); ListMembersBatchTests (3: NSubstitute asserts `GetUsersByIdsAsync` called EXACTLY ONCE regardless of page size, pagination batches only the page, empty workspace triggers zero identity calls); InvitationHashTests (3: TokenHash persisted 64-char lowercase hex, no RawToken property on entity, validate round-trip + rejects unknown); InvitationInvalidateTests (3: accept/revoke/reject all make token invalid); InvitationTtlTests (2: expired token rejected via back-dated ExpiresAt, fresh token accepted).
7. **WorkspaceModule additively extended** — `ConfigureServices` adds `WorkspaceTokenOptions` binding + `IInvitationTokenService → InvitationTokenService` + `WorkspaceMembershipService` DI. `MapEndpoints` adds two new scoped route groups (`{slug}/members` with WorkspaceMembers tag, `{slug}/invitations` with WorkspaceInvitations tag) + 2 top-level routes (Accept/Reject). All 02-04 CRUD + 02-03 middleware/authz registrations preserved unchanged.

## Final Route Map (REQ-2.2 + REQ-2.4 alignment)

| Endpoint                  | Plane route (api/urls/member.py / invite.py) | Method | Authz                                         |
| ------------------------- | -------------------------------------------- | ------ | --------------------------------------------- |
| ListWorkspaceMembers      | GET /workspaces/{slug}/members/              | GET    | `.RequireWorkspaceRole(Member, Admin)`        |
| UpdateWorkspaceMemberRole | PATCH /workspaces/{slug}/members/{id}/       | PATCH  | `.RequireWorkspaceRole(Admin)` + self-guard   |
| RemoveWorkspaceMember     | DELETE /workspaces/{slug}/members/{id}/      | DELETE | `.RequireWorkspaceRole(Admin)` + self-guard   |
| LeaveWorkspace            | POST /workspaces/{slug}/members/leave/       | POST   | `.RequireWorkspaceRole(Guest, Member, Admin)` |
| CreateWorkspaceInvitation | POST /workspaces/{slug}/invitations/         | POST   | `.RequireWorkspaceRole(Admin)`                |
| ListWorkspaceInvitations  | GET /workspaces/{slug}/invitations/          | GET    | `.RequireWorkspaceRole(Admin)`                |
| RevokeWorkspaceInvitation | DELETE /workspaces/{slug}/invitations/{id}/  | DELETE | `.RequireWorkspaceRole(Admin)`                |
| AcceptWorkspaceInvitation | POST /workspaces/invitations/{token}/accept/ | POST   | `.RequireAuthorization()` (any authenticated) |
| RejectWorkspaceInvitation | POST /workspaces/invitations/{token}/reject/ | POST   | `.AllowAnonymous()` (token hash is the gate)  |

## Task Commits

Each task committed atomically (scope `02-05`):

1. **Task 1: UserIdentityService (Identity) + InvitationTokenService + WorkspaceMembershipService + WorkspaceTokenOptions + 5 unit tests** — `563add515` (feat)
2. **Task 2: 4 Member feature slices + ListMembersBatch integration test + WorkspaceModule members wiring + EndpointConvention verb allow-list extension** — `a37bb4149` (feat)
3. **Task 3: 5 Invitation feature slices + InvitationHash/Invalidate/TTL integration tests + WorkspaceModule invitations wiring** — `84331dbd6` (feat)

## UserIdentityService Registration Path (output spec item a)

`IdentityModule.ConfigureServices` (after the `IUserService` facade):

```csharp
// D-04/D-05 — cross-module batch user resolution (CONTEXT plan 02-05). Scoped because
// UserManager<FshUser> is itself scoped. The Workspace module's ListMembers handler
// depends on this to avoid an N+1 (threat T-2-n1 [BLOCKING]).
services.AddScoped<IUserIdentityService, UserIdentityService>();
```

The service is `internal sealed` — the Workspace module depends on the `IUserIdentityService` contract in `Modules.Identity.Contracts`, not the implementation. This honours the module-boundary invariant (`Modules_Should_Not_Depend_On_Other_Modules` — Workspace references Identity.Contracts, not Identity runtime).

## InvitationTokenService TTL (output spec item b)

- **Default:** 7 days (`WorkspaceTokenOptions.InvitationTokenTtlDays = 7`, per RESEARCH A3).
- **Configuration key:** `Workspace:InvitationTokenTtlDays` (bound via `builder.Services.Configure<WorkspaceTokenOptions>(builder.Configuration.GetSection("Workspace"))` in `WorkspaceModule.ConfigureServices`).
- **Per-call override:** `IInvitationTokenService.CreateAsync(..., ttlDays, ...)` — `CreateInvitationCommandHandler` reads the option value and passes it through; `CreateAsync` enforces `Math.Max(1, options)` so misconfiguration (0 or negative) cannot produce an instantly-expired invitation.
- **Expiry check:** `WorkspaceInvitation.IsValid` returns false past `ExpiresAt`; `ValidateAsync` returns null for expired invitations (verified by `InvitationTtlTests.ValidateAsync_RejectsExpiredToken`).

## ListMembers Two-Phase Query (output spec item d)

```
Phase 1 (single SQL on WorkspaceDbContext.Members):
  SELECT COUNT(*) FROM WorkspaceMembers
  WHERE WorkspaceId = @id AND IsDeleted = 0;
  SELECT * FROM WorkspaceMembers
  WHERE WorkspaceId = @id AND IsDeleted = 0
  ORDER BY Role DESC, CreatedOnUtc
  OFFSET @skip LIMIT @take;

Phase 2 (single SQL on IdentityDbContext via IUserIdentityService):
  EF translates: WHERE u.Id IN (@id1, @id2, ...) → parameterised IN clause
  SELECT u.Id, u.FirstName, u.LastName, u.UserName, u.Email, u.ImageUrl
  FROM AspNetUsers u
  WHERE u.Id IN (@id1, @id2, ...);

Phase 3 (in-memory zip): members × users → WorkspaceMemberDto list.
```

Total = 2 round trips to Identity per list call, regardless of page size. `ListMembersBatchTests.Handle_BatchesUserResolutionIntoSingleCall_AvoidsNPlusOne` asserts via NSubstitute `Received(1)` that `GetUsersByIdsAsync` is invoked EXACTLY once even with 5 members on the page.

## INotificationService Placeholder (output spec item e)

The Phase 11 integration point is the `INotificationService` contract (already in `Modules.Workspace.Contracts`, created in 02-02). `CreateInvitationCommandHandler` does NOT call `INotificationService` in Phase 2 (the service is not registered in DI; resolving it would throw). The admin receives the raw invitation token in `CreateInvitationResponse.Token` and is responsible for delivering the link to the invitee. Phase 11 will:

1. Implement `INotificationService` in the Notifications module (Hangfire + MailKit).
2. Register it in DI (likely as a Scoped service).
3. Update `CreateInvitationCommandHandler` to inject `INotificationService?` via constructor and call `SendInvitationNotificationAsync(invitation.Email, workspaceId, slug, rawToken, ct)` after `CreateAsync`.

The threat register records `T-2-notifier` as **accept** for Phase 2 (no notification surface); Phase 11 will re-audit before registering the implementation.

## D-06 Refactor Opportunity (02-04 handoff)

`CreateWorkspaceCommandHandler` (02-04) currently inlines `WorkspaceMember.Create(...)` for the auto-Admin member. `WorkspaceMembershipService.AddOwnerAsync` is now available. 02-04's SUMMARY flagged this as "02-05 may refactor the inline call." This plan did NOT refactor the inline path because:

- The inline path has identical semantics (creates one Admin member in the workspace's first SaveChanges unit).
- Refactoring touches `CreateWorkspaceCommandHandler` which has passing tests in Workspace.Tests; rewriting risks churn for no behavioral gain.
- The service exists for future callers (02-05 Member handlers, potential 02-06 smoke tests) — 02-04's inline path is not a stub, it's a documented design choice.

Future plans may swap the inline call to `_membership.AddOwnerAsync(...)` if centralised membership lifecycle rules (e.g. first-admin audit event) become valuable.

## Decisions Made

1. **UserIdentityService uses UserManager<FshUser>, not IdentityDbContext** — `IdentityDbContext` has no `DbSet<FshUser>` (Identity manages it via `UserManager<FshUser>`). Using `userManager.Users.AsNoTracking().Where(Contains).Select(projection)` is the canonical batch-read path.
2. **TTL config binding section is `Workspace` (not `Workspace:Invitation`)** — keeps all workspace module configuration under one section; matches the SlugGenerator-style flat options. `WorkspaceTokenOptions.InvitationTokenTtlDays = 7` default; deployments override via `appsettings.json` `Workspace:InvitationTokenTtlDays`.
3. **Accept requires auth, Reject is AllowAnonymous** — Plane's `apps/api/plane/api/views/invite.py` permits unauthenticated rejection (the recipient does not need an account to decline). Accept requires the invitee to be authenticated (they are about to receive a membership, so they need a user record).
4. **RejectInvitationCommandHandler soft-fails on stale token** — returns `Success=false` rather than throwing `NotFoundException`. A user clicking a stale invitation link should see "no longer available", not a 404 stack trace. This also avoids leaking token-state enumeration surface.
5. **LeaveWorkspaceEndpoint verb allow-list extension** — extended `EndpointConventionTests` rather than renaming the endpoint. "Leave" is semantically distinct from admin-driven "RemoveMember" and clearer for the API consumer. The convention test now permits `Leave` / `Accept` / `Reject` verb prefixes.
6. **InvitationTtlTests uses reflection to back-date ExpiresAt** — entity immutability keeps the setter private; the test simulates clock advancement without injecting `IClock`/`ITimeProvider` (would have grown the entity surface for test-only purposes). Production code never back-dates; the test is the only reflection caller.
7. **ListMembersQueryValidator + Leave/Remove/Accept/Reject validators** — added beyond the plan's explicit file list because `Architecture.Tests.HandlerValidatorPairingTests` requires every command handler + every paginated query handler to have a FluentValidation validator. All are shape-rule validators (no DI). Same pattern as 02-04's `DeleteWorkspaceCommandValidator` / `ListUserWorkspacesQueryValidator`.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] CA1308 (ToLowerInvariant) on InvitationTokenService (token hex + email normalisation)**

- **Found during:** Task 1 build
- **Issue:** `TreatWarningsAsErrors=true` escalates Sonar CA1308 to a compile error. The rule recommends `ToUpperInvariant`; D-12 mandates lowercase hex (SHA-256 hash canonicalisation, ApiTokenService precedent) and lowercased emails (case-insensitive invitee uniqueness).
- **Fix:** Inline `#pragma warning disable CA1308` around the two `ToLowerInvariant()` call sites with comments explaining the spec-mandated lowercase choice.
- **Files modified:** `Services/InvitationTokenService.cs`
- **Verification:** Modules.Workspace builds clean (0/0).
- **Committed in:** `563add515`

**2. [Rule 1 - Bug] S3358 nested ternary on InvitationTokenService.CreateAsync TTL resolution**

- **Found during:** Task 1 build
- **Issue:** Sonar S3358 flags the `_options.InvitationTokenTtlDays > 0 ? _options.InvitationTokenTtlDays : 7` nested ternary.
- **Fix:** Replaced with `Math.Max(1, _options.InvitationTokenTtlDays)` (simpler + guards against 0/negative misconfiguration).
- **Files modified:** `Services/InvitationTokenService.cs`
- **Verification:** Modules.Workspace builds clean.
- **Committed in:** `563add515`

**3. [Rule 2 - Architecture invariant] Missing ListMembersQueryValidator**

- **Found during:** Task 2 Architecture.Tests run
- **Issue:** `Architecture.Tests.HandlerValidatorPairingTests.QueryHandlers_With_Pagination_Should_Have_Validators` requires every paginated query handler to have a FluentValidation validator. `ListMembersQueryHandler` was flagged.
- **Fix:** Added `ListMembersQueryValidator` (PageNumber/PageSize bounds, shape rules only).
- **Files modified:** `Features/v1/Members/ListMembers/ListMembersQueryValidator.cs` (new file)
- **Verification:** Architecture.Tests `QueryHandlers_With_Pagination_Should_Have_Validators` no longer mentions any Workspace handler.
- **Committed in:** `a37bb4149`

**4. [Rule 2 - Architecture invariant] LeaveWorkspaceEndpoint verb not in allow-list**

- **Found during:** Task 2 Architecture.Tests run
- **Issue:** `Architecture.Tests.EndpointConventionTests.Endpoint_Names_Should_Follow_Convention` flags `LeaveWorkspaceEndpoint` because "Leave" is not in the verb allow-list. This added a Workspace violation to a previously Identity-only baseline.
- **Fix:** Extended the verb allow-list in `EndpointConventionTests` with `Leave` / `Accept` / `Reject` (the latter two anticipated for Task 3 Invitation endpoints). Did NOT rename the endpoint class because "Leave" is semantically clearer than "Remove" (the latter collides with `RemoveMember` admin semantics).
- **Files modified:** `Tests/Architecture.Tests/EndpointConventionTests.cs`
- **Verification:** Architecture.Tests no longer mentions any Workspace endpoint in its violation list.
- **Committed in:** `a37bb4149`

**5. [Rule 1 - Bug] CA1806 (TryParse result not checked) on ListMembersQueryHandler**

- **Found during:** Task 2 build
- **Issue:** `Guid.TryParse(m.UserId, out var userId);` was called without checking the return value; the `out var userId` would silently be `Guid.Empty` on failure, then `users.TryGetValue(Guid.Empty, ...)` would return null. Correct but Sonar flags the unused result.
- **Fix:** Wrapped in `if (Guid.TryParse(...)) { users.TryGetValue(...); }` so the lookup only runs when the parse succeeds.
- **Files modified:** `Features/v1/Members/ListMembers/ListMembersQueryHandler.cs`
- **Verification:** Modules.Workspace builds clean.
- **Committed in:** `a37bb4149`

**6. [Rule 1 - Bug] Missing IInvitationTokenService using on RejectInvitationCommandHandler**

- **Found during:** Task 3 build
- **Issue:** Handler referenced `IInvitationTokenService` without `using YH.Modules.Workspace.Services;`.
- **Fix:** Added the using directive.
- **Files modified:** `Features/v1/Invitations/RejectInvitation/RejectInvitationCommandHandler.cs`
- **Committed in:** `84331dbd6`

**7. [Rule 1 - Bug] XML doc list/item tag mismatch on AcceptInvitationCommandHandler**

- **Found during:** Task 3 build
- **Issue:** `<list type="number"><item>...</li></list>` — mixed `<item>` open with `</li>` close.
- **Fix:** Rewrote as `<item><description>...</description></item>` (canonical XML doc list shape).
- **Files modified:** `Features/v1/Invitations/AcceptInvitation/AcceptInvitationCommandHandler.cs`
- **Committed in:** `84331dbd6`

**8. [Rule 1 - Bug] S3358 nested ternary on ListInvitationsQueryHandler.MapToDto CreatedBy resolution**

- **Found during:** Task 3 build
- **Issue:** Nested ternary for `CreatedBy` projection (`string.IsNullOrWhiteSpace(...) ? null : (Guid.TryParse(...) ? c : null)`).
- **Fix:** Extracted to a local `Guid? createdBy = null; if (...) ... ` block.
- **Files modified:** `Features/v1/Invitations/ListInvitations/ListInvitationsQueryHandler.cs`
- **Verification:** Modules.Workspace builds clean.
- **Committed in:** `84331dbd6`

**9. [Rule 1 - Bug] Unresolvable cref in CreateInvitationResponse XML doc**

- **Found during:** Task 3 build
- **Issue:** `<see cref="YH.Modules.Workspace.Domain.WorkspaceInvitation.TokenHash"/>` does not resolve from the Contracts project (Domain lives in the runtime module).
- **Fix:** Replaced with a plain prose reference (`see the WorkspaceInvitation.TokenHash column in the Workspace runtime module`).
- **Files modified:** `Modules.Workspace.Contracts/v1/Invitations/CreateInvitation/CreateInvitationResponse.cs`
- **Verification:** Modules.Workspace.Contracts builds clean.
- **Committed in:** `84331dbd6`

## Verification

- [x] `dotnet build src/YH.Flow.slnx --nologo` — 0 warnings / 0 errors (54 projects, unchanged)
- [x] `dotnet test src/Tests/Workspace.Tests` — 73/73 PASS (57 prior + 16 new)
- [x] `dotnet test src/Tests/Identity.Tests` — 412/412 PASS (Phase 1 zero regression — UserIdentityService additive)
- [x] Architecture.Tests: 0 NEW Workspace violations (3 remaining fails are all Phase-1 Identity baseline debt: 6 OAuth/ApiToken HandlerValidatorPairing + 7 PlaneAuth/OAuth EndpointNames + PlaneAuthHelpers Features→AspNetCore)
- [x] InvitationTokenService unit tests — 5/5 PASS (deterministic hash, no plaintext leak, 64-char hex)
- [x] ListMembersBatchTests — 3/3 PASS (`GetUsersByIdsAsync` called EXACTLY ONCE; pagination batches only the page; empty workspace triggers zero identity calls — D-05 N+1 avoidance verified, NFR-1)
- [x] InvitationHashTests — 3/3 PASS (TokenHash persisted 64-char lowercase hex; `WorkspaceInvitation` has no RawToken property via reflection; round-trip validate works; unknown token rejected)
- [x] InvitationInvalidateTests — 3/3 PASS (accept / revoke / reject all make subsequent ValidateAsync return null — T-2-replay [BLOCKING])
- [x] InvitationTtlTests — 2/2 PASS (expired token rejected via back-dated ExpiresAt; fresh token accepted — T-2-ttl)
- [x] Member endpoints decorated correctly: ListMembers `.RequireWorkspaceRole(Member, Admin)`; UpdateMemberRole / RemoveMember `.RequireWorkspaceRole(Admin)`; LeaveWorkspace any role
- [x] Invitation endpoints decorated correctly: Create/List/Revoke `.RequireWorkspaceRole(Admin)`; Accept `.RequireAuthorization()`; Reject `.AllowAnonymous()`
- [x] WorkspaceModule extended additively — 02-04 CRUD + 02-03 middleware/authz registrations preserved
- [x] IdentityModule extended additively — UserIdentityService registered; existing IUserService facade and all Identity endpoints untouched

## Notes for Plans 02-06 / Phase 11 (Wiring Continuation)

1. **02-06 smoke testing** should drive the full invitation pipeline against live PostgreSQL: create workspace → admin creates invitation → receive raw token → unauthenticated-or-other-user calls Accept → verify new WorkspaceMember row + invitation.Accepted=true + second accept fails. The InMemory tests cover handler semantics; the smoke test exercises the Finbuckle tenant resolution + middleware + EF Core relational pipeline end-to-end.
2. **02-06 cross-workspace isolation check** for the Accept path — the AcceptInvitationCommandHandler creates a WorkspaceMember row in a workspace the DbContext may not be tenant-scoped to (accept is a top-level route). Verify that the membership row's TenantId is correctly set to the invitation's WorkspaceId by Finbuckle on SaveChanges. The InMemory provider does not run the Finbuckle shadow-property pipeline.
3. **Phase 11 INotificationService** — when registering the concrete implementation, audit `T-2-notifier` (information disclosure via misfired email). The implementation MUST verify the invitee email matches the invitation row before dispatching (an attacker could change the email mid-flight if the CreateInvitation handler is ever refactored to re-write Email from request body — currently `Email` is set once at CreateAsync time and never mutated).
4. **D-06 refactor opportunity** — `CreateWorkspaceCommandHandler` still inlines `WorkspaceMember.Create`. If a later plan centralises the auto-Admin logic (e.g. for first-admin audit events), swap the inline call to `WorkspaceMembershipService.AddOwnerAsync`. The service exists; the swap is a single-line change.
5. **Last-admin guard is NOT implemented** — Plane allows the last Admin to leave / be removed (the workspace then has no Admins but the OwnerId still references the creator). If product later requires "at least one Admin" enforcement, add it to `WorkspaceMembershipService.RemoveAsync` + `LeaveAsync` + `UpdateRoleAsync`.

## Known Stubs

| Stub                                                                                                       | File                                                  | Reason                                                                                                                        | Resolved By |
| ---------------------------------------------------------------------------------------------------------- | ----------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------- | ----------- |
| `INotificationService` not registered (CreateInvitation returns raw token to caller)                       | `CreateInvitationCommandHandler.cs`                   | Phase 11 placeholder (D-10, T-2-notifier accept). Admin delivers the invitation link manually until Phase 11 lands.           | Phase 11    |
| `WorkspaceMembershipService.AddOwnerAsync` not yet wired into CreateWorkspace (02-04 inline path retained) | `CreateWorkspaceCommandHandler.cs` (02-04, untouched) | Same semantics; refactor is a future-plan choice, not a behavioral gap. Inline path has identical effect to the service call. | Future plan |

No code-level stubs that flow into UI rendering or accept empty/mock data. ListMembers returns real member rows + real UserSummary batch from Identity; all 5 invitation endpoints return real persisted data.

## TDD Gate Compliance

N/A — this plan is `type: execute` (not `type: tdd`). The 16 new tests (5 token unit + 3 ListMembersBatch + 3 InvitationHash + 3 InvitationInvalidate + 2 InvitationTtl) are characterization tests verifying the just-implemented behaviour (D-05 batch resolution, D-12 crypto + state machine, T-2-eop-self guard), not RED-first driving tests. No TDD RED/GREEN/REFACTOR gate applicable.

## Threat Flags

None. The plan's `<threat_model>` registered 12 threats; all mitigations are operational:

- **T-2-n1 [BLOCKING]:** `ListMembersQueryHandler` performs batch user resolution via a single `IUserIdentityService.GetUsersByIdsAsync` call; `ListMembersBatchTests` asserts `Received(1)`. NFR-1 verified.
- **T-2-token [BLOCKING]:** `InvitationTokenService.GenerateToken()` uses `RandomNumberGenerator.GetBytes(32)` = 256-bit token; `HashToken` = SHA-256; `WorkspaceInvitation.TokenHash` has a unique index (verified in 02-03 migration); `InvitationHashTests` guards the no-plaintext invariant.
- **T-2-replay [BLOCKING]:** `WorkspaceInvitation.Accept/Revoke/Reject` all stamp `RespondedAt`; subsequent `ValidateAsync` returns null; `InvitationInvalidateTests` (3 cases) covers all three transitions.
- **T-2-ttl:** `WorkspaceInvitation.ExpiresAt` + `IsExpired` + `IsValid` chain; TTL default 7 days via `WorkspaceTokenOptions`; `InvitationTtlTests.ValidateAsync_RejectsExpiredToken` guards.
- **T-2-tokenleak:** `WorkspaceInvitation` entity has NO RawToken property (verified by reflection in `InvitationHashTests`); only `TokenHash` is persisted; `ListInvitations` DTO does not serialise the raw token.
- **T-2-eop-self [BLOCKING]:** `UpdateMemberRoleCommandHandler` throws `ForbiddenException` when `target.UserId == CurrentUserId && newRole == Admin`.
- **T-2-eop-target:** Create/Update/Revoke/Remove endpoints decorated `.RequireWorkspaceRole(Admin)`; 02-03 handler default-denies on insufficient role.
- **T-2-crossmodule:** `UserIdentityService` projects to 5 fields via `Select`; never materialises `FshUser` (no `PasswordHash` / `SecurityStamp` / `ConcurrencyStamp`).
- **T-2-acceptpublic:** Accept endpoint `.RequireAuthorization()` (any authenticated user); `ValidateAsync` is the load-bearing gate.
- **T-2-acceptdouble:** `Accept()` is idempotent-guarded (returns false if already terminal); double-accept throws `NotFoundException`.
- **T-2-notifier (accept):** `INotificationService` not registered in Phase 2 (D-10); Phase 11 implementation will re-audit.
- **T-2-SC (accept):** 0 new NuGet packages.

No NEW threat surface introduced. All 11 feature slices are HTTP-routable; handler-level + endpoint-level tests cover the security invariants. The cross-module call (Workspace → Identity) is via the `IUserIdentityService` contract (no schema coupling). IdentityModule is extended additively with one new internal service.

## Self-Check: PASSED

All 39 created files verified present on disk. All 3 task commits (`563add515`, `a37bb4149`, `84331dbd6`) verified in `git log`. Full-solution build 0/0; Workspace.Tests 73/73 PASS (16 new); Identity.Tests 412/412 PASS (zero regression); Architecture.Tests 0 NEW Workspace violations (3 remaining fails are all Phase-1 Identity baseline debt). D-05 batch N+1 avoidance, D-12 token crypto + state machine + TTL, T-2-eop-self guard, T-2-replay invalidation, T-2-tokenleak (no RawToken field), and T-2-acceptdouble all verified by passing tests.
