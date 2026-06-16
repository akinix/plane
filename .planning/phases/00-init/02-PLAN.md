---
phase: "00"
plan: "02"
type: execute
wave: 2
depends_on:
  - "01"
files_modified:
  - "yh-flow/src/Host/YH.Flow.AppHost/AppHost.cs"
  - "yh-flow/src/Host/YH.Flow.Api/Program.cs"
  - "yh-flow/YH.Flow.slnx"
autonomous: false
requirements:
  - "ROADMAP T0.2, T0.6, T0.7"
  - "CONTEXT D-02, D-04, D-06, D-09, D-10, D-11, D-12"
---

# Plan 02: Aspire 编排配置与模块 DI 清理

<objective>
配置 AppHost.cs 中的 Aspire 编排服务（PostgreSQL, Redis, MinIO），
移除禁用模块（Catalog, Tickets, Chat, Billing）的 DI 注册，
重命名解决方案文件并验证项目引用完整性。
</objective>

<tasks>

## Task 2.1: 更新 AppHost.cs 中的资源命名和端口配置

<type>execute</type>
<files>
  <file>yh-flow/src/Host/YH.Flow.AppHost/AppHost.cs</file>
</files>
<read_first>
  - yh-flow/src/Host/YH.Flow.AppHost/AppHost.cs (当前内容，Wave 1 已部分替换)
  - .planning/phases/00-init/00-CONTEXT.md (D-09, D-12: 编排服务和资源命名)
  - .planning/phases/00-init/00-RESEARCH.md (Section 2: Aspire 编排详解)
</read_first>
<action>
在 AppHost.cs 中确认以下配置已正确 (Wave 1 的替换脚本应已处理大部分):

1. PostgreSQL 配置:
   - 数据库名: "yhflow-db" (原 "fsh-db")
   - pgAdmin 端口: 5050 (保持不变)
   - 数据卷: "{appPrefix}-postgres-data" → 重命名后自动变为 "yh-flow-postgres-data"

2. Redis/Valkey 配置:
   - 容器镜像: valkey/valkey:9.1.0 (保持不变)
   - RedisInsight 端口: 5540 (保持不变)

3. MinIO 配置:
   - Bucket 名: "yhflow-uploads" (原 "fsh-uploads")
   - API 端口: 9000, Console 端口: 9001 (保持不变)
   - CORS: 保留 AdminOrigin 和 DashboardOrigin (Phase 13 更新)

4. 前端引用注释 (D-11):
   - 确认 #if (frontend) 条件编译块中的 admin 和 dashboard AddJavaScriptApp 调用被注释掉
   - 或在 AppHost.cs 中移除 #if (frontend) 条件，直接注释掉前端部分

5. DbMigrator 环境变量:
   - DatabaseOptions__MigrationsAssembly 值应为 "YH.Flow.Migrations.PostgreSQL"
   - Seed__DefaultAdminPassword 保持 "123Pa$$word!"
</action>
<verify>
1. AppHost.cs 包含 "yhflow-db" 字符串
2. AppHost.cs 包含 "yhflow-uploads" 字符串
3. AppHost.cs 包含 Projects.YH_Flow_Api (非 Projects.FSH_Starter_Api)
4. AppHost.cs 包含 Projects.YH_Flow_DbMigrator
5. AppHost.cs 包含 "YH.Flow.Migrations.PostgreSQL"
6. 前端 JS App 引用被注释掉或排除
</verify>
<acceptance_criteria>
- yh-flow/src/Host/YH.Flow.AppHost/AppHost.cs 包含字符串 "yhflow-db"
- yh-flow/src/Host/YH.Flow.AppHost/AppHost.cs 包含字符串 "yhflow-uploads"
- yh-flow/src/Host/YH.Flow.AppHost/AppHost.cs 包含 Projects.YH_Flow_Api 引用
- yh-flow/src/Host/YH.Flow.AppHost/AppHost.cs 包含 Projects.YH_Flow_DbMigrator 引用
- yh-flow/src/Host/YH.Flow.AppHost/AppHost.cs 包含 "YH.Flow.Migrations.PostgreSQL" 作为 DatabaseOptions__MigrationsAssembly 值
- AppHost.cs 中前端 AddJavaScriptApp 调用被注释或排除 (D-11)
- AppHost.cs 中 pgAdmin 端口为 5050
- AppHost.cs 中 RedisInsight 端口为 5540
- AppHost.cs 中 MinIO 端口为 9000 和 9001
</acceptance_criteria>

## Task 2.2: 移除禁用模块的 DI 注册 (D-02)

<type>execute</type>
<files>
  <file>yh-flow/src/Host/YH.Flow.Api/Program.cs</file>
