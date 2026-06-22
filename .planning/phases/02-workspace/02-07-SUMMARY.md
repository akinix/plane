---
phase: 02-workspace
plan: 07
plan_id: 02-07
subsystem: workspace-relational-testing
type: execute
wave: 6
gap_closure: true
depends_on: [02-05, 02-06]
tags: [relational-tests, testcontainers, postgres, finbuckle, cr-02, tenant-scoping]
requirements_completed: [REQ-2.1, NFR-2]
tech-stack:
  added:
    - Testcontainers.PostgreSql 4.11.0 (central version; postgres:17-alpine container)
    - AsyncLocalMultiTenantContextAccessor<AppTenantInfo> (Finbuckle 10.1.0; reused for test tenant scope)
  patterns:
    - Testcontainers IAsyncLifetime fixture pattern (mirrors FshWebApplicationFactory.cs:29-77)
    - RAII tenant-scope helper (FinbuckleTestTenantScope: IDisposable)
    - Cross-tenant relational test with TRUNCATE-RESTART IDENTITY-CASCADE isolation
    - Per-tenant DbContext factory via shared AsyncLocal MultiTenantContextSetter
key-files:
  created:
    - yh-flow/src/Tests/Workspace.Tests/Integration/PostgresFixtures/WorkspacePostgresFixture.cs (182 lines)
    - yh-flow/src/Tests/Workspace.Tests/Integration/PostgresFixtures/FinbuckleTestTenantScope.cs (75 lines)
    - yh-flow/src/Tests/Workspace.Tests/Integration/CrossTenant/CrossTenantListUserWorkspacesTests.cs (172 lines)
  modified:
    - yh-flow/src/Tests/Workspace.Tests/Workspace.Tests.csproj (Testcontainers.PostgreSql + Migrations.PostgreSQL refs)
    - yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Workspaces/ListUserWorkspaces/ListUserWorkspacesQueryHandler.cs (CR-02 IgnoreQueryFilters + remarks update)
decisions:
  - "Finbuckle 10.1.0 API: IMultiTenantContextSetter.MultiTenantContext is set-only; reading the previous context requires IMultiTenantContextAccessor (concrete AsyncLocalMultiTenantContextAccessor<T> has read+write). FinbuckleTestTenantScope pairs the two DI slots."
  - "Testcontainers.PostgreSql is the only explicit PackageReference added — Npgsql.EntityFrameworkCore.PostgreSQL, Finbuckle.MultiTenant[.EntityFrameworkCore], and Microsoft.Extensions.Hosting.Abstractions all arrive transitively via Modules.Workspace/Multitenancy (verified via 'dotnet list package --include-transitive'). Explicit refs would trigger CS1703/NU1108 duplicate-ref errors under TreatWarningsAsErrors."
  - "TRUNCATE TABLE ... RESTART IDENTITY CASCADE used for test isolation — independent of full fixture re-init; keeps container alive across methods in the same test class."
  - "Workspace.Tests now ProjectReferences YH.Flow.Migrations.PostgreSQL (not just Modules.Workspace) — required so MigrateAsync() can load the InitialWorkspace migration assembly via the test project's deps.json."
metrics:
  duration: ~35m
  completed: 2026-06-22T02:47:00Z
  tasks_completed: 2
  tasks_total: 2
  files_created: 3
  files_modified: 2
  tests_added: 2
  tests_total_workspace: 98
---

# Phase 02 Plan 07: WorkspacePostgresFixture + CR-02 ListUserWorkspaces IgnoreQueryFilters Summary

First relational-test capability for the Workspace module + the CR-02 fix, closing one of three CRITICAL tenant-scoping defects (CR-02) that the 96/96 InMemory suite is structurally unable to catch. Establishes the Testcontainers PG fixture that 02-08 (CR-01 + CR-03) will reuse.

## What Was Built

### Task 1 — Relational test infrastructure (commit `eaebfa197`)

