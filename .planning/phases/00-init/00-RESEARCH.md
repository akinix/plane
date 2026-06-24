# Phase 0: 项目初始化 & 脚手架 - Research

**Researched:** 2026-06-16
**Status:** Complete

---

## 1. 模板结构分析

### 1.1 目录布局

```
fullstackhero-dotnet-starter-kit/
├── src/
│   ├── Directory.Build.props          # 通用构建属性 (net10.0, TreatWarningsAsErrors, SonarAnalyzer)
│   ├── Directory.Packages.props       # 中心包版本管理 (ManagePackageVersionsCentrally)
│   ├── FSH.Starter.slnx              # 解决方案文件 (SDK 格式)
│   ├── BuildingBlocks/               # 基础设施层 (11 个项目)
│   │   ├── Core/                     # 实体基类、值对象、接口
│   │   ├── Shared/                   # 工具类
│   │   ├── Persistence/              # EF Core 仓储模式
│   │   ├── Web/                      # 中间件、Filters、模块抽象
│   │   ├── Caching/                  # Redis 缓存
│   │   ├── Eventing/                 # 事件总线
│   │   ├── Eventing.Abstractions/    # 事件接口
│   │   ├── Storage/                  # S3/MinIO 对象存储
│   │   ├── Jobs/                     # Hangfire 后台任务
│   │   ├── Mailing/                  # 邮件发送
│   │   └── Quota/                    # 配额管理
│   ├── Modules/                      # 业务模块层 (11 个模块)
│   │   ├── Identity/                 # 认证授权 (保留)
│   │   ├── Multitenancy/             # 多租户 (保留)
│   │   ├── Auditing/                 # 审计日志 (保留)
│   │   ├── Files/                    # 文件管理 (保留)
│   │   ├── Webhooks/                 # Webhook (保留)
│   │   ├── Notifications/            # 通知 (保留)
│   │   ├── Catalog/                  # 目录 (禁用 - D-02)
│   │   ├── Tickets/                  # 工单 (禁用 - D-02)
│   │   ├── Chat/                     # 聊天 (禁用 - D-02)
│   │   ├── Billing/                  # 计费 (禁用 - D-02)
│   │   └── ...
│   ├── Host/                         # 宿主层
│   │   ├── FSH.Starter.Api/          # API 项目 (Minimal API)
│   │   ├── FSH.Starter.DbMigrator/   # 数据库迁移控制台
│   │   ├── FSH.Starter.AppHost/      # .NET Aspire 编排
│   │   └── FSH.Starter.Migrations.PostgreSQL/  # EF Core 迁移
│   ├── Tests/                        # 测试项目
│   └── Tools/                        # CLI 工具
├── clients/                          # 前端客户端
│   ├── admin/                        # React + Vite 管理端
│   └── dashboard/                    # React + Vite 仪表板
├── NuGet.Config                      # NuGet 源配置 (华为云镜像)
├── .editorconfig                     # 代码风格
└── AGENTS.md                         # 编码规范 & 10 条黄金法则
```

### 1.2 命名空间约定

| 层级              | 模板命名空间                        | 目标命名空间 (D-05)             |
| ----------------- | ----------------------------------- | ------------------------------- |
| BuildingBlocks    | `FSH.Framework.{Name}`              | `YH.Framework.{Name}`           |
| Modules           | `FSH.Modules.{Name}`                | `YH.Modules.{Name}`             |
| Host (API)        | `FSH.Starter.Api`                   | `YH.Flow.Api`                   |
| Host (DbMigrator) | `FSH.Starter.DbMigrator`            | `YH.Flow.DbMigrator`            |
| Host (AppHost)    | `FSH.Starter.AppHost`               | `YH.Flow.AppHost`               |
| Host (Migrations) | `FSH.Starter.Migrations.PostgreSQL` | `YH.Flow.Migrations.PostgreSQL` |

### 1.3 构建系统

