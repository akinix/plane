---
phase: 03
slug: project
status: draft
nyquist_compliant: true
wave_0_complete: false
created: 2026-06-24
---

# Phase 03 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property               | Value                                                                                      |
| ---------------------- | ------------------------------------------------------------------------------------------ |
| **Framework**          | xUnit 3 + FluentAssertions + NSubstitute                                                   |
| **Config file**        | `src/Tests/Project.Tests/Project.Tests.csproj` (Wave 0 creates)                            |
| **Quick run command**  | `dotnet test src/Tests/Project.Tests --no-build --filter "{TaskName}" --verbosity minimal` |
| **Full suite command** | `dotnet test src/Tests/Project.Tests --verbosity minimal`                                  |
| **Build command**      | `dotnet build src/YH.Flow.slnx --nologo`                                                   |
| **Estimated runtime**  | ~15 seconds                                                                                |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test src/Tests/Project.Tests --no-build --filter "FullyQualifiedName~{TaskName}" --verbosity minimal`
- **After every plan wave:** Run `dotnet test src/Tests/Project.Tests --verbosity minimal`
- **After last wave (full regression):** Run `dotnet test src/Tests/Project.Tests --verbosity minimal && dotnet test src/Tests/Workspace.Tests --verbosity minimal && dotnet test src/Tests/Identity.Tests --verbosity minimal`
- **Before `/gsd-verify-work`:** Full suite must be green
- **Max feedback latency:** 30 seconds

---

## Per-Task Verification Map

| Task ID  | Plan  | Wave | Requirement | Threat Ref  | Secure Behavior          | Test Type | Automated Command                                                                                   | File Exists | Status     |
| -------- | ----- | ---- | ----------- | ----------- | ------------------------ | --------- | --------------------------------------------------------------------------------------------------- | ----------- | ---------- |
| 03-01-01 | 03-01 | 1    | —           | —           | N/A                      | build     | `dotnet build src/YH.Flow.slnx --nologo`                                                            | ❌ W0       | ⬜ pending |
| 03-01-02 | 03-01 | 1    | —           | —           | N/A                      | build     | `dotnet build src/YH.Flow.slnx --nologo`                                                            | ❌ W0       | ⬜ pending |
| 03-01-03 | 03-01 | 1    | —           | —           | N/A                      | compile   | `dotnet build src/Tests/Project.Tests --nologo`                                                     | ❌ W0       | ⬜ pending |
| 03-02-01 | 03-02 | 2    | —           | —           | N/A                      | unit      | `dotnet test src/Tests/Project.Tests --no-build --filter "FullyQualifiedName~ProjectConfiguration"` | ❌ W0       | ⬜ pending |
| 03-02-02 | 03-02 | 2    | —           | —           | N/A                      | build     | `dotnet build src/YH.Flow.slnx --nologo`                                                            | ❌ W0       | ⬜ pending |
| 03-02-03 | 03-02 | 2    | —           | —           | N/A                      | build     | `dotnet build src/YH.Flow.slnx --nologo`                                                            | ❌ W0       | ⬜ pending |
| 03-03-01 | 03-03 | 3    | REQ-3.1     | T-03-01 / — | Authorization enforced   | unit      | `dotnet test src/Tests/Project.Tests --no-build --filter "FullyQualifiedName~ProjectCrud"`          | ❌ W0       | ⬜ pending |
| 03-03-02 | 03-03 | 3    | REQ-3.1     | T-03-01 / — | Authorization enforced   | unit      | `dotnet test src/Tests/Project.Tests --no-build --filter "FullyQualifiedName~ProjectCrud"`          | ❌ W0       | ⬜ pending |
| 03-04-01 | 03-04 | 4    | REQ-3.2     | T-03-02 / — | Role validation enforced | unit      | `dotnet test src/Tests/Project.Tests --no-build --filter "FullyQualifiedName~ProjectMember"`        | ❌ W0       | ⬜ pending |
| 03-04-02 | 03-04 | 4    | REQ-3.2     | T-03-02 / — | Role validation enforced | unit      | `dotnet test src/Tests/Project.Tests --no-build --filter "FullyQualifiedName~ProjectMember"`        | ❌ W0       | ⬜ pending |

_Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky_

---

## Wave 0 Requirements

- [ ] `src/Tests/Project.Tests/Project.Tests.csproj` — test project with xUnit + FluentAssertions + NSubstitute
- [ ] `src/Tests/Project.Tests/Usings.cs` — global using directives
- [ ] `src/Tests/Project.Tests/GlobalUsings.cs` — global using directives
- [ ] Existing infrastructure covers all phase requirements (InMemory DbContext from persistence building block)

---

## Manual-Only Verifications

| Behavior              | Requirement | Why Manual                                                                 | Test Instructions                                                                                                                             |
| --------------------- | ----------- | -------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------- |
| Identifier uniqueness | REQ-3.1     | DB unique constraint; InMemory test cannot verify PG unique index behavior | Run `dotnet test src/Tests/Project.Tests --filter "FullyQualifiedName~ProjectIdentifierUniqueness"` if Testcontainers integration test exists |
| Unsplash cover image  | REQ-3.3     | External API dependency                                                    | Deferred to Phase 9 Integration; Phase 3 only stores CoverImageUrl string                                                                     |

_If none: "All phase behaviors have automated verification."_

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 30s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
