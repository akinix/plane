---
phase: 5
slug: cycle
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-06-24
---

# Phase 5 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property               | Value                                                                |
| ---------------------- | -------------------------------------------------------------------- |
| **Framework**          | xUnit + Shouldly + AutoFixture + NSubstitute                         |
| **Config file**        | `src/Tests/WorkItems.Tests/WorkItems.Tests.csproj` — existing        |
| **Quick run command**  | `dotnet test src/Tests/WorkItems.Tests --nologo --verbosity minimal` |
| **Full suite command** | `dotnet test src/Tests/WorkItems.Tests --nologo --verbosity normal`  |
| **Estimated runtime**  | ~15 seconds                                                          |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test src/Tests/WorkItems.Tests --nologo --verbosity minimal`
- **After every plan wave:** Run full Workspace + Identity + WorkItems suite
- **Before `/gsd-verify-work`:** Full suite must be green
- **Max feedback latency:** 30 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Threat Ref | Secure Behavior  | Test Type | Automated Command                 | File Exists | Status     |
| ------- | ---- | ---- | ----------- | ---------- | ---------------- | --------- | --------------------------------- | ----------- | ---------- |
| 05-01-x | 01   | 1    | REQ-5.1     | —          | N/A (CRUD read)  | unit      | `dotnet test --filter Cycle`      | ❌ W0       | ⬜ pending |
| 05-02-x | 02   | 2    | REQ-5.1     | —          | N/A (CRUD write) | unit      | `dotnet test --filter Cycle`      | ❌ W0       | ⬜ pending |
| 05-03-x | 03   | 3    | REQ-5.2     | —          | N/A              | unit      | `dotnet test --filter CycleIssue` | ❌ W0       | ⬜ pending |
| 05-04-x | 04   | 4    | REQ-5.2     | —          | N/A              | unit+int  | `dotnet test --filter Burndown`   | ❌ W0       | ⬜ pending |

_Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky_

---

## Wave 0 Requirements

Existing infrastructure covers all phase requirements. Cycle tests will be added alongside code in the WorkItems.Tests project.

---

## Manual-Only Verifications

| Behavior                            | Requirement | Why Manual                           | Test Instructions                                        |
| ----------------------------------- | ----------- | ------------------------------------ | -------------------------------------------------------- |
| Cycle date overlap detection        | REQ-5.1     | 日期范围重叠需验证多种边界条件       | POST /cycles/date-check with overlapping dates           |
| Cycle archive/unarchive via real DB | REQ-5.1     | 需要真实 PostgreSQL 数据库           | Run testcontainers or manual curl against Aspire stack   |
| Issue transfer progress snapshot    | REQ-5.2     | 快照 JSON 格式和冻结时机的端到端验证 | Transfer completed cycle issues, verify snapshot content |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 30s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
