---
phase: 00-init
reviewed: 2026-06-16T12:00:00Z
depth: standard
files_reviewed: 10
files_reviewed_list:
  - yh-flow/src/Host/YH.Flow.AppHost/AppHost.cs
  - yh-flow/src/Host/YH.Flow.Api/Program.cs
  - yh-flow/src/Directory.Build.props
  - yh-flow/src/Directory.Packages.props
  - yh-flow/src/YH.Flow.slnx
  - yh-flow/CLAUDE.md
  - yh-flow/AGENTS.md
  - yh-flow/src/Host/YH.Flow.AppHost/YH.Flow.AppHost.csproj
  - yh-flow/src/Host/YH.Flow.Api/YH.Flow.Api.csproj
  - yh-flow/src/Host/YH.Flow.DbMigrator/YH.Flow.DbMigrator.csproj
findings:
  critical: 0
  warning: 1
  info: 4
  total: 5
status: issues_found
---

# Phase 00: Init — Code Review Report

**Reviewed:** 2026-06-16T12:00:00Z
**Depth:** standard
**Files Reviewed:** 10
**Status:** issues_found

## Summary

Phase 00 成功将 fullstackhero 模板复制到 `yh-flow/` 并完成 FSH→YH 命名空间重命名。AppHost Aspire 编排配置（PostgreSQL、Redis/Valkey、MinIO）结构正确，构建系统（`Directory.Build.props`、`Directory.Packages.props`）配置合理，文档（CLAUDE.md、AGENTS.md）准确描述了项目架构。`Program.cs` 正确移除了已禁用模块（Catalog、Tickets、Chat、Billing）的 DI 注册，保留了 6 个启用模块（Identity、Multitenancy、Auditing、Files、Webhooks、Notifications）。

主要问题：AppHost.csproj 引用了未使用的 `Aspire.Hosting.JavaScript` 包；多处残留 `fsh-` 前缀未重命名。

## Warnings

### WR-01: AppHost.csproj 引用未使用的 `Aspire.Hosting.JavaScript` 包

**File:** `yh-flow/src/Host/YH.Flow.AppHost/YH.Flow.AppHost.csproj:12`
**Issue:** `AppHost.cs` 中没有任何 `AddNodeApp()`、`AddNpmApp()` 等 JavaScript 资源调用（第 140-142 行注释明确说明前端将在 Phase 13 添加）。该包是模板遗留，增加不必要的依赖和恢复时间。
**Fix:**
```xml
<!-- 删除此行，Phase 13 需要时再加回 -->
<!-- <PackageReference Include="Aspire.Hosting.JavaScript" /> -->
```

## Info

### IN-01: ContainerRepository 仍使用 `fsh-` 前缀

**File:** `yh-flow/src/Host/YH.Flow.Api/YH.Flow.Api.csproj:14`
**Also:** `yh-flow/src/Host/YH.Flow.DbMigrator/YH.Flow.DbMigrator.csproj:17`
**Issue:** `ContainerRepository` 值分别为 `fsh-api` 和 `fsh-db-migrator`，未跟随 FSH→YH 重命名。容器镜像名称是对外可见的产物标识。
**Fix:**
```xml
<!-- Api.csproj -->
<ContainerRepository>yh-flow-api</ContainerRepository>

<!-- DbMigrator.csproj -->
<ContainerRepository>yh-flow-db-migrator</ContainerRepository>
```

### IN-02: DbMigrator 代码中残留 `fsh-` 前缀

**File:** `yh-flow/src/Host/YH.Flow.DbMigrator/Program.cs:60`
**Also:** `yh-flow/src/Host/YH.Flow.DbMigrator/PostgresMigratorLock.cs:17`
**Issue:** JWT 占位符键 `"fsh-dbmigrator-placeholder-never-mints-tokens-32+"` 和注释 `"for the fsh-db-migrator session lock"` 仍包含 `fsh-` 前缀。
**Fix:** 将 `fsh-dbmigrator` 改为 `yh-flow-dbmigrator`，将 `fsh-db-migrator` 改为 `yh-flow-db-migrator`。

### IN-03: 已禁用模块的 ProjectReference 仍保留在 Api.csproj

**File:** `yh-flow/src/Host/YH.Flow.Api/YH.Flow.Api.csproj:37-46`
**Issue:** Billing、Catalog、Tickets、Chat 的运行时 + Contracts 项目引用（共 8 个 ProjectReference）仍然存在，而 Program.cs 中这些模块的 DI 注册已被移除。项目引用可能是为了保持编译可用性和迁移支持，但缺少注释说明保留意图。
**Fix:** 添加注释说明保留原因，例如：
```xml
<!-- Disabled modules: code retained for future re-enablement.
     DI registrations removed in Program.cs (Phase 00).
     ProjectReferences kept so migrations compile and modules can be re-enabled by
     adding them back to moduleAssemblies + Mediator assemblies. -->
```

### IN-04: 解决方案文件残留模板条件指令

**File:** `yh-flow/src/YH.Flow.slnx:60,64,84,88`
**Issue:** `<!--#if (aspire) -->`、`<!--#endif -->`、`<!--#if (includeTools) -->` 是 dotnet template 引擎的条件指令。Phase 00 已将模板实例化为具体项目，这些指令应已被解析移除，否则 IDE 或工具可能误处理。
**Fix:** 移除 `<!--#if ...-->` 和 `<!--#endif -->` 行，保留其包裹的内容（AppHost 和 CLI 项目引用应直接保留在解决方案中）。

---

_Reviewed: 2026-06-16T12:00:00Z_
_Reviewer: Claude (gsd-code-reviewer)_
_Depth: standard_