- **`WorkspacePostgresFixture`** (`yh-flow/src/Tests/Workspace.Tests/Integration/PostgresFixtures/WorkspacePostgresFixture.cs`):
  - IAsyncLifetime + IDisposable fixture; spins up `postgres:17-alpine` via `Testcontainers.PostgreSql` 4.11.0 with `WithAutoRemove(true).WithCleanUp(true)`.
  - `InitializeAsync` runs the production `InitialWorkspace` migration on real PG → the `TenantId` shadow column + composite unique index `IX_WorkspaceMembers_Tenant_User` actually exist (the InMemory provider skips Finbuckle's `AdjustUniqueIndexes` + `ApplyTenantIsolationByDefault` pipeline entirely — the root cause the fixture exists to fix).
  - `CreateContextForTenant(AppTenantInfo tenant)` factory: stamps the chosen tenant onto the shared `AsyncLocalMultiTenantContextAccessor<AppTenantInfo>` (read from DI), then constructs a fresh `WorkspaceDbContext` whose Finbuckle filter observes that tenant.
  - `RootTenant` static field: `new AppTenantInfo(MultitenancyConstants.Root.Id, MultitenancyConstants.Root.Id, MultitenancyConstants.Root.Name)` — 3-arg constructor (id / identifier / name), deliberately NOT the 5-arg ConnectionString overload (connection is supplied via DbContextOptionsBuilder.UseNpgsql, not via TenantInfo).
  - `Services` property exposes the DI container for tests to resolve `IMultiTenantContextSetter` / `AsyncLocalMultiTenantContextAccessor<AppTenantInfo>`.
  - **DI wiring (for 02-08 reference)**:
    ```csharp
    services.AddSingleton<AsyncLocalMultiTenantContextAccessor<AppTenantInfo>>();
    services.AddSingleton<IMultiTenantContextAccessor<AppTenantInfo>>(sp => sp.GetRequiredService<AsyncLocalMultiTenantContextAccessor<AppTenantInfo>>());
    services.AddSingleton<IMultiTenantContextAccessor>(sp => sp.GetRequiredService<AsyncLocalMultiTenantContextAccessor<AppTenantInfo>>());
    services.AddSingleton<IMultiTenantContextSetter>(sp => sp.GetRequiredService<AsyncLocalMultiTenantContextAccessor<AppTenantInfo>>());
    ```
    The same singleton is resolved under all four DI slots so `CreateContextForTenant` (writes via setter) and `FinbuckleTestTenantScope` (reads via accessor, writes via setter) observe the same AsyncLocal slot.

- **`FinbuckleTestTenantScope`** (`yh-flow/src/Tests/Workspace.Tests/Integration/PostgresFixtures/FinbuckleTestTenantScope.cs`):
  - RAII helper: constructor captures previous `MultiTenantContext` via the accessor, sets new tenant via the setter; `Dispose()` restores.
  - **Signature (02-08 reuse path)**:
    ```csharp
    public FinbuckleTestTenantScope(
        AsyncLocalMultiTenantContextAccessor<AppTenantInfo> accessor,   // read previous
        IMultiTenantContextSetter setter,                               // write new + restore
        AppTenantInfo newTenant)
    ```
  - Design note: Finbuckle 10.1.0's `IMultiTenantContextSetter.MultiTenantContext` is a set-only property by library design ("implementation detail, not intended for general use" per the XML doc). Reading the previous value therefore goes through the concrete `AsyncLocalMultiTenantContextAccessor<AppTenantInfo>` whose property is read/write (backs onto an `AsyncLocal<T>` field).

### Task 2 — CR-02 fix + cross-tenant relational test (commit `ca9ac9d18`)

- **`ListUserWorkspacesQueryHandler.cs` line 56**: inserted `.IgnoreQueryFilters()` between `_db.Members` and `.AsNoTracking()`. Full chain now:

  ```csharp
  var workspaceIds = await _db.Members
      .IgnoreQueryFilters()           // CR-02: cross-workspace aggregation is the endpoint purpose
      .AsNoTracking()
      .Where(m => m.UserId == query.UserId && m.IsActive && !m.IsDeleted)
      .Select(m => m.WorkspaceId)
      .Distinct()
      .ToListAsync(cancellationToken)
      .ConfigureAwait(false);
  ```

  Handler `<remarks>` updated with an explicit "Tenant filter bypass (CR-02)" paragraph documenting the root cause + why ignoring the filter is correct behavior (NOT a leak).

- **`CrossTenantListUserWorkspacesTests`** (2 `[Fact]` tests):
  - `Returns_Workspaces_Across_Distinct_Tenants_On_Real_Postgres`: seeds one user with active memberships in two distinct workspace tenants; invokes the handler scoped to `RootTenant` (neither A nor B); asserts BOTH workspace ids surface. **This is the load-bearing CR-02 assertion** — pre-fix, Finbuckle's `TenantId` filter drops both rows; post-fix, `IgnoreQueryFilters()` surfaces both.
  - `Inactive_Membership_Row_Is_Excluded_From_Results`: boundary case — an `IsActive=false` row must NOT surface even with `IgnoreQueryFilters` (proves the business-rule Where clause is preserved).
  - `[Trait("Category", "Postgres")]` + `[Trait("Category", "RequiresDocker")]` allow CI to filter.
  - **Test isolation**: each method starts with `TRUNCATE TABLE ... RESTART IDENTITY CASCADE` across all three Workspace tables (WorkspaceMembers / WorkspaceInvitations / Workspaces).

## Acceptance Criteria — All Met

- `IgnoreQueryFilters` present at `ListUserWorkspacesQueryHandler.cs:56` ✓
- `CR-02` + `02-REVIEW` reference in handler comment + remarks ✓
- `IClassFixture<WorkspacePostgresFixture>`, `Returns_Workspaces_Across_Distinct_Tenants_On_Real_Postgres`, `Category=Postgres` in test file ✓
- Test body has ≥2 distinct workspace tenants + RootTenant handler invocation + dual workspaceId assertion ✓
- `dotnet build YH.Flow.slnx` = 0/0 ✓
- `dotnet test Workspace.Tests --filter Category=Postgres` = 2/2 pass on real PG (Docker up) ✓
- `dotnet test Workspace.Tests` (full) = 98/98 (96 InMemory preserved + 2 relational) ✓
- `WorkspaceMember.cs` / `WorkspaceMemberConfiguration.cs` / migration files untouched ✓ (verified via git diff scope — query-layer fix only)

## 6 Required SUMMARY Records

### 1. WorkspacePostgresFixture assembly path

`yh-flow/src/Tests/Workspace.Tests/Integration/PostgresFixtures/WorkspacePostgresFixture.cs` — namespace `YH.Tests.Workspace.Integration.PostgresFixtures`, type `public sealed class WorkspacePostgresFixture : IAsyncLifetime, IDisposable`.

### 2. `CreateContextForTenant(AppTenantInfo)` signature

```csharp
public WorkspaceDbContext CreateContextForTenant(AppTenantInfo tenant)
```

Stamps the tenant onto the shared AsyncLocal setter, then returns a new tracked `WorkspaceDbContext` constructed via `UseNpgsql(_postgres.GetConnectionString(), sql => sql.MigrationsAssembly("YH.Flow.Migrations.PostgreSQL"))`. Caller is responsible for disposing. **Throws `InvalidOperationException` if called before `InitializeAsync`.**

### 3. `FinbuckleTestTenantScope` usage

```csharp
var accessor = fixture.Services.GetRequiredService<AsyncLocalMultiTenantContextAccessor<AppTenantInfo>>();
var setter   = fixture.Services.GetRequiredService<IMultiTenantContextSetter>();
using (new FinbuckleTestTenantScope(accessor, setter, workspaceTenant))
{
    await using var ctx = fixture.CreateContextForTenant(workspaceTenant);
    await ctx.Members.AddAsync(WorkspaceMember.Create(...));
    await ctx.SaveChangesAsync();
}
```

On `Dispose()`, the previous `MultiTenantContext` is restored (null if scope was the first to ever set it).

### 4. CR-02 fix line + IgnoreQueryFilters call

`yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Workspaces/ListUserWorkspaces/ListUserWorkspacesQueryHandler.cs` — `.IgnoreQueryFilters()` is the first call after `_db.Members` (line 56 in the post-fix file). Comment above the call:

```
// CR-02 (02-REVIEW.md): IgnoreQueryFilters is REQUIRED here. This is a top-level endpoint
// (GET /api/v1/users/me/workspaces/); the DbContext is scoped to the caller's current/last
// tenant, so Finbuckle's auto-applied TenantId filter would otherwise drop every membership
// row belonging to a DIFFERENT tenant. Cross-workspace aggregation is the entire purpose of
// the endpoint (REQ-2.1). The Where clauses on UserId/IsActive/IsDeleted below are business
// rules and stay intact.
```

### 5. Test isolation strategy

Each `[Fact]` method runs `TRUNCATE TABLE "yhschema.Workspace"."WorkspaceMembers", "yhschema.Workspace"."WorkspaceInvitations", "yhschema.Workspace"."Workspaces" RESTART IDENTITY CASCADE;` at the start. Container + schema are kept alive for the test class lifetime (IClassFixture); rows are wiped per-method. This avoids the overhead of fixture re-init while still guaranteeing no cross-test state leakage.

### 6. `IMultiTenantContextSetter` DI resolution path (for 02-08)

```csharp
// Fixture DI registers ONE singleton instance under FOUR DI slots:
services.AddSingleton<AsyncLocalMultiTenantContextAccessor<AppTenantInfo>>();
services.AddSingleton<IMultiTenantContextAccessor<AppTenantInfo>>(sp => sp.GetRequiredService<AsyncLocalMultiTenantContextAccessor<AppTenantInfo>>());
services.AddSingleton<IMultiTenantContextAccessor>(sp => sp.GetRequiredService<AsyncLocalMultiTenantContextAccessor<AppTenantInfo>>());
services.AddSingleton<IMultiTenantContextSetter>(sp => sp.GetRequiredService<AsyncLocalMultiTenantContextAccessor<AppTenantInfo>>());

// 02-08 accept-invitation test will resolve the setter the same way FshJobActivator.cs:40 does:
var setter = fixture.Services.GetRequiredService<IMultiTenantContextSetter>();
setter.MultiTenantContext = new MultiTenantContext<AppTenantInfo>(workspaceTenant);
// ... SaveChanges ...
```

This matches the production DI resolution path (`FshJobActivator.cs:40`, `FshWebApplicationFactory.cs:277`) — same slot, same semantics.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added ProjectReference to YH.Flow.Migrations.PostgreSQL**

- **Found during:** Task 2 first `dotnet test --filter Category=Postgres` run
- **Issue:** `System.IO.FileNotFoundException: Could not load file or assembly 'YH.Flow.Migrations.PostgreSQL'` thrown by `MigrateAsync()`. EF Core's `MigrationsAssembly` resolver needs the assembly to be loadable via the test project's deps.json.
- **Fix:** Added `<ProjectReference Include="..\..\Host\YH.Flow.Migrations.PostgreSQL\YH.Flow.Migrations.PostgreSQL.csproj" />` to `Workspace.Tests.csproj`.
- **Files modified:** `yh-flow/src/Tests/Workspace.Tests/Workspace.Tests.csproj`
- **Commit:** `ca9ac9d18`

**2. [Rule 3 - Blocking] Finbuckle 10.1.0 API correction**

- **Found during:** Task 1 build — `IMultiTenantContextSetter.MultiTenantContext` is set-only by library design
- **Issue:** Plan pseudocode implied `setter.MultiTenantContext` was readable. Finbuckle's XML doc explicitly marks it as "implementation detail, not intended for general use" and only provides a setter.
- **Fix:** `FinbuckleTestTenantScope` constructor takes both `AsyncLocalMultiTenantContextAccessor<AppTenantInfo>` (read previous) and `IMultiTenantContextSetter` (write new + restore). The same DI singleton is registered under both slots so they observe the same AsyncLocal field.
- **Files modified:** `yh-flow/src/Tests/Workspace.Tests/Integration/PostgresFixtures/FinbuckleTestTenantScope.cs`
- **Commit:** `eaebfa197`

### Package-Legitimacy Check — N/A

No package install was attempted (Rule 3 exclusion does not apply). `Testcontainers.PostgreSql` 4.11.0 was already in `Directory.Packages.props` central version management (line 108); only the explicit `<PackageReference>` was added to the csproj. `Npgsql.EntityFrameworkCore.PostgreSQL`, `Finbuckle.MultiTenant[.EntityFrameworkCore]`, and `Microsoft.Extensions.Hosting.Abstractions` were already transitively present via `Modules.Workspace` ProjectReference (verified via `dotnet list package --include-transitive`).

## Authentication Gates

None.

## Known Stubs

None.

## Threat Flags

None. Threat model register (`T-02-07-01` through `T-02-07-SC`) fully mitigated as planned — no new threat surface introduced.

## Self-Check: PASSED

Files:

- FOUND: yh-flow/src/Tests/Workspace.Tests/Integration/PostgresFixtures/WorkspacePostgresFixture.cs
- FOUND: yh-flow/src/Tests/Workspace.Tests/Integration/PostgresFixtures/FinbuckleTestTenantScope.cs
- FOUND: yh-flow/src/Tests/Workspace.Tests/Integration/CrossTenant/CrossTenantListUserWorkspacesTests.cs
- FOUND: yh-flow/src/Modules/Workspace/Modules.Workspace/Features/v1/Workspaces/ListUserWorkspaces/ListUserWorkspacesQueryHandler.cs (modified)

Commits:

- FOUND: eaebfa197 — feat(02-07): add WorkspacePostgresFixture + FinbuckleTestTenantScope
- FOUND: ca9ac9d18 — fix(02-07): CR-02 IgnoreQueryFilters + cross-tenant relational test

Gates:

- `dotnet build YH.Flow.slnx --nologo` exit 0 (0 warnings / 0 errors)
- `dotnet test Workspace.Tests --filter "Category=Postgres"` exit 0 (2/2 pass on real PG, Docker up)
- `dotnet test Workspace.Tests` exit 0 (98/98 — 96 InMemory preserved + 2 relational)
- No edits to `WorkspaceMember.cs`, `WorkspaceMemberConfiguration.cs`, or migration files (git diff scope confirmed)
