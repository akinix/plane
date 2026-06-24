---
phase: "00"
plan: "03"
type: execute
wave: 3
depends_on:
  - "02"
files_modified:
  - "yh-flow/**/*"
autonomous: false
requirements:
  - "ROADMAP T0.8"
  - "CONTEXT deliverables: 可编译的 YH.Flow 解决方案, Aspire 编排可启动所有基础设施"
---

# Plan 03: 端到端验证

<objective>
验证 YH.Flow 解决方案可编译运行，Aspire 编排可启动所有基础设施服务，
确认所有 Phase 0 交付物满足验收标准。
</objective>

<tasks>

## Task 3.1: 完整编译验证

<type>execute</type>
<files>
<file>yh-flow/YH.Flow.slnx</file>
</files>
<read_first>

- yh-flow/YH.Flow.slnx (解决方案文件)
- yh-flow/src/Directory.Build.props (构建配置)
  </read_first>
  <action>

1. 清理所有编译缓存:
   - 删除所有 obj/ 和 bin/ 目录
2. 执行完整恢复和编译:
   - dotnet restore YH.Flow.slnx
   - dotnet build YH.Flow.slnx --no-restore
3. 确认编译输出:
   - "Build succeeded"
   - 零错误
   - 记录警告数量 (应尽可能少)
4. 验证无 FSH 残留:
   - grep -r "FSH\." yh-flow/src/ --include="_.cs" --include="_.csproj" --include="\*.json" | grep -v obj | grep -v bin
   - 应为空结果
     </action>
     <verify>
5. dotnet build 退出码 0
6. 输出包含 "Build succeeded"
7. grep "FSH\." 返回空
   </verify>
   <acceptance_criteria>

- 命令 dotnet build yh-flow/YH.Flow.slnx 退出码为 0
- 命令 dotnet build yh-flow/YH.Flow.slnx 输出包含 "Build succeeded"
- 命令 grep -r "FSH\." yh-flow/src/ --include="_.cs" --include="_.csproj" --include="\*.json" | grep -v obj | grep -v bin 返回空结果 (无 FSH 残留)
- 编译包含所有 BuildingBlocks 项目 (11 个)
- 编译包含所有 Modules 项目 (包括禁用模块 — D-02)
- 编译包含 4 个 Host 项目 (YH.Flow.Api, YH.Flow.DbMigrator, YH.Flow.AppHost, YH.Flow.Migrations.PostgreSQL)
  </acceptance_criteria>

## Task 3.2: Aspire 编排启动验证

<type>execute</type>
<files>
<file>yh-flow/src/Host/YH.Flow.AppHost/</file>
</files>
<read_first>

- yh-flow/src/Host/YH.Flow.AppHost/AppHost.cs (编排配置)
- .planning/phases/00-init/00-CONTEXT.md (D-09: 编排服务清单)
  </read_first>
  <action>

1. 在 yh-flow/src/Host/YH.Flow.AppHost/ 目录执行:
   dotnet run
