---
phase: 03-project
plan: 01
subsystem: api, test
tags: dotnet, csproj, scaffold, vertical-slice, contracts, module
requires:
  - phase: 02-workspace
    provides: Workspace module pattern (IModule, IModuleConstants, Contracts/DTOs structure, test project pattern, slnx wiring)
  - phase: 01-foundation
    provides: BuildingBlocks (Caching, Core, Persistence, Web), FshModule auto-discovery, host Program.cs wiring pattern
provides:
  - Modules.Project.Contracts assembly (DTOs, constants)
  - Modules.Project assembly (module skeleton with FshModule attribute)
  - Project.Tests test project (xunit scaffolding)
  - Host wiring (Api/DbMigrator Program.cs, Migration csproj, slnx)
affects: 03-02 (domain entities), 03-03 (project endpoint CRUD), 03-04 (member endpoints)
tech-stack:
  added: []
  patterns:
    - "Contracts project structure: AssemblyInfo intentionally empty (no FshModule)"
    - "Implementation project: FshModule Order=250 between Workspace (200) and Auditing (300)"
    - "Test project: xunit + Shouldly + AutoFixture + NSubstitute + InMemory"
    - "Host wiring: typeof(ProjectModule).Assembly in moduleAssemblies + typeof(ProjectConstants) in AddMediator"
key-files:
  created:
    - src/Modules/Project/Modules.Project.Contracts/Modules.Project.Contracts.csproj
    - src/Modules/Project/Modules.Project.Contracts/AssemblyInfo.cs
    - src/Modules/Project/Modules.Project.Contracts/Constants/ProjectConstants.cs
    - src/Modules/Project/Modules.Project.Contracts/DTOs/ProjectDto.cs
    - src/Modules/Project/Modules.Project.Contracts/DTOs/ProjectMemberDto.cs
    - src/Modules/Project/Modules.Project/Modules.Project.csproj
    - src/Modules/Project/Modules.Project/AssemblyInfo.cs
    - src/Modules/Project/Modules.Project/ProjectModuleConstants.cs
    - src/Modules/Project/Modules.Project/ProjectModule.cs
    - src/Tests/Project.Tests/Project.Tests.csproj
    - src/Tests/Project.Tests/GlobalUsings.cs
    - src/Tests/Project.Tests/Usings.cs
  modified:
    - src/YH.Flow.slnx
    - src/Host/YH.Flow.Api/Program.cs
    - src/Host/YH.Flow.DbMigrator/Program.cs
    - src/Host/YH.Flow.Migrations.PostgreSQL/YH.Flow.Migrations.PostgreSQL.csproj
key-decisions:
  - "ProjectModule Order=250 — between Workspace (200) and Auditing (300)"
  - "Modules.Project.csproj does NOT reference Finbuckle packages directly (comes transitively via Persistence)"
  - "Modules.Project.csproj references Workspace.Contracts + Identity.Contracts (Contracts only, not runtime)"
  - "ProjectDto uses JsonPropertyName for audit timestamps (created_at/updated_at/deleted_at) following WorkspaceDto pattern"
  - "ProjectModule.cs has TODO placeholders for Wave 2 (ProjectDbContext) and Wave 3/4 (endpoint registration)"
patterns-established:
  - "Contracts project: InternalsVisibleTo not needed (no [assembly: InternalsVisibleTo] in this pattern)"
  - "Project module skeleton: ConfigureServices/ConfigureMiddleware/MapEndpoints with TODO comments for future waves"
requirements-completed: [REQ-3.1, REQ-3.2, REQ-3.3]
duration: 12min
completed: 2026-06-24
---

# Phase 03 Plan 01: Scaffold Summary

**Project module skeleton (Contracts DTOs + implementation project + test project + host wiring) following Workspace module pattern**

## Performance

- **Duration:** 12 min
- **Started:** 2026-06-24T11:40:00Z (approx)
- **Completed:** 2026-06-24T11:52:00Z (approx)
- **Tasks:** 3
- **Files modified:** 16

