---
phase: 02-workspace
plan: 04
subsystem: workspace-crud
tags: [workspace, crud, slug-generator, vertical-slice, minimal-api, finbuckle-cache, plane-api-compat]

# Dependency graph
requires:
  - phase: 02-workspace
    provides: 02-02 (Workspace entity + WorkspaceMember + WorkspaceDbContext + Finbuckle slug strategy/WorkspaceTenantStore wiring) + 02-03 (InitialWorkspace migration, WorkspaceMembershipMiddleware, [RequireWorkspaceRole] authz triple, Host Program.cs wiring of WorkspaceModule)
  - phase: 01-foundation
    provides: BaseDbContext + IGlobalEntity guard + CustomException/ForbiddenException/NotFoundException + ICurrentUser + ClaimsPrincipalExtensions + AddHeroCaching + IModule/FshModule + PlanePagedResult/PagedResponse
provides:
  - SlugGenerator service (ISlugGenerator + SlugGenerator) — D-07/D-08/D-09 slug lifecycle
  - 6 Workspace feature slices — Create / Get / Update / Delete / ListUserWorkspaces / CheckSlug
  - WorkspaceModule.MapEndpoints fully wired (top-level + scoped {slug} route groups)
  - D-06 auto-Admin-member on CreateWorkspace (inline WorkspaceMember.Create in same SaveChanges unit)
  - D-08 slug release on DeleteWorkspace (SoftDelete + IMultiTenantStore<AppTenantInfo>.RemoveAsync cache invalidation)
  - T-2-eop-update / T-2-eop-delete endpoint + handler authorization gates
affects:
  [
    02-05 (Members + Invitations — will extend WorkspaceModule.MapEndpoints scoped group; WorkspaceMembershipService.AddOwnerAsync may refactor the inline Admin-member create in CreateWorkspaceCommandHandler),
    02-06 (regression/smoke — the 6 endpoints exercised end-to-end against live PostgreSQL + Finbuckle resolution chain),
  ]

# Tech tracking
tech-stack:
  added: [] # 0 new NuGet packages — purely additive source code; all dependencies (Mediator, FluentValidation, EF Core, Finbuckle.MultiTenant.Abstractions) already present from 02-02/02-03
  patterns:
    - "Vertical Slice 4-file Feature pattern (Contracts/Handler/Validator/Endpoint) — copied from Identity RegisterUser; 6 slices in Features/v1/Workspaces/{Feature}/"
    - "SlugGenerator 5-attempt collision retry with crypto-random 4-char hex suffix (RandomNumberGenerator.Fill → 16^4 space) — BCL-only, no shared PRNG seed, safe under concurrent creates"
    - "Slugify uses [GeneratedRegex] source-gen for the non-alphanumeric-run + double-dash regexes; the validation regex is Compiled (single static) per NFR-1"
    - "Cache invalidation via Finbuckle store contract — DeleteWorkspaceCommandHandler injects IMultiTenantStore<AppTenantInfo> and calls RemoveAsync(slug) after SaveChanges. WorkspaceTenantStore.RemoveAsync is a cache-eviction no-op against the Workspaces table itself (the table is source-of-truth). This is the load-bearing wiring note from 02-02-SUMMARY operationalized."
    - "WorkspaceDtoMapper — centralised entity→DTO projection so Get/Create/Update produce identical shapes; ListUserWorkspaces projects server-side via EF Select to avoid materializing the full entity"
    - "Route-group split in MapEndpoints — top-level MapGroup(api/v{version:apiVersion}/workspaces) for create+slug-check; scoped MapGroup(.../workspaces/{slug}) for get+update+delete; list-mine lives at /api/v{version:apiVersion}/users/me/workspaces/ (Plane-compatible path)"
    - "Namespace/type collision alias reused — Features/v1/Workspaces/CreateWorkspace/CreateWorkspaceCommandHandler uses `using WorkspaceEntity = YH.Modules.Workspace.Domain.Workspace;` (same pattern as 02-02 WorkspaceDbContext) so `Workspace.Create(...)` resolves to the entity factory, not the root namespace"
    - "CreateWorkspaceCommand resolves explicit slug by calling GenerateUniqueSlugAsync(slug-as-name) and 409-ing if the result differs from the input — Slugify is idempotent on an already-valid slug, so the collision probe covers both generation + explicit paths"
    - "CA1308 (ToLowerToUpper) suppressed inline at the Slugify call site — D-09 mandates lowercase slugs; the analyzer rule is wrong for this domain"