2. 等待 Aspire Dashboard 启动 (通常 http://localhost:18888)
3. 验证所有容器和服务启动:
   - PostgreSQL 容器运行中
   - pgAdmin 可访问 http://localhost:5050
   - Valkey/Redis 容器运行中
   - RedisInsight 可访问 http://localhost:5540
   - MinIO 容器运行中
   - MinIO 控制台可访问 http://localhost:9001
   - MinIO bucket "yhflow-uploads" 已创建 (init 容器执行)
   - DbMigrator 执行完成 (数据库迁移 + 种子数据)
   - Demo Seeder 执行完成 (acme/globex 租户)
   - API 服务启动
4. 验证 API 健康:
   - curl http://localhost:xxxx/ 返回 200 + {"message":"hello world!"}
5. 停止 Aspire: Ctrl+C
   </action>
   <verify>
6. Aspire Dashboard 可访问
7. 所有容器在 Dashboard 中显示为 "Running"
8. API 健康检查返回 200
9. pgAdmin, RedisInsight, MinIO Console 端口可访问
   </verify>
   <acceptance_criteria>

- 命令 cd yh-flow/src/Host/YH.Flow.AppHost && dotnet run 成功启动 Aspire Dashboard
- Aspire Dashboard 中显示以下资源状态为 Running: postgres, redis, redis-insight, minio, minio-init, yh-flow-db-migrator, yh-flow-demo-seeder, yh-flow-api
- pgAdmin 端口 5050 可访问 (curl http://localhost:5050 返回 HTTP 响应)
- RedisInsight 端口 5540 可访问 (curl http://localhost:5540 返回 HTTP 响应)
- MinIO 控制台端口 9001 可访问 (curl http://localhost:9001 返回 HTTP 响应)
- API 根路径返回 HTTP 200 和 JSON 响应包含 "hello world"
- 数据库中已创建演示租户 (通过 DbMigrator seed-demo 执行完成确认)
  </acceptance_criteria>

## Task 3.3: 最终清理与文档

<type>execute</type>
<files>
<file>yh-flow/AGENTS.md</file>
<file>yh-flow/CLAUDE.md</file>
</files>
<read_first>

- yh-flow/AGENTS.md (模板编码规范)
- .planning/phases/00-init/00-CONTEXT.md (项目上下文)
  </read_first>
  <action>

1. 更新 yh-flow/AGENTS.md (如存在):
   - 替换 FSH/fullstackhero 引用为 YH/YH.Flow
   - 保留 10 条黄金法则和编码规范内容

2. 创建或更新 yh-flow/CLAUDE.md:
   - 项目名称: YH.Flow
   - 模板来源: fullstackhero/dotnet-starter-kit
   - 构建命令: dotnet build YH.Flow.slnx
   - 运行命令: cd src/Host/YH.Flow.AppHost && dotnet run
   - 项目结构简述

3. 确认 yh-flow/scripts/rename-fsh-to-yh.ps1 保留在项目中 (D-08)

4. 确认 .editorconfig 保留 (模板代码规范)
   </action>
   <verify>
5. AGENTS.md 不包含 FSH/fullstackhero 的品牌引用 (或已替换为 YH)
6. CLAUDE.md 存在且包含项目基本信息
7. scripts/rename-fsh-to-yh.ps1 存在
8. .editorconfig 存在
   </verify>
   <acceptance_criteria>

- yh-flow/scripts/rename-fsh-to-yh.ps1 文件存在 (D-08: 脚本保留)
- yh-flow/.editorconfig 文件存在 (保留模板代码规范)
- yh-flow/CLAUDE.md 文件存在且包含 "YH.Flow" 字符串
- yh-flow/AGENTS.md 存在 (模板编码规范保留)
  </acceptance_criteria>

</tasks>

<verification>
Phase 0 完成时，以下所有条件必须满足:

1. 编译验证:
   - dotnet build YH.Flow.slnx 零错误通过
   - 无 FSH 命名空间残留

2. Aspire 编排验证:
   - PostgreSQL + pgAdmin (:5050) 运行
   - Redis/Valkey + RedisInsight (:5540) 运行
   - MinIO (:9000/:9001) 运行，bucket "yhflow-uploads" 已创建
   - DbMigrator 执行完成
   - Demo Seeder 执行完成
   - API 服务启动并响应健康检查

3. 文件完整性:
   - 所有 BuildingBlocks 保留 (D-03)
   - 所有 Modules 代码保留在磁盘 (D-01)
   - 禁用模块 DI 注册已移除 (D-02)
   - 重命名脚本保留 (D-08)
   - 解决方案为 YH.Flow.slnx (D-06)
     </verification>

<success_criteria>

- dotnet build YH.Flow.slnx 零错误
- Aspire 编排启动所有基础设施 (PostgreSQL, Redis, MinIO)
- API 服务健康检查通过
- Demo 数据已种子
- 零 FSH 命名空间残留
- 所有决策 (D-01 到 D-14) 已实施
  </success_criteria>

<must_haves>

- 可编译的 YH.Flow 解决方案 (零错误 dotnet build)
- Aspire 编排可启动 PostgreSQL + pgAdmin (D-09)
- Aspire 编排可启动 Redis/Valkey + RedisInsight (D-09)
- Aspire 编排可启动 MinIO + bucket 初始化 (D-09, D-12)
- DbMigrator 执行数据库迁移和种子数据 (D-10)
- API 服务响应健康检查
- 零 FSH 命名空间残留 (D-07)
- 重命名脚本保留在项目中 (D-08)
- 解决方案文件为 YH.Flow.slnx (D-06)
  </must_haves>

<threat_model>
Phase 0 为纯脚手架阶段，不涉及运行时安全威胁。
验证阶段使用开发环境默认凭据，仅限本地使用:

- Hangfire Dashboard: admin / Password123!
- Seed Admin: admin@root.com / 123Pa$$word!
- MinIO: minioadmin / minioadmin
- Ethereal SMTP: 测试用一次性凭据
  这些凭据不应在生产环境使用，Phase 1+ 会配置安全选项。
  </threat_model>
