---
phase: 16-issue
plan: 03
subsystem: "yh-flow/clients/web"
tags: ["issue-detail", "comment", "property-editor", "activity-log", "ui"]
requires: [16-01]
provides: [ISSU-03, ISSU-04, ISSU-05, ISSU-06, ISSU-08]
affects: []
tech-stack:
  added: []
  patterns:
    [
      "TanStack Query hooks with mock data",
      "observer() + MobX store pattern",
      "@plane/editor RichTextEditor/LiteTextEditor",
    ]
key-files:
  created:
    - "yh-flow/clients/web/app/issues/[issueId]/page.tsx"
    - "yh-flow/clients/web/app/components/issues/issue-detail-main.tsx"
    - "yh-flow/clients/web/app/components/issues/issue-detail-sidebar.tsx"
    - "yh-flow/clients/web/app/components/issues/property-editor.tsx"
    - "yh-flow/clients/web/app/components/issues/comment-list.tsx"
    - "yh-flow/clients/web/app/components/issues/comment-input.tsx"
    - "yh-flow/clients/web/app/components/issues/delete-issue-modal.tsx"
    - "yh-flow/clients/web/app/components/issues/activity-log.tsx"
    - "yh-flow/clients/web/src/lib/hooks/use-issues.ts"
    - "yh-flow/clients/web/src/lib/hooks/use-comments.ts"
  modified:
    - "yh-flow/clients/web/src/lib/mock-data.ts"
    - "yh-flow/clients/web/src/lib/hooks/index.ts"
decisions:
  - "D-P16-09: 双栏布局（65/35 split） — 左栏编辑器 + 右栏属性面板"
  - "D-P16-10: 属性字段采用内联编辑方式"
  - "D-P16-11: 评论组件位于 Issue 描述下方 — 评论列表 → 评论输入框"
  - "D-P16-12: 使用 @plane/editor 的 RichTextEditorWithRef 编辑描述"
metrics:
  duration: "~45 min"
  completed: "2026-06-29"
---

# Phase 16 Plan 03: Issue Detail Page Implementation Summary

## One-Liner

实现 Issue 详情页面：双栏布局（@plane/editor 描述编辑器 + 内联属性面板）、评论系统（创建/编辑/删除）、活动日志和软删除确认弹窗。

## Files Created

| #   | File                                             | Purpose                                                                                 |
| --- | ------------------------------------------------ | --------------------------------------------------------------------------------------- |
| 1   | `app/issues/[issueId]/page.tsx`                  | Issue 详情路由页面 — 双栏布局骨架 + 加载/空/错误状态                                    |
| 2   | `app/components/issues/issue-detail-main.tsx`    | 左栏 — 可编辑标题 + @plane/editor 富文本描述                                            |
| 3   | `app/components/issues/issue-detail-sidebar.tsx` | 右栏 — 属性面板（使用 PropertyEditor 子组件）                                           |
| 4   | `app/components/issues/property-editor.tsx`      | 6 个可复用属性编辑子组件（State/Priority/Assignee/Labels/Estimate/Date）                |
| 5   | `app/components/issues/comment-list.tsx`         | 评论列表（显示头像/名称/时间/HTML内容 + 编辑/删除）                                     |
| 6   | `app/components/issues/comment-input.tsx`        | 评论输入框（LiteTextEditor + 提交按钮）                                                 |
| 7   | `app/components/issues/delete-issue-modal.tsx`   | 软删除确认弹窗（AlertModalCore）                                                        |
| 8   | `app/components/issues/activity-log.tsx`         | 活动日志列表（按时间降序，中文描述）                                                    |
| 9   | `src/lib/hooks/use-issues.ts`                    | TanStack Query hooks: useIssue, useIssues, useProjectStates, useIssueMutations          |
| 10  | `src/lib/hooks/use-comments.ts`                  | TanStack Query hooks: useComments, useCreateComment, useUpdateComment, useDeleteComment |

## Files Modified

