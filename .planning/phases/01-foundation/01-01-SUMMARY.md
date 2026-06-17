---
phase: 01-foundation
plan: 01
subsystem: auth
tags: [authentication, api-key, session-cookie, jwt-bearer, policy-scheme, smart-selector]

# Dependency graph
requires:
  - phase: 00-init
    provides: YH.Flow Identity module scaffolding, JWT authentication baseline, Identity.Tests project conventions
provides:
  - API Key authentication scheme using X-Api-Key header
  - Session Cookie authentication scheme using .YHFlow.Session cookie
  - SmartSelector PolicyScheme routing API Key, Session Cookie, and JWT Bearer requests
  - IdentityModule registration for all three authentication schemes
affects: [01-03, 01-04, phase-09-integration, phase-13-frontend]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "AuthenticationScheme extension pattern: one internal Configure*Auth extension per scheme"
    - "PolicyScheme ForwardDefaultSelector order: API Key header -> Session Cookie -> JWT Bearer"
    - "API key handler resolves IApiTokenService from request DI and never accesses DbContext directly"

key-files:
  created:
    - yh-flow/src/Modules/Identity/Modules.Identity/Authorization/ApiKey/ApiKeyAuthenticationDefaults.cs
    - yh-flow/src/Modules/Identity/Modules.Identity/Authorization/ApiKey/ApiKeyAuthenticationOptions.cs
    - yh-flow/src/Modules/Identity/Modules.Identity/Authorization/ApiKey/ApiKeyAuthenticationHandler.cs
    - yh-flow/src/Modules/Identity/Modules.Identity/Authorization/ApiKey/ApiKeyAuthenticationExtensions.cs
    - yh-flow/src/Modules/Identity/Modules.Identity/Authorization/SessionCookie/SessionCookieAuthenticationDefaults.cs
    - yh-flow/src/Modules/Identity/Modules.Identity/Authorization/SessionCookie/SessionCookieAuthenticationExtensions.cs
    - yh-flow/src/Tests/Identity.Tests/Authorization/ApiKeyAuthenticationTests.cs
    - yh-flow/src/Tests/Identity.Tests/Authorization/SessionCookieAuthenticationTests.cs
    - yh-flow/src/Tests/Identity.Tests/Authorization/MultiSchemeSelectorTests.cs
  modified:
    - yh-flow/src/Modules/Identity/Modules.Identity/Authorization/Jwt/JwtAuthenticationExtensions.cs
    - yh-flow/src/Modules/Identity/Modules.Identity/IdentityModule.cs

key-decisions:
  - "SmartSelector string is used as the PolicyScheme default to keep JWT Bearer registered under the standard Bearer scheme."
  - "API Key takes precedence over Session Cookie when both are present, matching the plan's header-first routing rule."
  - "Session Cookie returns 401 on login redirect to preserve API semantics instead of browser 302 redirects."

patterns-established:
  - "Multi-scheme auth is centralized in JwtAuthenticationExtensions while individual schemes keep separate registration extensions."
  - "Session cookie auth is configured for HttpOnly, Secure Always, SameSite=Lax, 7-day sliding expiration."

requirements-completed: [REQ-1.3, NFR-2]

# Metrics
duration: 35min
completed: 2026-06-17
---

# Phase 1 Plan 01: Multi-Scheme Authentication Summary

**JWT Bearer, API Key, and Session Cookie authentication registered behind a SmartSelector PolicyScheme router**

## Performance

- **Duration:** 35 min
- **Started:** 2026-06-17T06:05:00Z
- **Completed:** 2026-06-17T06:40:00Z
- **Tasks:** 4
- **Files modified:** 11

## Accomplishments

- Added API Key authentication infrastructure with `X-Api-Key` header parsing and `IApiTokenService.ValidateAndGetOwnerAsync` validation.
- Added Session Cookie authentication using `.YHFlow.Session`, secure cookie settings, sliding 7-day expiration, and API-friendly 401 redirects.
- Reworked JWT auth registration to use `SmartSelector` as the default authenticate/challenge scheme.
- Implemented `ForwardDefaultSelector` routing: `X-Api-Key` -> `ApiKey`, `.YHFlow.Session` cookie -> `SessionCookie`, fallback -> `Bearer`.
- Registered API Key and Session Cookie schemes from `IdentityModule.ConfigureServices`.
- Added focused tests for API Key, Session Cookie, and SmartSelector behavior.

## Task Commits

1. **Task 1: API Key 认证方案实现**
   - `563c41b` (feat) — API Key authentication scheme, validation DTO/service interface usage, and tests.
2. **Task 2: Session Cookie 认证方案实现**
   - `5d95ad4` (feat) — Session Cookie scheme and tests.
3. **Task 3: PolicyScheme 路由选择器 + IdentityModule 注册**
   - `5d95ad4` (feat) — SmartSelector PolicyScheme, scheme priority tests, and IdentityModule registration.

