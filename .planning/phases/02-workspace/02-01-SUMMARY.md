---
phase: 02-workspace
plan: 01
subsystem: testing
tags: [finbuckle, multi-tenant, contracts, dotnet, xunit, spike]

# Dependency graph
requires:
  - phase: 01-foundation
    provides: MultitenancyModule (Finbuckle claim/header/query strategy chain), Identity.Contracts (UserDto), BuildingBlocks.Shared (AppTenantInfo)
provides:
  - Workspace.Tests project (xUnit scaffold for all subsequent Workspace plans)
  - Workspace.Contracts module (DTOs + interfaces + RestrictedSlugs constant)
  - Identity.Contracts UserSummary + IUserIdentityService (cross-module batch user resolution)
  - Q1 spike conclusion — Finbuckle 10.1.x external TryAddEnumerable works (no builder-coupling required)
affects:
  [
    02-02 (domain + Finbuckle wiring),
    02-03 (membership middleware + tenant isolation),
    02-04 (workspace CRUD + slug service),
    02-05 (members + invitations + IUserIdentityService impl),
    02-06 (regression),
  ]

# Tech tracking
tech-stack:
  added: [] # 0 new packages — purely additive source code + 2 contracts projects
  patterns:
    - "Finbuckle external registration: services.TryAddEnumerable(ServiceDescriptor.Singleton<IMultiTenantStrategy, T>()) outside the AddMultiTenant<T>() builder is supported in 10.1.x"
    - "FrozenSet<T> with StringComparer.OrdinalIgnoreCase for static deny-lists (Sonar S2386/S3887 compliant under TreatWarningsAsErrors)"
    - "Contracts projects expose only DTOs/interfaces/constants — no FshModule attribute (compare Modules.Auditing/AssemblyInfo.cs which DOES register FshModule on the implementation module)"

key-files:
  created:
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/Modules.Workspace.Contracts.csproj
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/WorkspaceRole.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/ICurrentWorkspaceContext.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/IWorkspaceTenantResolver.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/INotificationService.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/Constants/RestrictedSlugs.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/DTOs/WorkspaceDto.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/DTOs/WorkspaceMemberDto.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/DTOs/WorkspaceInvitationDto.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/AssemblyInfo.cs
    - yh-flow/src/Modules/Identity/Modules.Identity.Contracts/DTOs/UserSummary.cs
    - yh-flow/src/Modules/Identity/Modules.Identity.Contracts/Services/IUserIdentityService.cs
    - yh-flow/src/Tests/Workspace.Tests/Workspace.Tests.csproj
    - yh-flow/src/Tests/Workspace.Tests/GlobalUsings.cs
    - yh-flow/src/Tests/Workspace.Tests/Usings.cs
    - yh-flow/src/Tests/Workspace.Tests/TestData/WorkspaceTestFixture.cs
    - yh-flow/src/Tests/Workspace.Tests/Spike/FinbuckleExternalStrategyRegistrationTests.cs
  modified:
    - yh-flow/src/YH.Flow.slnx (registered Workspace.Contracts module + Workspace.Tests test project)

key-decisions:
  - "Q1 RESOLVED: external TryAddEnumerable works in Finbuckle 10.1.x — plan 02-02 should use the preferred path (WorkspaceModule.ConfigureServices registers slug strategy/store directly), NOT the AddWorkspaceTenantResolution builder-callback fallback"
  - "Strategy contract is NON-generic IMultiTenantStrategy (no IMultiTenantStrategy<T>) in Finbuckle 10.1.x — important for 02-02 to know"
  - "WorkspaceRole added None=0 sentinel (CA1008); never persisted on a membership row"
  - "ICurrentWorkspaceContext.SetContext(...) renamed from Set() to avoid reserved-keyword collision (CA1716) under TreatWarningsAsErrors"
  - "RestrictedSlugs uses FrozenSet<string> (not HashSet) to satisfy Sonar analyzers S2386/S3887"
  - "RestrictedSlugs port: 62 UNIQUE entries (Plane source lists 65 with 3 duplicates: monitor/config/mobile)"
  - "Task execution order adapted to 3→2→1 to satisfy compile-time dependency (WorkspaceMemberDto references UserSummary which lives in Task 3)"
  - "Workspace.Tests project references Modules.Multitenancy (not Modules.Workspace — that module lands in 02-02) for the Q1 spike; spike tests verify DI registration only, no DbContext"

