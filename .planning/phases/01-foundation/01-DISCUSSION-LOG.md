# Phase 1: Foundation — 基础设施 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-06-16
**Phase:** 01-foundation
**Areas discussed:** 多认证方式协商, OAuth Provider 框架, Rate Limiting 策略, EF Core + 数据库 Schema 策略

---

## 多认证方式协商

### Q1: 四种认证方式如何共存？

| Option | Description | Selected |
|--------|-------------|----------|
| 多 Scheme 自动协商 | ASP.NET Core 多 AuthenticationScheme，按 Header 自动选择 | ✓ |
| 单一自定义中间件 | 自定义 Handler 统一检查所有认证方式 | |
| 按客户端类型分流 | JWT 给 API，Cookie 给 Web，API Key 给程序化访问 | |

**User's choice:** 多 Scheme 自动协商（推荐）

### Q2: OAuth 登录成功后发放什么凭证？

| Option | Description | Selected |
|--------|-------------|----------|
| OAuth → JWT + Cookie 双发 | 回调中生成 JWT 并同时设置 Session Cookie | ✓ |
| OAuth → JWT only | 仅生成 JWT | |
| OAuth → Cookie only | 仅设置 Session Cookie | |

**User's choice:** OAuth → JWT + Cookie 双发（推荐）

### Q3: API Key 认证的具体设计？

| Option | Description | Selected |
|--------|-------------|----------|
| 数据库存储 + 多 Key | 参考 Plane APIToken，每个用户多个 Key，含名称/过期时间 | ✓ |
| 简化版单 Key | 一个用户一个 Key，哈希存储 | |
| 你决定 | Claude 自行决定 | |

**User's choice:** 数据库存储 + 多 Key（推荐）

### Q4: 认证端点如何处理？

