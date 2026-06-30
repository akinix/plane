---
phase: 17-calendar-gantt-sheet
plan: 01
status: complete
commit: edad74fd9
completed_at: "2026-06-29"
requirements: [FILT-01, FILT-02, KANB-04]
---

# Plan 01 完成: 筛选引擎基础设施

## 创建的文件

### app/components/issues/filters/

- **types.ts** — 7 个类型定义: TFilterCriteria, TSortConfig, TGroupByOptions, TSubGroupByOptions, TViewLayout, TIssueView, TColumnVisibility
- **use-filters.ts** — TanStack Query hook，URL query params 双向同步（FILT-01）
- **use-sorting.ts** — 排序配置 hook + 客户端排序函数（FILT-02）
- **use-group-by.ts** — 5 种分组方式: state/priority/assignees/created_by/none（D-P17-12）
- **use-sub-group-by.ts** — 3 种子分组: state/priority/none（KANB-04）

### app/store/

- **issue.store.ts** — 扩展: activeView 改为 TViewLayout, 添加 visibleColumnIds, filters 改为 TFilterCriteria
- **types.ts** — 导出新类型

## Self-Check: PASSED

- [x] 筛选引擎类型定义完成（7 个类型）
- [x] useFilters hook 通过 TanStack Query + URL query params 管理筛选状态
- [x] useSorting hook 提供排序配置和客户端排序函数
- [x] useGroupBy hook 支持 5 种分组方式
- [x] useSubGroupBy hook 支持 3 种子分组
- [x] IssueStore.activeView 支持所有 5 种视图
- [x] TypeScript 编译通过（lint 修复后 hooks 通过）