patterns-established:
  - "Cross-module contract pattern: Identity.Contracts exposes UserSummary + IUserIdentityService for batch user resolution; Workspace.Contracts depends on it (D-04/D-05)"
  - "Phase 11 placeholder pattern: INotificationService lives in Workspace.Contracts with explicit 'Phase 11 placeholder' doc; Phase 2 CreateInvitation handler will resolve-and-short-circuit when impl is null"
  - "Wave 0 spike pattern: write minimal stub classes inside the test file (private nested) to validate DI behaviour without depending on not-yet-built implementation code"

requirements-completed: [] # plan frontmatter `requirements: []` — Wave 0 unblocks REQ-2.1~2.4 downstream

# Metrics
duration: 14min
completed: 2026-06-17
---

# Phase 2 Plan 01: Wave 0 — Workspace Scaffolding Summary

**Wave 0 infrastructure delivered:** Workspace.Tests + Workspace.Contracts + Identity.Contracts extensions, with the Q1 Finbuckle spike PASSING — confirming that plan 02-02 can register the workspace slug strategy/store via external `TryAddEnumerable` without touching the Phase 1 `AddMultiTenant<AppTenantInfo>()` builder.

## Performance

- **Duration:** 14 min
- **Started:** 2026-06-17T23:38:33Z
- **Completed:** 2026-06-17T23:53:18Z
- **Tasks:** 3/3
- **Files created:** 17
- **Files modified:** 1 (YH.Flow.slnx)
- **Build:** `dotnet build src/YH.Flow.slnx` — 0 warnings, 0 errors (51 projects + 2 new)
- **Tests:** Workspace.Tests 2/2 PASS (Q1 spike); Identity.Tests 412/412 unchanged (no regression)

## Accomplishments

1. **Q1 spike RESOLVED (the blocking question for plan 02-02)** — external `TryAddEnumerable` of strategy + store appended OUTSIDE the `AddMultiTenant<AppTenantInfo>()` builder works correctly in Finbuckle 10.1.x. Plan 02-02 can use the preferred path (`WorkspaceModule.ConfigureServices` registers slug strategy/store directly). The `AddWorkspaceTenantResolution` builder-callback fallback is NOT needed.
2. **Workspace.Contracts module shipped** with 6 public types: `WorkspaceRole` (None/Guest/Member/Admin), `ICurrentWorkspaceContext`, `IWorkspaceTenantResolver`, `INotificationService` (Phase 11 placeholder), 3 DTOs (Workspace/WorkspaceMember/WorkspaceInvitation), `RestrictedSlugs` (62 entries ported from Plane).
3. **Identity.Contracts extended** additively with `UserSummary` (4-field slim DTO) + `IUserIdentityService` (batch resolution contract for D-04/D-05 N+1 avoidance); Phase 1 Identity.Tests 412/412 unchanged.
4. **Workspace.Tests project scaffolded** with shared `WorkspaceTestFixture` (Wave 0 inert; `WorkspaceDbContext` wiring lands in 02-02 per plan note a) and the Q1 spike test.

## Task Commits

Each task was committed atomically (scope `02-01`):

1. **Task 1: Workspace.Tests project + Q1 spike** — `e6431840e` (feat)
2. **Task 2: Workspace.Contracts module** — `a0b55653b` (feat)
3. **Task 3: Identity.Contracts UserSummary + IUserIdentityService** — `e9ed9d8a6` (feat)

**Plan metadata:** pending (this SUMMARY commit)

## Files Created/Modified

### Workspace.Contracts module (`yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/`)

- `Modules.Workspace.Contracts.csproj` — class library; ProjectReference to Identity.Contracts for UserSummary
- `AssemblyInfo.cs` — intentionally empty (Contracts ≠ module; no FshModule attribute)
- `WorkspaceRole.cs` — enum with None=0 (CA1008), Guest=5, Member=15, Admin=20 (Plane `ROLE_CHOICES`)
- `ICurrentWorkspaceContext.cs` — per-request workspace context interface (D-03); `SetContext` renamed from `Set` (CA1716)
- `IWorkspaceTenantResolver.cs` — optional facade, NOT implemented in Phase 2 (YAGNI)
- `INotificationService.cs` — Phase 11 placeholder (D-10)
- `DTOs/WorkspaceDto.cs` — Plane workspace.py field mapping (name/slug/logo/owner_id/organization_size/timezone/background_color/audit timestamps)
- `DTOs/WorkspaceMemberDto.cs` — id/role/is_active/user(UserSummary?)/workspace_id/audit
- `DTOs/WorkspaceInvitationDto.cs` — id/email/role/accepted/responded_at/message/created_at/expires_at/workspace_id/created_by; raw token NEVER serialized (D-12)
- `Constants/RestrictedSlugs.cs` — 62 unique entries ported from Plane `RESTRICTED_WORKSPACE_SLUGS` (D-09, T-02-01)

