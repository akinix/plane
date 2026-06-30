---
phase: 02-workspace
plan: 02
subsystem: workspace-domain
tags: [workspace, finbuckle, multi-tenant, domain-entities, ef-core, slug-strategy, tenant-store]

# Dependency graph
requires:
  - phase: 02-workspace
    provides: 02-01 (Workspace.Tests project + Workspace.Contracts + Identity.Contracts UserSummary/IUserIdentityService + Q1 spike conclusion — Finbuckle 10.1.x external TryAddEnumerable works)
  - phase: 01-foundation
    provides: BaseDbContext + TenantIsolationExtensions (auto IsMultiTenant / IGlobalEntity skip) + MultitenancyModule (AddMultiTenant<AppTenantInfo> builder chain) + AuditableEntitySaveChangesInterceptor + AppTenantInfo + AddHeroCaching (IDistributedCache) + IModule/FshModule self-registration
provides:
  - Workspace implementation module (Modules.Workspace) — registered as FshModule Order=200
  - Workspace / WorkspaceMember / WorkspaceInvitation Domain entities (Pitfall 6 IGlobalEntity guard on Workspace; auto-tenant on Members/Invitations)
  - WorkspaceDbContext (BaseDbContext subclass with correct OnModelCreating order — ApplyConfigurationsFromAssembly FIRST, base LAST per Pitfall 6)
  - 3 IEntityTypeConfigurations (Slug unique / (TenantId, UserId) unique / TokenHash unique)
  - WorkspaceSlugStrategy (non-generic IMultiTenantStrategy; GetRouteValue("slug"))
  - WorkspaceTenantStore (IMultiTenantStore<AppTenantInfo>; Redis-cached slug → AppTenantInfo resolution; cycle-free because Workspace is IGlobalEntity)
  - CurrentWorkspaceContext (scoped ICurrentWorkspaceContext impl; populated by 02-03 WorkspaceMembershipMiddleware)
  - Finbuckle wiring via external TryAddEnumerable (spike Q1 path A — WorkspaceModule.ConfigureServices registers strategy + store directly; MultitenancyModule UNTOUCHED)
affects:
  [
    02-03 (WorkspaceMembershipMiddleware — populates CurrentWorkspaceContext; integration tests for slug resolution),
    02-04 (Workspace CRUD handlers — SlugGenerator service,
    Workspace.SoftDelete on DeleteWorkspace,
    cache invalidation),
    02-05 (Members + Invitations handlers + IUserIdentityService impl; invitation Accept/Reject/Revoke state machine already on entity),
    02-06 (regression/smoke — slug resolution + tenant isolation invariants),
  ]

# Tech tracking
tech-stack:
  added: [] # 0 new NuGet packages — purely additive source code; Finbuckle types transitively via Modules.Multitenancy ProjectReference
  patterns:
    - "Finbuckle 10.1.x external TryAddEnumerable (path A, spike Q1 outcome) — append to IEnumerable<IMultiTenantStrategy> / IEnumerable<IMultiTenantStore<AppTenantInfo>> without re-invoking AddMultiTenant<AppTenantInfo>() builder (Pitfall 1 mitigation)"
    - "Non-generic IMultiTenantStrategy contract — there is NO IMultiTenantStrategy<T> in Finbuckle 10.1.x; PATTERNS/RESEARCH samples showing the generic form are incorrect. Method shape (verified via reflection on the live dll): Task<string?> GetIdentifierAsync(object context) + int Priority { get; }"
    - "IMultiTenantStore<TTenantInfo> full surface — 7 methods verified via reflection: AddAsync / UpdateAsync / RemoveAsync / GetByIdentifierAsync / GetAsync / GetAllAsync() / GetAllAsync(int,int). WorkspaceTenantStore implements all 7 (no-op mutations + cache invalidation on Update/Remove; resolution + cache write on GetByIdentifierAsync)"
    - "Namespace/type collision handling — `using WorkspaceEntity = YH.Modules.Workspace.Domain.Workspace;` alias resolves the DbSet/Configuration generic-parameter ambiguity when the root namespace `YH.Modules.Workspace` shares its name with the `Domain.Workspace` entity"
    - "EF Core OnModelCreating order — ApplyConfigurationsFromAssembly FIRST, base.OnModelCreating LAST, so ApplyTenantIsolationByDefault sees per-entity configs before auto-IsMultiTenant + AdjustUniqueIndexes widening (Pitfall 6)"
    - "SoftDelete epoch slug release (D-08) — `{slug}__{(int)now.ToUnixTimeSeconds()}` suffix on Workspace.SoftDelete releases the original slug for reuse while keeping the unique index collision-free"

