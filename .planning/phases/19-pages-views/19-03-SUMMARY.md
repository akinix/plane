---
phase: 19-pages-views
plan: 03
type: execute
created: "2026-06-30T12:00:00Z"
duration_minutes: 45
tasks:
  total: 2
  completed: 2
files:
  created:
    - yh-flow/clients/web/app/components/pages/editor/page-root.tsx
    - yh-flow/clients/web/app/components/pages/editor/editor-body.tsx
    - yh-flow/clients/web/app/components/pages/editor/title.tsx
    - yh-flow/clients/web/app/components/pages/editor/header/root.tsx
    - yh-flow/clients/web/app/components/pages/editor/header/logo-picker.tsx
    - yh-flow/clients/web/app/components/pages/editor/toolbar/root.tsx
    - yh-flow/clients/web/app/components/pages/editor/toolbar/toolbar.tsx
    - yh-flow/clients/web/app/components/pages/editor/toolbar/color-dropdown.tsx
    - yh-flow/clients/web/app/components/pages/editor/toolbar/options-dropdown.tsx
    - yh-flow/clients/web/app/components/pages/header/root.tsx
    - yh-flow/clients/web/app/components/pages/header/actions.tsx
    - yh-flow/clients/web/app/components/pages/header/copy-link-control.tsx
    - yh-flow/clients/web/app/components/pages/navigation-pane/root.tsx
    - yh-flow/clients/web/app/components/pages/navigation-pane/tabs-list.tsx
    - yh-flow/clients/web/app/components/pages/navigation-pane/tab-panels/outline.tsx
    - yh-flow/clients/web/app/components/pages/navigation-pane/tab-panels/info/root.tsx
    - yh-flow/clients/web/app/components/pages/navigation-pane/tab-panels/info/document-info.tsx
    - yh-flow/clients/web/app/components/pages/navigation-pane/tab-panels/info/actors-info.tsx
    - yh-flow/clients/web/app/components/pages/loaders/page-loader.tsx
    - yh-flow/clients/web/app/workspaces/[workspaceId]/pages/[pageId]/page.tsx
  modified:
    - yh-flow/clients/web/app/routes.ts
  deleted: []
commits:
  - hash: 67004bf0d
    message: "feat(19-03): register editor route + create editor entry, page-root, editor-body, title, page-loader"
    files:
      - routes.ts
      - [pageId]/page.tsx
      - page-root.tsx
      - editor-body.tsx
      - title.tsx
      - page-loader.tsx
  - hash: eb5884ff8
    message: "feat(19-03): create editor header, toolbar, navigation pane, and actions components"
    files:
      - editor/header/root.tsx
      - editor/header/logo-picker.tsx
      - editor/toolbar/root.tsx
      - editor/toolbar/toolbar.tsx
      - editor/toolbar/color-dropdown.tsx
      - editor/toolbar/options-dropdown.tsx
      - header/root.tsx
      - header/actions.tsx
      - header/copy-link-control.tsx
      - navigation-pane/root.tsx
      - navigation-pane/tabs-list.tsx
      - navigation-pane/tab-panels/outline.tsx
      - navigation-pane/tab-panels/info/root.tsx
      - navigation-pane/tab-panels/info/document-info.tsx
      - navigation-pane/tab-panels/info/actors-info.tsx
subsystem: flow-web-frontend
requirements:
  - PAGE-02
  - PAGE-03
key_decisions:
  - "使用 EditorRefApi.setEditorValue 在组件挂载时注入初始 editor content，替代 Plane 的实时协同模式"
  - "editor-body 使用简化 fileHandler（无实际上传逻辑），mock 阶段无需后端文件服务"
  - "title.tsx 使用 click-to-edit 模式（显示 → 编辑切换），blur 后 800ms debounce 自动保存"
  - "header/actions.tsx 的归档确认弹窗内联实现（使用本地 useState），删除复用 delete-page-modal.tsx"
  - "navigation-pane 不采用 Plane 的 query-param 驱动模式，使用 props 控制展开/收起和 Tab 状态"
  - "大纲解析 regex 提取 <h1>-<h6> 标签，无编辑器 ref 监听（简化实现，mock 阶段无需实时同步）"
---

# Phase 19 Plan 03: Pages 编辑器页面（独立编辑模式）