### Identity.Contracts extension (`yh-flow/src/Modules/Identity/Modules.Identity.Contracts/`)

- `DTOs/UserSummary.cs` — 4-field slim record (Id/DisplayName/Email/AvatarUrl); excludes PasswordHash/SecurityStamp (T-02-02 mitigation)
- `Services/IUserIdentityService.cs` — `GetUsersByIdsAsync(IEnumerable<Guid>)` batch contract for D-05 N+1 avoidance; implementation in 02-05

### Tests (`yh-flow/src/Tests/Workspace.Tests/`)

- `Workspace.Tests.csproj` — xUnit + Shouldly + AutoFixture + NSubstitute + EF InMemory; ProjectReferences to Workspace.Contracts + Multitenancy (NOT Modules.Workspace, which lands in 02-02)
- `GlobalUsings.cs` — Shouldly + Xunit
- `Usings.cs` — Finbuckle + AppTenantInfo usings for the spike
- `TestData/WorkspaceTestFixture.cs` — Wave 0 inert scaffold; `WorkspaceDbContext` wiring lands in 02-02
- `Spike/FinbuckleExternalStrategyRegistrationTests.cs` — 2/2 PASS; Q1 authoritative answer

### Modified

- `yh-flow/src/YH.Flow.slnx` — registered Workspace.Contracts module under `/Modules/Workspace/` and Workspace.Tests under `/Tests/`

## Decisions Made

1. **Q1 resolution (BLOCKING — feeds plan 02-02):** External `TryAddEnumerable` works. Plan 02-02 uses the **preferred path** — `WorkspaceModule.ConfigureServices` registers the slug strategy/store via `services.TryAddEnumerable(ServiceDescriptor.Singleton<IMultiTenantStrategy, WorkspaceSlugStrategy>())`. The `AddWorkspaceTenantResolution(this MultiTenantBuilder<AppTenantInfo>)` builder-callback fallback is NOT required. **Plan 02-02 may delete the fallback path from RESEARCH.md Pattern 1 action item #3.**
2. **Critical API correction for 02-02:** Finbuckle 10.1.x strategy contract is **non-generic `IMultiTenantStrategy`** — there is no `IMultiTenantStrategy<T>`. Stores remain generic `IMultiTenantStore<T>`. The PATTERNS.md/RESEARCH.md samples use `IMultiTenantStrategy<AppTenantInfo>` which would NOT compile in 02-02 — they must use the non-generic interface.
3. **RestrictedSlugs count discrepancy:** Plane source lists 65 entries with 3 duplicates (monitor, config, mobile). Ported 62 unique entries to a `FrozenSet<string>` (Sonar S2387/S2387 compliant).
4. **WorkspaceRole.None=0 sentinel added** for CA1008 compliance (TreatWarningsAsErrors); documented as never persisted.
5. **Workspace.Tests references Modules.Multitenancy (not Modules.Workspace)** because the implementation module is built in 02-02; the Q1 spike only validates Finbuckle DI behaviour and does not need `WorkspaceDbContext`.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Task execution order adapted to 3→2→1**

- **Found during:** Plan parse (pre-execution)
- **Issue:** Plan lists tasks in order 1→2→3, but Task 2's `WorkspaceMemberDto` references `UserSummary` (Task 3's file), and Task 2's action text explicitly states "Task 3 会先创建 UserSummary，所以本 task 编译时 UserSummary 已存在". In plan order, Task 2 would not compile.
- **Fix:** Executed Task 3 (Identity.Contracts extension — purely additive, no Phase 1 surface change) before Task 2. Verified Identity.Tests 412/412 unchanged between Task 3 commit and subsequent work. Task 1 (independent test scaffolding) ran last as planned.
- **Files affected:** None beyond the planned file set; only commit order changed.
- **Verification:** Identity.Tests 412/412 after Task 3 commit; full solution build 0/0 after all three tasks.
- **Committed in:** e9ed9d8a6 (Task 3 commit, executed first)

**2. [Rule 1 - Bug] WorkspaceRole missing None=0 (CA1008)**