</files>
<read_first>
  - yh-flow/src/Host/YH.Flow.Api/Program.cs (当前内容，Wave 1 已替换命名空间)
  - .planning/phases/00-init/00-CONTEXT.md (D-02: 仅移除 DI 注册，项目保留)
  - .planning/phases/00-init/00-RESEARCH.md (Section 3: 模块禁用策略)
</read_first>
<action>
在 Program.cs 中移除 Catalog, Tickets, Chat, Billing 四个模块的 DI 注册:

1. 移除 using 语句:
   - using YH.Modules.Catalog;
   - using YH.Modules.Tickets;
   - using YH.Modules.Chat;
   - using YH.Modules.Chat.Contracts.v1.Commands; (如有)
   - using YH.Modules.Billing;

2. 移除 AddMediator 注册 (Assemblies 列表中的以下条目):
   - typeof(YH.Modules.Billing.Contracts.BillingContractsMarker)
   - typeof(YH.Modules.Billing.BillingModule)
   - typeof(YH.Modules.Catalog.Contracts.CatalogContractsMarker)
   - typeof(YH.Modules.Catalog.CatalogModule)
   - typeof(YH.Modules.Tickets.Contracts.TicketsContractsMarker)
   - typeof(YH.Modules.Tickets.TicketsModule)
   - typeof(YH.Modules.Chat.Contracts.v1.Commands.CreateChannelCommand)
   - typeof(YH.Modules.Chat.ChatModule)

3. 移除 AddModules 注册 (moduleAssemblies 数组中的以下条目):
   - typeof(BillingModule).Assembly
   - typeof(CatalogModule).Assembly
   - typeof(TicketsModule).Assembly
   - typeof(YH.Modules.Chat.ChatModule).Assembly

注意: 保留所有其他模块 (Identity, Multitenancy, Auditing, Files, Webhooks, Notifications)
注意: 项目文件 (.csproj) 保留在解决方案中，仅移除运行时注册
</action>
<verify>
1. Program.cs 不包含 "using YH.Modules.Catalog" 或 "using FSH.Modules.Catalog"
2. Program.cs 不包含 "using YH.Modules.Tickets" 或 "using FSH.Modules.Tickets"
3. Program.cs 不包含 "using YH.Modules.Chat" 或 "using FSH.Modules.Chat"
4. Program.cs 不包含 "using YH.Modules.Billing" 或 "using FSH.Modules.Billing"
5. Program.cs 不包含 CatalogModule, TicketsModule, ChatModule, BillingModule 的任何引用
</verify>
<acceptance_criteria>
- yh-flow/src/Host/YH.Flow.Api/Program.cs 不包含字符串 "Catalog" (排除注释)
- yh-flow/src/Host/YH.Flow.Api/Program.cs 不包含字符串 "Tickets" (排除注释)
- yh-flow/src/Host/YH.Flow.Api/Program.cs 不包含字符串 "Chat" (排除注释)
- yh-flow/src/Host/YH.Flow.Api/Program.cs 不包含字符串 "Billing" (排除注释)
- yh-flow/src/Host/YH.Flow.Api/Program.cs 仍包含 IdentityModule 引用 (保留模块)
- yh-flow/src/Host/YH.Flow.Api/Program.cs 仍包含 MultitenancyModule 引用 (保留模块)
- yh-flow/src/Host/YH.Flow.Api/Program.cs 仍包含 AuditingModule 引用 (保留模块)
- yh-flow/src/Modules/Catalog/ 目录仍存在 (D-01: 代码保留在磁盘)
- yh-flow/src/Modules/Tickets/ 目录仍存在
- yh-flow/src/Modules/Chat/ 目录仍存在
- yh-flow/src/Modules/Billing/ 目录仍存在
</acceptance_criteria>

## Task 2.3: 验证解决方案文件 (D-06)

<type>execute</type>
<files>
  <file>yh-flow/YH.Flow.slnx</file>
</files>
<read_first>
  - yh-flow/YH.Flow.slnx (Wave 1 已重命名)
  - .planning/phases/00-init/00-CONTEXT.md (D-06: 解决方案文件命名)
</read_first>
<action>
验证 YH.Flow.slnx 文件内容:

1. 确认文件包含所有保留项目的引用:
   - 所有 BuildingBlocks 项目 (Core, Shared, Persistence, Web, Caching, Eventing, etc.)
   - 所有 Modules 项目 (包括被禁用的 Catalog, Tickets, Chat, Billing — D-02: 项目保留在 slnx 中)
   - Host 项目: YH.Flow.Api, YH.Flow.DbMigrator, YH.Flow.AppHost, YH.Flow.Migrations.PostgreSQL