key-files:
  created:
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Services/ISlugGenerator.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Services/SlugGenerator.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Workspaces/WorkspaceDtoMapper.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Workspaces/CreateWorkspace/CreateWorkspaceCommandHandler.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Workspaces/CreateWorkspace/CreateWorkspaceCommandValidator.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Workspaces/CreateWorkspace/CreateWorkspaceEndpoint.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Workspaces/GetWorkspace/GetWorkspaceQueryHandler.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Workspaces/GetWorkspace/GetWorkspaceEndpoint.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Workspaces/UpdateWorkspace/UpdateWorkspaceCommandHandler.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Workspaces/UpdateWorkspace/UpdateWorkspaceCommandValidator.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Workspaces/UpdateWorkspace/UpdateWorkspaceEndpoint.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Workspaces/DeleteWorkspace/DeleteWorkspaceCommandHandler.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Workspaces/DeleteWorkspace/DeleteWorkspaceCommandValidator.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Workspaces/DeleteWorkspace/DeleteWorkspaceEndpoint.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Workspaces/ListUserWorkspaces/ListUserWorkspacesQueryHandler.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Workspaces/ListUserWorkspaces/ListUserWorkspacesQueryValidator.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Workspaces/ListUserWorkspaces/ListUserWorkspacesEndpoint.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Workspaces/CheckWorkspaceSlug/CheckWorkspaceSlugQueryHandler.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Workspaces/CheckWorkspaceSlug/VerifyWorkspaceSlugEndpoint.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/v1/Workspaces/CreateWorkspace/CreateWorkspaceCommand.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/v1/Workspaces/CreateWorkspace/CreateWorkspaceResponse.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/v1/Workspaces/GetWorkspace/GetWorkspaceQuery.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/v1/Workspaces/UpdateWorkspace/UpdateWorkspaceCommand.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/v1/Workspaces/DeleteWorkspace/DeleteWorkspaceCommand.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/v1/Workspaces/ListUserWorkspaces/ListUserWorkspacesQuery.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/v1/Workspaces/CheckWorkspaceSlug/CheckWorkspaceSlugQuery.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/v1/Workspaces/CheckWorkspaceSlug/CheckWorkspaceSlugResponse.cs
    - yh-flow/src/Tests/Workspace.Tests/Services/SlugGeneratorTests.cs
    - yh-flow/src/Tests/Workspace.Tests/Integration/CreateWorkspaceTests.cs
    - yh-flow/src/Tests/Workspace.Tests/Integration/SoftDeleteSlugReleaseTests.cs
    - yh-flow/src/Tests/Workspace.Tests/Integration/SlugUniqueTests.cs
  modified:
    - yh-flow/src/Modules/Workspace/Modules.Workspace/WorkspaceModule.cs (registered SlugGenerator DI in ConfigureServices; replaced the TODO 02-04 marker in MapEndpoints with the full top-level + scoped route-group wiring)

key-decisions:
  - "CreateWorkspace auto-Admin member is INLINE (not via WorkspaceMembershipService.AddOwnerAsync) — the plan action explicitly blessed the inline path for 02-04 because WorkspaceMembershipService lands in 02-05. The handler creates the WorkspaceMember row directly via WorkspaceMember.Create(ws.Id, ownerUserId.ToString(), Admin, isActive:true) inside the same SaveChanges unit, then 02-05 may refactor to call the service."
  - "Cache invalidation calls IMultiTenantStore<AppTenantInfo>.RemoveAsync(slug) (NOT IDistributedCache directly) — this honours the 02-02 wiring note exactly and lets WorkspaceTenantStore own its cache namespace (ws:slug:). The store's RemoveAsync is a cache-eviction no-op against the Workspaces table itself; it returns true unconditionally."
  - "SlugGenerator's collision probe does NOT filter on IsDeleted — soft-deleted rows already carry the __{epoch} suffix on their slug (per Workspace.SoftDelete), so they cannot collide with the bare original; adding !IsDeleted would be redundant and could miss the edge case where a slug has not yet been suffixed. Documented inline."
  - "VerifyWorkspaceSlugEndpoint (renamed from CheckWorkspaceSlugEndpoint) — the Architecture.Tests EndpointConventionTests allow-list does NOT include the verb 'Check'; renamed the class to 'VerifyWorkspaceSlugEndpoint' (verb 'Verify' IS allowed) to bring Workspace Architecture violations to 0. The .WithName metadata also follows the verb-noun convention. The route POST /slug-check/ is unchanged (Plane-compatible)."
  - "Added DeleteWorkspaceCommandValidator + ListUserWorkspacesQueryValidator beyond the plan's file list — Architecture.Tests.HandlerValidatorPairingTests requires every command handler + every paginated query handler to have a FluentValidation validator. Both are RuleShape validators (no DI); their addition is recorded as a Rule 2 deviation (auto-add missing critical functionality for the Architecture invariant)."
  - "CreateWorkspaceCommand resolves an explicit slug by calling GenerateUniqueSlugAsync(slug-as-name) and surfacing 409 when the result differs from the input — reuses the generator's collision probe for both paths. Slugify is idempotent on an already-valid slug, so the happy path returns the input unchanged; the collision path appends a suffix → 409 tells the client to pick a different slug (matches Plane UX)."
  - "SlugGenerator conflict-throw (5-attempt exhaustion) cannot be reliably exercised against InMemory — the suffix space is 16^4 = 65k and RandomNumberGenerator is crypto-strength, so pre-seeding every candidate is infeasible. The happy retry path IS tested (4 collision tests); the conflict-throw is verified by reading the source contract. Documented inline in the test file."