- **Found during:** Task 2 build
- **Issue:** `TreatWarningsAsErrors=true` (CLAUDE.md / Directory.Build.props) escalated analyzer warning CA1008 ("add a zero-valued member named 'None'") to a compile error.
- **Fix:** Added `None = 0` sentinel with XML doc explaining it is never persisted on a membership row.
- **Files modified:** `WorkspaceRole.cs`
- **Verification:** Workspace.Contracts builds clean.
- **Committed in:** a0b55653b

**3. [Rule 1 - Bug] ICurrentWorkspaceContext.Set collides with reserved keyword (CA1716)**

- **Found during:** Task 2 build
- **Issue:** CA1716 flags `Set` as a reserved-language-keyword collision; elevated to error.
- **Fix:** Renamed `Set(Guid, string, WorkspaceRole?)` to `SetContext(...)`. Documented the rename rationale in the XML doc comment so plan 02-03's `WorkspaceMembershipMiddleware` uses the new name.
- **Files modified:** `ICurrentWorkspaceContext.cs`
- **Verification:** Workspace.Contracts builds clean.
- **Committed in:** a0b55653b

**4. [Rule 1 - Bug] RestrictedSlugs.List mutable public collection (Sonar S2386/S3887)**

- **Found during:** Task 2 build
- **Issue:** Sonar S2386/S3887 flag public mutable `HashSet<T>` field as a security/maintainability concern; elevated to error under `AnalysisMode=AllEnabledByDefault`.
- **Fix:** Changed `HashSet<string>` to `FrozenSet<string>` (immutable, .NET 8+). Used `new HashSet<string>(...).ToFrozenSet(StringComparer.OrdinalIgnoreCase)` to preserve case-insensitive semantics.
- **Files modified:** `RestrictedSlugs.cs`
- **Verification:** Workspace.Contracts builds clean.
- **Committed in:** a0b55653b

**5. [Rule 1 - Bug] Finbuckle 10.1.x strategy contract is non-generic**

- **Found during:** Task 1 build
- **Issue:** Initial spike used `IMultiTenantStrategy<AppTenantInfo>` per RESEARCH §Example 1 / PATTERNS.md samples; Finbuckle 10.1.x has NO generic strategy interface (`CS0308: non-generic IMultiTenantStrategy cannot be used with type arguments`).
- **Fix:** Changed spike stub + `TryAddEnumerable` registration to non-generic `IMultiTenantStrategy`. Documented this in the spike file's `<remarks>` as a critical Q1 finding for plan 02-02.
- **Files modified:** `FinbuckleExternalStrategyRegistrationTests.cs`
- **Verification:** Q1 spike 2/2 PASS.
- **Committed in:** e6431840e

## Q1 Spike Authoritative Conclusion (FOR PLAN 02-02)