**TipTap 富文本编辑器（复用 forked DocumentEditorWithRef）集成自动保存、Header 操作区（返回/权限切换/收藏/归档/删除）、悬浮工具栏、右侧导航面板（大纲/信息）**

## Performance

- **Duration:** 45 min
- **Started:** 2026-06-30T12:00:00Z
- **Completed:** 2026-06-30T12:45:00Z
- **Tasks:** 2
- **Files modified:** 21 (20 created, 1 modified)

## Accomplishments

- 注册编辑器路由 `workspaces/:workspaceId/pages/:pageId` 并创建入口组件
- 创建 PageEditorRoot 全页容器（数据加载 → 骨架屏 → Header + Title + EditorBody + 导航面板完整布局）
- 集成 forked DocumentEditorWithRef（TipTap 编辑器），组件挂载时注入 initial HTML content
- 实现自动保存数据流：EditorBody.onChange → 1500ms debounce → updatePage mutation 保存 description_html
- 实现标题区域 click-to-edit + blur 后 800ms debounce 自动保存
- 创建编辑器 Header（返回按钮、页面名称、访问权限切换下拉、收藏星标、归档/删除更多操作）
- 创建悬浮工具栏（heading 下拉 + bold/italic/strikethrough + list/link/quote/code 按钮）
- 创建颜色选择器和更多选项下拉（全屏/字数统计预留）
- 创建页面归档确认弹窗和删除确认弹窗（复用 delete-page-modal.tsx）
- 创建右侧导航面板（可折叠 280px 面板，大纲 Tab 解析 HTML headings + 信息 Tab 显示元信息和参与者）

## Task Commits

1. **Task 1: 注册编辑器路由 + 创建编辑器入口 + EditorRoot + EditorBody + Title + Loader** - `67004bf` (feat)
2. **Task 2: 创建编辑器 Header + Toolbar + Navigation Pane + Actions** - `eb5884f` (feat)

## Files Created/Modified

### Modified

- `yh-flow/clients/web/app/routes.ts` — 追加编辑器路由 `workspaces/:workspaceId/pages/:pageId`

### Created (core editor)

- `yh-flow/clients/web/app/workspaces/[workspaceId]/pages/[pageId]/page.tsx` — 编辑器页面入口路由组件
- `yh-flow/clients/web/app/components/pages/editor/page-root.tsx` — 编辑器全页容器（自动保存 + 骨架屏 + 错误处理）
- `yh-flow/clients/web/app/components/pages/editor/editor-body.tsx` — TipTap 编辑器主体（DocumentEditorWithRef 封装）
- `yh-flow/clients/web/app/components/pages/editor/title.tsx` — 页面标题编辑输入框（click-to-edit + auto-save on blur）
- `yh-flow/clients/web/app/components/pages/loaders/page-loader.tsx` — 编辑器全页加载骨架屏

### Created (editor header & toolbar)

- `yh-flow/clients/web/app/components/pages/editor/header/root.tsx` — 编辑器 Header（返回按钮 + 访问权限切换 + 收藏 + 更多操作）
- `yh-flow/clients/web/app/components/pages/editor/header/logo-picker.tsx` — Logo 选择器（Emoji 图标展示）
- `yh-flow/clients/web/app/components/pages/editor/toolbar/root.tsx` — 工具栏容器（含导航面板展开按钮）
- `yh-flow/clients/web/app/components/pages/editor/toolbar/toolbar.tsx` — 悬浮工具栏（heading/bold/italic/list/link/quote/code）
- `yh-flow/clients/web/app/components/pages/editor/toolbar/color-dropdown.tsx` — 预设文字颜色选择器
- `yh-flow/clients/web/app/components/pages/editor/toolbar/options-dropdown.tsx` — 更多选项（全屏/字数统计预留）

### Created (page header & navigation pane)

