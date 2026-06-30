# Phase 2: Workspace — Plan Index

**Phase Goal:** 完整工作区管理 API + 多租户数据隔离（REQ-2.1~REQ-2.4 + NFR-1~NFR-4 覆盖）

**Decisions Implemented:** D-01 ~ D-12（全部 LOCKED，从 CONTEXT.md）
**Source Audit:** GOAL ✓ · REQ-2.1/2.2/2.3/2.4 ✓ · RESEARCH（§Architectural Responsibility Map / §Code Examples / §Pitfalls 1-6 / §Validation）✓ · CONTEXT D-01~D-12 ✓ —— 全覆盖，无遗留项。

---

## Wave → Plan 映射

| Plan            | Wave | Objective                                                                                                        | Autonomous                             | REQ 覆盖                            | Tasks | Decisions                         |
| --------------- | ---- | ---------------------------------------------------------------------------------------------------------------- | -------------------------------------- | ----------------------------------- | ----- | --------------------------------- |
| `02-01-PLAN.md` | 0    | Wave 0 spike Q1 + Workspace.Tests 脚手架 + Workspace.Contracts 契约                                              | yes (含 spike 验证但无人工 checkpoint) | (基础设施) — 解锁后续 REQ           | 3     | D-01 (spike)、D-03、D-04          |
| `02-02-PLAN.md` | 1    | Domain 实体 + WorkspaceDbContext + EF 迁移 + Finbuckle slug 解析 wiring                                          | yes                                    | REQ-2.1（实体基础） · NFR-2（隔离） | 3     | D-01、D-04、D-06、D-07~D-09       |
| `02-03-PLAN.md` | 2    | [BLOCKING] 迁移生成+应用+验证 + DbMigrator 注册 + WorkspaceMembershipMiddleware + `[RequireWorkspaceRole]` authz | yes                                    | NFR-2（隔离应用）· D-02、D-11       | 3     | D-02、D-11                        |
| `02-04-PLAN.md` | 3    | Workspace CRUD + slug service + 设置端点                                                                         | yes                                    | REQ-2.1、REQ-2.3                    | 2     | D-06、D-07~D-09                   |
| `02-05-PLAN.md` | 4    | Member 端点（batch N+1 避免）+ Invitation 端点（token crypto）                                                   | yes                                    | REQ-2.2、REQ-2.4                    | 3     | D-04、D-05、D-10、D-12            |
| `02-06-PLAN.md` | 5    | [BLOCKING] 全量回归 + Phase gate smoke（端到端生命周期）                                                         | **no**（含 `checkpoint:human-verify`） | NFR-1、NFR-3、NFR-4 + 全部          | 2     | D-08（slug 释放验证）、D-10、D-12 |

---

## Requirement Coverage Audit

| Requirement                      | Covered By Plans                                                                                 |
| -------------------------------- | ------------------------------------------------------------------------------------------------ |
| REQ-2.1（工作区 CRUD + slug）    | 02-02（实体）→ 02-04（端点）→ 02-06（smoke）                                                     |
| REQ-2.2（成员管理）              | 02-05（member 端点 + IUserIdentityService）→ 02-06（smoke）                                      |
| REQ-2.3（工作区设置）            | 02-04（UpdateWorkspace + settings 字段占位）                                                     |
| REQ-2.4（邀请）                  | 02-05（invitation 端点 + token crypto）→ 02-06（smoke）                                          |
| NFR-1（性能 P95 < 200ms）        | 02-05（N+1 避免 batch 解析）→ 02-06（回归）                                                      |
| NFR-2（多租户隔离）              | 02-02（实体配置 + Workspace = IGlobalEntity）→ 02-03（迁移 + 中间件 + authz）→ 02-06（隔离测试） |
| NFR-3（软删除 + 异常 + Serilog） | 02-04（SoftDelete epoch）→ 02-06（smoke 验证 slug 释放）                                         |
| NFR-4（模块化 + 规范）           | 全部 plan（贴 Identity 模式）                                                                    |

---

## Decision Coverage Audit

| Decision                                              | Plan                                                                            | Task                           |
| ----------------------------------------------------- | ------------------------------------------------------------------------------- | ------------------------------ |
| D-01（Finbuckle slug 解析）                           | 02-01（spike Q1）→ 02-02（strategy + store wiring）                             | spike + Domain                 |
| D-02（成员中间件）                                    | 02-03                                                                           | 中间件任务                     |
| D-03（ICurrentWorkspaceContext）                      | 02-01                                                                           | Contracts 任务                 |
| D-04（标量 UserId 引用）                              | 02-01 + 02-02 + 02-05                                                           | Contracts / 实体 / ListMembers |
| D-05（batch IUserIdentityService）                    | 02-01 + 02-05                                                                   | Contracts / ListMembers        |
| D-06（OwnerId 标量 + 自动 Admin member）              | 02-02 + 02-04                                                                   | 实体 / CreateWorkspace         |
| D-07（混合 slug 来源）                                | 02-04                                                                           | SlugGenerator                  |
| D-08（软删除 epoch）                                  | 02-02（实体 SoftDelete 方法）+ 02-04（DeleteWorkspace handler）+ 02-06（smoke） |
| D-09（受限词 + 格式校验）                             | 02-01（RestrictedSlugs.cs）+ 02-04（SlugGenerator）                             |
| D-10（Phase 2 仅生成链接，预留 INotificationService） | 02-01（INotificationService 抽象）+ 02-05（CreateInvitation 返回 token）        |
| D-11（[RequireWorkspaceRole] authz）                  | 02-03                                                                           | authz 任务                     |
| D-12（CSPRNG + SHA-256 + TTL token）                  | 02-05                                                                           | InvitationTokenService         |