## Files Created/Modified

- `Modules.Identity/Authorization/ApiKey/ApiKeyAuthenticationDefaults.cs` — API Key scheme and header constants.
- `Modules.Identity/Authorization/ApiKey/ApiKeyAuthenticationOptions.cs` — configurable API Key header option.
- `Modules.Identity/Authorization/ApiKey/ApiKeyAuthenticationHandler.cs` — custom handler validating raw API keys through `IApiTokenService`.
- `Modules.Identity/Authorization/ApiKey/ApiKeyAuthenticationExtensions.cs` — API Key scheme DI registration.
- `Modules.Identity/Authorization/SessionCookie/SessionCookieAuthenticationDefaults.cs` — Session Cookie scheme and cookie constants.
- `Modules.Identity/Authorization/SessionCookie/SessionCookieAuthenticationExtensions.cs` — secure cookie authentication registration.
- `Modules.Identity/Authorization/Jwt/JwtAuthenticationExtensions.cs` — SmartSelector default scheme and routing logic.
- `Modules.Identity/IdentityModule.cs` — registration for API Key and Session Cookie schemes.
- `Tests/Identity.Tests/Authorization/ApiKeyAuthenticationTests.cs` — API Key handler and registration coverage.
- `Tests/Identity.Tests/Authorization/SessionCookieAuthenticationTests.cs` — cookie scheme configuration coverage.
- `Tests/Identity.Tests/Authorization/MultiSchemeSelectorTests.cs` — SmartSelector routing coverage.

## Decisions Made

- **Header-first routing:** API Key requests take precedence over Session Cookie requests because an explicit API credential should be honored before ambient browser state.
- **JWT fallback:** Requests without API Key or Session Cookie continue through the existing `Bearer` JWT scheme to preserve current API behavior.
- **401 instead of 302:** Cookie authentication redirects are converted to HTTP 401 so API clients receive machine-readable auth failures.

## Deviations from Plan

### Auto-fixed Issues

**1. Recovery execution instead of fresh worktree dispatch**

- **Found during:** Wave 1 resume gate
- **Issue:** Partial `01-01` files and an implementation commit already existed, but `01-01-SUMMARY.md` was missing.
- **Fix:** Continued on the main working tree, preserved existing work, completed remaining Session Cookie and SmartSelector tasks, then created this SUMMARY.
- **Files modified:** `01-01-SUMMARY.md` plus remaining plan files.
- **Verification:** Directed test and build commands passed.
- **Committed in:** `5d95ad4` for code; SUMMARY commit follows this file.

---

**Total deviations:** 1 recovery-mode adjustment
**Impact on plan:** No scope change. Recovery avoided duplicating or overwriting already completed API Key work.

## Issues Encountered

- Initial parallel executor attempt stopped after failing to locate a root-level `CLAUDE.md`; execution was recovered manually with the correct `yh-flow/CLAUDE.md` project context.
- Root-level unrelated changes (`.husky/pre-commit`, `pnpm-lock.yaml`, `.planning/config.json`) were present during recovery and intentionally left out of the `01-01` code commit.

## Verification

- `cd yh-flow && dotnet test src/Tests/Identity.Tests/Identity.Tests.csproj --filter "ApiKeyAuthentication|SessionCookieAuthentication|MultiSchemeSelector" --no-restore` — passed, 20 tests.
- `cd yh-flow && dotnet build src/Modules/Identity/Modules.Identity/Modules.Identity.csproj --no-restore` — passed, 0 errors, 0 warnings.

## Threat Mitigations Applied

| Threat ID | Component                     | Mitigation                                                                          | Status       |
| --------- | ----------------------------- | ----------------------------------------------------------------------------------- | ------------ |
| T-01-01   | API Key AuthenticationHandler | Handler validates via `IApiTokenService` and builds claims only from service result | ✅ Mitigated |
| T-01-02   | Session Cookie                | ASP.NET Core cookie authentication with HttpOnly/Secure/SameSite settings           | ✅ Mitigated |
| T-01-03   | PolicyScheme Selector         | Selector only inspects credential presence for routing                              | ✅ Accepted  |
| T-01-04   | Claims Construction           | API Key claims derive from validation result, not client-provided data              | ✅ Mitigated |

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- `01-03` can build Plane-compatible `/auth/*` endpoints on top of the registered SmartSelector and session cookie scheme.
- `01-04` can implement API Token CRUD behind the `IApiTokenService` contract and use `X-Api-Key` authentication.
- `01-05` can verify all authentication schemes after database schema push.

---

_Phase: 01-foundation_
_Completed: 2026-06-17_

## Self-Check: PASSED

Key files exist, directed tests pass, and Identity module builds with 0 errors and 0 warnings.