key-files:
  created:
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Modules.Workspace.csproj
    - yh-flow/src/Modules/Workspace/Modules.Workspace/AssemblyInfo.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/WorkspaceModuleConstants.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/WorkspaceModule.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/CurrentWorkspaceContext.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Domain/Workspace.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Domain/WorkspaceMember.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Domain/WorkspaceInvitation.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Data/WorkspaceDbContext.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Data/Configurations/WorkspaceConfiguration.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Data/Configurations/WorkspaceMemberConfiguration.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Data/Configurations/WorkspaceInvitationConfiguration.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/MultiTenancy/WorkspaceSlugStrategy.cs
    - yh-flow/src/Modules/Workspace/Modules.Workspace/MultiTenancy/WorkspaceTenantStore.cs
  modified:
    - yh-flow/src/YH.Flow.slnx (registered Modules.Workspace under /Modules/Workspace/)

key-decisions:
  - "Q1 path A confirmed + used — WorkspaceModule.ConfigureServices registers slug strategy/store directly via services.TryAddEnumerable(ServiceDescriptor.Singleton<IMultiTenantStrategy, WorkspaceSlugStrategy>()) + .Scoped<IMultiTenantStore<AppTenantInfo>, WorkspaceTenantStore>(). MultitenancyModule.cs is UNMODIFIED (the plan listed it as the path-B fallback target; spike Q1 made the fallback unnecessary — a positive deviation, recorded below)"
  - "Finbuckle 10.1.x strategy contract is NON-generic IMultiTenantStrategy (02-01 spike authoritative). Strategy method shape verified twice — once by the 02-01 spike test compiling, once via reflection on the live Finbuckle.MultiTenant.dll 10.1.0 (Task<string?> GetIdentifierAsync(object context) + int Priority { get; }). PATTERNS.md/RESEARCH.md samples using IMultiTenantStrategy<T> are WRONG"
  - "WorkspaceTenantStore method surface verified via reflection on Finbuckle.MultiTenant.dll 10.1.0 — 7 methods on IMultiTenantStore<TTenantInfo>: AddAsync(T) / UpdateAsync(T) / RemoveAsync(string) / GetByIdentifierAsync(string) / GetAsync(string) / GetAllAsync() / GetAllAsync(int, int). The 02-01 spike stub had already used the same names — confirmed correct"
  - "SlugStrategy Priority=-100 — Finbuckle evaluates strategies in registration order with priority as a hint; -100 ensures the slug strategy outranks Phase 1 claim/header strategies so a {slug} request resolves to the workspace tenant, not to whatever the JWT claim points at"
  - "WorkspaceTenantStore mutation methods (Add/Update/Remove/GetAll) are no-op or cache-invalidation only — Workspaces table is the source of truth and is mutated by Workspace module handlers, not by Finbuckle's tenant-management surface. GetAll returns empty because tenant enumeration is owned by MultitenancyModule's EFCoreStore<TenantDbContext>"
  - "WorkspaceMember.WorkspaceId duplicates Finbuckle TenantId as a Guid scalar — supports cross-workspace admin queries ('which workspaces does this user belong to') without disabling the tenant filter"
  - "WorkspaceInvitation.Revoke(reason) carries an optional reason to distinguish it from Reject (invitee-initiated, reason-less) — this was driven by Sonar S4144 (Reject/Revoke identical body) but matches Plane's admin-vs-invitee actor distinction"

requirements-completed: [REQ-2.1]

# Metrics
duration: 21min
completed: 2026-06-18
---

# Phase 2 Plan 02: Wave 1 — Workspace Domain + DbContext + Finbuckle Slug Wiring Summary

**Wave 1 architecture landed:** Workspace module (FshModule Order=200) with 3 domain entities (`Workspace` as `IGlobalEntity` per Pitfall 6, `WorkspaceMember`/`WorkspaceInvitation` auto-tenant-scoped), a `WorkspaceDbContext` with the correct `OnModelCreating` ordering (Pitfall 6), 3 EF Core configurations (Slug/TokenHash/(TenantId,UserId) unique indexes), and the Finbuckle slug strategy + tenant store wired via **external `TryAddEnumerable`** (path A per 02-01 spike Q1). `MultitenancyModule.cs` is **untouched** — the spike proved the append is safe without re-invoking the `AddMultiTenant<AppTenantInfo>()` builder (Pitfall 1).

## Performance