| Option | Description | Selected |
|--------|-------------|----------|
| 复用 FSH 端点 + 适配格式 | 复用 FSH Identity 模块的 /auth/* 端点，调整响应格式 | ✓ |
| 全部重写 | 完全新建认证端点 | |
| 你决定 | Claude 自行决定 | |

**User's choice:** 复用 FSH 端点 + 适配格式（推荐）

---

## OAuth Provider 框架

### Q1: Phase 1 的 OAuth 实现深度？

| Option | Description | Selected |
|--------|-------------|----------|
| 框架 + 可插拔接口 | Phase 1 只搭框架，具体 Provider 在 Phase 9 | ✓ |
| 四个 Provider 全实现 | Phase 1 全部跑通 | |
| 框架 + 部分 Provider | 先实现 1-2 个常用 Provider | |

**User's choice:** 框架 + 可插拔接口（推荐）

### Q2: OAuth 回调流程设计？

| Option | Description | Selected |
|--------|-------------|----------|
| 回调重定向 + 双发凭证 | 重定向到前端指定 URL，同时发放 JWT + Cookie | ✓ |
| 回调返回中间页 | 返回中间页面，前端 JS 提取 token | |
| 你决定 | Claude 自行决定 | |

**User's choice:** 回调重定向 + 双发凭证（推荐）

### Q3: OAuth Provider 配置如何管理？

| Option | Description | Selected |
|--------|-------------|----------|
| appsettings 配置 | ClientId/Secret 放 appsettings.json | |
| 数据库动态配置 | 配置存数据库，运行时管理 | ✓ |
| 你决定 | Claude 自行决定 | |

**User's choice:** 数据库动态配置

### Q4: OAuth Provider 配置放在哪个模块？

| Option | Description | Selected |
|--------|-------------|----------|
| Identity 模块 + 管理端点 | 在 Identity 模块中创建配置实体和 CRUD 端点 | ✓ |
| 独立 Config 模块 | 单独模块，Identity 通过 Contracts 读取 | |
| 你决定 | Claude 自行决定 | |

**User's choice:** Identity 模块 + 管理端点（推荐）

---

## Rate Limiting 策略

### Q1: Rate Limiting 策略如何处理？

| Option | Description | Selected |
|--------|-------------|----------|
| 复用 FSH 方案 + 调参 | 直接使用 FSH 四层限流，通过 appsettings 调参 | ✓ |
| 扩展分层策略 | 增加按端点/按角色的差异化策略 | |
| 你决定 | Claude 自行决定 | |

**User's choice:** 复用 FSH 方案 + 调参（推荐）
**Notes:** FSH 默认 Auth 10次/分钟 已匹配 Plane 需求

### Q2: 限流触发后的响应格式？

| Option | Description | Selected |
|--------|-------------|----------|
| Plane 兼容格式优先 | 检查 Plane 是否有自定义限流响应格式并适配 | |
| FSH 标准格式 | 直接用 FSH 的 RFC 7807 ProblemDetails | ✓ |
| 你决定 | Claude 自行决定 | |

**User's choice:** FSH 标准格式

### Q3: 是否需要端点级别的限流配置覆盖？

| Option | Description | Selected |
|--------|-------------|----------|
| 全局配置即可 | 仅通过 appsettings 配置全局默认值 | ✓ |
| 支持端点级覆盖 | 支持 [EnableRateLimiting] 特性对特定端点设置不同策略 | |
| 你决定 | Claude 自行决定 | |

**User's choice:** 全局配置即可（推荐）

---

## EF Core + 数据库 Schema 策略

### Q1: EF Core Schema 分离策略？

| Option | Description | Selected |
|--------|-------------|----------|
| 模块独立 Schema | 每个模块独立 Schema（yhschema.Identity, yhschema.Workspace...） | ✓ |
| 共享 Schema + 表前缀 | 所有模块共享 yhschema，通过表名前缀区分 | |
| 全部 public Schema | 全部用 public Schema，不做分离 | |

**User's choice:** 模块独立 Schema（推荐）

### Q2: EF Core 迁移如何组织？

| Option | Description | Selected |
|--------|-------------|----------|
| 模块内迁移 | 每个模块的迁移放在各自的 DbContext 项目中 | ✓ |
| 集中式迁移 | 所有迁移集中在 YH.Flow.Migrations.PostgreSQL 项目中 | |
| 你决定 | Claude 自行决定 | |

**User's choice:** 模块内迁移（推荐）

### Q3: 数据库连接字符串如何配置？

| Option | Description | Selected |
|--------|-------------|----------|
| 单一连接字符串 | 所有 DbContext 共用同一数据库 | ✓ |
| 每模块独立连接 | 每个模块可配置独立连接字符串 | |
| 你决定 | Claude 自行决定 | |

**User's choice:** 单一连接字符串（推荐）

### Q4: 数据库迁移如何执行？

| Option | Description | Selected |
|--------|-------------|----------|
| DbMigrator 启动时迁移 | Aspire 启动时 DbMigrator 先运行，执行所有迁移 | ✓ |
| API 启动时自动迁移 | API 启动时自动检测并执行迁移 | |
| 手动迁移 | 手动执行迁移脚本 | |

**User's choice:** DbMigrator 启动时迁移（推荐）

---

## Claude's Discretion

以下领域用户选择了"你决定"或委托 Claude 自行决定具体实现：

- EF Core 实体配置的具体实现细节（IEntityTypeConfiguration 的组织方式）
- Serilog Sink 配置和 OpenTelemetry Exporter 的具体选择
- CORS 策略的具体域名配置（开发阶段可宽松）
- Security Headers 的具体配置
- 全局异常处理的具体错误格式适配（Plane 错误字段映射）
- 分页格式适配的具体实现（count/next/previous/results）
- Idempotency 中间件的具体实现

## Deferred Ideas

- **具体 OAuth Provider 实现** — Phase 9 Integration 实现 GitHub/GitLab/Gitea/Google Provider
- **Slack OAuth 集成** — Phase 9 Integration（REQ-9.4）
- **端点级限流覆盖** — 后续如有特殊端点需要差异化限流
- **Magic Link 认证** — Plane 支持 magic code 登录，一期不实现
- **Per-模块独立数据库** — 一期用单一数据库 + Schema 分离，后续如需物理隔离可迁移
