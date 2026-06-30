---
phase: 16
slug: issue
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-06-29
---

# Phase 16 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property               | Value                                        |
| ---------------------- | -------------------------------------------- |
| **Framework**          | None (typecheck only)                        |
| **Config file**        | none — `yh-flow/clients/web/tsconfig.json`   |
| **Quick run command**  | `cd yh-flow/clients/web && npx tsc --noEmit` |
| **Full suite command** | `cd yh-flow/clients/web && npx tsc --noEmit` |
| **Estimated runtime**  | ~15 seconds                                  |

---

## Sampling Rate

- **After every task commit:** Run `cd yh-flow/clients/web && npx tsc --noEmit`
- **After every plan wave:** Run `cd yh-flow/clients/web && npx tsc --noEmit`
- **Before `/gsd-verify-work`:** Full suite must be green
- **Max feedback latency:** ~15 seconds

---

## Per-Task Verification Map

| Task ID  | Plan | Wave | Requirement                        | Threat Ref | Secure Behavior                                  | Test Type   | Automated Command                            | File Exists | Status     |
| -------- | ---- | ---- | ---------------------------------- | ---------- | ------------------------------------------------ | ----------- | -------------------------------------------- | ----------- | ---------- |
| 16-01-01 | 01   | 1    | ISSU-01~08, KANB-01~03, KANB-05    | —          | N/A (mock layer)                                 | compilation | `cd yh-flow/clients/web && npx tsc --noEmit` | ❌ W0       | ⬜ pending |
| 16-01-02 | 01   | 1    | ISSU-06                            | —          | N/A (mock layer)                                 | compilation | `cd yh-flow/clients/web && npx tsc --noEmit` | ❌ W0       | ⬜ pending |
| 16-01-03 | 01   | 1    | ISSU-01, ISSU-02, KANB-01          | —          | N/A (mock layer)                                 | compilation | `cd yh-flow/clients/web && npx tsc --noEmit` | ❌ W0       | ⬜ pending |
| 16-02-01 | 02   | 2    | ISSU-02                            | T-16-02-01 | Filter/sort input accepted                       | compilation | `cd yh-flow/clients/web && npx tsc --noEmit` | ❌ W0       | ⬜ pending |
| 16-02-02 | 02   | 2    | ISSU-02, ISSU-07                   | T-16-02-02 | Selection state managed in MobX only             | compilation | `cd yh-flow/clients/web && npx tsc --noEmit` | ❌ W0       | ⬜ pending |
| 16-03-01 | 03   | 2    | ISSU-03, ISSU-04, ISSU-08          | T-16-03-01 | Editor output sanitized by @plane/editor         | compilation | `cd yh-flow/clients/web && npx tsc --noEmit` | ❌ W0       | ⬜ pending |
| 16-03-02 | 03   | 2    | ISSU-04                            | T-16-03-02 | Property input transient; no persistence in mock | compilation | `cd yh-flow/clients/web && npx tsc --noEmit` | ❌ W0       | ⬜ pending |
| 16-03-03 | 03   | 2    | ISSU-03, ISSU-05, ISSU-06          | T-16-03-03 | Comment HTML sanitized by @plane/editor          | compilation | `cd yh-flow/clients/web && npx tsc --noEmit` | ❌ W0       | ⬜ pending |
| 16-03-04 | 03   | 2    | ISSU-03                            | —          | Activity data read-only; no mutation             | compilation | `cd yh-flow/clients/web && npx tsc --noEmit` | ❌ W0       | ⬜ pending |
| 16-04-01 | 04   | 2    | KANB-01, KANB-02, KANB-03, KANB-05 | T-16-04-01 | Drag state update = TanStack mutation            | compilation | `cd yh-flow/clients/web && npx tsc --noEmit` | ❌ W0       | ⬜ pending |
| 16-04-02 | 04   | 2    | ISSU-01                            | —          | Issue create form input                          | compilation | `cd yh-flow/clients/web && npx tsc --noEmit` | ❌ W0       | ⬜ pending |
| 16-05-01 | 05   | 2    | UI-04                              | —          | Command palette search — no user data exposed    | compilation | `cd yh-flow/clients/web && npx tsc --noEmit` | ❌ W0       | ⬜ pending |
| 16-05-02 | 05   | 2    | UI-04                              | —          | Route wiring — navigation only                   | compilation | `cd yh-flow/clients/web && npx tsc --noEmit` | ❌ W0       | ⬜ pending |

_Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky_

---

## Wave 0 Requirements

- [ ] `yh-flow/clients/web/package.json` — `@hello-pangea/dnd` and `cmdk` dependencies added
- [ ] `yh-flow/clients/web/src/lib/mock-data.ts` — MOCK_ISSUES, MOCK_STATES, MOCK_LABELS, MOCK_COMMENTS, MOCK_ISSUE_ACTIVITIES exported

_Existing infrastructure (tsconfig, @plane/types, @plane/ui, @plane/editor) covers all component type checking requirements._

---

## Manual-Only Verifications

| Behavior                                               | Requirement | Why Manual                                    | Test Instructions                                                         |
| ------------------------------------------------------ | ----------- | --------------------------------------------- | ------------------------------------------------------------------------- |
| Issue list renders with correct filter/sort/pagination | ISSU-02     | No test framework; visual verification needed | Open project, click Issues tab, verify list displays with correct columns |
| Issue detail page shows dual-column layout             | ISSU-03     | Visual layout verification                    | Click an issue row, verify left editor + right property panel             |
| Kanban drag-and-drop moves card between columns        | KANB-02     | Interactive drag behavior requires human test | Switch to Kanban view, drag a card to a different column                  |
| Cmd+K command palette opens and searches               | UI-04       | Keyboard interaction verification             | Press Cmd+K, type issue name, verify search results and navigation        |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 15s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