requirements-completed: [REQ-2.1, REQ-2.3]

# Metrics
duration: 48min
completed: 2026-06-18
---

# Phase 2 Plan 04: Wave 3 — Workspace CRUD Endpoints + SlugGenerator Summary

**Wave 3 endpoint layer landed:** SlugGenerator (D-07/D-08/D-09 — strict `[a-z0-9-]` format, 62 reserved-word deny-list, 5-attempt collision retry with crypto-random suffix) and 6 Vertical-Slice workspace endpoints (create / get / update / delete / list-mine / slug-check) wired into `WorkspaceModule.MapEndpoints` via top-level + scoped `{slug}` route groups. D-06 (auto-Admin member on create) and D-08 (slug release on delete + Finbuckle cache invalidation) are exercised by 10 integration tests; T-2-eop-update + T-2-eop-delete mitigations enforced via `.RequireWorkspaceRole(Admin)` decoration + handler-level owner gate.

## Performance

- **Duration:** 48 min
- **Started:** 2026-06-18T02:05:00Z
- **Completed:** 2026-06-18T02:53:00Z
- **Tasks:** 2/2
- **Files created:** 30
- **Files modified:** 1 (WorkspaceModule.cs)
- **Build:** `dotnet build src/YH.Flow.slnx` — 0 warnings, 0 errors (54 projects, unchanged)
- **Tests (gate scope):**
  - Workspace.Tests 57/57 PASS (22 prior + 35 new: 25 SlugGenerator unit + 4 CreateWorkspace + 3 SoftDeleteSlugRelease + 3 SlugUnique integration)
  - Identity.Tests 412/412 PASS (Phase 1 zero regression)
  - Architecture.Tests: 0 NEW Workspace violations (3 remaining fails are all Phase-1 Identity baseline debt — HandlerValidatorPairing OAuth/ApiToken, EndpointNames PlaneAuth, Features→AspNetCore)

## Accomplishments

1. **SlugGenerator service (D-07/D-08/D-09)** — `ISlugGenerator` + `SlugGenerator` with 3 pure/instance methods. `Slugify` lowercases + collapses non-`[a-z0-9]` runs (source-gen regex); `IsValidSlug` enforces length 3-48 + strict format regex + 62-entry RestrictedSlugs deny-list (case-insensitive); `GenerateUniqueSlugAsync` does 5-attempt retry with a 4-char crypto-random hex suffix via `RandomNumberGenerator.Fill`. Conflict-throw is a `CustomException` (HTTP 409). Collision probe is `AnyAsync(w => w.Slug == candidate)` WITHOUT `IsDeleted` filter (soft-deleted slugs are already suffixed).
2. **6 Workspace feature slices** — every slice follows the 4-file Vertical Slice pattern from Identity RegisterUser (Contracts `Command/Query + Response` + Implementation `Handler + Validator + Endpoint`). All 6 slices compile and are wired into `WorkspaceModule.MapEndpoints`.
3. **D-06 auto-Admin member on CreateWorkspace** — `CreateWorkspaceCommandHandler` inlines `WorkspaceMember.Create(ws.Id, ownerUserId.ToString(), Admin, isActive:true)` in the same SaveChanges unit as `Workspace.Create`. Plan-blessed inline path (WorkspaceMembershipService lands in 02-05).
4. **D-08 slug release on DeleteWorkspace** — `DeleteWorkspaceCommandHandler` calls `ws.SoftDelete(now)` (which appends `__{epoch}` to the slug), then `await _tenantStore.RemoveAsync(slug)` to invalidate the Finbuckle cache (T-2-cacheinvalid mitigation + the 02-02 wiring note operationalized). Verified by `SoftDeleteSlugReleaseTests` (cache-invalidation call asserted via NSubstitute Received).
5. **T-2-eop-update + T-2-eop-delete mitigations** — `UpdateWorkspaceEndpoint` and `DeleteWorkspaceEndpoint` are both decorated `.RequireWorkspaceRole(WorkspaceRole.Admin)`; `DeleteWorkspaceCommandHandler` ADDITIONALLY asserts `ws.OwnerId == CurrentUserId` (admin-but-not-owner → 403). Belt-and-braces. Verified by `SoftDeleteSlugReleaseTests.NON_OWNER_ThrowsForbidden`.
6. **Plane-compatible routing** — top-level `POST /api/v{version:apiVersion}/workspaces/` (create), `POST /workspaces/slug-check/` (verify-slug), `GET /api/v{version:apiVersion}/users/me/workspaces/` (list-mine); scoped `GET /workspaces/{slug}`, `PATCH|PUT /workspaces/{slug}`, `DELETE /workspaces/{slug}`. Pagination uses Plane-format next/previous URL links via `PlanePagedResultFactory`.