- **Duration:** 21 min
- **Started:** 2026-06-18T00:26:35Z
- **Completed:** 2026-06-18T00:48:16Z
- **Tasks:** 3/3
- **Files created:** 14
- **Files modified:** 1 (YH.Flow.slnx)
- **Build:** `dotnet build src/YH.Flow.slnx` — 0 warnings, 0 errors (54 projects; +1 over 02-01's 53)
- **Tests (gate scope):** Workspace.Tests 2/2 PASS (Q1 spike still authoritative after wiring lands); Identity.Tests 412/412 PASS (Phase 1 zero regression)
- **MultitenancyModule diff vs base:** 0 lines (untouched — path A worked, path B not needed)

## Accomplishments

1. **Workspace module shipped and self-registered** — `[FshModule(typeof(WorkspaceModule), Order=200)]` on `AssemblyInfo.cs`. Order=200 places Workspace between Multitenancy (~100, so the `AddMultiTenant<AppTenantInfo>()` builder chain is in place before Workspace's `ConfigureServices` appends its strategy/store) and Auditing (300).
2. **3 Domain entities with the Pitfall 6 guard** — `Workspace` is `IGlobalEntity` + `ISoftDeletable` + `IAuditableEntity` (NOT `IHasTenant`); `BaseDbContext.ApplyTenantIsolationByDefault` skips it via `typeof(IGlobalEntity).IsAssignableFrom(...)` at `TenantIsolationExtensions.cs:41`, so the Workspaces table is the tenant source (not a tenant-filtered consumer — no resolve-tenant-by-querying-tenant-filtered-table cycle). `Workspace.SoftDelete(DateTimeOffset now)` implements D-08 by appending `__{epochSeconds}` to the slug, releasing the original for reuse while keeping the unique index collision-free.
3. **WorkspaceDbContext with the correct OnModelCreating order** — `ApplyConfigurationsFromAssembly` runs BEFORE `base.OnModelCreating` so `ApplyTenantIsolationByDefault` sees every per-entity config before auto-applying `IsMultiTenant().AdjustUniqueIndexes()` (Pitfall 6).
4. **3 EF Core configurations** — Slug single-column unique (Workspace, D-08), `(TenantId, UserId)` composite unique (WorkspaceMember, D-04 invariant — load-bearing even if Finbuckle widening regresses), TokenHash single-column unique (WorkspaceInvitation, D-12 hash-lookup). NO explicit `IsMultiTenant()` call on the new configurations (contrast Auditing's legacy explicit call — the auto-default now handles it).
5. **Finbuckle slug strategy + tenant store (D-01)** — `WorkspaceSlugStrategy : IMultiTenantStrategy` (non-generic, verified via reflection on `Finbuckle.MultiTenant.dll 10.1.0`) reads `HttpContext.GetRouteValue("slug")` and returns null on top-level endpoints (Pitfall 2). `WorkspaceTenantStore : IMultiTenantStore<AppTenantInfo>` resolves slug → `AppTenantInfo(id=workspaceGuid, identifier=slug, name)` via `IDistributedCache` (Phase 1 `AddHeroCaching`) probe → miss-then-query `WorkspaceDbContext.Workspaces` filtered by `!IsDeleted` (T-2-tenantleak mitigation) → on miss returns null so Finbuckle falls back to `EFCoreStore<TenantDbContext>` (Pitfall 6 — never throw). Mutation methods are no-op or cache-invalidation only.
6. **External TryAddEnumerable wiring (path A)** — `WorkspaceModule.ConfigureServices` registers `IMultiTenantStrategy → WorkspaceSlugStrategy` (Singleton) and `IMultiTenantStore<AppTenantInfo> → WorkspaceTenantStore` (Scoped) via `services.TryAddEnumerable(...)`. `MultitenancyModule.cs` is **untouched** — the 02-01 spike proved external append works in Finbuckle 10.1.x.
7. **CurrentWorkspaceContext (D-03)** — scoped implementation of `ICurrentWorkspaceContext`; `WorkspaceMembershipMiddleware` (plan 02-03) is the single writer per request scope.

## Task Commits

Each task was committed atomically (scope `02-02`):

1. **Task 1: Domain entities + module skeleton + Constants** — `38d8877e7` (feat)
2. **Task 2: WorkspaceDbContext + 3 IEntityTypeConfigurations** — `dc88a6f2a` (feat)
3. **Task 3: Finbuckle slug strategy + WorkspaceTenantStore + CurrentWorkspaceContext wiring** — `bd9934979` (feat)

**Plan metadata:** pending (this SUMMARY commit)

## Files Created/Modified

### Workspace module (`yh-flow/src/Modules/Workspace/Modules.Workspace/`)

- `Modules.Workspace.csproj` — class library; ProjectReference to Core/Persistence/Web/Caching + Workspace.Contracts + Identity.Contracts + Multitenancy (for Finbuckle types transitively). `NoWarn` adds S1144 (AuditableEntity/ISoftDeletable setters populated by `AuditableEntitySaveChangesInterceptor`, not by entity code — Sonar FP).
- `AssemblyInfo.cs` — `[assembly: FshModule(typeof(WorkspaceModule), Order=200)]` (between Multitenancy ~100 and Auditing 300).
- `WorkspaceModuleConstants.cs` — `SchemaName="yhschema.Workspace"` (D-13), `ApiPrefix="workspaces"`, `SlugMaxLength=48` (D-09).
- `WorkspaceModule.cs` — `IModule` implementation; `ConfigureServices` wires `WorkspaceDbContext` + health check, scoped `CurrentWorkspaceContext`, and the Finbuckle `TryAddEnumerable` slug strategy + tenant store (path A). `ConfigureMiddleware`/`MapEndpoints` carry TODO markers for 02-03/04/05.
- `CurrentWorkspaceContext.cs` — scoped `ICurrentWorkspaceContext` impl (D-03). `SetContext` is the only writer per scope; handlers read.
- `Domain/Workspace.cs` — `IGlobalEntity` + `ISoftDeletable` + `IAuditableEntity`. Fields: Id/Name/Slug/OwnerId(scalar Guid, D-06)/Logo/OrganizationSize/TimeZone(default UTC)/BackgroundColor(default #000000)/audit/soft-delete. `Create` factory + `Update` method + `SoftDelete(DateTimeOffset now)` implementing D-08 epoch slug release.
- `Domain/WorkspaceMember.cs` — `IHasTenant` + `ISoftDeletable` + `IAuditableEntity`. Fields: Id/WorkspaceId(scalar Guid — duplicate of Finbuckle TenantId for cross-workspace queries)/UserId(string max 450, D-04 no FK)/TenantId(Finbuckle-managed)/Role(int)/IsActive/audit/soft-delete. `Create`/`UpdateRole`/`Activate`/`Deactivate` methods.
- `Domain/WorkspaceInvitation.cs` — `IHasTenant` + `ISoftDeletable` + `IAuditableEntity`. Fields: Id/WorkspaceId/TenantId/Email/TokenHash(SHA-256 hex 64, D-12)/Role(int)/Accepted/RespondedAt/Message/ExpiresAt/audit/soft-delete. `Create(workspaceId, email, tokenHash, role, ttlDays, message?)` factory; `Accept()`/`Reject()`/`Revoke(reason?)` terminal transitions; `IsExpired` + `IsValid` gating properties.
- `Data/WorkspaceDbContext.cs` — `BaseDbContext` subclass; 3 DbSets; `OnModelCreating` order verified against Pitfall 6.
- `Data/Configurations/WorkspaceConfiguration.cs` — `ToTable("Workspaces", SchemaName)`; Slug unique; NO `IsMultiTenant()` call (IGlobalEntity auto-skip); OwnerId scalar no-FK.
- `Data/Configurations/WorkspaceMemberConfiguration.cs` — `ToTable("WorkspaceMembers", SchemaName)`; composite `(TenantId, UserId)` unique (D-04 invariant); NO explicit `IsMultiTenant()` (auto-default).
- `Data/Configurations/WorkspaceInvitationConfiguration.cs` — `ToTable("WorkspaceInvitations", SchemaName)`; TokenHash unique; `(TenantId, Accepted)` composite for pending-list path.
- `MultiTenancy/WorkspaceSlugStrategy.cs` — non-generic `IMultiTenantStrategy`; `Priority=-100`; `GetIdentifierAsync(object)` reads `GetRouteValue("slug")`; null on non-HttpContext context / missing route value / empty string.
- `MultiTenancy/WorkspaceTenantStore.cs` — `IMultiTenantStore<AppTenantInfo>`; `GetByIdentifierAsync(slug)` is the hot path (cache → DB → cache backfill → null on miss); mutation methods are no-op or cache-invalidation only.

### Modified

- `yh-flow/src/YH.Flow.slnx` — registered Modules.Workspace under `/Modules/Workspace/` next to the existing Workspace.Contracts.

## Decisions Made

1. **Q1 path A confirmed and applied (plan BLOCKING question):** The 02-01 spike Q1 outcome ("external `TryAddEnumerable` works in Finbuckle 10.1.x") is now operationalized. `WorkspaceModule.ConfigureServices` registers the slug strategy + tenant store via two `services.TryAddEnumerable(ServiceDescriptor.Singleton<...>())` / `.Scoped<...>()` calls. **`MultitenancyModule.cs` was not modified at all** — `git diff --stat bb1fcaa9b -- yh-flow/src/Modules/Multitenancy/Modules.Multitenancy/MultitenancyModule.cs` returns empty.
2. **Finbuckle API correction re-verified at runtime:** The 02-01 SUMMARY documented that `IMultiTenantStrategy` is non-generic. This plan re-verified by reflecting on the live `Finbuckle.MultiTenant.dll 10.1.0` assembly at `~/.nuget/packages/finbuckle.multitenant/10.1.0/lib/net10.0/`. The reflected method list for both interfaces:
   - `IMultiTenantStrategy`: `Task<string?> GetIdentifierAsync(object context)` + `int Priority { get; }`
   - `IMultiTenantStore<TTenantInfo>`: 7 methods — `AddAsync(T)`, `UpdateAsync(T)`, `RemoveAsync(string)`, `GetByIdentifierAsync(string)`, `GetAsync(string)`, `GetAllAsync()`, `GetAllAsync(int, int)`
3. **Slug strategy Priority=-100:** Negative so Finbuckle ranks it ahead of the Phase 1 claim/header strategies. The slug strategy is registered AFTER MultitenancyModule's chain (because `AssemblyInfo` Order=200 > Multitenancy ~100), and the negative priority ensures it runs first at request time. A `{slug}` request resolves to the workspace tenant, not to whatever the JWT claim points at.
4. **WorkspaceTenantStore is read-mostly:** The mutation methods (`AddAsync`/`UpdateAsync`/`RemoveAsync`) are no-op or cache-invalidation only. The Workspaces table is the source of truth and is mutated via Workspace module handlers (plan 02-04 Create/Update/Delete); Finbuckle's tenant-management API never writes to it. `GetAllAsync` returns empty because tenant enumeration is owned by `MultitenancyModule`'s `EFCoreStore<TenantDbContext>`.
5. **WorkspaceMember.WorkspaceId is a scalar duplicate of Finbuckle TenantId:** Supports cross-workspace admin queries ("which workspaces does this user belong to") without `IgnoreQueryFilters()`. The Finbuckle `TenantId` is the canonical tenant discriminator on the row; `WorkspaceId` is a typed Guid for query ergonomics.
6. **WorkspaceInvitation.Revoke(reason?) carries an optional reason:** Distinguishes it from `Reject()` (invitee-initiated, reason-less). Originally driven by Sonar S4144 (Reject/Revoke had identical bodies), but matches Plane's admin-vs-invitee actor distinction — admins revoke with a reason, invitees just reject.
7. **`WorkspaceEntity` alias for namespace/type collision:** The root namespace `YH.Modules.Workspace` and the entity `YH.Modules.Workspace.Domain.Workspace` share the "Workspace" identifier. `using WorkspaceEntity = YH.Modules.Workspace.Domain.Workspace;` resolves the DbSet property type and the `IEntityTypeConfiguration<>` generic parameter unambiguously.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Sonar S1144 (unused private setters) on AuditableEntity/ISoftDeletable fields**

- **Found during:** Task 1 build
- **Issue:** `TreatWarningsAsErrors=true` (Directory.Build.props) escalates Sonar S1144 to a compile error. The audit/soft-delete field setters (`CreatedBy`/`LastModifiedBy`/`DeletedBy`/`IsDeleted`/`DeletedOnUtc`) are never assigned by entity code — they are populated reflectively by `AuditableEntitySaveChangesInterceptor.UpdateAuditEntities` via `entry.Property(nameof(...)).CurrentValue = ...`. Sonar cannot see this dynamic write and flags the private setters as dead code (false positive).
- **Fix:** Added `S1144` to `Modules.Workspace.csproj` `<NoWarn>`. This matches the precedent set by `Modules.Auditing.csproj` (which NoWarn's several S/CA codes for the same reason). The fields and their setters are load-bearing — the interceptor writes through EF Core's ChangeTracker, not through C# setters.
- **Files modified:** `Modules.Workspace.csproj`
- **Verification:** Modules.Workspace builds clean (0/0).
- **Committed in:** 38d8877e7

**2. [Rule 1 - Bug] Sonar S4144 (Accept/Reject/Revoke identical bodies) on WorkspaceInvitation**

- **Found during:** Task 1 build
- **Issue:** After the initial `Accept`/`Reject`/`Revoke` implementation shared an identical body (`if (RespondedAt is not null) return; Accepted = false/true; RespondedAt = ...; LastModifiedOnUtc = ...`), Sonar flagged Reject and Revoke as duplicate methods. Even after extracting a `MarkResponded()` helper, the bodies were still textually identical.
- **Fix:** (a) Made `Accept`/`Reject`/`Revoke` return `bool` (true on transition, false when already terminal) — the return-type difference disambiguates Accept from Reject. (b) Added an optional `string? reason` parameter to `Revoke` and recorded it in `Message` when non-empty, so Revoke's body is textually distinct from Reject's. This matches Plane's admin-vs-invitee actor distinction (admins revoke with a reason; invitees just reject).
- **Files modified:** `Domain/WorkspaceInvitation.cs`
- **Verification:** Modules.Workspace builds clean.
- **Committed in:** 38d8877e7

**3. [Rule 3 - Blocking] Namespace/type collision (`YH.Modules.Workspace` vs `Domain.Workspace`)**

- **Found during:** Task 2 build
- **Issue:** `WorkspaceDbContext` and `WorkspaceConfiguration` referenced `Workspace` as a type but the compiler resolved it to the `YH.Modules.Workspace` namespace (`CS0118: "Workspace" is a namespace, but here it is used as a type`). This was not caught during planning because the analog (`AuditDbContext.cs`) does not have a namespace/type collision — Auditing's namespace is `YH.Modules.Auditing`, not `YH.Modules.Audit`.
- **Fix:** Added `using WorkspaceEntity = YH.Modules.Workspace.Domain.Workspace;` alias in both files and changed `DbSet<Workspace>` → `DbSet<WorkspaceEntity>` and `IEntityTypeConfiguration<Workspace>` → `IEntityTypeConfiguration<WorkspaceEntity>`. Inline comments reference the entity as `WorkspaceEntity` consistently.
- **Files affected:** `Data/WorkspaceDbContext.cs`, `Data/Configurations/WorkspaceConfiguration.cs`.
- **Verification:** Modules.Workspace builds clean.
- **Committed in:** dc88a6f2a

**4. [Rule 1 - Bug] XML cref resolution errors on extension method + alias + overloaded method**

- **Found during:** Task 3 build
- **Issue:** Three XML doc `cref` references failed under `GenerateDocumentationFile=true`:
  - (a) `<see cref="HttpContext.GetRouteValue(string)"/>` — `GetRouteValue` is an extension method in `Microsoft.AspNetCore.Routing`, not resolvable as a cref.
  - (b) `<see cref="WorkspaceEntity"/>` in `WorkspaceTenantStore.cs` — XML doc cannot resolve C# `using` aliases to cref identifiers.
  - (c) `<inheritdoc cref="GetAllAsync"/>` — ambiguous between `GetAllAsync()` and `GetAllAsync(int, int)` overloads.
- **Fix:** (a) replaced the cref with an inline code reference `<c>httpContext.GetRouteValue("slug")</c>`; (b) replaced `<see cref="WorkspaceEntity"/>` with `<c>Workspace</c> (<c>YH.Modules.Workspace.Domain.Workspace</c>)`; (c) disambiguated to `<inheritdoc cref="GetAllAsync()"/>`.
- **Files modified:** `MultiTenancy/WorkspaceSlugStrategy.cs`, `MultiTenancy/WorkspaceTenantStore.cs`.
- **Verification:** Modules.Workspace builds clean.
- **Committed in:** bd9934979

### Positive Deviation (NOT a bug fix)

**Path A chosen; MultitenancyModule.cs unmodified (vs plan's "if path B, modify MultitenancyModule")**

- **Found during:** Plan parse (pre-execution) — confirmed via 02-01-SUMMARY Q1 conclusion.
- **Issue:** Plan 02-02 Task 3 `<files>` lists `MultitenancyModule.cs` as a modified target. The 02-01 spike Q1 outcome (path A — external `TryAddEnumerable` works) makes that modification unnecessary. Plan Task 3 action explicitly says "wiring 方案由 02-01 SUMMARY 决定: 若 spike 结论是 'DI 外部 TryAddEnumerable 可行', 则在 WorkspaceModule.ConfigureServices 注册" — so this is the plan-blessed outcome, not an unplanned deviation.
- **Outcome:** `git diff --stat bb1fcaa9b -- yh-flow/src/Modules/Multitenancy/Modules.Multitenancy/MultitenancyModule.cs` returns empty. The Phase 1 strategy chain (`WithClaimStrategy`/`WithHeaderStrategy`/`WithDelegateStrategy`) and the two stores (`DistributedCacheStore`/`EFCoreStore<TenantDbContext>`) are 100% untouched. This is the spike Q1 success criterion operationalized.
- **Committed in:** bd9934979

## Q1 Spike Path Operationalized (FOR PLANS 02-03 / 02-04 / 02-05)

**Implementation:** `WorkspaceModule.ConfigureServices` lines 88-101 (in `bd9934979`):

```csharp
builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<
    IMultiTenantStrategy, WorkspaceSlugStrategy>());
builder.Services.TryAddEnumerable(ServiceDescriptor.Scoped<
    IMultiTenantStore<AppTenantInfo>, WorkspaceTenantStore>());
```

**Verification it works at DI resolution time:** The 02-01 spike test (`FinbuckleExternalStrategyRegistrationTests`) is still 2/2 PASS after the Workspace module landed — the test references the Finbuckle DI surface and the append behaviour. Plan 02-03 adds an integration test that drives the full resolution chain (HTTP request → slug strategy → store → AppTenantInfo stamped on `HttpContext`).

## Verification

- [x] `dotnet build src/YH.Flow.slnx --nologo` — 0 warnings / 0 errors (54 projects: 53 prior + Modules.Workspace)
- [x] Workspace entity compiles as `IGlobalEntity` (grep `: IHasDomainEvents, IGlobalEntity, ISoftDeletable, IAuditableEntity` on `Domain/Workspace.cs:21`)
- [x] `WorkspaceDbContext.cs` contains `ApplyConfigurationsFromAssembly` (line 60) BEFORE `base.OnModelCreating` (line 64) — Pitfall 6 order verified
- [x] `WorkspaceSlugStrategy` checks slug non-empty (grep `GetRouteValue("slug") is not string slug || string.IsNullOrEmpty(slug)` on `WorkspaceSlugStrategy.cs:87`)
- [x] `MultitenancyModule.cs` strategy chain (WithClaimStrategy/WithHeaderStrategy/WithDelegateStrategy) UNCHANGED — `git diff --stat bb1fcaa9b -- MultitenancyModule.cs` is empty
- [x] `dotnet test src/Tests/Workspace.Tests` (full project) — 2/2 PASS (Q1 spike still authoritative)
- [x] `dotnet test src/Tests/Identity.Tests` — 412/412 PASS (Phase 1 zero regression; T-2-fintenant BLOCKING threat confirmed not tripped)

## Notes for Plans 02-03 / 02-04 / 02-05 (Wiring Continuation)

1. **TenantId column auto-add (Pitfall 6):** `WorkspaceMember` and `WorkspaceInvitation` tables will have a `TenantId` column generated by the EF Core migration (plan 02-06) — Finbuckle's `IsMultiTenant()` (auto-applied via `ApplyTenantIsolationByDefault`) injects it as a shadow property. The unique indexes on those tables (`(TenantId, UserId)` / `(TenantId, Accepted)` / `TokenHash`) will be widened automatically by `AdjustUniqueIndexes()` where appropriate.
2. **Migration generation (02-06):** When `dotnet ef migrations add Initial -c WorkspaceDbContext` runs, the migration must produce the schema with the correct unique indexes (Slug on Workspaces, (TenantId, UserId) on WorkspaceMembers, TokenHash on WorkspaceInvitations). The configurations in this plan define those — migration is purely additive.
3. **Cache invalidation (02-04):** `WorkspaceTenantStore` caches slug → AppTenantInfo for 30 minutes. The Workspace CRUD handlers in 02-04 MUST call `IMultiTenantStore<AppTenantInfo>.RemoveAsync(slug)` (or `UpdateAsync(tenantInfo)`) after any rename / slug transfer / delete so the cache does not serve stale resolutions. Both methods are no-ops on the Workspaces table but invalidate the cache entry — that is their entire role.
4. **CurrentWorkspaceContext population (02-03):** `WorkspaceMembershipMiddleware` is the single writer per scope. It reads `IMultiTenantContextAccessor<AppTenantInfo>.MultiTenantContext?.TenantInfo` (populated by Finbuckle after the slug strategy + WorkspaceTenantStore resolved), parses `tenantInfo.Id` as the workspace Guid, queries `WorkspaceDbContext.Members` for the current user's role, and calls `ICurrentWorkspaceContext.SetContext(workspaceId, slug, role)`. If `tenantInfo` is null (top-level endpoint, no `{slug}`) the context stays null-valued — `RequireWorkspaceRoleAuthorizationHandler` (02-03) must fail authorization with a null role (Plane semantics: non-member → 403).
5. **SlugStrategy Priority=-100 ordering:** Finbuckle evaluates strategies in registration order with `Priority` as a hint. The slug strategy is registered AFTER the Phase 1 chain (because WorkspaceModule Order=200 > Multitenancy ~100) but its negative priority should still rank it ahead. If runtime tracing (02-03 integration test) shows the claim strategy winning, swap to a more negative priority. The Phase 1 `WithClaimStrategy(ClaimConstants.Tenant)` is no-op pre-auth (Pitfall 1 / `MultitenancyModule.cs:96-97`) so it should never produce a non-null identifier on a `{slug}` request.

## Known Stubs

| Stub                                                                                     | File                                                                 | Line                   | Reason                                                                                                                                                                             | Resolved By                                                                     |
| ---------------------------------------------------------------------------------------- | -------------------------------------------------------------------- | ---------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------- |
| `WorkspaceModule.ConfigureMiddleware` empty                                              | `yh-flow/src/Modules/Workspace/Modules.Workspace/WorkspaceModule.cs` | whole method           | Intentional — `WorkspaceMembershipMiddleware` is implemented in plan 02-03. Inline `// TODO 02-03` marks the insertion point.                                                      | Plan 02-03 (Wave 2).                                                            |
| `WorkspaceModule.MapEndpoints` empty                                                     | `yh-flow/src/Modules/Workspace/Modules.Workspace/WorkspaceModule.cs` | whole method           | Intentional — Workspace CRUD + members + invitations endpoints land in plans 02-04 / 02-05. Inline `// TODO 02-04/05` marks the insertion points.                                  | Plans 02-04, 02-05.                                                             |
| `WorkspaceMembershipService` / `SlugGenerator` / `InvitationTokenService` not registered | `yh-flow/src/Modules/Workspace/Modules.Workspace/WorkspaceModule.cs` | ConfigureServices TODO | These services are owned by plans 02-04 / 02-05 — registering them now would require defining them now (out of Wave 1 scope). Inline `// TODO 02-05` marks the registration block. | Plans 02-04 (SlugGenerator), 02-05 (MembershipService, InvitationTokenService). |

No code-level stubs that flow into UI rendering or accept empty/mock data — the entities, DbContext, strategy, store, and context all have complete implementations.

## TDD Gate Compliance

N/A — this plan is `type: execute` (not `type: tdd`). No TDD RED/GREEN/REFACTOR gate applicable. The 02-01 spike is a discovery test (asserts current Finbuckle behaviour), not a feature-driving test. Plan 02-03 will add integration tests for the slug resolution chain and the IGlobalEntity guard (T-2-isolation BLOCKING threat verification).

## Threat Flags

None. The plan's `<threat_model>` registered 8 threats; all mitigations are in place:

- **T-2-isolation [BLOCKING]:** `Workspace` is `IGlobalEntity`; `ApplyTenantIsolationByDefault` skips it; the resolution cycle is impossible (the Workspaces table is the tenant source, not a tenant-filtered consumer). 02-03 integration test will verify end-to-end.
- **T-2-fintenant [BLOCKING]:** `MultitenancyModule.cs` is UNMODIFIED. `AddMultiTenant<AppTenantInfo>` was never re-invoked. `TryAddEnumerable` is the safe append mechanism (02-01 spike Q1).
- **T-2-slugstrategy:** `WorkspaceSlugStrategy` strictly checks `GetRouteValue("slug") is string slug && !string.IsNullOrEmpty(slug)`; returns null on top-level endpoints (Pitfall 2).
- **T-2-tenantleak:** `WorkspaceTenantStore` filters `!w.IsDeleted` so soft-deleted workspaces stop resolving; returns null on miss (Pitfall 6 — never throws).
- **T-2-eop:** Composite unique `(TenantId, UserId)` on `WorkspaceMemberConfiguration`; `AdjustUniqueIndexes` widens consistently.
- **T-2-softdelete:** `Workspace.SoftDelete(now)` implements D-08 `__{epochSeconds}` slug release. 02-04 `DeleteWorkspace` handler will call it.
- **T-2-cache (accept):** `ws:slug:` cache namespace; payload is non-sensitive (workspaceGuid + slug + name). 02-04 handlers invalidate on rename/delete.
- **T-2-SC (accept):** 0 new NuGet packages — purely additive source code.

No NEW threat surface introduced. The plan added 14 source files but no HTTP endpoints (endpoints land in 02-04/05), no schema migrations (02-06), and no changes to the Phase 1 strategy chain (MultitenancyModule untouched).

## Self-Check: PASSED

All 14 created files verified present on disk. All 3 task commits (`38d8877e7`, `dc88a6f2a`, `bd9934979`) verified in `git log`. Full-solution build 0/0; Workspace.Tests 2/2 + Identity.Tests 412/412 PASS. `MultitenancyModule.cs` zero-diff against base `bb1fcaa9b` confirmed (path A — no Phase 1 regression).
