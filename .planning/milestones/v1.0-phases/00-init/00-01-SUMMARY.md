---
phase: 00-init
plan: 01
subsystem: infra
tags: [dotnet, aspire, fullstackhero, namespace-rename, powershell]

# Dependency graph
requires:
  - phase: none
    provides: fullstackhero template at D:/github/fullstackhero-dotnet-starter-kit/

provides:
  - yh-flow/ directory with complete template structure
  - FSH -> YH namespace rename completed across all source files
  - YH.Flow.slnx solution file
  - PowerShell rename script (scripts/rename-fsh-to-yh.ps1)
  - Build verified: 51 projects, 0 errors, 0 warnings

affects: [01-foundation, all subsequent phases]

# Tech tracking
tech-stack:
  added: [.NET 10, Aspire 13.4, EF Core 10, PowerShell]
  patterns: [modular-monolith, YH.Framework.*, YH.Modules.*, YH.Flow.*]

key-files:
  created:
    - yh-flow/scripts/rename-fsh-to-yh.ps1

  modified:
    - yh-flow/src/YH.Flow.slnx
    - yh-flow/src/Directory.Build.props
    - yh-flow/src/Host/YH.Flow.AppHost/AppHost.cs
    - 1500+ source files (namespace replacements)

key-decisions:
  - "Excluded .vs/, deploy/, docs/, templates/ from template copy"
  - "Slimmed clients/admin and clients/dashboard to only package.json (D-13)"
  - "Script uses -File flag and excludes .vs/ to avoid directory-as-file errors"
  - "Added YH.Starter. -> YH.Flow. handling to script for idempotency"

patterns-established:
  - "YH.Framework.{Name} for BuildingBlocks namespaces"
  - "YH.Modules.{Name} for Module namespaces"
  - "YH.Flow.{Name} for Host project namespaces"
  - "PowerShell scripts in yh-flow/scripts/ for repeatable operations"

requirements-completed: [ROADMAP T0.1, T0.3, T0.4, T0.5, CONTEXT D-01, D-03, D-05, D-07, D-08]

# Metrics
duration: 20min
completed: 2026-06-16
---

# Phase 00 Plan 01: 模板复制与命名空间重命名 Summary

Full template copy to yh-flow/ with comprehensive FSH→YH namespace rename, 51-project solution building with zero errors.

## Performance

- **Duration:** 20 min
- **Started:** 2026-06-16T06:08:27Z
- **Completed:** 2026-06-16T06:28:48Z
- **Tasks:** 4
- **Files modified:** 1526+

## Accomplishments

- Copied fullstackhero template to yh-flow/ with proper exclusions
- Created 7-phase PowerShell rename script (reusable for future module creation)
- Executed comprehensive FSH→YH namespace replacement across 1500+ files
- Build verified: `dotnet build YH.Flow.slnx` — 51 projects, 0 errors, 0 warnings

## Task Commits

Each task was committed atomically:

1. **Task 1.1: Copy template to yh-flow/** - `003031bb7` (feat)
2. **Task 1.2: Create PowerShell rename script** - `c5190e2ea` (feat)
3. **Task 1.3: Execute rename script** - `4f4aac87e` (feat)
4. **Task 1.4: Fix residuals and verify build** - `2585c690e` (fix)

## Files Created/Modified

- `yh-flow/scripts/rename-fsh-to-yh.ps1` - 7-phase rename script (reusable)
- `yh-flow/src/YH.Flow.slnx` - Solution file (renamed from FSH.Starter.slnx)
- `yh-flow/src/Directory.Build.props` - Build properties (Authors→YHFlow, Company→YHFlow)
- `yh-flow/src/Host/YH.Flow.AppHost/AppHost.cs` - Aspire orchestration (yhflow-db, yhflow-uploads)
- `yh-flow/src/Host/YH.Flow.Api/YH.Flow.Api.csproj` - API project (renamed)
- `yh-flow/src/Host/YH.Flow.DbMigrator/YH.Flow.DbMigrator.csproj` - DbMigrator (renamed)
- `yh-flow/src/Host/YH.Flow.Migrations.PostgreSQL/YH.Flow.Migrations.PostgreSQL.csproj` - Migrations (renamed)
- `yh-flow/clients/admin/package.json` - Retained per D-13
- `yh-flow/clients/dashboard/package.json` - Retained per D-13

## Decisions Made

- Excluded .vs/, deploy/, docs/, templates/, .agents/ from copy (not in plan's keep list)
- Slimmed clients/admin and clients/dashboard to only package.json (per D-13)
- Added -File flag and .vs exclusion to all Get-ChildItem calls in rename script
- FSH.CLI.csproj renamed to YH.CLI.csproj (consistent with FSH→YH naming)
- Kept Fsh-prefixed code identifiers (FshCore, FshJobActivator, etc.) — these are internal class names not covered by D-07's scope

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Script failed on .vs/FSH.Starter.slnx directory**

- **Found during:** Task 1.3 (Execute rename script)
- **Issue:** Get-ChildItem matched `.vs/FSH.Starter.slnx/` directory as a .slnx file, causing Get-Content to fail
- **Fix:** Added `-File` flag and `.vs` exclusion to all Get-ChildItem calls; removed .vs directory from yh-flow
- **Files modified:** yh-flow/scripts/rename-fsh-to-yh.ps1
- **Verification:** Script re-ran successfully through all 7 phases
- **Committed in:** 2585c690e

**2. [Rule 1 - Bug] Residual YH.Starter. namespaces after rename**

- **Found during:** Task 1.4 (Build verification)
- **Issue:** 100+ files had `YH.Starter.` instead of `YH.Flow.` — the script's FSH.Starter.→YH.Flow. replacement didn't fire on these files in the first execution, leaving intermediate `YH.Starter.` state
- **Fix:** Applied sed replacement `YH.Starter.` → `YH.Flow.` across all .cs and .json files; added `YH.Starter.` → `YH.Flow.` rule to script Phase 2 for idempotency
- **Files modified:** 100+ migration files, Host project sources, appsettings
- **Verification:** `grep -r "YH.Starter." src/` returns zero results
- **Committed in:** 2585c690e

**3. [Rule 1 - Bug] FSH.CLI.csproj reference in slnx after rename**

- **Found during:** Task 1.4 (Build verification)
- **Issue:** Renamed FSH.CLI.csproj→YH.CLI.csproj but slnx still referenced old path
- **Fix:** Updated slnx to reference `Tools/CLI/YH.CLI.csproj`
- **Files modified:** yh-flow/src/YH.Flow.slnx
- **Verification:** `dotnet build` succeeds with 0 errors
- **Committed in:** 2585c690e

---

**Total deviations:** 3 auto-fixed (3 Rule 1 - Bug)

**Impact on plan:** All auto-fixes necessary for successful compilation. No scope creep.

## Issues Encountered

- rsync not available on Windows — used cp -r instead
- scripts/ directory gitignored by Plane's root .gitignore — used `git add -f` for the rename script
- First script execution stopped at Phase 4 due to .vs directory error — required script fix and re-run

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- yh-flow/ directory has complete template structure with YH namespaces
- Solution builds cleanly (51 projects, 0 errors, 0 warnings)
- Rename script preserved for future module creation
- All BuildingBlocks and Modules retained (including disabled ones per D-01)
- Ready for Phase 1: Foundation — module DI cleanup, Aspire configuration

## Self-Check: PASSED

All 8 key files verified present. All 4 task commits verified in git log.

---

Phase: 00-init

Completed: 2026-06-16