## Accomplishments

- Created `Modules.Project.Contracts` csproj with Plane-compatible DTOs (ProjectDto, ProjectMemberDto) and constants
- Created `Modules.Project` implementation project with FshModule attribute (Order=250) and bare-bones module skeleton
- Created `Project.Tests` test project with xunit + Shouldly + AutoFixture + NSubstitute references
- Wired Project module into host: both Api and DbMigrator Program.cs register ProjectModule + ProjectConstants
- Added Project migration folder and project reference to YH.Flow.Migrations.PostgreSQL
- Added slnx entries for /Modules/Project/ folder and Project.Tests under /Tests/ folder
- Full solution build passes: 56 projects, 0 errors, 0 warnings

## Task Commits

Each task was committed atomically:

1. **Task 1: Create Modules.Project.Contracts project with DTOs and constants** - `a8588f92b` (feat)
2. **Task 2: Create Modules.Project implementation project with ModuleConstants and Module skeleton** - `408fb2ffb` (feat)
3. **Task 3: Create Project.Tests test project and wire all modules into host** - `c6eab6233` (feat)

## Files Created/Modified

### Contracts project (Task 1)

- `yh-flow/src/Modules/Project/Modules.Project.Contracts/Modules.Project.Contracts.csproj` — RootNamespace YH.Modules.Project.Contracts, references Identity.Contracts
- `yh-flow/src/Modules/Project/Modules.Project.Contracts/AssemblyInfo.cs` — Intentionally empty (Contracts is not a module)
- `yh-flow/src/Modules/Project/Modules.Project.Contracts/Constants/ProjectConstants.cs` — ForbiddenIdentifierCharsPattern, IdentifierMaxLength, NameMaxLength, DescriptionMaxLength, SlugMaxLength
- `yh-flow/src/Modules/Project/Modules.Project.Contracts/DTOs/ProjectDto.cs` — Full Plane-compatible ProjectSerializer DTO with audit timestamps
- `yh-flow/src/Modules/Project/Modules.Project.Contracts/DTOs/ProjectMemberDto.cs` — Membership DTO with UserSummary reference

### Implementation project (Task 2)

- `yh-flow/src/Modules/Project/Modules.Project/Modules.Project.csproj` — References BuildingBlocks + Contracts + Identity.Contracts + Workspace.Contracts
- `yh-flow/src/Modules/Project/Modules.Project/AssemblyInfo.cs` — `[assembly: FshModule(typeof(ProjectModule), 250)]`
- `yh-flow/src/Modules/Project/Modules.Project/ProjectModuleConstants.cs` — ModuleId/Name/ApiPrefix/SchemaName
- `yh-flow/src/Modules/Project/Modules.Project/ProjectModule.cs` — Skeleton with TODO placeholders

### Test project + host wiring (Task 3)

- `yh-flow/src/Tests/Project.Tests/Project.Tests.csproj` — xunit project with Shouldly/AutoFixture/NSubstitute/InMemory
- `yh-flow/src/Tests/Project.Tests/GlobalUsings.cs` — Shouldly + Xunit
- `yh-flow/src/Tests/Project.Tests/Usings.cs` — Empty (no Finbuckle usings needed)
- `yh-flow/src/YH.Flow.slnx` — Added /Modules/Project/ folder and Project.Tests
- `yh-flow/src/Host/YH.Flow.Api/Program.cs` — Added ProjectModule + ProjectConstants
- `yh-flow/src/Host/YH.Flow.DbMigrator/Program.cs` — Added ProjectModule + ProjectConstants
- `yh-flow/src/Host/YH.Flow.Migrations.PostgreSQL/YH.Flow.Migrations.PostgreSQL.csproj` — Added Project reference + Folder include

## Decisions Made

