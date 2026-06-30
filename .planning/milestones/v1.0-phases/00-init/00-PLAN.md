---
phase: "00"
plan: "01"
type: execute
wave: 1
depends_on: []
files_modified:
  - "yh-flow/**/*"
  - "scripts/rename-fsh-to-yh.ps1"
autonomous: false
requirements:
  - "ROADMAP T0.1, T0.3, T0.4, T0.5"
  - "CONTEXT D-01, D-03, D-05, D-07, D-08"
---

# Plan 01: 模板复制与命名空间重命名

<objective>
将 fullstackhero 模板复制到 yh-flow/ 目录，执行全面的 FSH → YH 命名空间替换，
创建 PowerShell 重命名脚本，配置构建基础设施（Directory.Build.props, Directory.Packages.props, NuGet.Config）。
</objective>

<tasks>

## Task 1.1: 复制模板到 yh-flow/

<type>execute</type>
<files>
<file>yh-flow/ (target directory)</file>
<file>D:/github/fullstackhero-dotnet-starter-kit/ (source template)</file>
</files>
<read_first>

- D:/github/fullstackhero-dotnet-starter-kit/src/ (source structure)
- .planning/phases/00-init/00-CONTEXT.md (D-01: 保留所有模块代码)
  </read_first>
  <action>

1. 将 D:/github/fullstackhero-dotnet-starter-kit/ 整个目录复制到 d:/github/akinix-plane/yh-flow/
2. 排除: .git/, .vs/, bin/, obj/, node_modules/ 目录
3. 保留: src/, clients/, NuGet.Config, .editorconfig, AGENTS.md, .gitignore
4. 复制完成后确认目录结构完整: src/BuildingBlocks/, src/Modules/, src/Host/, clients/
5. (D-13) 精简 clients/ 目录: 删除 clients/admin/ 和 clients/dashboard/ 中除 package.json 外的所有文件和目录
   - 保留: clients/admin/package.json, clients/dashboard/package.json
   - 删除: clients/admin/src/, clients/admin/node_modules/, clients/admin/public/ 等
   - 删除: clients/dashboard/src/, clients/dashboard/node_modules/, clients/dashboard/public/ 等
     </action>
     <verify>
6. ls yh-flow/src/BuildingBlocks/ 应包含 11 个子目录
7. ls yh-flow/src/Modules/ 应包含 11 个子目录
8. ls yh-flow/src/Host/ 应包含 4 个子目录
9. yh-flow/NuGet.Config 存在
10. yh-flow/src/Directory.Build.props 存在
11. yh-flow/clients/admin/package.json 存在
12. yh-flow/clients/dashboard/package.json 存在
13. yh-flow/clients/admin/src/ 不存在 (已删除)
    </verify>
    <acceptance_criteria>

- yh-flow/src/BuildingBlocks/ 包含 Core, Shared, Persistence, Web, Caching, Eventing, Eventing.Abstractions, Storage, Jobs, Mailing, Quota 共 11 个目录
- yh-flow/src/Modules/ 包含 Identity, Multitenancy, Auditing, Files, Webhooks, Notifications, Catalog, Tickets, Chat, Billing 共 10+ 个目录
- yh-flow/src/Host/ 包含 FSH.Starter.Api, FSH.Starter.DbMigrator, FSH.Starter.AppHost, FSH.Starter.Migrations.PostgreSQL 共 4 个目录
- yh-flow/NuGet.Config 文件存在且包含 huawei 包源
- yh-flow/src/Directory.Build.props 文件存在且包含 TargetFramework net10.0
- yh-flow/clients/admin/package.json 文件存在 (D-13)
- yh-flow/clients/dashboard/package.json 文件存在 (D-13)
- yh-flow/clients/admin/ 目录中仅包含 package.json (无 src/, node_modules/ 等)
- yh-flow/clients/dashboard/ 目录中仅包含 package.json (无 src/, node_modules/ 等)
  </acceptance_criteria>

## Task 1.2: 创建 PowerShell 重命名脚本

<type>execute</type>
<files>
<file>yh-flow/scripts/rename-fsh-to-yh.ps1</file>
</files>
<read_first>

- .planning/phases/00-init/00-RESEARCH.md (Section 4: 重命名策略)
- .planning/phases/00-init/00-CONTEXT.md (D-05, D-07, D-08)
  </read_first>
  <action>
  在 yh-flow/scripts/ 目录创建 rename-fsh-to-yh.ps1 脚本，包含以下替换逻辑:

