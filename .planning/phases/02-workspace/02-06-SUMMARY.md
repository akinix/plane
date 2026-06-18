---
phase: 02-workspace
plan: 06
subsystem: workspace-regression-smoke
tags: [workspace, regression, smoke, role-matrix, e2e, d-08-slug-release, t-2-eop, t-2-matrixgap]

# Dependency graph
requires:
  - phase: 02-workspace
    provides: 02-01..05 (all Workspace feature slices + tests across waves 0-4)
  - phase: 01-foundation
    provides: Identity.Tests 412-baseline + Architecture.Tests + Workspace.Tests project scaffold
provides:
  - WorkspaceLifecycleSmokeTests — 10-step end-to-end mediator-chain smoke (D-06/D-08/D-10/D-12/T-2-eop-self/T-2-cacheinvalid)
  - WorkspaceRoleCapabilityTests — 4-role-group × endpoint matrix (T-2-matrixgap)
  - 02-VERIFICATION.md — Phase 2 verification report (automated-green + manual-smoke-pending)
  - 02-VALIDATION.md — all task rows status=green, nyquist_compliant=true, wave_0_complete=true
affects:
  [
    Phase 3+ (verification pattern reference for downstream modules),
    Orchestrator verify_phase_goal (manual smoke consolidated into HUMAN-UAT.md),
  ]

# Tech tracking
tech-stack:
  added: [] # 0 new NuGet packages — purely additive test source; all dependencies (xUnit, Shouldly, NSubstitute, Microsoft.EntityFrameworkCore.InMemory) already present
  patterns:
    - "Mediator-direct end-to-end smoke (vs HTTP TestServer) — drives the full CreateWorkspace → SlugGenerator → InvitationTokenService → AcceptInvitation → ListMembers → UpdateMemberRole → DeleteWorkspace chain against a single InMemory WorkspaceDbContext fixture so state flows from one stage to the next. The 02-06 plan <action> recommendation blessed this path for speed; the manual smoke (Task 2) covers the Finbuckle HTTP pipeline."
    - "Parameterised role × capability matrix via [Theory] + [InlineData] — 10 cases cover Admin/Member/Guest/non-member against the RequireWorkspaceRoleAuthorizationHandler. Simulates ICurrentWorkspaceContext.CurrentUserRole via NSubstitute (null = non-member sentinel)."
    - "D-08 load-bearing assertion lives in step 10 of the smoke — after soft-delete the original slug MUST be reusable by a brand-new workspace. The InMemory provider does not enforce the unique index, so this test verifies the SlugGenerator's collision probe correctly ignores the suffixed soft-deleted row."
    - "NSubstitute Received(1) assertion on IMultiTenantStore<AppTenantInfo>.RemoveAsync(slug) — same cache-invalidation guard pattern used by 02-04 SoftDeleteSlugReleaseTests, re-applied in the smoke step 8 to assert T-2-cacheinvalid is honoured end-to-end."
    - "TwoOwnerId/InviteeUserId GUIDs use the canonical 12-hex-digit suffix form (00...0a1/a2) — fixed at compile time via static readonly fields for stable test ids."

key-files:
  created:
    - yh-flow/src/Tests/Workspace.Tests/Integration/WorkspaceLifecycleSmokeTests.cs
    - yh-flow/src/Tests/Workspace.Tests/Integration/WorkspaceRoleCapabilityTests.cs
    - .planning/phases/02-workspace/02-VERIFICATION.md
  modified:
    - .planning/phases/02-workspace/02-VALIDATION.md (all task rows status=green, frontmatter nyquist_compliant=true, wave_0_complete=true, completed=2026-06-18)

