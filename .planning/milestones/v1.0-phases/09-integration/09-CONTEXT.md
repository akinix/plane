# Phase 9: Integration — 集成 - Context

**Gathered:** 2026-06-25
**Status:** Deferred to future milestone

<domain>
## Phase Boundary

实现 GitHub, GitLab, Gitea, Slack, Unsplash 五个外部集成的后端支持。

**范围内（REQ-9.1 ~ REQ-9.5）：**

- REQ-9.1: GitHub 集成 — OAuth、仓库列表、项目关联、Issue 双向同步、评论同步、标签映射
- REQ-9.2: GitLab 集成 — OAuth、项目列表、双向同步
- REQ-9.3: Gitea 集成 — OAuth、双向同步
- REQ-9.4: Slack 集成 — OAuth、项目通知同步、事件筛选
- REQ-9.5: Unsplash 集成 — 图片搜索、封面设置

**此阶段决策记录供后续里程碑参考。**

</domain>

<decisions>
## Implementation Decisions

### 阶段状态

- **全部推迟到后续里程碑** — Phase 9 所有 5 个集成均不在当前 v1.0 里程碑范围内

### 模块架构（已锁定，供后续参考）

- **`Modules.Integration` 父模块** — 新建独立模块
- **按提供者拆分子命名空间** — GitHub / Slack / GitLab / Gitea / Unsplash 各一个子命名空间
- **独立 `IntegrationDbContext`** — schema `yhschema.Integration`
- **Contracts 分离** — `Modules.Integration.Contracts`

### 同步机制（已锁定，供后续参考）

- **混合模式** — Webhook 接收端点（本模块内）+ Hangfire 定时任务轮询兜底
- Webhook 接收端属于 Phase 9（接收 GitHub/GitLab 事件），不属于 Phase 10（发往外部的 Webhook）

### 存储模型（已锁定，供后续参考）

- 参考 Plane 设计：`Integration`（全局注册）→ `WorkspaceIntegration`（工作区级 OAuth 凭证）→ 各提供者特定 sync 模型
- 基础模型：`Integration / WorkspaceIntegration`（通用）
- 提供者特定：`GithubRepositorySync / GithubIssueSync / GithubCommentSync / SlackProjectSync` 等

### Claude's Discretion

- 实现优先级：GitHub 最复杂（Repository/Issue/Comment sync、Label mapping）→ Slack + Unsplash（中等）→ GitLab + Gitea（OAuth + 基础 sync，复用 GitHub sync 模式）
- 各集成之间的 Wave 划分留给后续里程碑规划阶段决定

</decisions>

<canonical_refs>

## Canonical References

**后续实现时 MUST read 以下参考资料：**

### Plane 集成实现参考

- `apps/api/plane/db/models/integration/base.py` — Integration + WorkspaceIntegration 基础模型
- `apps/api/plane/db/models/integration/github.py` — GitHub 同步模型（Repository/Issue/Comment）
- `apps/api/plane/db/models/integration/slack.py` — Slack 同步模型（SlackProjectSync）
- `apps/api/plane/app/views/external/base.py` — Unsplash 搜索 + AI 端点
- `apps/api/plane/authentication/provider/oauth/github.py` — GitHub OAuth 流程
- `apps/api/plane/authentication/provider/oauth/gitlab.py` — GitLab OAuth 流程
- `apps/api/plane/authentication/provider/oauth/gitea.py` — Gitea OAuth 流程

### YH.Flow 现有模式参考

- Phase 6 Module 模式（`.planning/phases/06-module/06-CONTEXT.md`）— 与 WorkItems 模块类似的模式
- Phase 7 Page 模式（`.planning/phases/07-page/07-CONTEXT.md`）— 独立模块 + 独立 DbContext 模式
- Phase 5 Cycle 模式（WorkItems 垂直切片模式）

### 需求文档

- `.planning/REQUIREMENTS.md` §REQ-9.1 ~ REQ-9.5 — 各集成需求定义
- `.planning/codebase/INTEGRATIONS.md` — Plane 集成全面分析

</canonical_refs>

<code_context>

## Existing Code Insights

### Reusable Assets

- **Modules 模块创建模式** — Page/View 模块是新建独立模块的最佳参考模板（Contracts + Domain + DbContext + 注册扩展方法）
- **Phase 1 OAuth 框架** — 已有 GitHub/GitLab/Gitea OAuth Provider 框架，集成复用 OAuth 流程
- **Hangfire 后台任务** — 已有 Hangfire 配置，用于轮询同步任务
- **WorkspaceIntegration 模型** — Plane 通用 Integration/WorkspaceIntegration 模型中转设计

### Established Patterns

- **独立模块 + Contracts 分离** — 每个新领域模块遵循统一模式
- **垂直切片 + CQRS** — 每个 Feature 一个目录（Command/Query/Handler/Endpoint/Validator）
- **snake_case JSON** — JsonPropertyName 与 Plane 兼容
- **软删除 + 审计字段** — ISoftDeletable + IAuditableEntity

### Integration Points

- 模块注册: `AddIntegrationModule()` 扩展方法
- OAuth 凭证复用 Phase 1 现有的 Identity 模块 OAuth 框架
- 后台任务用 Hangfire（已有配置）
- Issue 同步需要 Phase 4 WorkItems 模块的 Issue 查询和 Mutation 端点
- Webhook 接收端点在本模块内实现

</code_context>

<specifics>
## Specific Ideas

Phase 9 全部推迟到后续里程碑，以下为后续实现的关键参考：

1. **模块结构**：`Modules.Integration` 父模块，子命名空间按提供者拆分
2. **同步机制**：Webhook 接收 + Hangfire 轮询兜底
3. **GitHub sync 数据模型**（参考 Plane `github.py`）：
   - `GithubRepository`（name, url, config, repository_id, owner）
   - `GithubRepositorySync`（关联 Project, Label, credentials）
   - `GithubIssueSync`（repo_issue_id, github_issue_id, issue_url）
   - `GithubCommentSync`（repo_comment_id）
4. **Slack sync 数据模型**（参考 Plane `slack.py`）：
   - `SlackProjectSync`（access_token, scopes, bot_user_id, webhook_url, team_id, team_name）

</specifics>

<deferred>
## Deferred Ideas

### 全部推迟到后续里程碑

- **GitHub 集成**（REQ-9.1）— GitHub OAuth + Issue/Comment/Repository 双向同步
- **GitLab 集成**（REQ-9.2）— OAuth（含自托管）+ 双向同步
- **Gitea 集成**（REQ-9.3）— OAuth + 双向同步
- **Slack 集成**（REQ-9.4）— OAuth + 项目通知同步
- **Unsplash 集成**（REQ-9.5）— 图片搜索 + 封面设置
- IntegrationDbContext + 迁移（推迟到下一个里程碑创建）

</deferred>

---

_Phase: 9-Integration_
_Context gathered: 2026-06-25_
_Status: Deferred - all integration work moved to future milestone_