阶段 1 — csproj 文件内容替换:
递归搜索 yh-flow/src/\*_/_.csproj
替换: AssemblyName FSH. → YH.
替换: RootNamespace FSH. → YH.
替换: FSH.Starter.Api → YH.Flow.Api
替换: FSH.Starter.DbMigrator → YH.Flow.DbMigrator
替换: FSH.Starter.AppHost → YH.Flow.AppHost
替换: FSH.Starter.Migrations.PostgreSQL → YH.Flow.Migrations.PostgreSQL

阶段 2 — C# 源文件命名空间替换:
递归搜索 yh-flow/src/\*_/_.cs
替换: namespace FSH. → namespace YH.
替换: using FSH. → using YH.
替换: typeof(FSH. → typeof(YH.

阶段 3 — 配置文件替换:
搜索 appsettings\*.json, launchSettings.json
替换: FSH.Starter.Migrations.PostgreSQL → YH.Flow.Migrations.PostgreSQL

阶段 4 — 解决方案文件替换:
搜索 \*.slnx
替换: FSH.Starter → YH.Flow (项目名和路径)
重命名文件: FSH.Starter.slnx → YH.Flow.slnx

阶段 5 — 构建属性替换:
Directory.Build.props: Authors FullStackHero → YHFlow, Company → YHFlow
PackageTags FSH;FullStackHero → YH;YHFlow

阶段 6 — AppHost.cs 特殊处理:
替换: Projects.FSH_Starter_Api → Projects.YH_Flow_Api
替换: Projects.FSH_Starter_DbMigrator → Projects.YH_Flow_DbMigrator
替换: "fsh-db" → "yhflow-db"
替换: "fsh-uploads" → "yhflow-uploads"

阶段 7 — 目录物理重命名:
src/Host/FSH.Starter.Api/ → src/Host/YH.Flow.Api/
src/Host/FSH.Starter.DbMigrator/ → src/Host/YH.Flow.DbMigrator/
src/Host/FSH.Starter.AppHost/ → src/Host/YH.Flow.AppHost/
src/Host/FSH.Starter.Migrations.PostgreSQL/ → src/Host/YH.Flow.Migrations.PostgreSQL/
</action>
<verify>

1. 脚本文件存在于 yh-flow/scripts/rename-fsh-to-yh.ps1
2. 脚本包含 7 个阶段的替换逻辑
3. 脚本使用 PowerShell 语法 (Get-ChildItem, -Recurse, (Get-Content) -replace)
   </verify>
   <acceptance_criteria>

- yh-flow/scripts/rename-fsh-to-yh.ps1 文件存在
- 脚本包含 Get-ChildItem 递归搜索 \*.csproj 文件的逻辑
- 脚本包含 namespace FSH 到 YH 的替换
- 脚本包含 Projects.FSH_Starter_Api 到 Projects.YH_Flow_Api 的替换
- 脚本包含 "fsh-db" 到 "yhflow-db" 的替换
- 脚本包含 "fsh-uploads" 到 "yhflow-uploads" 的替换
- 脚本包含目录重命名命令 (Rename-Item) 将 FSH.Starter.Api 重命名为 YH.Flow.Api
- 脚本包含目录重命名命令将 FSH.Starter.AppHost 重命名为 YH.Flow.AppHost
  </acceptance_criteria>

## Task 1.3: 执行重命名脚本

<type>execute</type>
<files>
<file>yh-flow/scripts/rename-fsh-to-yh.ps1</file>
<file>yh-flow/src/**/\*.csproj</file>
<file>yh-flow/src/**/_.cs</file>
<file>yh-flow/src/\*\*/_.slnx</file>
</files>
<read_first>

- yh-flow/scripts/rename-fsh-to-yh.ps1 (脚本内容)
- .planning/phases/00-init/00-CONTEXT.md (D-05: 命名方案)
  </read_first>
  <action>

1. 在 yh-flow/ 目录下执行 PowerShell 脚本: powershell -File scripts/rename-fsh-to-yh.ps1
2. 脚本执行完成后，验证关键文件已更新:
   - 检查 yh-flow/src/Host/YH.Flow.Api/YH.Flow.Api.csproj 存在
   - 检查 yh-flow/src/Host/YH.Flow.AppHost/YH.Flow.AppHost.csproj 存在
   - 检查 YH.Flow.slnx 存在 (原 FSH.Starter.slnx 已重命名)
3. 验证无残留 FSH 引用: grep -r "FSH\." yh-flow/src/ --include="_.cs" --include="_.csproj" | grep -v "obj/" | grep -v "bin/"
4. 如果有残留引用，手动修复
   </action>
   <verify>
5. yh-flow/src/Host/YH.Flow.Api/ 目录存在
6. yh-flow/src/Host/YH.Flow.AppHost/ 目录存在
7. yh-flow/src/Host/YH.Flow.DbMigrator/ 目录存在
8. yh-flow/src/Host/YH.Flow.Migrations.PostgreSQL/ 目录存在
9. yh-flow/YH.Flow.slnx 存在
10. grep "FSH\." 在 .cs 和 .csproj 文件中无结果 (排除 obj/ 和 bin/)
    </verify>
    <acceptance_criteria>

- yh-flow/src/Host/YH.Flow.Api/YH.Flow.Api.csproj 文件存在且包含 RootNamespace YH.Flow.Api
- yh-flow/src/Host/YH.Flow.AppHost/YH.Flow.AppHost.csproj 文件存在且包含 RootNamespace YH.Flow.AppHost
- yh-flow/YH.Flow.slnx 文件存在
- 命令 grep -r "FSH\." yh-flow/src/ --include="_.cs" --include="_.csproj" | grep -v "obj/" | grep -v "bin/" 返回空结果
- yh-flow/src/Host/YH.Flow.AppHost/AppHost.cs 包含 Projects.YH_Flow_Api 引用
- yh-flow/src/Host/YH.Flow.AppHost/AppHost.cs 包含 "yhflow-db" 字符串
- yh-flow/src/Host/YH.Flow.AppHost/AppHost.cs 包含 "yhflow-uploads" 字符串
  </acceptance_criteria>

## Task 1.4: 清理 obj/bin 并验证编译

<type>execute</type>
<files>
<file>yh-flow/src/**/obj/</file>
<file>yh-flow/src/**/bin/</file>
</files>
<read_first>

- yh-flow/YH.Flow.slnx (解决方案文件)
- yh-flow/src/Directory.Build.props (构建配置)
  </read_first>
  <action>

1. 删除所有 obj/ 和 bin/ 目录:
   find yh-flow/src -type d -name "obj" -exec rm -rf {} + 2>/dev/null
   find yh-flow/src -type d -name "bin" -exec rm -rf {} + 2>/dev/null
2. 在 yh-flow/src/ 目录执行: dotnet restore YH.Flow.slnx
3. 执行: dotnet build YH.Flow.slnx --no-restore
4. 记录编译错误和警告，为 Wave 2 修复做准备
   </action>
   <verify>
5. dotnet restore 完成无致命错误
6. dotnet build 输出编译结果 (可能有错误需要 Wave 2 修复)
   </verify>
   <acceptance_criteria>

- dotnet restore YH.Flow.slnx 命令退出码为 0 或仅含非致命警告
- 所有 obj/ 和 bin/ 目录已清除
- 编译输出中不包含 "FSH.Starter" 相关的找不到项目错误 (项目引用路径已正确更新)
  </acceptance_criteria>

</tasks>

<verification>
1. yh-flow/ 目录包含完整的模板文件结构
2. 所有 FSH 命名空间已替换为 YH
3. 所有 Host 项目目录已重命名
4. YH.Flow.slnx 解决方案文件存在
5. PowerShell 重命名脚本保留在 yh-flow/scripts/
</verification>

<success_criteria>

- 模板完整复制到 yh-flow/
- FSH → YH 命名空间替换 100% 完成
- 目录重命名完成 (4 个 Host 项目)
- 编译基础设施 (Directory.Build.props, Directory.Packages.props, NuGet.Config) 就绪
- 重命名脚本可重复执行
  </success_criteria>

<must_haves>

- 所有 BuildingBlocks 项目保留并正确重命名命名空间 (D-03)
- 所有模块项目保留在磁盘上 (D-01)
- Host 项目重命名为 YH.Flow.Api, YH.Flow.DbMigrator, YH.Flow.AppHost (D-04)
- 命名空间遵循 YH.Framework.{Name}, YH.Modules.{Name}, YH.Flow.{Name} 方案 (D-05)
- 解决方案文件为 YH.Flow.slnx (D-06)
- 全面替换 FSH → YH (D-07)
- PowerShell 脚本保留在项目中 (D-08)
  </must_haves>

<threat_model>
Phase 0 为纯脚手架阶段，不涉及运行时安全威胁。

- 无网络端点暴露
- 无用户输入处理
- 无数据存储
- 安全相关配置 (JWT, CORS, Rate Limiting) 在 Phase 1 实现
  </threat_model>