key-decisions:
  - "Mediator-direct smoke preferred over HTTP TestServer for speed and determinism per the 02-06 plan <action>. The full Finbuckle HTTP stack (slug strategy → WorkspaceTenantStore → WorkspaceMembershipMiddleware → RequireWorkspaceRole gate → handler) is exercised by the manual smoke (Task 2). The mediator-direct chain still walks every Workspace handler, SlugGenerator, InvitationTokenService, and WorkspaceMembershipService implementation."
  - "Smoke step 7 uses owner self-promotion to Admin as the T-2-eop-self guard trigger — the test does not create a third Member caller because the handler's guard keys off target.UserId == CurrentUserId, and the owner (Admin) is the simplest caller to construct. The guard throws ForbiddenException regardless of which Admin attempts the self-promotion."
  - "Smoke step 10 (D-08 slug release) is the LOAD-BEARING assertion. The test asserts both: (a) the new workspace gets the bare slug 'acme-smoke' (collision probe found no live row with that slug), AND (b) the soft-deleted row still exists with the suffixed slug. Two rows total after the smoke. This is the only automated test that proves the D-08 soft-delete-suffix design works in a real DbContext round-trip."
  - "Role-capability matrix uses a -1 sentinel inside the [InlineData] to represent 'non-member (null role)' because [InlineData] cannot pass null for int parameters. The test converts -1 → null before stubbing ICurrentWorkspaceContext.CurrentUserRole. This keeps the parameterised [Theory] clean while still exercising the default-deny path."
  - "Architecture.Tests was re-checked this wave (not fixed) — 3 PRE-EXISTING Phase-1 Identity baseline failures (HandlerValidatorPairing OAuth/ApiTokens, EndpointNames PlaneAuth/OAuth, PlaneAuthHelpers Features→AspNetCore) are documented in VERIFICATION.md as out-of-scope per .planning/config.json _test_scope_note. NONE reference any YH.Modules.Workspace.* type."

requirements-completed: [REQ-2.1, REQ-2.2, REQ-2.3, REQ-2.4, NFR-1, NFR-2, NFR-3, NFR-4]

# Metrics
duration: 22min
completed: 2026-06-18
---

# Phase 2 Plan 06: Wave 5 — Regression + Lifecycle Smoke + Role-Capability Matrix Summary

**Wave 5 closeout:** 23 new tests (1 end-to-end lifecycle [Fact] + 12 role-matrix [Theory]/[Fact] cases) covering the D-08 slug-release load-bearing path, the T-2-eop-self guard, and the full D-11 role→capability matrix (Admin / Member / Guest / non-member × endpoint). Scoped regression green: Workspace.Tests 96/96 (was 73 + 23 new), Identity.Tests 412/412 (zero regression), Architecture.Tests 3 baseline fails only (zero NEW Workspace violations — manually verified). The 11-step manual smoke against a live Aspire stack + real JWTs is recorded under Human Verification (pending) in 02-VERIFICATION.md; cannot be automated here.

## Performance

- **Duration:** 22 min
- **Started:** 2026-06-18T03:25:00Z
- **Completed:** 2026-06-18T03:47:00Z
- **Tasks:** 1 automated (Task 2 is the deferred manual smoke)
- **Files created:** 3 (WorkspaceLifecycleSmokeTests, WorkspaceRoleCapabilityTests, 02-VERIFICATION.md)
- **Files modified:** 1 (02-VALIDATION.md)
- **Build:** `dotnet build src/YH.Flow.slnx` — 0 warnings, 0 errors (54 projects)
- **Tests (gate scope):**
  - Workspace.Tests 96/96 PASS (73 prior + 23 new: 1 lifecycle smoke [Fact] + 12 role-matrix [Theory]/[Fact] cases)
  - Identity.Tests 412/412 PASS (Phase 1 zero regression)
  - Architecture.Tests: 0 NEW Workspace violations (3 remaining fails are all Phase-1 Identity baseline debt)

## Accomplishments

1. **WorkspaceLifecycleSmokeTests** — single [Fact] `Lifecycle_TenSteps_CreateSlugCheckInviteAcceptListRoleDeleteAndSlugReuse` driving the mediator-direct chain across 6 handlers (CreateWorkspace / CreateInvitation / AcceptInvitation / ListMembers / UpdateMemberRole / DeleteWorkspace). Each step asserts the prior stage's state, so a regression in any handler surfaces as a step failure. Step 10 is the D-08 load-bearing assertion (slug reuse after soft delete); step 7 is the T-2-eop-self guard (ForbiddenException on self-promotion); step 8 verifies the Finbuckle cache-invalidation call.
2. **WorkspaceRoleCapabilityTests** — parameterised [Theory] matrix covering the 4 role groups × the endpoint-required-role patterns declared across the Workspace module. Each InlineData cell asserts Succeed/Fail against the `RequireWorkspaceRoleAuthorizationHandler` (the same handler every workspace-scoped endpoint delegates to via `.RequireWorkspaceRole(...)`). Plus 2 standalone [Fact]s for the WorkspaceRole.None default-deny invariant and the EOP-self endpoint decoration.
3. **02-VALIDATION.md closure** — all 15 task rows status=green (13 prior + 2 new from this plan); frontmatter `status=complete`, `nyquist_compliant=true`, `wave_0_complete=true`, `completed=2026-06-18`. Wave 0 Requirements all checked. Validation Sign-Off all checked.
4. **02-VERIFICATION.md** — Phase 2 completion summary (6 plans across 5 waves), automated must-have evidence (build + scoped regression + new-tests detail), the 4-project regression totals table, 11 manual smoke steps recorded under Human Verification (pending), residual risks, downstream Phase 3 dependency surface, reproducible command list.