**Question:** Does Finbuckle 10.1.x allow appending `IMultiTenantStrategy` / `IMultiTenantStore<AppTenantInfo>` registrations from OUTSIDE the `AddMultiTenant<AppTenantInfo>()` builder chain (so a downstream module can plug in a slug strategy/store without re-invoking the builder and clobbering Phase 1's claim/header/query chain)?

**Answer: YES — preferred path confirmed.**

- `services.TryAddEnumerable(ServiceDescriptor.Singleton<IMultiTenantStrategy, WorkspaceSlugStrategyStub>())` registered AFTER `services.AddMultiTenant<AppTenantInfo>().WithClaimStrategy(...)` resolves correctly: `provider.GetServices<IMultiTenantStrategy>()` returns both the existing `ClaimStrategy` AND the appended stub (count ≥ 2).
- Same for stores: `services.TryAddEnumerable(ServiceDescriptor.Scoped<IMultiTenantStore<AppTenantInfo>, WorkspaceTenantStoreStub>())` registered AFTER `.WithStore<EFCoreStore<TenantDbContext, AppTenantInfo>>(...)` resolves to ≥ 2 stores including the stub.
- `TryAddEnumerable` (not `TryAdd`) is correct — it appends to the existing enumerable rather than no-op'ing on first match.

**Action for 02-02:**

1. **Use the preferred path:** `WorkspaceModule.ConfigureServices` registers the slug strategy + `WorkspaceTenantStore` via `services.TryAddEnumerable(...)` directly. No `AddWorkspaceTenantResolution` builder-callback fallback is needed.
2. **CRITICAL API correction:** Strategy contract is `IMultiTenantStrategy` (non-generic), NOT `IMultiTenantStrategy<AppTenantInfo>` as PATTERNS.md/RESEARCH.md samples imply. The 02-02 plan MUST update the code samples. `WithDelegateStrategy<HttpContext, AppTenantInfo>` (builder method) still works — it produces a non-generic `IMultiTenantStrategy` instance under the hood.
3. **`AddMultiTenant<AppTenantInfo>` second-call Anti-Pattern still holds** (RESEARCH §Pitfall 1): never call `services.AddMultiTenant<AppTenantInfo>(...)` from `WorkspaceModule`. The spike confirms `TryAddEnumerable` is the safe append mechanism.

**Test evidence:** `dotnet test src/Tests/Workspace.Tests --filter "FullyQualifiedName~Spike"` → 2/2 PASS.

## Verification

- [x] `dotnet build src/YH.Flow.slnx --nologo` — 0 warnings / 0 errors (53 projects total: 51 prior + Workspace.Contracts + Workspace.Tests)
- [x] `dotnet test src/Tests/Workspace.Tests --filter "FullyQualifiedName~Spike"` — 2/2 PASS (Q1 conclusion authoritative)
- [x] `dotnet test src/Tests/Identity.Tests` — 412/412 PASS (no Phase 1 regression)
- [x] Q1 spike conclusion recorded in this SUMMARY (authoritative input to plan 02-02)
- [x] Workspace.Contracts exposes 6+ public types (WorkspaceRole + 3 interfaces + 3 DTOs + RestrictedSlugs)
- [x] RestrictedSlugs contains 62 unique entries (matches Plane source after deduplication; plan estimate of 66 was approximate)
- [x] UserSummary cross-project reference resolves (Workspace.Contracts ProjectReferences Identity.Contracts)

## Known Stubs

| Stub                                                      | File                                                                                    | Line       | Reason                                                                                                                                                                                                 | Resolved By                                                                                                            |
| --------------------------------------------------------- | --------------------------------------------------------------------------------------- | ---------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------- |
| `WorkspaceTestFixture` is inert (no `WorkspaceDbContext`) | `yh-flow/src/Tests/Workspace.Tests/TestData/WorkspaceTestFixture.cs`                    | 13-46      | `WorkspaceDbContext` type does not exist until plan 02-02 builds the implementation module (per plan Task 1 action note (a)). Fixture exposes only `RootUserId` / `RootUserEmail` constants in Wave 0. | Plan 02-02 (Wave 1) — adds `CreateInMemoryContext()` + seed root user. Documented inline in the fixture's `<summary>`. |
| `INotificationService` has no implementation              | `yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/INotificationService.cs`     | whole file | Intentional — D-10 defers email dispatch to Phase 11 (Hangfire + MailKit). Phase 2's `CreateInvitation` handler (plan 02-05) will resolve-and-short-circuit when no implementation is registered.      | Phase 11 (Notifications). Stub is by design per CONTEXT.md `<deferred>`.                                               |
| `IWorkspaceTenantResolver` has no implementation          | `yh-flow/src/Modules/Workspace/Modules.Workspace.Contracts/IWorkspaceTenantResolver.cs` | whole file | Intentional YAGNI — no consumer in Phase 2 (PATTERNS.md §No Analog). Downstream modules should read `ICurrentWorkspaceContext.CurrentWorkspaceId` directly.                                            | First downstream consumer that genuinely needs it (likely Phase 3 Project).                                            |

## TDD Gate Compliance

N/A — this plan is `type: execute` (not `type: tdd`). No TDD RED/GREEN/REFACTOR gate applicable. The Q1 spike is a discovery test (asserts current Finbuckle behaviour), not a feature-driving test.

## Threat Flags

None. The plan's `<threat_model>` registered T-02-01 (RestrictedSlugs integrity) and T-02-02 (UserSummary field allow-list) as the two mitigated threats; both mitigations are in place:

- T-02-01: `RestrictedSlugs` ported with 62 unique entries; 02-04 SlugGenerator tests will assert coverage of critical entries (api/admin/settings/auth/web/billing/sign-in/sign-up).
- T-02-02: `UserSummary` has 4 fields only (Id/DisplayName/Email/AvatarUrl); XML doc explicitly calls out PasswordHash/SecurityStamp exclusion; 02-05 implementation must use Select projection.

No new threat surface introduced (Wave 0 has no HTTP endpoints, no runtime entry points per the plan's threat register).

## Self-Check: PENDING

Self-check will run after SUMMARY is committed and verified.