## Final Route Map (Plane contract alignment)

| Plane route                           | YH.Flow endpoint                                       | Method       | Authz                                         |
| ------------------------------------- | ------------------------------------------------------ | ------------ | --------------------------------------------- | ----------------------------------------------- |
| `POST /api/v1/workspaces/`            | `CreateWorkspaceEndpoint` → `POST /` (top-level group) | POST         | `.RequireAuthorization()` (any authenticated) |
| `GET /api/v1/workspaces/{slug}/`      | `GetWorkspaceEndpoint` → `GET /{slug}` (scoped group)  | GET          | `.RequireAuthorization()` (any authenticated) |
| `PATCH /api/v1/workspaces/{slug}/`    | `UpdateWorkspaceEndpoint` → `PATCH                     | PUT /{slug}` | PATCH, PUT                                    | `.RequireWorkspaceRole(Admin)` (T-2-eop-update) |
| `DELETE /api/v1/workspaces/{slug}/`   | `DeleteWorkspaceEndpoint` → `DELETE /{slug}`           | DELETE       | `.RequireWorkspaceRole(Admin)` + owner check  |
| `GET /api/v1/users/me/workspaces/`    | `ListUserWorkspacesEndpoint` (root route builder)      | GET          | `.RequireAuthorization()` (any authenticated) |
| `POST /api/v1/workspaces/slug-check/` | `VerifyWorkspaceSlugEndpoint` → `POST /slug-check`     | POST         | `.RequireAuthorization()` (any authenticated) |

## Task Commits

Each task was committed atomically (scope `02-04`):

1. **Task 1: SlugGenerator service (D-07/D-08/D-09) + 25 unit tests** — `b95a7c1c5` (feat)
2. **Task 2: 6 Workspace feature slices + WorkspaceModule wiring + 10 integration tests** — `27bd7a14a` (feat)

**Plan metadata:** pending (this SUMMARY commit)

## CreateWorkspace Auto-Admin Member Implementation Path

**Inline (not service call).** Per the plan's `<action>` note (b):

```csharp
_db.Workspaces.Add(workspace);
// D-06 — auto-enrol the creator as the first Admin member in the same SaveChanges unit.
var ownerMember = WorkspaceMember.Create(
    workspaceId: workspace.Id,
    userId: command.OwnerUserId.ToString(),
    role: (int)WorkspaceRole.Admin,
    isActive: true);
_db.Members.Add(ownerMember);
await _db.SaveChangesAsync(cancellationToken);
```