2. 确认项目名称已更新:
   - FSH.Starter.Api → YH.Flow.Api
   - FSH.Starter.DbMigrator → YH.Flow.DbMigrator
   - FSH.Starter.AppHost → YH.Flow.AppHost
   - FSH.Starter.Migrations.PostgreSQL → YH.Flow.Migrations.PostgreSQL

3. 如果 slnx 中仍有 FSH 引用，手动修复

4. 确认 FSH.Starter.slnx 已不存在 (已重命名)
</action>
<verify>
1. YH.Flow.slnx 包含 YH.Flow.Api 项目引用
2. YH.Flow.slnx 包含 YH.Flow.AppHost 项目引用
3. YH.Flow.slnx 不包含 FSH.Starter 前缀
4. FSH.Starter.slnx 不存在
</verify>
<acceptance_criteria>
- yh-flow/YH.Flow.slnx 文件存在
- yh-flow/FSH.Starter.slnx 文件不存在 (已重命名)
- YH.Flow.slnx 包含 "YH.Flow.Api" 字符串
- YH.Flow.slnx 包含 "YH.Flow.AppHost" 字符串
- YH.Flow.slnx 包含 "YH.Flow.DbMigrator" 字符串
- YH.Flow.slnx 包含 "YH.Flow.Migrations.PostgreSQL" 字符串
- YH.Flow.slnx 不包含 "FSH.Starter" 字符串
- YH.Flow.slnx 包含 Catalog 模块项目引用 (D-02: 项目保留在解决方案中)
</acceptance_criteria>

## Task 2.4: 编译修复 (如有错误)

<type>execute</type>
<files>
  <file>yh-flow/src/**/*.cs</file>
  <file>yh-flow/src/**/*.csproj</file>
</files>
<read_first>
  - yh-flow/YH.Flow.slnx (解决方案结构)
  - Wave 1 Task 1.4 的编译输出 (已知错误列表)
</read_first>
<action>
1. 在 yh-flow/src/ 执行: dotnet build YH.Flow.slnx
2. 分析编译错误:
   - 如果是命名空间引用错误 → 修复 using 语句
   - 如果是项目引用路径错误 → 修复 .csproj 中的 ProjectReference
   - 如果是 AppHost 类型引用错误 → 修复 Projects.YH_Flow_* 引用
   - 如果是模块间引用错误 → 确认被引用模块的命名空间已正确更新
3. 逐个修复编译错误
4. 重新编译直到零错误
</action>
<verify>
1. dotnet build YH.Flow.slnx 输出 "Build succeeded"
2. 零编译错误
</verify>
<acceptance_criteria>
- 命令 dotnet build yh-flow/YH.Flow.slnx 输出包含 "Build succeeded"
- 命令 dotnet build yh-flow/YH.Flow.slnx 退出码为 0
- 编译输出中 error 数量为 0
</acceptance_criteria>

</tasks>

<verification>
1. AppHost.cs 正确配置所有基础设施服务
2. 禁用模块的 DI 注册已移除
3. 解决方案文件包含正确的项目引用
4. 项目编译通过零错误
</verification>

<success_criteria>
- Aspire 编排包含 PostgreSQL, Redis, MinIO (D-09)
- 数据库名为 yhflow-db (D-12)
- MinIO bucket 为 yhflow-uploads (D-12)
- Catalog, Tickets, Chat, Billing 的 DI 注册已移除 (D-02)
- 禁用模块的项目文件保留在磁盘和解决方案中 (D-01)
- 解决方案文件为 YH.Flow.slnx (D-06)
- dotnet build 零错误通过
</success_criteria>

<must_haves>
- PostgreSQL + pgAdmin :5050 配置正确 (D-09)
- Redis/Valkey + RedisInsight :5540 配置正确 (D-09)
- MinIO :9000/:9001 配置正确 (D-09)
- 不含 RabbitMQ (D-09)
- Demo Seeder 保留 (D-10)
- 前端 JS App 引用注释掉 (D-11)
- 数据库名 yhflow-db (D-12)
- Bucket 名 yhflow-uploads (D-12)
- Program.cs 不含 Catalog/Tickets/Chat/Billing 引用 (D-02)
- 禁用模块代码保留在磁盘 (D-01)
- 编译零错误
</must_haves>

<threat_model>
Phase 0 为纯脚手架阶段，不涉及运行时安全威胁。
- 开发环境凭据 (Hangfire, Seed) 从模板继承，仅用于本地开发
- 无外部端点暴露
- 安全配置在 Phase 1 实现
</threat_model>
