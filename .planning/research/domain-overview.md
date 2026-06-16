# Domain Research: Project Management Platform

**Research Date:** 2026-06-16

## Domain Concepts

### Core Entities

| Entity | Description | Relationships |
|--------|-------------|---------------|
| **Workspace** | 顶层租户容器，隔离不同团队/组织的数据 | 1:N Projects, 1:N Members |
| **Project** | 工作区内的项目，包含所有工作项和配置 | N:1 Workspace, 1:N Issues/Cycles/Modules/Pages/Views |
| **Issue** | 核心工作项（任务、Bug、功能请求等） | N:1 Project, N:1 State, N:N Labels, 1:N Comments |
| **Cycle** | 时间盒迭代/冲刺，组织 Issues 的周期 | N:1 Project, N:N Issues |
| **Module** | 功能分组/史诗，跨周期的 Issue 分组 | N:1 Project, N:N Issues |
| **Page** | 富文本文档，支持嵌入工作区/项目级别 | N:1 Workspace 或 N:1 Project |
| **View** | 保存的筛选器和展示配置 | N:1 Project |
| **State** | Issue 工作流状态（Backlog → Todo → In Progress → Done） | N:1 Project |
| **Label** | 标签/分类 | N:1 Project, N:N Issues |
| **Estimate** | 工作量估算系统 | N:1 Project, 1:N Issues |

### Workflow Concepts

- **State Transitions**: Issue 在状态之间流转，遵循项目定义的工作流
- **Cycle Lifecycle**: 创建 → 开始 → 结束 → 归档
- **Module Lifecycle**: 创建 → 活跃 → 完成 → 归档
- **Intake/Inbox**: 外部提交的问题收集箱（类似工单系统）

### Integration Concepts

- **GitHub/GitLab/Gitea Sync**: 双向 Issue 同步
  - 自动创建/更新 Issues
  - 评论同步
  - Repository → Project 映射
- **Slack Sync**: 项目通知同步到 Slack 频道
- **Webhook**: 事件驱动的 HTTP 回调
  - 支持的事件类型: issue, project, module, cycle, issue_comment
  - HMAC-SHA256 签名
  - 重试机制

## User Roles & Permissions

| Role | Level | Description |
|------|-------|-------------|
| **Admin** | 20 | 完全控制工作区和所有项目 |
| **Member** | 15 | 可创建/编辑项目和内容 |
| **Guest** | 5 | 只读访问特定项目 |

### Permission Granularity

权限系统支持：
- **Workspace-level**: 管理工作区设置、成员、计费
- **Project-level**: 管理项目设置、成员
- **Entity-level**: 创建/编辑/删除 Issue、Cycle、Module 等

## API Compatibility Requirements

新 .NET API 需要与以下 Plane API 结构兼容：

```
/api/v1/workspaces/{slug}/           — 工作区详情和设置
/api/v1/workspaces/{slug}/projects/  — 项目 CRUD
/api/v1/workspaces/{slug}/projects/{id}/issues/     — 工作项 CRUD
/api/v1/workspaces/{slug}/projects/{id}/cycles/     — 周期 CRUD
/api/v1/workspaces/{slug}/projects/{id}/modules/    — 模块 CRUD
/api/v1/workspaces/{slug}/projects/{id}/pages/      — 页面 CRUD
/api/v1/workspaces/{slug}/projects/{id}/views/      — 视图 CRUD
/api/v1/workspaces/{slug}/projects/{id}/states/     — 状态 CRUD
/api/v1/workspaces/{slug}/projects/{id}/labels/     — 标签 CRUD
/api/v1/workspaces/{slug}/projects/{id}/estimates/  — 估算 CRUD
/api/v1/workspaces/{slug}/projects/{id}/intake/     — 收件箱
/api/v1/workspaces/{slug}/webhooks/                 — Webhook 管理
/api/v1/workspaces/{slug}/integrations/             — 集成管理

/auth/                                              — 认证端点
/api/public/workspaces/{slug}/                      — 公开空间
```

### Key Response Patterns to Maintain

```json
// Paginated response
{
  "count": 100,
  "next": "https://.../api/v1/workspaces/slug/projects/id/issues/?page=2",
  "previous": null,
  "results": [...]
}

// Standard object response
{
  "id": "uuid",
  "created_at": "2026-06-16T10:00:00Z",
  "updated_at": "2026-06-16T12:00:00Z",
  "created_by": "user-uuid",
  "updated_by": "user-uuid",
  ...
}

// Error response
{
  "error": "Error message",
  "error_code": 400,
  "error_detail": { "field": ["validation message"] }
}
```
