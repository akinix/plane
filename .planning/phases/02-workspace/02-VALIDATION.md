---
phase: 2
slug: workspace
status: complete
nyquist_compliant: true
wave_0_complete: true
created: 2026-06-17
completed: 2026-06-18
---

# Phase 2 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.
> Source: `02-RESEARCH.md` §Validation Architecture (Nyquist Dimension 8).

---

## Test Infrastructure

| Property               | Value                                                                               |
| ---------------------- | ----------------------------------------------------------------------------------- |
| **Framework**          | xUnit + FluentAssertions（贴 Phase 1 `Identity.Tests`，412/412 已过）               |
| **Config file**        | `Directory.Packages.props`（锁定版本）；各模块 `*.Tests` 项目                       |
| **Quick run command**  | `dotnet test src/Tests/Workspace.Tests --filter "FullyQualifiedName~Unit" --nologo` |
| **Full suite command** | `dotnet test src/YH.Flow.slnx --nologo`                                             |
| **Estimated runtime**  | ~60 seconds（quick） / ~3-5 minutes（full solution）                                |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test src/Tests/Workspace.Tests --filter "FullyQualifiedName~Unit" --nologo`
- **After every plan wave:** Run `dotnet test src/YH.Flow.slnx --nologo`（全量回归，确保不破坏 Phase 1 + 其它模块）
- **Before `/gsd-verify-work`:** Full suite must be green + 手工 smoke（见下）
- **Max feedback latency:** 60 seconds（quick）/ 5 minutes（full）

---

## Per-Task Verification Map

> 任务行由 planner 在 PLAN.md 中产出；此表在执行过程中随任务推进填充。
> Threat Ref 列指向 PLAN.md 的 `<threat_model>` block。

| Task ID      | Plan  | Wave | Requirement        | Threat Ref                       | Secure Behavior                                                                             | Test Type         | Automated Command                                                                           | File Exists | Status   |
| ------------ | ----- | ---- | ------------------ | -------------------------------- | ------------------------------------------------------------------------------------------- | ----------------- | ------------------------------------------------------------------------------------------- | ----------- | -------- |
| 2-W0         | 02-01 | 0    | —                  | —                                | N/A（脚手架 + Finbuckle DI spike）                                                          | spike             | `dotnet test src/Tests/Workspace.Tests/Spike/FinbuckleExternalStrategyRegistrationTests.cs` | ✅          | ✅ green |
| 2-01 (T2.1)  | 02-04 | 3    | REQ-2.1 / D-07~09  | T-2-slug                         | slug 生成 `[a-z0-9-]` + 受限词拒绝 + 冲突重试                                               | unit              | `dotnet test src/Tests/Workspace.Tests --filter SlugGenerator`                              | ✅          | ✅ green |
| 2-01b (T2.1) | 02-04 | 3    | REQ-2.1 / D-08     | T-2-softdelete                   | 软删除追加 `__{epoch}` 释放 slug                                                            | integration       | `dotnet test src/Tests/Workspace.Tests --filter SoftDelete`                                 | ✅          | ✅ green |
| 2-01c (T2.1) | 02-04 | 3    | REQ-2.1            | T-2-slug                         | slug 唯一性竞态（DB 唯一约束兜底）                                                          | integration       | `dotnet test src/Tests/Workspace.Tests --filter SlugUnique`                                 | ✅          | ✅ green |
| 2-01d (T2.1) | 02-04 | 3    | REQ-2.1 / D-06     | —                                | 创建 workspace 自动建 Admin `WorkspaceMember` + 设 OwnerId                                  | unit              | `dotnet test src/Tests/Workspace.Tests --filter CreateWorkspace`                            | ✅          | ✅ green |
| 2-02 (T2.8)  | 02-02 | 1    | NFR-2 / D-01       | T-2-isolation                    | slug→tenant 解析正确填充 TenantInfo；`Workspace` 实体 = `IGlobalEntity`（不 `IHasTenant`）  | integration       | `dotnet test src/Tests/Workspace.Tests --filter SlugTenantResolve`                          | ✅          | ✅ green |
| 2-03 (T2.9)  | 02-03 | 2    | NFR-2 / D-02       | T-2-idor                         | 跨 workspace 数据隔离（A 的 member 不出现于 B）                                             | integration       | `dotnet test src/Tests/Workspace.Tests --filter TenantIsolation`                            | ✅          | ✅ green |
| 2-04 (T2.9)  | 02-03 | 2    | D-02               | T-2-eop                          | 非成员访问 workspace 端点 → 403；slug 不存在 → 404（不泄露存在性）                          | integration       | `dotnet test src/Tests/Workspace.Tests --filter Membership`                                 | ✅          | ✅ green |
| 2-05 (T2.9)  | 02-03 | 2    | D-11               | T-2-eop                          | role 不足 → 403（Member 改设置、Guest 写、Member 提权自己）                                 | integration       | `dotnet test src/Tests/Workspace.Tests --filter RequireWorkspaceRole`                       | ✅          | ✅ green |
| 2-06 (T2.5)  | 02-05 | 4    | REQ-2.2 / D-05     | T-2-n1                           | list members 批量查用户无 N+1（单次 SQL）                                                   | integration       | `dotnet test src/Tests/Workspace.Tests --filter ListMembers`                                | ✅          | ✅ green |
| 2-07 (T2.6)  | 02-05 | 4    | REQ-2.4 / D-12     | T-2-token                        | 邀请 token 哈希存储（DB 无明文）                                                            | unit              | `dotnet test src/Tests/Workspace.Tests --filter InvitationHash`                             | ✅          | ✅ green |
| 2-08 (T2.6)  | 02-05 | 4    | REQ-2.4 / D-12     | T-2-replay                       | accept/reject/revoke 后 token 失效                                                          | integration       | `dotnet test src/Tests/Workspace.Tests --filter InvitationInvalidate`                       | ✅          | ✅ green |
| 2-09 (T2.6)  | 02-05 | 4    | D-12               | T-2-token                        | token 过期拒绝（TTL）                                                                       | unit              | `dotnet test src/Tests/Workspace.Tests --filter InvitationTtl`                              | ✅          | ✅ green |
| 2-10 (T2.9)  | 02-06 | 5    | REQ-2.1~2.4 + D-08 | T-2-e2egap / T-2-slugreleasereal | 端到端 workspace 生命周期 10 步（创建→slug-check→邀请→接受→list→role→delete→slug 释放复用） | integration smoke | `dotnet test src/Tests/Workspace.Tests --filter WorkspaceLifecycleSmoke`                    | ✅          | ✅ green |
| 2-11 (T2.9)  | 02-06 | 5    | D-11               | T-2-matrixgap                    | role × capability 矩阵（Admin/Member/Guest/non-member × endpoint）                          | unit matrix       | `dotnet test src/Tests/Workspace.Tests --filter WorkspaceRoleCapability`                    | ✅          | ✅ green |

_Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky_

---

## Wave 0 Requirements

- [x] `src/Tests/Workspace.Tests/` 项目（csproj + 引用 Workspace + TestUtils）—— 模仿 `src/Tests/Identity.Tests/`
- [x] `src/Tests/Workspace.Tests/TestData/WorkspaceTestFixture.cs` —— 共享 fixture scaffolding（InMemory 模式）
- [x] `src/Tests/Workspace.Tests/Services/SlugGeneratorTests.cs` —— REQ-2.1 slug 规则（02-04）
- [x] `src/Tests/Workspace.Tests/Authorization/RequireWorkspaceRoleHandlerTests.cs` —— D-11（02-03）
- [x] `src/Tests/Workspace.Tests/Integration/TenantIsolationTests.cs` —— NFR-2 跨 workspace 隔离（02-03）
- [x] Q1 spike: `src/Tests/Workspace.Tests/Spike/FinbuckleExternalStrategyRegistrationTests.cs` —— 验证 builder 外追加 strategy（02-01）

---

## Manual-Only Verifications

| Behavior                  | Requirement | Why Manual                            | Test Instructions                                                                                                                                                               |
| ------------------------- | ----------- | ------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 端到端 workspace 生命周期 | REQ-2.1~2.4 | 需真实 auth token + DB + 跨端点状态流 | Phase gate smoke（手工 11 步）：见 `02-VERIFICATION.md` §Human Verification。自动化 10 步 smoke 已由 `WorkspaceLifecycleSmokeTests` 覆盖；手工 smoke 是最终 HTTP 真实 auth 验证 |
| 邀请链接形态              | REQ-2.4     | URL 由前端/Phase 13 定                | 确认 Phase 2 只产 `token + slug`，邀请 URL 形态 `{WEB_URL}/{slug}/invitation?token=...` 由前端拼装                                                                              |

---

## Validation Sign-Off

- [x] All tasks have `<automated>` verify or Wave 0 dependencies
- [x] Sampling continuity: no 3 consecutive tasks without automated verify
- [x] Wave 0 covers all MISSING references
- [x] No watch-mode flags
- [x] Feedback latency < 60s（quick）
- [x] `nyquist_compliant: true` set in frontmatter

**Approval:** ✅ automated gate complete; manual smoke pending (see `02-VERIFICATION.md`)