- **Directory.Build.props** 位于 `src/` 根目录，定义全局属性：
  - `TargetFramework=net10.0`
  - `TreatWarningsAsErrors=true`
  - `EnforceCodeStyleInBuild=true`
  - `AnalysisMode=AllEnabledByDefault`
  - SonarAnalyzer.CSharp 全局引用
  - `IsPackable=false`（默认不打包，仅 CLI 项目启用）

- **Directory.Packages.props** 位于 `src/`，中心包管理：
  - `ManagePackageVersionsCentrally=true`
  - Aspire 13.4.0 系列
  - FluentValidation 12.1.1
  - OpenTelemetry 1.15.x
  - Npgsql 10.0.2

- **NuGet.Config** 位于根目录（注意：实际在根目录而非 src/）：
  - 华为云镜像源优先
  - globalPackagesFolder 指向 `F:\nuget\packages`

---

## 2. Aspire 编排详解 (AppHost.cs)

### 2.1 资源清单

| 资源        | 类型                            | 端口      | 生命周期   | 备注                            |
| ----------- | ------------------------------- | --------- | ---------- | ------------------------------- |
| postgres    | AddPostgres                     | 默认      | Persistent | 含 pgAdmin :5050                |
| fsh-db      | PostgreSQL DB                   | -         | -          | 数据库名 → `yhflow-db` (D-12)   |
| redis       | Container (valkey/valkey:9.1.0) | 6379      | Persistent | 含 RedisInsight :5540           |
| minio       | Container (minio/minio)         | 9000/9001 | Persistent | Bucket: `yhflow-uploads` (D-12) |
| minio-init  | Container (minio/mc)            | -         | -          | 初始化 bucket                   |
| db-migrator | Project (DbMigrator)            | -         | -          | 迁移 + 种子数据                 |
| demo-seeder | Project (DbMigrator)            | -         | -          | 演示租户 (acme/globex)          |
| api         | Project (Api)                   | -         | -          | 主 API 服务                     |
| admin       | JavaScriptApp                   | :5173     | -          | 前端管理端 (D-11: 注释掉)       |
| dashboard   | JavaScriptApp                   | :5174     | -          | 前端仪表板 (D-11: 注释掉)       |

### 2.2 编排依赖链

```
postgres ─────────┬──→ db-migrator ──→ demo-seeder ──┐
                  │                                   │
redis ────────────┤                                   ├──→ api
                  │                                   │
minio ──→ minio-init ─────────────────────────────────┘
```

### 2.3 关键环境变量

API 服务接收以下环境变量（通过 WithEnvironment 注入）：

- `DatabaseOptions__Provider=POSTGRESQL`
- `DatabaseOptions__ConnectionString` (来自 postgres)
- `DatabaseOptions__MigrationsAssembly=FSH.Starter.Migrations.PostgreSQL` → 需改为 `YH.Flow.Migrations.PostgreSQL`
- `CachingOptions__Redis` (来自 redis)
- `Storage__Provider=s3` + S3 配置 (来自 minio)
- `HangfireOptions__UserName/Password`
- `MailOptions__*` (Ethereal 测试邮件)

---

## 3. 模块禁用策略 (D-01, D-02)

### 3.1 API Program.cs 中的模块注册

模板的 `Program.cs` 通过两个地方注册模块：

**1. Mediator 注册 (AddMediator):**

```csharp
builder.Services.AddMediator(o => {
    o.Assemblies = [
        // ...保留的模块...
        typeof(FSH.Modules.Billing.Contracts.BillingContractsMarker),
        typeof(FSH.Modules.Billing.BillingModule),
        typeof(FSH.Modules.Catalog.Contracts.CatalogContractsMarker),
        typeof(FSH.Modules.Catalog.CatalogModule),
        typeof(FSH.Modules.Tickets.Contracts.TicketsContractsMarker),
        typeof(FSH.Modules.Tickets.TicketsModule),
        typeof(FSH.Modules.Chat.Contracts.v1.Commands.CreateChannelCommand),
        typeof(FSH.Modules.Chat.ChatModule),
        // ...
    ];
});
```

**2. 模块程序集注册 (AddModules):**