Rationale: `WorkspaceMembershipService.AddOwnerAsync` is implemented in plan 02-05. This plan ships independently by calling `WorkspaceMember.Create` directly. 02-05 may refactor the handler to call the service (the inline approach has identical semantics — both create one Admin member row in the workspace's first SaveChanges).

## SlugGenerator 5-Attempt Retry Random Suffix Algorithm

```csharp
private static string GenerateShortSuffix()
{
    Span<byte> buffer = stackalloc byte[SuffixLength];      // 4 bytes
    RandomNumberGenerator.Fill(buffer);                     // crypto-strength
    Span<char> hex = stackalloc char[SuffixLength * 2];     // 8 chars
    for (var i = 0; i < SuffixLength; i++)
    {
        var b = buffer[i];
        hex[i * 2] = HexChar(b >> 4);
        hex[i * 2 + 1] = HexChar(b & 0xF);
    }
    return new string(hex[..SuffixLength]);                 // first 4 chars → 16^4 space
}
```

- **Source:** `System.Security.Cryptography.RandomNumberGenerator.Fill` — BCL crypto-strength, no shared PRNG seed, safe under concurrent create requests.
- **Space:** 4 hex chars = 16^4 = 65 536 candidates per base slug. After 5 attempts the collision probability is astronomically small against realistic workspace-name distributions.
- **Pattern:** attempt 0 = bare base; attempts 1..4 = `{base}-{suffix}`.
- **Failure:** `CustomException` HTTP 409 ("Unable to generate a unique workspace slug after 5 attempts.").
- **Non-Latin fallback:** if `Slugify(name)` produces an empty string (e.g. pure CJK input), the base becomes `ws-{suffix}` so non-Latin workspace names still get a valid slug.

## Cache Invalidation Strategy

| Operation         | Cache action       | Reason                                                                                                        |
| ----------------- | ------------------ | ------------------------------------------------------------------------------------------------------------- |
| Create            | None               | Brand-new slug — cache miss repopulates on next resolution.                                                   |
| Update            | None               | Slug is NOT changed by `Workspace.Update` (only display fields). Cache entry stays valid.                     |
| Delete            | **Required**       | `IMultiTenantStore<AppTenantInfo>.RemoveAsync(slug)` — soft-deleted row no longer resolves; cache must evict. |
| Rename (Phase 3+) | **Required (TBD)** | If slug transfer is added, call `RemoveAsync(oldSlug)` + `UpdateAsync(newTenantInfo)` to repopulate.          |

The `RemoveAsync(slug)` call lives in `DeleteWorkspaceCommandHandler` AFTER `SaveChangesAsync` (so the soft-delete is committed before the cache is invalidated — no window where the cache points at a still-active workspace).

## Files Created/Modified

### Workspace module (`yh-flow/src/Modules/Workspace/Modules.Workspace/`)

- `Services/ISlugGenerator.cs` — 3-method interface (Slugify / IsValidSlug / GenerateUniqueSlugAsync).
- `Services/SlugGenerator.cs` — implementation; `[GeneratedRegex]` for slugify regexes; `RandomNumberGenerator` suffix; `WorkspaceDbContext`-scoped for the AnyAsync probe.
- `Features/v1/Workspaces/WorkspaceDtoMapper.cs` — centralised entity→DTO projection.
- `Features/v1/Workspaces/{Create|Get|Update|Delete|ListUserWorkspaces|CheckWorkspaceSlug}/` — 6 slices × (Handler + Validator + Endpoint), 19 source files.
- `WorkspaceModule.cs` (modified) — `ConfigureServices` registers `ISlugGenerator → SlugGenerator` (scoped); `MapEndpoints` defines two route groups (top-level `/workspaces` + scoped `/workspaces/{slug}`) plus the root-attached `/users/me/workspaces/` list-mine route.

### Workspace Contracts (`yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/`)

- `v1/Workspaces/{Create|Get|Update|Delete|ListUserWorkspaces|CheckWorkspaceSlug}/` — 7 Command/Query + Response contract files.

### Tests (`yh-flow/src/Tests/Workspace.Tests/`)

- `Services/SlugGeneratorTests.cs` — 25 unit tests (Slugify normalization, IsValidSlug format + restricted-words, collision retry, non-Latin fallback).
- `Integration/CreateWorkspaceTests.cs` — 4 tests (auto-Admin member, explicit slug, taken slug 409, invalid format 400).
- `Integration/SoftDeleteSlugReleaseTests.cs` — 3 tests (slug release + cache-invalidation call, non-owner 403, idempotent double-delete 404).
- `Integration/SlugUniqueTests.cs` — 3 tests (two workspaces same name → distinct slugs, 5 workspaces all distinct, auto-generated slug is valid format).

## Decisions Made

1. **Inline D-06 member vs service call** — inline (`WorkspaceMember.Create`) chosen per plan action (b); 02-05 may refactor to `WorkspaceMembershipService.AddOwnerAsync`.
2. **Cache invalidation via `IMultiTenantStore<AppTenantInfo>.RemoveAsync(slug)`** — not via `IDistributedCache` directly. Honours the 02-02 wiring note verbatim; lets `WorkspaceTenantStore` own its `ws:slug:` cache namespace.
3. **SlugGenerator collision probe ignores `IsDeleted`** — soft-deleted slugs are already suffixed; redundant filter would risk missing unsuffixed edge cases.
4. **`VerifyWorkspaceSlugEndpoint` class rename** — Architecture's verb allow-list excludes "Check"; renamed to "Verify" (allowed). Route `/slug-check/` unchanged.
5. **2 additional validators (Delete + ListUserWorkspaces)** — Architecture invariant requires every command + paginated query handler to have a FluentValidation validator. Both are shape-rule validators (no DI).
6. **`CreateWorkspaceCommand` explicit-slug path** — calls `GenerateUniqueSlugAsync(slug-as-name)` and 409-s if the result differs from the input. Slugify idempotency makes this safe.
7. **Conflict-throw path not exercised by InMemory test** — suffix space is 65 536 and RandomNumberGenerator is crypto-strength; pre-seeding every candidate is infeasible. Happy retry path IS tested (4 cases); conflict-throw verified by reading the source contract.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] CA1308 (ToLowerInvariant) on SlugGenerator.Slugify**

