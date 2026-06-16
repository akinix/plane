# YH.Flow — Project Context

**Project:** YH.Flow (展示名称: Flow)  
**Type:** 后端迁移 + 前后端重写  
**Start Date:** 2026-06-16  
**Status:** 🚀 Initializing

---

## 1. Vision

将 Plane（开源项目管理平台）的后端从 Python/Django 迁移到 .NET 10 C#，采用 fullstackhero/dotnet-starter-kit 模块化单体架构模板，同时保持与现有 Plane 前端 API 兼容。重写前端以保持 UI 风格一致。

**展示名称 "Flow"** — 代表工作流、流动的效率。

## 2. Mission

构建一个高性能、可扩展的企业级项目管理平台，核心功能包括：

- **工作区（Workspace）管理** — 多租户隔离，成员管理
- **项目管理** — 项目 CRUD，成员权限
- **问题跟踪（Issues）** — 工作项生命周期管理
- **周期管理（Cycles）** — 迭代/冲刺管理
- **模块管理（Modules）** — 功能分组/史诗
- **页面（Pages）** — 文档管理（一期不含实时协作）
- **视图（Views）** — 自定义筛选和展示
- **集成（Integrations）** — GitHub/GitLab/Gitea/Slack 同步
- **Webhook** — 事件驱动的外部通知
- **文件管理** — 附件上传和存储
- **通知** — 站内通知系统

## 3. Architecture Strategy

### 3.1 后端架构

- **模板**: fullstackhero/dotnet-starter-kit (.NET 10)
- **架构模式**: 模块化单体 (Modular Monolith) + 垂直切片 (Vertical Slice)
- **CQRS**: Mediator 3.x 源代码生成器
- **数据库**: PostgreSQL + EF Core 10
- **缓存**: Redis (Valkey)
- **认证**: JWT Bearer + ASP.NET Identity
- **多租户**: Finbuckle (header/query-string 策略)
- **API 风格**: Minimal API + 版本控制
- **后台任务**: Hangfire
- **文件存储**: MinIO/S3
- **代码规范**: 严格遵守 fullstackhero 项目规范

### 3.2 前端架构

- **框架**: React 19 + TypeScript
- **构建**: Vite 7
- **状态管理**: TanStack Query v5
- **路由**: React Router 7
- **样式**: Tailwind CSS 4 + Radix UI (shadcn 风格)
- **表单**: React Hook Form + Zod
- **风格**: 保持与 Plane Web 一致的 UI 布局和菜单结构

### 3.3 API 兼容性

新 .NET API 必须保持与 Plane 现有 API (Django REST Framework) 的接口兼容：
- **URL 路径**: 保持一致的 `/api/v1/` 前缀和路由结构
- **认证方式**: 同时支持 Session Cookie 和 API Key (`X-Api-Key` header)
- **响应格式**: 保持 JSON 结构一致
- **分页**: 兼容现有分页参数和响应格式

## 4. Module Mapping

| Plane Django App | YH.Flow .NET Module | Type | Notes |
|-----------------|---------------------|------|-------|
| `plane.db` (models) | `YH.Modules.Workspace` | New | 多租户 + 工作区 CRUD |
| — | `YH.Modules.Project` | New | 项目 CRUD + 成员 |
| — | `YH.Modules.WorkItems` | New | 问题 + 状态 + 标签 + 估算 |
| — | `YH.Modules.Cycle` | New | 迭代/冲刺管理 |
| — | `YH.Modules.Module` | New | 功能分组/史诗 |
| — | `YH.Modules.Page` | New | 文档管理 |
| — | `YH.Modules.View` | New | 视图/筛选器 |
| — | `YH.Modules.Integration` | New | GitHub/GitLab/Gitea/Slack |
| — | `YH.Modules.Analytics` | New | 仪表板分析 |
| `plane.authentication` | `YH.Modules.Identity` | Adapted | 用户、角色、权限、OAuth |
| — | `YH.Modules.Multitenancy` | Adapted | Finbuckle 多租户 |
| — | `YH.Modules.Auditing` | Adapted | 审计日志 |
| `plane.bgtasks` (webhook) | `YH.Modules.Webhooks` | Adapted | 外发 Webhook |
| `plane.bgtasks` (email) | `YH.Modules.Notifications` | Adapted | 通知系统 |
| `plane.settings/storage` | `YH.Modules.Files` | Adapted | 文件上传/存储 |
| `plane.license` | `YH.Modules.Billing` | Adapted (Optional) | 许可证/订阅 |