| File                     | Changes                                                                                                                                                |
| ------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `src/lib/mock-data.ts`   | 添加 MOCK_STATES (16 states), MOCK_LABELS (10 labels), MOCK_ISSUES (17 issues), MOCK_ISSUE_COMMENTS (6 comments), MOCK_ISSUE_ACTIVITIES (8 activities) |
| `src/lib/hooks/index.ts` | 添加 use-issues 和 use-comments 导出                                                                                                                   |

## Tasks Executed

### Task 1: Issue Detail Page + Dual-Column Layout

- `app/issues/[issueId]/page.tsx` — 路由页面，使用 useParams 获取 workspaceId/projectId/issueId，加载/404/正常三态渲染
- `app/components/issues/issue-detail-main.tsx` — 左栏：可编辑标题（点击切换 input），@plane/editor RichTextEditorWithRef 编辑描述，保存按钮调用 onUpdate
- `app/components/issues/issue-detail-sidebar.tsx` — 右栏：6 个属性字段（状态/优先级/负责人/标签/估算/截止日期），内联编辑 + 删除按钮

**Commit:** `ac28a8ca4`

### Task 2: PropertyEditor Component

- `app/components/issues/property-editor.tsx` — 6 个导出子组件：PropertyEditorState（按 TStateGroups 分组）、PropertyEditorPriority（5 级中文标签）、PropertyEditorAssignee（头像+名称列表）、PropertyEditorLabels（多选切换）、PropertyEditorEstimate（数字输入+快速选项）、PropertyEditorDate（原生 date input）
- 更新 IssueDetailSidebar 使用 PropertyEditor 子组件

**Commit:** `ed1e1ed`

### Task 3: Comment System + Delete Issue

- `comment-list.tsx` — 评论列表（useComments hook），支持编辑（内联 LiteTextEditor）和删除（内联确认），current user 限制操作按钮
- `comment-input.tsx` — 评论输入框（LiteTextEditor），内容检测禁用提交，提交后清空编辑器
- `delete-issue-modal.tsx` — AlertModalCore 确认弹窗，软删除（设置 archived_at），完成后导航回列表

**Commit:** `6e3fa6a17`

### Task 4: ActivityLog Component

- `activity-log.tsx` — 从 MOCK_ISSUE_ACTIVITIES 过滤当前 Issue 活动，按时间降序排列
- 每条显示：操作人头像 + 名称 + 中文动作描述 + 相对时间
- 描述生成器：依据 field/verb/old_value/new_value 生成中文文本

**Commit:** `89614a344`

## Key Decisions

- **D-P16-09（双栏布局）**: 左栏 flex-[2] 约 65%，右栏 flex-[1] 约 35%
- **D-P16-10（内联编辑）**: 每个属性点击直接展开下拉面板，选择后即时保存
- **D-P16-11（评论顺序）**: 评论列表在上，评论输入框在下
- **D-P16-12（编辑器复用）**: Issue 描述使用 RichTextEditorWithRef，评论使用 LiteTextEditorWithRef

## Deviations from Plan

### Auto-fixed Issues

None — plan executed exactly as written.

## Verification Results

- `npx tsc --noEmit` — 零错误（仅 pre-existing editor/ui 库类型错误，不在本次范围）
- `@plane/editor` RichTextEditorWithRef — 用于 Issue 描述编辑
- `@plane/editor` LiteTextEditorWithRef — 用于评论输入和编辑
- `@plane/ui` AlertModalCore — 用于删除确认弹窗
- PropertyEditor 6 个子组件全部导出
- MOCK_ISSUE_ACTIVITIES — ActivityLog 数据源

## Known Stubs

- 文件上传和 @提及功能在 mock 阶段使用 no-op handler（`@plane/editor` 的 fileHandler/mentionHandler）
- deleteIssue 突变目前 mock 数据层会移除 issue（不影响 UI，后续真实 API 替换）
- 活动日志数据目前直接从 MOCK_ISSUE_ACTIVITIES 读取，后续可改为 useQuery hook
- 评论创建使用固定的 "user-1"（mock 层无真实用户认证）
- PropertyEditor 使用直接的 mock 数据源（MOCK_STATES/MOCK_LABELS/MOCK_MEMBERS），后续应改为通过 API hooks 获取

## Self-Check: PASSED