- **Module order 250:** Workspace is 200, Project is 250, Auditing is 300 — Project depends on workspace infrastructure
- **No direct Finbuckle references:** IHasTenant and BaseDbContext come transitively via Persistence
- **Contracts-only cross-module refs:** Only Identity.Contracts and Workspace.Contracts referenced (not runtime projects), preserving module boundary
- **TODO comments as intended stubs:** ProjectModule.cs has commented-out code for future waves — these are NOT accidental stubs

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] ProjectConstants class must be `static` (not `sealed`)**

- **Found during:** Task 1 (build verification)
- **Issue:** SonarAnalyzer rule S1118 requires pure-constant classes (all members are implicitly static) to be `static` rather than `sealed`
- **Fix:** Changed `public sealed class ProjectConstants` to `public static class ProjectConstants`
- **Files modified:** src/Modules/Project/Modules.Project.Contracts/Constants/ProjectConstants.cs
- **Verification:** Build passes with 0 errors
- **Committed in:** a8588f92b (Task 1 commit)

**2. [Rule 1 - Bug] Removed unresolvable cref in XML doc comment**

- **Found during:** Task 2 (build verification)
- **Issue:** `<see cref="ProjectDbContext"/>` in XML doc could not be resolved because ProjectDbContext does not exist yet (will be created in Wave 2/03-02)
- **Fix:** Changed to plain text "ProjectDbContext" without cref
- **Files modified:** src/Modules/Project/Modules.Project/ProjectModule.cs
- **Verification:** Build passes with 0 errors
- **Committed in:** 408fb2ffb (Task 2 commit)

**3. [Rule 1 - Bug] Removed unused `using` statements**

- **Found during:** Task 2 (build after cref fix)
- **Issue:** Unused `using Asp.Versioning;`, `using Microsoft.AspNetCore.Http;`, `using Microsoft.Extensions.DependencyInjection;`, `using Microsoft.Extensions.Diagnostics.HealthChecks;`, `using YH.Framework.Persistence;` would violate TreatWarningsAsErrors
- **Fix:** Removed unused usings; kept only those needed by the current code
- **Files modified:** src/Modules/Project/Modules.Project/ProjectModule.cs
- **Verification:** Build passes with 0 errors
- **Committed in:** 408fb2ffb (Task 2 commit)

---

**Total deviations:** 3 auto-fixed (3 Rule 1 - Bug)
**Impact on plan:** All auto-fixes necessary for TreatWarningsAsErrors compliance. No scope creep.

## Issues Encountered

None — all tasks executed as planned with minor build-time corrections (deviations documented above).

## Known Stubs

- **ProjectModule.cs** — `ConfigureServices`, `ConfigureMiddleware`, `MapEndpoints` are intentionally empty with TODO comments. These will be filled in by Waves 2-4 (plans 03-02, 03-03, 03-04). This is by design per the plan's wave structure.

## Threat Flags

None — no new network endpoints, auth paths, or schema changes introduced in this scaffold plan.

## User Setup Required

None — no external service configuration required.

## Verification Results

- `dotnet build src/YH.Flow.slnx --nologo` — 0 errors, 0 warnings (56 projects)
- `dotnet build src/Tests/Project.Tests/Project.Tests.csproj --nologo` — 0 errors (test project compiles)
- FshModule attribute `[assembly: FshModule(typeof(ProjectModule), 250)]` is present in AssemblyInfo.cs
- ProjectModule is registered in both Api and DbMigrator Program.cs via `moduleAssemblies` array
- Migration project references Modules.Project

## Next Phase Readiness

- Scaffold is complete: Contracts DTOs, module skeleton, test project, and host wiring all in place
- Ready for Wave 2 (03-02): Domain entities (Project, ProjectMember), ProjectDbContext, and EF Core configurations
- Zero new NuGet packages added — matches the threat model's accept disposition for T-3-scaffold-01/02

## Self-Check: PASSED

- All 12 created files verified on disk
- All 3 commit hashes confirmed in git log
- Build: 56 projects, 0 errors, 0 warnings

---

_Phase: 03-project Plan: 01_
_Completed: 2026-06-24_