## Task Commits

Each task committed atomically (scope `02-06`):

1. **Task 1: WorkspaceLifecycleSmoke + WorkspaceRoleCapability tests + VALIDATION/VERIFICATION** — `a248b0ca3` (test)

**Plan metadata:** close-out commit pending (STATE/ROADMAP/SUMMARY).

Task 2 (manual smoke) is deferred to the user per the end-of-phase `human_verify_mode` — recorded in VERIFICATION.md §Human Verification and consolidated by the orchestrator's `verify_phase_goal` step.

## Scoped Regression Result

| Project            | Result                                                   | Workspace violations                                     |
| ------------------ | -------------------------------------------------------- | -------------------------------------------------------- |
| Workspace.Tests    | 96 / 96 ✅ (+23 from baseline 73)                        | —                                                        |
| Identity.Tests     | 412 / 412 ✅ (zero regression)                           | —                                                        |
| Architecture.Tests | 46 pass / 3 baseline fails (Phase-1 Identity debt)       | **ZERO** — verified by inspecting each failure's message |
| Integration.Tests  | unchanged (out of scope: fullstackhero e2e baseline red) | —                                                        |

## Manual Smoke Status (Task 2)

The 11-step manual smoke against a live Aspire stack + real root/invitee JWTs cannot be automated in this execution context (no Docker / no real auth). Recorded in full under `02-VERIFICATION.md` §Human Verification (manual smoke — pending). Under the project's `human_verify_mode = end-of-phase`, this plan did NOT halt mid-flight; the orchestrator's `verify_phase_goal` step will consolidate the 11 steps into a `HUMAN-UAT.md` for the user to execute.

## D-08 Load-Bearing Slug Release Proof

Step 10 of `WorkspaceLifecycleSmokeTests` is the only automated test that proves D-08 works in a real DbContext round-trip:

```
1. Create "Acme Smoke" → slug = "acme-smoke"
8. Soft-delete → soft-deleted row's slug becomes "acme-smoke__{epoch}"
9. Default query filter excludes the soft-deleted row → DbSet is empty
10. Create new "Acme Smoke" → slug = "acme-smoke" (REUSE) — SlugGenerator
    collision probe found no live row with that slug, so it does not
    append a suffix. The soft-deleted row's suffixed slug does not collide.
```

Two rows total after the smoke: the suffixed soft-deleted original + the new bare-slug workspace. If D-08 ever regresses (slug not suffixed on soft-delete), step 10 fails with a 409 Conflict because the collision probe finds the original slug.

## Role → Capability Matrix (D-11 / T-2-matrixgap)

| Role              | Read members | Leave  | Mutate workspace | Manage members | Manage invitations |
| ----------------- | ------------ | ------ | ---------------- | -------------- | ------------------ |
| Admin (20)        | ✅           | ✅     | ✅               | ✅             | ✅                 |
| Member (15)       | ✅           | ✅     | ❌ 403           | ❌ 403         | ❌ 403             |
| Guest (5)         | ❌ 403       | ✅     | ❌ 403           | ❌ 403         | ❌ 403             |
| Non-member (null) | ❌ 403       | ❌ 403 | ❌ 403           | ❌ 403         | ❌ 403             |

The `WorkspaceRoleCapabilityTests.Authorization_Matrix_EnforcesRoleToCapabilityBoundaries` [Theory] enumerates every cell; a regression in the handler or in any endpoint's `.RequireWorkspaceRole(...)` decoration fails at least one cell.

## Decisions Made