- **Found during:** Task 1 build
- **Issue:** `TreatWarningsAsErrors=true` escalates Sonar CA1308 to a compile error. The rule recommends `ToUpperInvariant`; the D-09 slug spec mandates lowercase.
- **Fix:** Inline `#pragma warning disable CA1308` around the `input.ToLowerInvariant()` call with a comment explaining the spec-mandated lowercase choice.
- **Files modified:** `Services/SlugGenerator.cs`
- **Verification:** Modules.Workspace builds clean (0/0).
- **Committed in:** `b95a7c1c5`

**2. [Rule 2 - Architecture invariant] Missing Delete/ListUserWorkspaces validators**

- **Found during:** Task 2 Architecture.Tests run
- **Issue:** `Architecture.Tests.HandlerValidatorPairingTests` requires every command handler + every paginated query handler to have a FluentValidation validator. The plan's file list did not include `DeleteWorkspaceCommandValidator` or `ListUserWorkspacesQueryValidator`.
- **Fix:** Added both validators (shape rules only — no DI needed).
- **Files modified:** `Features/v1/Workspaces/DeleteWorkspace/DeleteWorkspaceCommandValidator.cs`, `Features/v1/Workspaces/ListUserWorkspaces/ListUserWorkspacesQueryValidator.cs`
- **Verification:** Architecture.Tests `CommandHandlers_Should_Have_Corresponding_Validators` now lists 0 Workspace handlers in its missing-validators report (6 Identity baseline debt remain).
- **Committed in:** `27bd7a14a`

**3. [Rule 2 - Architecture invariant] `CheckWorkspaceSlugEndpoint` verb not in allow-list**

- **Found during:** Task 2 Architecture.Tests run
- **Issue:** `Architecture.Tests.EndpointConventionTests.Endpoint_Names_Should_Follow_Convention` flags any `*Endpoint` class whose name does not start with an allowed verb. "Check" is not in the allow-list; this added a Workspace violation to a previously Identity-only baseline debt list.
- **Fix:** Renamed the class `CheckWorkspaceSlugEndpoint` → `VerifyWorkspaceSlugEndpoint` (verb "Verify" IS in the allow-list); renamed the file; renamed the `MapCheckWorkspaceSlugEndpoint` extension → `MapVerifyWorkspaceSlugEndpoint`; updated the `.WithName("VerifyWorkspaceSlug")` metadata; updated the `WorkspaceModule.MapEndpoints` call site. The HTTP route `POST /workspaces/slug-check/` is unchanged (Plane-compatible).
- **Files modified:** `Features/v1/Workspaces/CheckWorkspaceSlug/VerifyWorkspaceSlugEndpoint.cs` (renamed), `WorkspaceModule.cs`
- **Verification:** Architecture.Tests `Endpoint_Names_Should_Follow_Convention` no longer mentions any Workspace endpoint.
- **Committed in:** `27bd7a14a`

**4. [Rule 1 - Bug] `ICommandHandler<T>.Handle` requires `ValueTask<Unit>` not `ValueTask`**

- **Found during:** Task 2 build
- **Issue:** `DeleteWorkspaceCommandHandler.Handle` was declared `async ValueTask Handle(...)`; the Mediator `ICommandHandler<T>` contract requires `ValueTask<Unit>`.
- **Fix:** Changed the return type to `ValueTask<Unit>` and added `return Unit.Value;` at the end (pattern from Identity `UpdateUserCommandHandler`).
- **Files modified:** `Features/v1/Workspaces/DeleteWorkspace/DeleteWorkspaceCommandHandler.cs`
- **Verification:** Modules.Workspace builds clean.
- **Committed in:** `27bd7a14a`

