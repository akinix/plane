---
phase: 07-page
plan: 03
type: execute
tags: [test, domain-tests, handler-tests, integration, regression]
requires: [07-02]
provides: [full-test-suite]
affects: [Page.Tests]
tech-stack:
  added: [PageTestFixture, PageCrudTests, PageListSummaryTests, PageArchiveTests, PageDescriptionTests, PageFavoriteTests]
  patterns: [InMemory-DbContext-fixture, IClassFixture-handler-tests, Shouldly-assertions, NSubstitute-mocks]
key-files:
  created:
    - yh-flow/src/Tests/Page.Tests/Features/PageTestFixture.cs
    - yh-flow/src/Tests/Page.Tests/Features/PageCrudTests.cs
    - yh-flow/src/Tests/Page.Tests/Features/PageListSummaryTests.cs
    - yh-flow/src/Tests/Page.Tests/Features/PageArchiveTests.cs
    - yh-flow/src/Tests/Page.Tests/Features/PageDescriptionTests.cs
    - yh-flow/src/Tests/Page.Tests/Features/PageFavoriteTests.cs
  modified:
    - yh-flow/src/Tests/Page.Tests/Usings.cs
    - yh-flow/src/Tests/Page.Tests/TestData/TestPageFactory.cs
    - yh-flow/src/Tests/Page.Tests/Domain/PageDomainTests.cs (unchanged — already complete)
    - yh-flow/src/Tests/Page.Tests/Domain/ProjectPageDomainTests.cs (unchanged — already complete)
    - yh-flow/src/Tests/Page.Tests/Domain/PageFavoriteDomainTests.cs (unchanged — already complete)
decisions:
  - "IClassFixture<PageTestFixture> pattern for handler test classes (consistent with xUnit best practices)"
  - "Each test creates its own InMemory database (Guid.NewGuid() unique name) for isolation per T-7-03-01"
  - "Fixture provides non-static CreateDbContext() for IClassFixture compatibility; CA1822 suppressed via pragma"
  - "using var db pattern for proper DbContext disposal, CA2000 compliance"
  - "Reflection-based inspection of anonymous type returned by GetPageSummaryQueryHandler"
metrics:
  duration: "~30 min"
  completed_date: "2026-06-25"
  total_tests: 53
  domain_tests: 23 (Page: 15, ProjectPage: 4, PageFavorite: 4)
  handler_tests: 30 (CRUD: 8, ListSummary: 4, Archive: 6, Description: 6, Favorite: 6)
  build_errors: 0
  regression: 0
---

# Phase 7 Plan 3: 全量测试套件 — Summary

Page 模块领域测试增强 + Handler 功能集成测试。

## 执行内容

### Task 1: 领域测试基础设施完善

- **Usings.cs**: 扩展全局 using 指令（Finbuckle, NSubstitute, EF Core, DTO namespaces 等）
- **TestPageFactory.cs**: 新增 `CreateEntityWithDescription(string html, string stripped, string? json, string name)` 方法
- **PageTestFixture.cs** (NEW): InMemory PageDbContext 工厂，隔离 InMemory 数据库名（每个调用唯一 GUID），stubbed `IMultiTenantContextAccessor<AppTenantInfo>` + `IOptions<DatabaseOptions>` + `IHostEnvironment`
- 领域测试原封不动（Page 15 + ProjectPage 4 + PageFavorite 4 = 23 测试均已通过）

### Task 2: CRUD + List/Summary + Archive Handler 测试

- **PageCrudTests.cs** (8 tests): Create/Get/Update/Delete 的 happy path + not-found 异常路径
- **PageListSummaryTests.cs** (4 tests): 默认顶层页面、归档筛选、SortOrder 排序、Summary 聚合数据
- **PageArchiveTests.cs** (6 tests): Archive/Unarchive 功能 + 幂等性 + 不存在的 ID 异常

### Task 3: Description + Favorite Handler 测试 + 全量回归

- **PageDescriptionTests.cs** (6 tests): Get/Update 描述内容 + 局部更新 + null 描述 + not-found 异常
- **PageFavoriteTests.cs** (6 tests): Add/Remove 收藏 + 重复添加幂等性 + 不存在收藏幂等性 + Empty PageId 异常
- **全量回归**: 解决方案构建 0 错误，所有单元测试项目零回归

## 测试概要

| 测试文件                | 测试数 | 覆盖场景                                            |
| ----------------------- | ------ | --------------------------------------------------- |
| PageDomainTests         | 15     | Create/Archive/Unarchive/Update/SoftDelete 各种变体 |
| ProjectPageDomainTests  | 4      | Create/SoftDelete 及空值守卫                        |
| PageFavoriteDomainTests | 4      | Create/SoftDelete 及空值守卫                        |
| PageCrudTests           | 8      | Create/Get/Update/Delete happy + exception paths    |
| PageListSummaryTests    | 4      | 默认列表/归档筛选/SortOrder排序/Summary聚合         |
| PageArchiveTests        | 6      | Archive/Unarchive 功能/幂等/异常                    |
| PageDescriptionTests    | 6      | Get/Update description + 局部更新/异常              |
| PageFavoriteTests       | 6      | Add/Remove 收藏 + 幂等/异常                         |
| **Total**               | **53** |                                                     |

## 关键模式

- **测试隔离**: 每个测试方法使用 `_fixture.CreateDbContext()` —> `UseInMemoryDatabase($"page-{Guid.NewGuid():n}")`（T-7-03-01 Mitigation）
- **双重校验**: 每个 Handler 测试同时验证 Handler 返回值和 DbContext 最终状态（T-7-03-02 Mitigation）
- **using var**: `using var db = _fixture.CreateDbContext()` 确保每个测试 DbContext 正确释放
- **Fluent 断言**: Shouldly（`.ShouldBe()`, `.ShouldNotBeNull()`, `.ShouldThrowAsync<T>()`）

## 验证结果

```
dotnet build src/YH.Flow.slnx --nologo
> Build succeeded. 0 warnings, 0 errors.

dotnet test src/Tests/Page.Tests/Page.Tests.csproj --no-restore
> Passed! - Failed: 0, Passed: 53, Skipped: 0, Total: 53

dotnet test src/Tests/WorkItems.Tests/WorkItems.Tests.csproj --no-restore
> Passed! - Failed: 0, Passed: 264, Skipped: 0, Total: 264

dotnet test src/Tests/Workspace.Tests/Workspace.Tests.csproj --no-restore
> Passed! - Failed: 0, Passed: 100, Skipped: 0, Total: 100
```

## 提交记录

| Hash      | Message                                                               |
| --------- | --------------------------------------------------------------------- |
| 39351e513 | test(07-03): enhance domain test infrastructure + add PageTestFixture |
| 806bee87f | test(07-03): add handler tests for CRUD + List/Summary + Archive      |
| 3718ebc3a | test(07-03): add handler tests for Description + Favorite             |

## Deviations from Plan

- 领域测试文件（`PageDomainTests.cs`、`ProjectPageDomainTests.cs`、`PageFavoriteDomainTests.cs`）未修改 — 原来的 23 个测试已覆盖全部 plan 需求
- 计划中 `GlobalUsings.cs` 使用已有的 `Usings.cs`（ImplicitUsings=enable 时 xUnit 项目惯例）
- `TestPageFactory.CreateEntityWithDescription` 签名扩展了 `name` 和 `projectId` 参数以支持更灵活的测试数据

## Known Stubs

无。

## Threat Flags

无 — 测试代码不引入新的安全表面。
