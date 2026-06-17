---
phase: 2
slug: workspace
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-06-17
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

| Task ID      | Plan | Wave | Requirement       | Threat Ref     | Secure Behavior                                                                            | Test Type        | Automated Command                                                                           | File Exists | Status     |
| ------------ | ---- | ---- | ----------------- | -------------- | ------------------------------------------------------------------------------------------ | ---------------- | ------------------------------------------------------------------------------------------- | ----------- | ---------- |
| 2-W0         | W0   | 0    | —                 | —              | N/A（脚手架）                                                                              | spike            | `dotnet test src/Tests/Workspace.Tests/Spike/FinbuckleExternalStrategyRegistrationTests.cs` | ❌ W0       | ⬜ pending |
| 2-01 (T2.1)  | TBD  | 1    | REQ-2.1 / D-07~09 | T-2-slug       | slug 生成 `[a-z0-9-]` + 受限词拒绝 + 冲突重试                                              | unit             | `dotnet test src/Tests/Workspace.Tests --filter SlugGenerator`                              | ❌ W0       | ⬜ pending |
| 2-01b (T2.1) | TBD  | 1    | REQ-2.1 / D-08    | T-2-softdelete | 软删除追加 `__{epoch}` 释放 slug                                                           | unit/integration | `dotnet test src/Tests/Workspace.Tests --filter SoftDelete`                                 | ❌ W0       | ⬜ pending |
| 2-01c (T2.1) | TBD  | 1    | REQ-2.1           | T-2-slug       | slug 唯一性竞态（DB 唯一约束兜底）                                                         | integration      | `dotnet test src/Tests/Workspace.Tests --filter SlugUnique`                                 | ❌ W0       | ⬜ pending |
| 2-01d (T2.1) | TBD  | 1    | REQ-2.1 / D-06    | —              | 创建 workspace 自动建 Admin `WorkspaceMember` + 设 OwnerId                                 | unit             | `dotnet test src/Tests/Workspace.Tests --filter CreateWorkspace`                            | ❌ W0       | ⬜ pending |
| 2-02 (T2.8)  | TBD  | 1    | NFR-2 / D-01      | T-2-isolation  | slug→tenant 解析正确填充 TenantInfo；`Workspace` 实体 = `IGlobalEntity`（不 `IHasTenant`） | integration      | `dotnet test src/Tests/Workspace.Tests --filter SlugTenantResolve`                          | ❌ W0       | ⬜ pending |
| 2-03 (T2.9)  | TBD  | 2    | NFR-2 / D-02      | T-2-idor       | 跨 workspace 数据隔离（A 的 member 不出现于 B）                                            | integration      | `dotnet test src/Tests/Workspace.Tests --filter TenantIsolation`                            | ❌ W0       | ⬜ pending |
| 2-04 (T2.9)  | TBD  | 2    | D-02              | T-2-eop        | 非成员访问 workspace 端点 → 403；slug 不存在 → 404（不泄露存在性）                         | integration      | `dotnet test src/Tests/Workspace.Tests --filter Membership`                                 | ❌ W0       | ⬜ pending |
| 2-05 (T2.9)  | TBD  | 2    | D-11              | T-2-eop        | role 不足 → 403（Member 改设置、Guest 写、Member 提权自己）                                | integration      | `dotnet test src/Tests/Workspace.Tests --filter RequireWorkspaceRole`                       | ❌ W0       | ⬜ pending |
| 2-06 (T2.5)  | TBD  | 4    | REQ-2.2 / D-05    | T-2-n1         | list members 批量查用户无 N+1（单次 SQL）                                                  | integration      | `dotnet test src/Tests/Workspace.Tests --filter ListMembers`                                | ❌ W0       | ⬜ pending |
| 2-07 (T2.6)  | TBD  | 4    | REQ-2.4 / D-12    | T-2-token      | 邀请 token 哈希存储（DB 无明文）                                                           | unit             | `dotnet test src/Tests/Workspace.Tests --filter InvitationHash`                             | ❌ W0       | ⬜ pending |
| 2-08 (T2.6)  | TBD  | 4    | REQ-2.4 / D-12    | T-2-replay     | accept/reject/revoke 后 token 失效                                                         | integration      | `dotnet test src/Tests/Workspace.Tests --filter InvitationInvalidate`                       | ❌ W0       | ⬜ pending |
| 2-09 (T2.6)  | TBD  | 4    | D-12              | T-2-token      | token 过期拒绝（TTL）                                                                      | unit             | `dotnet test src/Tests/Workspace.Tests --filter InvitationTtl`                              | ❌ W0       | ⬜ pending |

_Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky_

---

## Wave 0 Requirements

- [ ] `src/Tests/Workspace.Tests/` 项目（csproj + 引用 Workspace + TestUtils）—— 模仿 `src/Tests/Identity.Tests/`
- [ ] `src/Tests/Workspace.Tests/TestData/WorkspaceTestFixture.cs` —— 共享 fixture：起 PostgreSQL（Testcontainers 或 Aspire）+ seed root user
- [ ] `src/Tests/Workspace.Tests/Services/SlugGeneratorTests.cs` —— REQ-2.1 slug 规则
- [ ] `src/Tests/Workspace.Tests/Authorization/RequireWorkspaceRoleHandlerTests.cs` —— D-11
- [ ] `src/Tests/Workspace.Tests/Integration/TenantIsolationTests.cs` —— NFR-2 跨 workspace 隔离（贴 `src/Tests/Architecture.Tests/TenantIsolationTests.cs` 模式）
- [ ] Q1 spike: `src/Tests/Workspace.Tests/Spike/FinbuckleExternalStrategyRegistrationTests.cs` —— 验证 builder 外追加 strategy（解锁 D-01 的前置）

---

## Manual-Only Verifications

| Behavior                  | Requirement | Why Manual                            | Test Instructions                                                                                                                                                |
| ------------------------- | ----------- | ------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 端到端 workspace 生命周期 | REQ-2.1~2.4 | 需真实 auth token + DB + 跨端点状态流 | Phase gate smoke：用 API Key/JWT → 创建 workspace → slug-check → 邀请 → 接受邀请 → list members → 改 role → leave → 删除工作区（验证 slug 释放可被新工作区复用） |
| 邀请链接形态              | REQ-2.4     | URL 由前端/Phase 13 定                | 确认 Phase 2 只产 `token + slug`，邀请 URL 形态 `{WEB_URL}/{slug}/invitation?token=...` 由前端拼装                                                               |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 60s（quick）
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