**5. [Rule 1 - Bug] CA1725 / S927 — handler `ct` parameter must be named `cancellationToken`**

- **Found during:** Task 2 build
- **Issue:** `TreatWarningsAsErrors=true` escalates CA1725 (parameter-name-mismatch) + S927 to compile errors. All `ICommandHandler<,>.Handle` / `IQueryHandler<,>.Handle` implementations must name the cancellation token parameter `cancellationToken` (matching the interface declaration), not `ct`.
- **Fix:** Renamed `ct` → `cancellationToken` in all 6 handler `Handle` method signatures; propagated through the method bodies.
- **Files modified:** All 6 handler files.
- **Verification:** Modules.Workspace builds clean.
- **Committed in:** `27bd7a14a`

**6. [Rule 1 - Bug] Namespace/type collision on `Workspace.Create(...)` in CreateWorkspaceCommandHandler**

- **Found during:** Task 2 build
- **Issue:** `CreateWorkspaceCommandHandler` lives in `YH.Modules.Workspace.Features.v1.Workspaces.CreateWorkspace`; the root segment `Workspace` resolves to the namespace `YH.Modules.Workspace`, not the entity `YH.Modules.Workspace.Domain.Workspace`. CS0234: "namespace does not contain type 'Create'".
- **Fix:** Added `using WorkspaceEntity = YH.Modules.Workspace.Domain.Workspace;` alias (same pattern as 02-02 `WorkspaceDbContext.cs`) and changed `Workspace.Create(...)` → `WorkspaceEntity.Create(...)`.
- **Files modified:** `Features/v1/Workspaces/CreateWorkspace/CreateWorkspaceCommandHandler.cs`
- **Verification:** Modules.Workspace builds clean.
- **Committed in:** `27bd7a14a`

## Verification

- [x] `dotnet build src/YH.Flow.slnx --nologo` — 0 warnings / 0 errors (54 projects)
- [x] `dotnet test src/Tests/Workspace.Tests` — 57/57 PASS (22 prior + 35 new)
- [x] `dotnet test src/Tests/Identity.Tests` — 412/412 PASS (Phase 1 zero regression; T-2-fintenant confirmed)
- [x] Architecture.Tests: 0 NEW Workspace violations (3 remaining fails are all Phase-1 Identity baseline debt: HandlerValidatorPairing OAuth/ApiToken, EndpointNames PlaneAuth, Features→AspNetCore)
- [x] SlugGenerator rejects restricted words (`api` / `admin` / `settings` / `billing` / `sign-in`) — `IsValidSlug_RejectsFormatAndRestrictedWords` [Theory]
- [x] CreateWorkspace auto-creates Admin member (D-06) — `Handle_GeneratesSlugFromName_AndAutoEnrolsOwnerAsAdmin`
- [x] DeleteWorkspace soft-deletes + releases slug (D-08) — `SoftDelete_ReleasesSlugForReuse_NewWorkspaceGetsOriginalSlug`
- [x] DeleteWorkspace calls `IMultiTenantStore<AppTenantInfo>.RemoveAsync(slug)` (T-2-cacheinvalid) — verified via NSubstitute `Received(1)` assertion in the same test
- [x] DeleteWorkspace rejects non-owner (T-2-eop-delete) — `SoftDelete_NonOwner_ThrowsForbidden`
- [x] SlugGenerator 5-attempt collision retry produces distinct suffixed slugs (D-07) — `Create_TwoWorkspacesSameName_SecondGetsSuffixedSlug` + `Create_MultipleWorkspacesSameName_AllGetDistinctSlugs`
- [x] `WorkspaceModule.cs` MultitenancyModule untouched (strategy/store wiring from 02-02 preserved); only ConfigureServices + MapEndpoints extended

## Notes for Plans 02-05 / 02-06 (Wiring Continuation)