---

## Cross-Plan Dependencies (Wave ordering)

```
Wave 0 (02-01)
  ├─ Workspace.Tests 脚手架 ← 02-02/03/04/05/06 所有 <automated> 依赖
  ├─ Workspace.Contracts (DTOs/ICurrentWorkspaceContext/RestrictedSlugs/INotificationService) ← 02-02+
  ├─ Identity.Contracts (UserSummary/IUserIdentityService) ← 02-05
  └─ spike Q1 结果 ← 02-02 Finbuckle wiring

Wave 1 (02-02) — depends 02-01
  ├─ Domain 实体 + WorkspaceDbContext + 配置
  └─ Finbuckle slug strategy/store wiring

Wave 2 (02-03) — depends 02-02
  ├─ [BLOCKING] EF 迁移生成+应用+验证（schema_push_requirement）
  ├─ DbMigrator + Api Program.cs 注册
  ├─ WorkspaceMembershipMiddleware
  └─ RequireWorkspaceRoleAuthorizationHandler

Wave 3 (02-04) — depends 02-03
  ├─ SlugGenerator service + 冲突重试
  └─ Workspace CRUD endpoints + settings

Wave 4 (02-05) — depends 02-03 (authz + middleware)
  ├─ Member endpoints (batch IUserIdentityService)
  └─ Invitation endpoints + InvitationTokenService

Wave 5 (02-06) — depends 02-04 + 02-05
  ├─ 全量回归 dotnet test src/YH.Flow.slnx
  └─ [BLOCKING] Phase gate smoke（checkpoint:human-verify）
```

---

## Files Modified Summary (parallelism check)

| Plan  | 修改文件（独占，无跨 plan 同文件冲突）                                                                                                                                                                                            |
| ----- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 02-01 | `Tests/Workspace.Tests/*` · `Modules.Workspace.Contracts/*` · `Modules.Identity.Contracts/{UserSummary,IUserIdentityService}` · spike test                                                                                        |
| 02-02 | `Modules.Workspace/{Domain,Data,MultiTenancy,WorkspaceModule.cs,WorkspaceModuleConstants.cs,AssemblyInfo.cs}` · `MultitenancyModule.cs`(可能)                                                                                     |
| 02-03 | `Host/YH.Flow.Migrations.PostgreSQL/Workspace/*` · `Host/YH.Flow.DbMigrator/{Program.cs,csproj}` · `Host/YH.Flow.Migrations.PostgreSQL.csproj` · `Host/YH.Flow.Api/Program.cs` · `Modules.Workspace/{Middleware,Authorization}/*` |
| 02-04 | `Modules.Workspace/Services/{ISlugGenerator,SlugGenerator}.cs` · `Modules.Workspace/Features/v1/Workspaces/*`                                                                                                                     |
| 02-05 | `Modules.Workspace/Services/{IInvitationTokenService,InvitationTokenService,WorkspaceMembershipService}.cs` · `Modules.Identity/Services/UserIdentityService.cs` · `Modules.Workspace/Features/v1/{Members,Invitations}/*`        |
| 02-06 | `Tests/Workspace.Tests/Integration/*` · `02-VERIFICATION.md` · `02-VALIDATION.md`(nyquist_compliant=true)                                                                                                                         |

**冲突检查：** Wave 内的 plans 之间无 `files_modified` 重叠。Wave 2 内唯一文件是 02-03 独占（中间件 + authz + 迁移），不存在并行。

---

## Residual Risks

1. **Q1 spike 失败风险（HIGH 影响，LOW 概率）** — 若 Finbuckle 10.1.1 不允许 builder 外追加 strategy/store，回退方案是 `Workspace.Contracts.AddWorkspaceTenantResolution(this MultiTenantBuilder)` 扩展方法由 `MultitenancyModule` 调用。Wave 0 的 spike 任务会显式验证并产出决定（落入 02-01 任务输出）。
2. **Testcontainers / Aspire 集成测试基础设施** — `WorkspaceTestFixture` 需起 PostgreSQL，若与 Phase 1 `Identity.Tests` fixture 共享实例，可能复杂化；02-01 任务会贴 Phase 1 fixture 模式，必要时降级为 InMemory + 显式 SQL 验证。
3. **手工 smoke 依赖人工** — 02-06 的 `checkpoint:human-verify` 是规划阶段接受的 manual-only（需真实 token + 端到端跨端点状态流），不可自动化。

---