```csharp
var moduleAssemblies = new Assembly[] {
    // ...保留的模块...
    typeof(BillingModule).Assembly,
    typeof(CatalogModule).Assembly,
    typeof(TicketsModule).Assembly,
    typeof(FSH.Modules.Chat.ChatModule).Assembly,
    // ...
};
builder.AddModules(moduleAssemblies);
```

### 3.2 需要移除的 using 语句和注册

按 D-02 要求，需要移除以下 4 个模块的 DI 注册：

| 模块    | using 语句                                                        | AddMediator 条目                          | AddModules 条目                  |
| ------- | ----------------------------------------------------------------- | ----------------------------------------- | -------------------------------- |
| Catalog | `using FSH.Modules.Catalog;`                                      | `CatalogContractsMarker`, `CatalogModule` | `typeof(CatalogModule).Assembly` |
| Tickets | `using FSH.Modules.Tickets;`                                      | `TicketsContractsMarker`, `TicketsModule` | `typeof(TicketsModule).Assembly` |
| Chat    | `using FSH.Modules.Chat;` + `using FSH.Modules.Chat.Contracts...` | `CreateChannelCommand`, `ChatModule`      | `typeof(ChatModule).Assembly`    |
| Billing | `using FSH.Modules.Billing;`                                      | `BillingContractsMarker`, `BillingModule` | `typeof(BillingModule).Assembly` |

**重要：** 项目文件 (.csproj) 保留在解决方案中参与编译（D-02），只移除运行时注册。

---

## 4. 重命名策略 (D-07, D-08)

### 4.1 需要替换的范围

| 类别                  | 具体文件/内容                          | 说明                                                                  |
| --------------------- | -------------------------------------- | --------------------------------------------------------------------- |
| csproj                | AssemblyName, RootNamespace            | 所有 30+ 个 .csproj 文件                                              |
| .cs                   | 所有 namespace 声明                    | `FSH.` → `YH.`                                                        |
| .cs                   | using 语句                             | `using FSH.` → `using YH.`                                            |
| launchSettings.json   | 应用名称                               | 如有引用                                                              |
| appsettings\*.json    | 配置键中的引用                         | `FSH.Starter.Migrations.PostgreSQL` → `YH.Flow.Migrations.PostgreSQL` |
| AppHost.cs            | `Projects.FSH_Starter_*` 类型引用      | Aspire 自动生成的类型名                                               |
| AppHost.cs            | 环境变量值                             | `FSH.Starter.Migrations.PostgreSQL` → `YH.Flow.Migrations.PostgreSQL` |
| AppHost.cs            | `appPrefix` 计算逻辑                   | 自动从 AssemblyName 推导，重命名后自动生效                            |
| Dockerfile            | 标签                                   | 如有引用                                                              |
| README                | 项目名称引用                           | 全文替换                                                              |
| .slnx                 | 项目名称和路径                         | 解决方案文件中的引用                                                  |
| Directory.Build.props | Authors, Company, PackageTags 等元数据 | FSH → YH                                                              |

### 4.2 PowerShell 脚本方案

建议创建 `scripts/rename-fsh-to-yh.ps1`，分 4 个阶段：

**阶段 1: 目录重命名**

- `src/BuildingBlocks/` 下各 .csproj 中的 `<RootNamespace>` 和 `<AssemblyName>`
- `src/Modules/` 下各 .csproj
- `src/Host/` 下各项目：`FSH.Starter.Api` → `YH.Flow.Api` 等

**阶段 2: 文件内容替换**

- 递归搜索 `*.cs`, `*.csproj`, `*.json`, `*.props`, `*.slnx`, `*.md`
- 正则替换 `FSH\.` → `YH.` (注意 `FSH.` 作为命名空间前缀)
- 正则替换 `FSH_` → `YH_` (Aspire 生成的 `Projects.FSH_Starter_*` 类型)
- 正则替换 `FSH-Starter` → `YH-Flow` (appPrefix 推导: `fsh-starter` → `yh-flow`)
- 替换 `fullstackhero` / `FullStackHero` → 项目名 (在元数据中)

**阶段 3: 目录物理重命名**

