# Phase 0: 项目初始化 & 脚手架 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-06-16
**Phase:** 0-项目初始化 & 脚手架
**Areas discussed:** 模块裁剪策略, 命名空间与项目重命名, Aspire 编排配置, 前端客户端目录结构

---

## 模块裁剪策略

### Billing 模块处理

| Option              | Description                                             | Selected |
| ------------------- | ------------------------------------------------------- | -------- |
| 移除 Billing        | 一期不需要许可证管理，Phase 0 先移除                    |          |
| 保留 Billing 但禁用 | 保留代码在 Modules/ 中但不注册到 DI                     |          |
| 全部保留禁用        | Catalog/Tickets/Chat/Billing 全部保留代码，移除 DI 注册 | ✓        |

**User's choice:** 全部保留禁用
**Notes:** 与 ROADMAP.md 有偏差（ROADMAP 写的是移除），需要后续更新 ROADMAP。

### 清理深度

| Option             | Description                                                                  | Selected |
| ------------------ | ---------------------------------------------------------------------------- | -------- |
| 仅移除 DI 注册     | 保留模块所有代码文件，仅从 ServiceCollectionExtensions 中移除 AddXxxModule() | ✓        |
| 移除 DI + 排除编译 | 移除 DI 注册并从 .slnx 中排除模块项目                                        |          |

**User's choice:** 仅移除 DI 注册

### BuildingBlocks 处理

| Option   | Description                           | Selected |
| -------- | ------------------------------------- | -------- |
| 全部保留 | BuildingBlocks 是基础设施，与业务无关 | ✓        |
| 按需调整 | 清理跨模块依赖                        |          |

**User's choice:** 全部保留

### Host 项目处理

| Option            | Description                      | Selected |
| ----------------- | -------------------------------- | -------- |
| 保留两个 + 重命名 | YH.Flow.Api + YH.Flow.DbMigrator | ✓        |
| 仅保留 API        | Phase 0 只建 API Host            |          |

**User's choice:** 保留两个 + 重命名

---

## 命名空间与项目重命名

### 命名方案

| Option                        | Description                                                                                             | Selected |
| ----------------------------- | ------------------------------------------------------------------------------------------------------- | -------- |
| YH.Framework._ / YH.Modules._ | BuildingBlocks: YH.Framework.Core/Shared/... ; Modules: YH.Modules.Identity/... ; Host: YH.Flow.Api/... | ✓        |
| YH.\* (扁平)                  | YH.Core / YH.Identity / YH.Api                                                                          |          |

**User's choice:** YH.Framework._ / YH.Modules._

### 文件命名

| Option       | Description                                  | Selected |
| ------------ | -------------------------------------------- | -------- |
| YH.Flow.slnx | 解决方案文件 YH.Flow.slnx，项目目录 yh-flow/ | ✓        |
| Flow.slnx    | 更简短                                       |          |

**User's choice:** YH.Flow.slnx

### 重命名范围

| Option     | Description                                                                                        | Selected |
| ---------- | -------------------------------------------------------------------------------------------------- | -------- |
| 全面替换   | csproj/AssemblyName/RootNamespace + .cs namespace + launchSettings + Docker + appsettings + README | ✓        |
| 仅代码层面 | 只改 csproj 和 .cs namespace                                                                       |          |

**User's choice:** 全面替换

### 执行方式

| Option                | Description                                 | Selected |
| --------------------- | ------------------------------------------- | -------- |
| 手动重命名 + 查找替换 | 逐个手动修改                                |          |
| 脚本批量处理          | PowerShell 脚本批量处理，脚本保留供后期重用 | ✓        |

**User's choice:** 编写 PowerShell 脚本处理，保留脚本后期重用

---

## Aspire 编排配置

### 编排服务

| Option                        | Description                                                                          | Selected |
| ----------------------------- | ------------------------------------------------------------------------------------ | -------- |
| PG + Redis + MinIO            | PostgreSQL (pgAdmin :5050) + Redis/Valkey (RedisInsight :5540) + MinIO (:9000/:9001) | ✓        |
| PG + Redis + MinIO + RabbitMQ | 全部四种基础设施                                                                     |          |

**User's choice:** PG + Redis + MinIO (不含 RabbitMQ)

### Demo Seeder

| Option           | Description                     | Selected |
| ---------------- | ------------------------------- | -------- |
| 保留 Demo Seeder | 保留 acme/globex 演示数据播种器 | ✓        |
| 移除 Demo Seeder | 只保留基础 DbMigrator           |          |

**User's choice:** 保留 Demo Seeder

### 前端编排

| Option           | Description                   | Selected |
| ---------------- | ----------------------------- | -------- |
| 注释掉前端引用   | 保留代码但注释，Phase 13 启用 | ✓        |
| 完全移除前端引用 | 清理 AppHost 中所有前端引用   |          |

**User's choice:** 注释掉前端引用

### 资源命名

| Option                     | Description       | Selected |
| -------------------------- | ----------------- | -------- |
| yhflow-db / yhflow-uploads | 与项目名称一致    | ✓        |
| flow-db / flow-uploads     | 使用展示名称 Flow |          |

**User's choice:** yhflow-db / yhflow-uploads

---

## 前端客户端目录结构

### 目录处理

| Option              | Description                        | Selected |
| ------------------- | ---------------------------------- | -------- |
| 保留目录 + 清理内容 | 保留 clients/ 结构，清理为最小占位 | ✓        |
| 移除整个 clients/   | Phase 0 不涉及前端                 |          |

**User's choice:** 保留目录 + 清理内容

### 清理深度

| Option              | Description                       | Selected |
| ------------------- | --------------------------------- | -------- |
| 仅保留 package.json | 删除 src/ 源码、配置文件、public/ | ✓        |
| 保留完整骨架        | 保留所有配置文件                  |          |

**User's choice:** 仅保留 package.json

---

## Claude's Discretion

- PowerShell 重命名脚本的具体实现细节
- appsettings 中 SMTP/Ethereal 测试账户配置
- Hangfire Dashboard 凭据配置
- .editorconfig 和代码分析规则微调

## Deferred Ideas

- RabbitMQ 集成 — 一期不需要
- Flow Web 前端 — Phase 13
- Billing 模块启用 — 后续评估
- CI/CD 脚本 — 不在项目约束范围内
