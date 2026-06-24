# Phase 0: 项目初始化 & 脚手架 - Context

**Gathered:** 2026-06-16
**Status:** Ready for planning

<domain>
## Phase Boundary

基于 fullstackhero/dotnet-starter-kit (.NET 10) 模板创建 YH.Flow 项目结构。重命名命名空间 FSH → YH，移除不需要模块的 DI 注册，配置构建基础设施（Directory.Build.props, Directory.Packages.props, NuGet.Config），搭建 .NET Aspire AppHost 编排，验证解决方案可编译运行。

此为纯脚手架阶段 — 不涉及业务逻辑实现、数据库迁移或 API 端点开发。
</domain>

<decisions>
## Implementation Decisions

### 模块裁剪策略

- **D-01:** 所有模板模块代码保留在磁盘上 — Catalog, Tickets, Chat, Billing 不删除，供后续参考
- **D-02:** 禁用的模块（Catalog, Tickets, Chat, Billing）仅移除 ServiceCollectionExtensions 中的 `AddXxxModule()` DI 注册调用，项目保留在 .slnx 中参与编译
- **D-03:** BuildingBlocks（Core, Shared, Persistence, Web, Caching, Eventing, Storage, Jobs, Mailing, Quota）全部保留
- **D-04:** Host 项目保留 API + DbMigrator 两个，重命名为 YH.Flow.Api / YH.Flow.DbMigrator

### 命名空间与项目重命名

- **D-05:** 命名方案保留模板层级 — `YH.Framework.{Name}` (BuildingBlocks), `YH.Modules.{Name}` (Modules), `YH.Flow.{Name}` (Host)
- **D-06:** 解决方案文件命名为 `YH.Flow.slnx`
- **D-07:** 全面替换 FSH → YH：csproj (AssemblyName/RootNamespace)、所有 .cs namespace、launchSettings.json、Dockerfile 标签、appsettings、源代码字符串常量、README
- **D-08:** PowerShell 脚本批量处理重命名，脚本保留在项目中供后期重用

### Aspire 编排配置

- **D-09:** 编排服务：PostgreSQL (含 pgAdmin :5050) + Redis/Valkey (含 RedisInsight :5540) + MinIO (:9000/console :9001)。不含 RabbitMQ（一期用 Hangfire 做后台任务）
- **D-10:** 保留 Demo Seeder（acme/globex 演示租户和用户），方便开发测试
- **D-11:** 前端 JavaScript App 引用（Admin :5173 / Dashboard :5174）注释掉，待 Phase 13 启用
- **D-12:** 资源命名：数据库 `yhflow-db`，MinIO bucket `yhflow-uploads`

### 前端客户端目录结构

- **D-13:** 保留 `clients/` 目录结构，每个客户端（admin, dashboard）仅保留 `package.json`
- **D-14:** Phase 13 在 `clients/web/` 实现真正的 Flow Web 前端

### Claude's Discretion

- Power​Shell 重命名脚本的具体实现细节
- appsettings 中 SMTP/Ethereal 测试账户配置（从模板继承即可）
- Hangfire Dashboard 凭据配置
- .editorconfig 和代码分析规则的微调
  </decisions>

<specifics>
## Specific Ideas

- 重命名用的 PowerShell 脚本需要保留在项目中（如 `scripts/rename-fsh-to-yh.ps1`），方便后续模块创建时复用
- 模板的 10 条黄金法则和 AGENTS.md 规范需要在新项目中保留引用
- Demo Seeder 的演示账户（admin@root.com）和密码从模板继承
  </specifics>

<canonical_refs>

## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 模板参考（Fullstackhero）

- `D:/github/fullstackhero-dotnet-starter-kit/AGENTS.md` — 模板编码规范和 10 条黄金法则
- `D:/github/fullstackhero-dotnet-starter-kit/.agents/rules/` — 模板 Agent 规则目录
- `D:/github/fullstackhero-docs/` — Fullstackhero 文档（Astro）

### 项目规划文档

- `.planning/PROJECT.md` — 项目愿景、架构策略、模块映射、约束条件
- `.planning/REQUIREMENTS.md` — 13 个阶段的需求定义
- `.planning/ROADMAP.md` — 阶段结构和依赖关系
- `.planning/STATE.md` — 项目状态和关键决策日志

### 代码库分析（Plane 原始代码）

- `.planning/codebase/STACK.md` — Plane 技术栈分析
- `.planning/codebase/ARCHITECTURE.md` — Plane 架构分析
- `.planning/codebase/CONVENTIONS.md` — Plane 编码约定

### 研究文档

- `.planning/research/fullstackhero-patterns.md` — FSH 模式适配分析
- `.planning/research/domain-overview.md` — 领域实体模型
- `.planning/research/api-migration-mapping.md` — Django → .NET API 映射

</canonical_refs>

<code_context>

## Existing Code Insights

### Reusable Assets

- **fullstackhero/dotnet-starter-kit** (`D:/github/fullstackhero-dotnet-starter-kit/`) — 完整模板项目，包含所有 BuildingBlocks、Modules、Host、Tests、Tools
- **BuildingBlocks 基础设施** — Core (实体基类/值对象)、Shared (工具类)、Persistence (EF Core 仓储)、Web (中间件/Filters)、Caching (Redis)、Eventing (事件总线)、Storage (S3/MinIO)

### Established Patterns

- **模块结构** — 每个模块含 `{Module}.csproj` + `{Module}.Contracts.csproj`，Contracts 暴露接口/DTO 供跨模块引用
- **Host 项目** — API (Minimal API) + DbMigrator (控制台) + AppHost (Aspire 编排) + Migrations (EF Core 迁移)
- **命名约定** — RootNamespace 匹配项目路径，Assembly 名与目录名一致
- **构建系统** — Directory.Build.props (通用属性) + Directory.Packages.props (NuGet 中心包管理) + NuGet.Config (源配置)

### Integration Points

- **AppHost → API** — Aspire 编排通过 Project Reference 启动 API
- **AppHost → DbMigrator** — 编排在 API 之前运行迁移 + 种子数据
- **AppHost → Infrastructure** — 容器化 PostgreSQL/Redis/MinIO，Persistent 生命周期
  </code_context>

<deferred>
## Deferred Ideas

- **RabbitMQ 集成** — 一期不需要，Hangfire 满足后台任务需求。后续异步消息传递可考虑
- **Flow Web 前端** — Phase 13 实现，Phase 0 仅保留 clients/ 目录骨架
- **Billing 模块启用** — 保留代码在磁盘，后续许可证管理阶段评估能否复用
- **CI/CD 脚本** — 不在项目约束范围内

</deferred>

---

_Phase: 00-init_
_Context gathered: 2026-06-16_