## 5. Technology Stack

### 5.1 Backend (YH.Flow)

| Component | Technology | Version |
|-----------|-----------|---------|
| Runtime | .NET SDK | 10.0.100 |
| Language | C# | latest |
| Web Framework | ASP.NET Core | 10.0 |
| ORM | EF Core | 10.0 |
| Database | PostgreSQL | 16+ |
| Cache | Redis (Valkey) | 7.2+ |
| Message Broker | RabbitMQ | 3.13+ |
| Auth | ASP.NET Identity + JWT | — |
| Multi-Tenant | Finbuckle | 10.x |
| CQRS | Mediator | 3.x (source-gen) |
| Validation | FluentValidation | latest |
| Background Jobs | Hangfire | latest |
| Object Storage | MinIO / S3 | — |
| API Docs | Scalar / OpenAPI | — |
| Observability | Serilog + OpenTelemetry | latest |
| Orchestration | .NET Aspire | latest |

### 5.2 Frontend (Flow Web)

| Component | Technology | Version |
|-----------|-----------|---------|
| Framework | React | 19 |
| Language | TypeScript | 5.x |
| Build | Vite | 7.x |
| Routing | React Router | 7.x |
| Server State | TanStack Query | 5.x |
| Styling | Tailwind CSS | 4.x |
| UI Primitives | Radix UI | latest |
| Forms | React Hook Form + Zod | latest |
| HTTP Client | apiFetch (custom) | — |
| Real-time | SSE (Server-Sent Events) | — |

## 6. Constraints

1. **原始代码不可修改** — Plane (Django/React) 代码保持原样，方便后续拉取上游更新
2. **API 兼容优先** — 新 API 的路由和响应格式必须与 Plane 现有 API 一致
3. **代码规范** — 严格遵守 fullstackhero 的 10 条黄金法则和代码规范
4. **无 CI/CD** — 一期不包含 CI/CD 脚本
5. **无实时协作** — 一期 Pages 为普通 CRUD，不做 Yjs/Hocuspocus 实时协作
6. **Web 优先** — 前端仅考虑 Web，不考虑桌面端

## 7. Project Location

```
d:/github/akinix-plane/
├── yh-flow/                    # YH.Flow 项目根目录
│   ├── src/
│   │   ├── BuildingBlocks/     # 共享基础设施
│   │   ├── Modules/            # 业务模块
│   │   ├── Host/               # API 主机 + 迁移器
│   │   └── Tests/              # 测试项目
│   ├── clients/
│   │   └── web/                # Flow Web 前端
│   └── docs/
├── apps/                       # 原始 Plane 代码 (不变)
├── packages/                   # 原始 Plane 代码 (不变)
└── .planning/                  # 项目规划文档
```

## 8. Reference Materials

- **原始后端**: `d:/github/akinix-plane/apps/api/` (Django)
- **原始前端**: `d:/github/akinix-plane/apps/web/` (React)
- **模板项目**: `D:/github/fullstackhero-dotnet-starter-kit/` (.NET 10)
- **模板文档**: `D:/github/fullstackhero-docs/` (Astro)
- **代码库映射**: `.planning/codebase/` (7 篇分析文档)
- **模板规范**: `D:/github/fullstackhero-dotnet-starter-kit/AGENTS.md`
- **模板规则**: `D:/github/fullstackhero-dotnet-starter-kit/.agents/rules/`