- `yh-flow/clients/web/app/components/pages/header/root.tsx` — 页面列表 Header 容器
- `yh-flow/clients/web/app/components/pages/header/actions.tsx` — 归档确认弹窗 + 删除弹窗
- `yh-flow/clients/web/app/components/pages/header/copy-link-control.tsx` — 复制当前 URL 到剪贴板
- `yh-flow/clients/web/app/components/pages/navigation-pane/root.tsx` — 可折叠 280px 右侧导航面板容器
- `yh-flow/clients/web/app/components/pages/navigation-pane/tabs-list.tsx` — Tab 导航（大纲 / 信息）
- `yh-flow/clients/web/app/components/pages/navigation-pane/tab-panels/outline.tsx` — 文档大纲（正则解析 h1-h6）
- `yh-flow/clients/web/app/components/pages/navigation-pane/tab-panels/info/root.tsx` — 信息面板容器
- `yh-flow/clients/web/app/components/pages/navigation-pane/tab-panels/info/document-info.tsx` — 文档元信息（创建/更新人+时间）
- `yh-flow/clients/web/app/components/pages/navigation-pane/tab-panels/info/actors-info.tsx` — 参与者信息（所有者/创建者/编辑者）

## Decisions Made

1. **Component 接口简化**: 编辑器组件（page-root/editor-body/title/header）接受简单 props（workspaceId, pageId, TPage），不采用 Plane 复杂的 TPageInstance store 模式，适配 Flow Web 的 mock 数据层
2. **editor-body fileHandler 简化**: 使用空实现 fileHandler（getAsset/upload/delete/restore 返回 null/空），mock 阶段无需后端文件服务
3. **title 实现模式**: 使用 click-to-edit 切换（display mode → 编辑 mode），blur 时触发 800ms debounce 保存，不采用 Plane 的 TextArea + autoFocus 模式
4. **归档确认弹窗**: 在 header/actions.tsx 中内联实现（本地 useState 控制），不依赖 PageStore 的 archiveModal 状态；删除则复用 delete-page-modal.tsx
5. **导航面板状态**: 使用 props（isOpen/onClose）控制展开收起，不采用 Plane 的 URL query-param 驱动模式
6. **大纲 Tab**: 纯正则解析（`<h1>-<h6>`），不连接 editorRef，mock 阶段无需实时同步 editor 的 headings 数据流

## Deviations from Plan

None — 计划按预期完整执行。

## Issues Encountered

- **Pre-commit hook oxfmt SIGKILL**: pnpm exec oxfmt 在 Windows 上被 SIGKILL 终止，但 oxlint 检查通过后仍然可以正常提交（husky 未阻塞）
- **oxlint --deny-warnings 严格 lint**: 新创建的文件需要满足 jsx-a11y（click-events-have-key-events, prefer-tag-over-role, no-static-element-interactions）和 react（no-array-index-key）规则，已全部修复

## Known Stubs

- `filters/root.tsx` 和 `applied-filters/root.tsx` — 已在 19-02 创建，为预留扩展
- `color-dropdown.tsx` — 颜色选择器仅展示预设色板，未连接 editor interaction（计划在后续版本对接 EditorRefApi）
- `options-dropdown.tsx` — 全屏/字数统计为 placeholder，未实现实际功能
- logo-picker.tsx — 简化实现，未对接 @plane/ui EmojiPicker 组件（仅设置默认 emoji）

## Verification

- [x] `routes.ts` 包含 `workspaces/:workspaceId/pages/:pageId` 路由
- [x] `[pageId]/page.tsx` 已创建并导出 PageEditorPage 组件
- [x] `page-root.tsx` 使用 `usePageDetail` 获取数据 + 1500ms debounce 自动保存
- [x] `editor-body.tsx` 导入 `DocumentEditorWithRef` from `@/lib/editor`
- [x] `title.tsx` 实现 click-to-edit + blur 自动保存（800ms debounce）
- [x] `page-loader.tsx` 提供全页加载骨架屏
- [x] `editor/header/root.tsx` 包含返回按钮、访问权限切换、收藏按钮、更多操作
- [x] `toolbar/toolbar.tsx` 提供格式化按钮（heading, bold, italic, list, link, quote, code）
- [x] `navigation-pane` 提供"大纲"和"信息"两个 Tab
- [x] `header/actions.tsx` 归档/删除使用确认弹窗
- [x] 所有文案使用中文 per UI-SPEC

## Threat Surface

None — 所有变更为纯前端 UI 组件。大纲仅解析本地 description_html 字符串，无数据泄露风险。自动保存为纯客户端 mock 操作，无持久化风险。