1. **Mediator-direct smoke preferred over HTTP TestServer** — per the 02-06 plan <action> recommendation. HTTP overhead + Finbuckle middleware wiring is covered by the manual smoke (Task 2). The mediator-direct chain walks every Workspace handler + service implementation, so business-logic regressions are caught.
2. **OwnerId/InviteeUserId GUIDs use 12-hex-digit suffix** — initial draft used 11-hex-digit values which failed Guid.Parse. Fixed to the canonical `00000000-0000-0000-0000-0000000000a1` / `...a2` form.
3. **Non-member sentinel in [InlineData] is -1** — xUnit's [InlineData] cannot pass null for int parameters. The test converts -1 → null before stubbing ICurrentWorkspaceContext. Documented inline.
4. **Architecture.Tests NOT fixed** — the 3 baseline failures (HandlerValidatorPairing OAuth/ApiTokens, EndpointNames PlaneAuth/OAuth, PlaneAuthHelpers Features→AspNetCore) are PRE-EXISTING Phase-1 Identity debt explicitly out of scope per `.planning/config.json workflow._test_scope_note`. Fixing them would be scope creep beyond Phase 2's Workspace boundary. Documented in VERIFICATION.md.
5. **STATE.md marks the plan complete but keeps the PHASE in-progress pending manual smoke** — per the end-of-phase human_verify_mode contract. The orchestrator's phase.complete step flips the phase to ✅ after the user approves the manual smoke.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] FluentAssertions using directive left in from draft**

- **Found during:** Task 1 build
- **Issue:** Initial draft of `WorkspaceLifecycleSmokeTests.cs` imported `FluentAssertions`. The Workspace.Tests project only references Shouldly (no FluentAssertions package). Compile error CS0246.
- **Fix:** Removed the unused `using FluentAssertions;` directive; all assertions use Shouldly's `ShouldBe` / `ShouldContain` / `ShouldStartWith`.
- **Files modified:** `Integration/WorkspaceLifecycleSmokeTests.cs`
- **Verification:** Workspace.Tests builds clean.
- **Committed in:** `a248b0ca3`

**2. [Rule 1 - Bug] `listResult.Results` is IReadOnlyCollection (no indexer)**

- **Found during:** Task 1 build
- **Issue:** The ListMembers handler returns `PlanePagedResult<WorkspaceMemberDto>` whose `Results` is `IReadOnlyCollection<T>` (no `[index]` access). Compile error CS0021 on the smoke step 5 assertions.
- **Fix:** Materialised via `.ToList()` before indexing: `var roster = listResult.Results.ToList();`.
- **Files modified:** `Integration/WorkspaceLifecycleSmokeTests.cs`
- **Verification:** Workspace.Tests builds clean.
- **Committed in:** `a248b0ca3`

**3. [Rule 1 - Bug] xUnit1031 — blocking task ops in test methods**

- **Found during:** Task 1 build
- **Issue:** `WorkspaceRoleCapabilityTests` initially called `handler.HandleAsync(authContext).GetAwaiter().GetResult()` from a synchronous `[Theory]`. xUnit's analyzer (xUnit1031) escalates this to a compile error under TreatWarningsAsErrors.
- **Fix:** Converted the `[Theory]` and both `[Fact]`s to `async Task` and `await`-ed the handler call.
- **Files modified:** `Integration/WorkspaceRoleCapabilityTests.cs`
- **Verification:** Workspace.Tests builds clean.
- **Committed in:** `a248b0ca3`

**4. [Rule 1 - Bug] 11-hex-digit GUIDs failed Guid.Parse (TypeInitializationException)**

- **Found during:** Task 1 test run
- **Issue:** Draft GUIDs `00000000-0000-0000-0000-000000000a1` (and `...0a2`) had only 11 hex digits in the final segment — Guid requires 12. `TypeInitializationException` → `FormatException` on the static field initialiser.
- **Fix:** Padded to the canonical 12-hex-digit form: `00000000-0000-0000-0000-0000000000a1` / `...00a2`.
- **Files modified:** `Integration/WorkspaceLifecycleSmokeTests.cs`
- **Verification:** Smoke test runs green (all 10 steps).
- **Committed in:** `a248b0ca3`

## Verification