- 将 `FSH.Starter.Api/` 目录重命名为 `YH.Flow.Api/`
- 将 `FSH.Starter.AppHost/` 目录重命名为 `YH.Flow.AppHost/`
- 将 `FSH.Starter.DbMigrator/` 目录重命名为 `YH.Flow.DbMigrator/`
- 将 `FSH.Starter.Migrations.PostgreSQL/` 目录重命名为 `YH.Flow.Migrations.PostgreSQL/`

**阶段 4: 验证**

- `dotnet build` 确保编译通过
- 检查所有 `FSH` 引用是否清除

### 4.3 Aspire 类型名注意

AppHost.cs 中使用的 `Projects.FSH_Starter_Api` 和 `Projects.FSH_Starter_DbMigrator` 是 Aspire SDK 根据项目名自动生成的。重命名项目目录后，这些类型名会自动变为 `Projects.YH_Flow_Api` 和 `Projects.YH_Flow_DbMigrator`。需要在 AppHost.cs 中同步更新这些引用。

---

## 5. 解决方案文件 (D-06)

- 当前: `FSH.Starter.slnx` (SDK 格式)
- 目标: `YH.Flow.slnx`
- slnx 是 XML 格式，需要更新其中的项目名称和路径引用
- 需要更新所有 ProjectReference 路径

---

## 6. 前端客户端处理 (D-11, D-13)

### 6.1 当前结构

- `clients/admin/` — 完整的 React + Vite 管理端
- `clients/dashboard/` — 完整的 React + Vite 仪表板

### 6.2 Phase 0 处理

- **D-11:** 在 AppHost.cs 中注释掉 `AddJavaScriptApp` 调用（模板已有 `#if (frontend)` 条件编译）
- **D-13:** 保留 `clients/` 目录结构，每个客户端仅保留 `package.json`
- **D-14:** 实际的 Flow Web 前端在 Phase 13 于 `clients/web/` 实现

### 6.3 MinIO CORS 配置

AppHost.cs 中设置了 CORS:

```csharp
const string AdminOrigin = "http://localhost:5173";
const string DashboardOrigin = "http://localhost:5174";
```

Phase 0 可暂时保留这些值，Phase 13 时更新为新的前端端口。

---

## 7. 关键风险和注意事项

### 7.1 编译依赖

- 移除 DI 注册后，被禁用模块的代码仍在编译范围内
- 如果模块间有编译依赖（如 Catalog 引用 Core），需确保 BuildingBlocks 的引用链完整
- 被禁用模块的 `using` 语句必须从 Program.cs 移除，否则编译警告（未使用的 using）

### 7.2 Aspire 类型安全

- AppHost.cs 中的 `Projects.FSH_Starter_*` 类型是编译时生成的
- 目录重命名后必须清理 `obj/` 目录并重新编译，否则缓存的旧类型名会导致编译错误

### 7.3 NuGet.Config 路径

- 当前 NuGet.Config 位于仓库根目录（非 src/），包含华为云镜像和全局包路径配置
- 复制时需确认路径 `F:\nuget\packages` 在目标机器上可用，或改为可移植配置

### 7.4 项目物理位置

- 目标位置: `d:/github/akinix-plane/yh-flow/`
- 模板位置: `D:/github/fullstackhero-dotnet-starter-kit/`
- 两者为独立目录，不涉及 git submodule

---

## 8. 验证清单

Phase 0 完成时的验证步骤:

1. `dotnet build YH.Flow.slnx` — 零错误零警告编译通过
2. `dotnet run --project src/Host/YH.Flow.AppHost` — Aspire 编排启动
3. pgAdmin 可访问 `http://localhost:5050`
4. RedisInsight 可访问 `http://localhost:5540`
5. MinIO 控制台可访问 `http://localhost:9001`
6. API 健康检查 `http://localhost:xxxx/` 返回 200
7. 数据库中演示租户已创建 (acme/globex)
8. `grep -r "FSH\." src/` — 无残留的 FSH 命名空间引用

---

## RESEARCH COMPLETE

Phase 0 研究完成。已覆盖模板结构、Aspire 编排、模块禁用策略、重命名方案、风险点。