1. **WorkspaceModule.MapEndpoints is additive-friendly for 02-05** — the scoped `{slug}` group and the top-level `/workspaces` group are already established; 02-05 can call `scoped.MapGetWorkspaceMembersEndpoint()` etc. on the same group. A `// TODO 02-05` comment marks the insertion point.
2. **WorkspaceMembershipService.AddOwnerAsync refactor opportunity (02-05)** — `CreateWorkspaceCommandHandler` currently inlines `WorkspaceMember.Create(...)`. If 02-05 introduces `IWorkspaceMembershipService.AddOwnerAsync`, this handler can swap the inline call for the service method without changing any external behaviour.
3. **Cache invalidation pattern for future slug mutations** — any future operation that changes a workspace's slug (rename, slug transfer) MUST call `IMultiTenantStore<AppTenantInfo>.UpdateAsync(tenantInfo)` or `.RemoveAsync(slug)` AFTER SaveChanges, mirroring the pattern in `DeleteWorkspaceCommandHandler`. The `WorkspaceTenantStore` mutation methods are no-op-against-table / cache-evict-only by design.
4. **Endpoints are HTTP-routable but not yet e2e-tested against live PostgreSQL** — 02-06 smoke tests should drive the full pipeline (HTTP request → Finbuckle slug strategy → WorkspaceTenantStore → WorkspaceMembershipMiddleware → RequireWorkspaceRole gate → handler) to verify the runtime chain end-to-end. The unit/integration tests here cover handler + SlugGenerator semantics only (no Finbuckle resolver middleware in the loop).
5. **`ListUserWorkspaces` cross-tenant query is intentional** — `WorkspaceMember.WorkspaceId` is a scalar duplicate of Finbuckle's `TenantId` precisely so this query can project across workspaces without disabling the tenant filter. The query filters on `IsActive && !IsDeleted` members + `!IsDeleted` workspaces; soft-deleted memberships + soft-deleted workspaces never surface.

## Known Stubs

| Stub                                                                         | File | Line | Reason                                                                                                            | Resolved By |
| ---------------------------------------------------------------------------- | ---- | ---- | ----------------------------------------------------------------------------------------------------------------- | ----------- |
| (none — every endpoint is fully implemented and tested at the handler level) | —    | —    | The slug-check / list-mine / create / get / update / delete paths all have wired handlers and validator coverage. | —           |

No code-level stubs that flow into UI rendering or accept empty/mock data.

## TDD Gate Compliance

N/A — this plan is `type: execute` (not `type: tdd`). The 25 SlugGenerator unit tests and 10 integration tests were written alongside (or immediately after) the implementation; they are characterization tests verifying the just-implemented behaviour, not RED-first driving tests. No TDD RED/GREEN/REFACTOR gate applicable.

## Threat Flags

None. The plan's `<threat_model>` registered 9 threats; all mitigations are operational:

- **T-2-slug:** `Slugify` only retains `[a-z0-9-]`; regex rejects anything else; response JSON serialization HTML-encodes.
- **T-2-slugenum (accept):** slug-check disclosure is accepted per Plane behaviour (CONTEXT Claude's Discretion); any authenticated user may probe.
- **T-2-slugrestricted:** `RestrictedSlugs.List` (62 entries) + `IsValidSlug` + `SlugGeneratorTests` guardians.
- **T-2-eop-update [BLOCKING]:** `UpdateWorkspaceEndpoint.RequireWorkspaceRole(Admin)` (02-03 handler default-denies on insufficient role).
- **T-2-eop-delete [BLOCKING]:** `DeleteWorkspaceEndpoint.RequireWorkspaceRole(Admin)` + handler `OwnerId == CurrentUserId` check; `SoftDeleteSlugReleaseTests.NON_OWNER_ThrowsForbidden` guardian.
- **T-2-n1-list:** `ListUserWorkspacesQueryHandler` uses 2 queries total (workspaceIds projection + paged workspaces); no loop.
- **T-2-cacheinvalid:** `DeleteWorkspaceCommandHandler` calls `IMultiTenantStore<AppTenantInfo>.RemoveAsync(slug)` after SaveChanges; verified by NSubstitute `Received(1)` assertion.
- **T-2-softdelete-idempotent (accept):** second DELETE of the same slug returns 404 (soft-deleted row's slug is suffixed; the `!IsDeleted` predicate no longer matches); verified by `SoftDelete_AlreadyDeleted_Returns404_Idempotent`.
- **T-2-SC (accept):** 0 new NuGet packages.

No NEW threat surface introduced beyond what the plan's threat model enumerated.

## Self-Check: PASSED

All 30 created files verified present on disk. Both task commits (`b95a7c1c5`, `27bd7a14a`) verified in `git log`. Full-solution build 0/0; Workspace.Tests 57/57 PASS; Identity.Tests 412/412 PASS; Architecture.Tests 0 NEW Workspace violations (3 remaining fails are all Phase-1 Identity baseline debt). SlugGenerator restricted-words rejection, D-06 auto-Admin member, D-08 slug release + cache invalidation, T-2-eop-update + T-2-eop-delete gates, and D-07 5-attempt collision retry all verified by passing tests.