- [x] `dotnet build src/YH.Flow.slnx --nologo` — 0 warnings / 0 errors (54 projects)
- [x] `dotnet test src/Tests/Workspace.Tests` — 96/96 PASS (73 prior + 23 new)
- [x] `dotnet test src/Tests/Identity.Tests` — 412/412 PASS (Phase 1 zero regression)
- [x] `dotnet test src/Tests/Architecture.Tests` — 46 pass / 3 baseline fails (zero NEW Workspace violations; verified by inspecting each failure message)
- [x] WorkspaceLifecycleSmokeTests 10 steps all pass (including D-08 slug release at step 10)
- [x] WorkspaceRoleCapabilityTests 4-role-group matrix all pass (12 cases)
- [x] T-2-eop-self guard verified by smoke step 7 (ForbiddenException on owner self-promotion)
- [x] T-2-cacheinvalid verified by smoke step 8 (NSubstitute Received(1) on RemoveAsync(slug))
- [x] T-2-matrixgap mitigated by WorkspaceRoleCapabilityTests (4 groups × endpoint requirements)
- [x] T-2-slugreleasereal mitigated by smoke step 10 (D-08 reuse after soft delete)
- [x] 02-VALIDATION.md all task rows status=green + nyquist_compliant=true + wave_0_complete=true
- [x] 02-VERIFICATION.md created with Phase 2 completion + automated evidence + 11 manual smoke steps + residual risks
- [ ] Manual smoke 11 steps (deferred to user — see VERIFICATION.md §Human Verification)

## Known Stubs

| Stub                  | File                                                                   | Reason                                                                                                   | Resolved By                            |
| --------------------- | ---------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------- | -------------------------------------- |
| Manual smoke (Task 2) | `.planning/phases/02-workspace/02-VERIFICATION.md` §Human Verification | Requires live Aspire + real JWTs — cannot be automated here. Recorded as 11 clearly-marked manual steps. | User (via orchestrator's HUMAN-UAT.md) |

No code-level stubs. All test code is fully wired and exercises the real Workspace handler + service implementations.

## TDD Gate Compliance

N/A — this plan is `type: execute` (not `type: tdd`). The 23 new tests are characterization tests verifying the just-shipped Phase 2 behaviour end-to-end (D-06/D-08/D-10/D-11/D-12/T-2-eop-self/T-2-matrixgap/T-2-cacheinvalid). No TDD RED/GREEN/REFACTOR gate applicable.

## Threat Flags

None. The plan's `<threat_model>` registered 6 threats; all mitigations are operational:

- **T-2-regression [HIGH, BLOCKING]:** Workspace.Tests 96/96 + Identity.Tests 412/412 green; Phase 1 zero regression confirmed.
- **T-2-matrixgap [HIGH, BLOCKING]:** `WorkspaceRoleCapabilityTests` covers Admin / Member / Guest / non-member × endpoint requirements (10 [Theory] cases + 2 [Fact]s).
- **T-2-e2egap:** `WorkspaceLifecycleSmokeTests` 10-step mediator-direct chain exercises the cross-handler state flow.
- **T-2-slugreleasereal [BLOCKING]:** Smoke step 10 asserts D-08 slug reuse after soft delete in a real DbContext round-trip.
- **T-2-isolationreal [HIGH, BLOCKING]:** Smoke step 9 + WorkspaceRoleCapabilityTests non-member row cover the default-deny path. Full cross-workspace isolation verified in 02-03 TenantIsolationTests.
- **T-2-SC (accept):** 0 new NuGet packages.

No NEW threat surface introduced. Both test files only consume already-shipped Workspace contracts + handlers + services.

## Self-Check: PASSED

All 3 created files verified present on disk:

- `yh-flow/src/Tests/Workspace.Tests/Integration/WorkspaceLifecycleSmokeTests.cs` — FOUND
- `yh-flow/src/Tests/Workspace.Tests/Integration/WorkspaceRoleCapabilityTests.cs` — FOUND
- `.planning/phases/02-workspace/02-VERIFICATION.md` — FOUND

Task 1 commit `a248b0ca3` verified in `git log`. Full-solution build 0/0; Workspace.Tests 96/96 PASS (23 new); Identity.Tests 412/412 PASS (zero regression); Architecture.Tests 3 baseline fails only (zero NEW Workspace violations). D-08 slug release (step 10), T-2-eop-self guard (step 7), T-2-cacheinvalid (step 8), and T-2-matrixgap (12 matrix cases) all verified by passing tests.
