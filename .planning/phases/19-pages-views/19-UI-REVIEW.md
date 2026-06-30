# Phase 19 — UI Review

**Audited:** 2026-06-30
**Baseline:** UI-SPEC.md (design contract)
**Screenshots:** not captured (no dev server detected at localhost:3000, 5173, or 8080)
**Files audited:** 43 files (37 created, 6 modified across sub-plans 19-01~19-04)

---

## Registry Safety

| Registry | Blocks Used | Status |
|----------|-------------|--------|
| shadcn official | none | not applicable — Plane UI fork |
| Third-party | none | no third-party registries declared |

Registry audit: 0 third-party blocks checked, no flags.

---

## Pillar Scores

| Pillar | Score | Key Finding |
|--------|-------|-------------|
| 1. Copywriting | 2/4 | Missing "创建页面" primary CTA; empty state body copy deviates from UI-SPEC; missing "按最近打开排序" option; "Icon" label in English |
| 2. Visuals | 3/4 | No page-level "页面"/"视图" heading; favorite star hidden until hover (reduced affordance); "Color" label in English in toolbar |
| 3. Color | 2/4 | Hardcoded `bg-red-500`/`text-red-500` instead of `bg-danger-primary`/`text-danger-primary`; inconsistent favorite star color (yellow-500 vs custom-amber); access badges don't use accent per spec |
| 4. Typography | 3/4 | Inline `text-[10px]`/`text-[11px]` for badges deviating from 4-size scale; `text-2xs` non-standard class in view-list-item |
| 5. Spacing | 3/4 | `gap-1.5` (6px) throughout components violates 4-point grid; `py-20` (80px) vs spec `64px` in empty states; `px-page-x` custom class usage |
| 6. Experience Design | 2/4 | Missing "创建页面" CTA (page cannot be created from list); missing undo toast for page restore; wrong delete modal open logic in views-list; no reduced opacity on archived cards |

**Overall: 15/24**

---

## Top 3 Priority Fixes

1. **Missing "创建页面" primary CTA button in page list** — User has no way to create a new page from the page list view. The `CreatePageModal` exists but is never wired in. Fix: Add a "创建页面" button in `pages-list-main-content.tsx` header bar (next to search/sort) that calls `pageStore.openPageModal("create")` and renders `<CreatePageModal>`. This is the primary interaction of PAGE-02.

2. **Empty state body copy deviates from UI-SPEC contract** — All three page list tab empty states show wrong body text:
   - Public tab: shows "公开页面可以被工作区内所有人查看和编辑。" instead of "创建一个页面开始撰写文档。"
   - Private tab: shows "私人页面仅创建者可查看。" instead of "在创建页面时选择"私人"即可仅自己可见。"
   - Archived tab: shows "归档的页面将在此处显示。" instead of "归档的页面会出现在这里，随时可以恢复。"
   Fix: Update the `emptyMessages` object in `app/components/pages/list/root.tsx` lines 16-28 to match the UI-SPEC table verbatim.

3. **Hardcoded `bg-red-500`/`text-red-500` for destructive actions** — Delete buttons in `delete-page-modal.tsx`, `dropdowns/actions.tsx`, `header/actions.tsx` use Tailwind `bg-red-500` and `text-red-500` instead of the semantic tokens `bg-danger-primary` and `text-danger-primary` defined in the UI-SPEC color contract. Fix: Replace all `bg-red-500`/`hover:bg-red-600` with `bg-danger-primary`/`hover:bg-danger-primary/90`, and `text-red-500` with `text-danger-primary`.

---

## Detailed Findings

### Pillar 1: Copywriting (2/4)

**合同偏离 | BLOCKER: Missing "创建页面" primary CTA**
- The `CreatePageModal` component exists (`modals/create-page-modal.tsx`) but is never rendered in the page list view. `pages-list-main-content.tsx` has no trigger button or modal import. The UI-SPEC explicitly requires "创建页面" as the primary CTA (PAGE-02 requirement). Without it, users cannot initiate page creation from the list view.
- File: `app/components/pages/pages-list-main-content.tsx` (no reference to "创建页面" anywhere)

**合同偏离 | WARNING: Empty state body copy does not match UI-SPEC**
In `list/root.tsx` lines 16-28, the empty state body text differs from the contract:

| Tab | UI-SPEC | Actual | Impact |
|-----|---------|--------|--------|
| Public | "创建一个页面开始撰写文档。" | "公开页面可以被工作区内所有人查看和编辑。" | Misleading — describes access instead of prompting creation |
| Private | "在创建页面时选择"私人"即可仅自己可见。" | "私人页面仅创建者可查看。" | Less instructive |
| Archived | "归档的页面会出现在这里，随时可以恢复。" | "归档的页面将在此处显示。" | Lacks the key "随时可以恢复" promise |

- File: `app/components/pages/list/root.tsx` lines 17-28

**合同偏离 | WARNING: Missing "按最近打开排序" sort option**
- UI-SPEC defines 4 sort options: name, created_at, updated_at, **opened_at**. The order-by.tsx component only implements 3, omitting "按最近打开排序".
- File: `app/components/pages/list/order-by.tsx` lines 15-22 — missing `{ key: "opened_at", label: "按最近打开排序" }`

**合同偏离 | WARNING: No page-level "页面"/"视图" heading**
- UI-SPEC Display typography role defines "页面级标题（"页面" / "视图" heading）" using text-lg (18px) font-semibold. However, neither `pages-list-main-content.tsx` nor `views-list.tsx` renders a page-level heading. The pages list starts directly with tab navigation, and views starts with tabs.
- Files: `app/components/pages/pages-list-main-content.tsx`, `app/components/views/views-list.tsx`

**英文残留 | WARNING: "Icon" label in logo picker**
- `logo-picker.tsx` line 33 renders `Icon` in English. All UI copy must be Chinese per contract.
- File: `app/components/pages/editor/header/logo-picker.tsx` line 33

**英文残留 | WARNING: "Color" label in color dropdown**
- `color-dropdown.tsx` line 34 renders `Color` in English instead of "文字颜色" or similar Chinese label.
- File: `app/components/pages/editor/toolbar/color-dropdown.tsx` line 34

**已满足 | 通过项**
- Tab labels: "公开"/"私人"/"已归档" ✓
- Search placeholder: "搜索页面..."/"搜索视图..." ✓
- Sort labels: "按名称排序"/"按创建时间排序"/"按更新时间排序" ✓ (missing opened_at)
- Empty state headings: "暂无公开页面"/"暂无私人页面"/"没有已归档的页面"/"暂无视图" ✓
- Editor title placeholder: "无标题" ✓
- Archive confirmation: "确定归档此页面？归档后仅归档选项卡中可见。" ✓
- Delete confirmation: "确定删除此页面？此操作不可撤销。" ✓
- View empty state: "暂无视图" + "在 Issue 视图中保存当前配置即可创建视图。" ✓
- View save modal: "保存为视图" ✓
- View create button: "新建视图" ✓
- FilterSaveModal: "保存为视图" ✓

---

### Pillar 2: Visuals (3/4)

**合同偏离 | WARNING: No page-level heading for hierarchy**
- The UI-SPEC Display role defines "页面" / "视图" as page-level headings (text-lg, font-semibold). The page list and view list directly render tab navigation without any page title. This flattens the visual hierarchy.
- For comparison: Phase 16 Issue list and Phase 18 Module/Cycle lists typically have page headings.

**交互问题 | WARNING: Favorite star hidden until hover in page cards**
- `block.tsx` line 103: `opacity-0 group-hover:opacity-100` — The favorite star icon is invisible until hover. This reduces affordance: users cannot tell that favoriting is available without mousing over each card. On mobile (touch devices), hover doesn't exist, making the favorite feature effectively hidden.
- File: `app/components/pages/list/block.tsx` line 103

**合同偏离 | WARNING: Color dropdown uses English "Color" label**
- `color-dropdown.tsx` line 34 displays "Color" as the dropdown trigger text. Should be Chinese.

**已满足 | 通过项**
- Page cards show logo/emoji, name, access badge, favorite star, date ✓
- Access badge uses Earth/Lock icons with text ✓
- Editor toolbar formatting buttons use consistent lucide icons ✓
- Navigation pane has clean 280px collapsible layout ✓
- Loading skeleton (PageLoader, PageContentLoader) mimic actual content structure ✓
- Archive badge with Archive icon + formatted date ✓
- View cards show name, description, access badge, favorite star, creator avatar ✓

---

### Pillar 3: Color (2/4)

**合同偏离 | WARNING: Hardcoded `bg-red-500`/`text-red-500` instead of semantic tokens**
Delete and destructive action elements use hardcoded Tailwind red colors instead of the `danger-primary`/`danger-subtle` semantic tokens defined in UI-SPEC:

| Location | Used | Should Be |
|----------|------|-----------|
| `modals/delete-page-modal.tsx:60` | `bg-red-500` | `bg-danger-primary` |
| `modals/delete-page-modal.tsx:60` | `hover:bg-red-600` | `hover:bg-danger-primary/90` |
| `header/actions.tsx:46` | `text-red-500` | `text-danger-primary` |
| `dropdowns/actions.tsx:81` | `text-red-500` | `text-danger-primary` |
| `view-list-item-action.tsx:65` | `text-red-500` | `text-danger-primary` |
| `quick-actions.tsx:65` | `text-red-500` | `text-danger-primary` |
| `page-form.tsx:56` | `text-red-500` | `text-danger-primary` |

This affects 7 locations across both pages and views components.

**合同偏离 | WARNING: Inconsistent favorite star color**
- Pages (block.tsx:103, favorite-control.tsx:43): `fill-yellow-500 text-yellow-500` — uses Tailwind yellow
- Views (view-list-item.tsx:98): `fill-custom-amber text-custom-amber` — uses CSS variable
- The UI-SPEC says favorite star uses "accent" / "filled accent, empty grey" but doesn't specify a specific color. However, the inconsistency between pages and views implementations is problematic. Views uses the proper custom-amber while pages uses hardcoded Tailwind yellow-500.

**合同偏离 | WARNING: Public access badge does not use accent per spec**
- UI-SPEC specifies public access badges should use accent blue background (oklch(0.4799 0.1158 242.91) at 10%). The current implementation uses `bg-custom-background-80` (grey) for all access badges regardless of access level.
- Files: `block.tsx`, `view-list-item.tsx`

**合同偏离 | MINOR: Archived badge uses accent opacity instead of grey**
- UI-SPEC specifies archived state uses "reduced opacity (50%)" with grey tones. Current `archived-badge.tsx` uses `bg-custom-primary/20` (accent at 20% opacity) which uses the primary accent color instead of grey.
- File: `app/components/pages/header/archived-badge.tsx` line 18

**已满足 | 通过项**
- Semantic text tokens used throughout: text-custom-text-100/200/300/400 ✓
- Background surface tokens: bg-custom-background-80/90/100 ✓
- Border tokens: border-custom-border-200 ✓
- Primary accent: bg-custom-primary, text-custom-primary ✓
- Active tab indicator uses border-custom-primary ✓
- No hardcoded hex colors found in pages/views components ✓

---

### Pillar 4: Typography (3/4)

**合同偏离 | WARNING: Inline font sizes outside the 4-size scale**
The UI-SPEC defines 4 sizes: 12px (xs), 14px (sm), 16px (base), 18px (lg). The implementation uses several inline sizes:

| Size | Location | Usage |
|------|----------|-------|
| `text-[10px]` | `list/block.tsx:75` | Archived badge label |
| `text-[11px]` | `list/block.tsx:82`, `page-form.tsx:56,84`, `archived-badge.tsx:20` | Access badge, validation text, access description |
| `text-[2rem]` | `editor/title.tsx:48,60` | Editor page title (this is intentional for editor but outside scale) |
| `text-2xs` | `view-list-item.tsx:64,69,85` | View access badge, creator avatar (non-standard class) |

**已满足 | 通过项**
- Body text uses text-sm (14px) in page cards, editor, view lists ✓
- Label text uses text-xs (12px) in tab navigation, access badges, metadata ✓
- Heading uses text-base (16px) or text-lg (18px) in modal titles, empty states ✓
- Heading weights: font-medium, font-semibold as specified ✓
- Body weight: font-normal (default) ✓

---

### Pillar 5: Spacing (3/4)

**合同偏离 | WARNING: `gap-1.5` (6px) violates 8-point grid**
UI-SPEC inherits "multiples of 4" spacing from Tailwind defaults. `gap-1.5` equals 6px (1.5 * 4px), which is not a multiple of 4. This pattern appears in:

| File | Line(s) |
|------|---------|
| `list/order-by.tsx` | 35 |
| `list/block.tsx` | 80 |
| `search-input.tsx` | 43 |
| `page-form.tsx` | 72 |
| `archived-badge.tsx` | 18 |
| `editor/header/root.tsx` | 71 |
| `header/actions.tsx` | 36, 46 |
| `modals/page-form.tsx` | 84 |
| `navigation-pane/tab-panels/outline.tsx` | 46 |

These should use `gap-2` (8px) or `gap-1` (4px) to stay on the 4-point grid.

**合同偏离 | WARNING: Empty state padding uses `py-20` (80px) vs spec `64px`**
- `list/root.tsx:34` and `views-list.tsx:129` use `py-20` (5rem = 80px). UI-SPEC specifies "64px top/bottom" for empty state. The correct class would be `py-16` (4rem = 64px).

**已满足 | 通过项**
- Page card padding: p-4 (16px = md) ✓
- Page card gap: gap-3 (12px, between sm and md) ✓
- Tab padding: px-4 py-3 (16px/12px) ✓
- Search input: px-2.5 py-1.5 (10px/6px — minor, matches Plane pattern) ✓
- Editor toolbar: min-h-[40px] ✓
- Navigation pane: 280px ✓
- Editor header: h-[48px] ✓

---

### Pillar 6: Experience Design (2/4)

**功能缺失 | BLOCKER: Cannot create page from page list**
- The UI-SPEC Interaction Contract specifies "Click '创建页面' -> Modal opens with PageForm" as the first interaction. However, `pages-list-main-content.tsx` does not render a "创建页面" button and does not import or use `CreatePageModal`. Users have no way to create a new page from the list view. The modal exists but is orphaned.
- Files: `app/components/pages/pages-list-main-content.tsx`, `app/components/pages/modals/create-page-modal.tsx`

**状态缺失 | WARNING: No undo toast for page restore**
- UI-SPEC page 4 (Destructive Actions table) specifies "Restore Page (archived): Optimistic restore + undo toast with '撤销' (5s timeout)." No toast implementation exists in any of the page components. Restore action in `dropdowns/actions.tsx` silently succeeds without user feedback.
- File: `app/components/pages/dropdowns/actions.tsx` line 24-28 (no toast after restore)

**合同偏离 | WARNING: No reduced opacity on archived page cards**
- UI-SPEC Color section states: "Archived pages/cards use reduced opacity (50%) for text and background, with a subtle 'archive' icon indicator." Archived pages display normally with only a small "已归档" badge. No opacity reduction is applied.
- File: `app/components/pages/list/block.tsx` (no opacity classes conditional on `archived_at`)

**逻辑错误 | MINOR BUG: Delete view modal uses wrong state for isOpen**
- `views-list.tsx` line 158: `isOpen={viewStore.viewDeleting}` — This checks the "is deleting in progress" flag instead of "delete modal should be open" (which would be `!!viewStore.deleteViewId`). The `viewDeleting` is set to `true` simultaneously with `deleteViewId` in `openDeleteModal`, so the modal opens. But after deletion completes, `viewDeleting` is set to `false` in `closeDeleteModal` before the modal can read it. This race condition could cause flicker or missed state.
- File: `app/components/views/views-list.tsx` line 158

**交互优化 | MINOR: Favorite star hidden until hover**
- As noted in Visuals, the favorite star is invisible until the card is hovered. On touch devices this means the feature is inaccessible. Consider showing unfilled stars at reduced opacity (50%) when not hovered, similar to the Plane pattern.
- File: `app/components/pages/list/block.tsx` line 103

**已满足 | 通过项**
- Page list loading skeleton: PageContentLoader renders tab bar + 6 card placeholders ✓
- Page list error state: "加载页面列表失败" centered ✓
- Editor loading: PageLoader renders header + title + content skeleton ✓
- Editor error state: "页面加载失败" + "重试" button ✓
- View list loading: Inline animate-pulse skeleton ✓
- View list error: "加载视图列表失败" ✓
- Debounced search (controlled via PageStore/ViewStore) ✓
- Sort dropdown (asc/desc toggle) ✓
- Favorite toggle optimistically updates via mutation + invalidate ✓
- Delete confirmation modal (page + view) ✓
- Archive confirmation modal ✓
- Copy link to clipboard ✓
- Editor auto-save: 1500ms debounce ✓
- Title auto-save: 800ms debounce on blur ✓
- Navigation pane collapse/expand ✓
- Tabs: 公开/私人/已归档 for pages, 全部/我创建的 for views ✓
- FilterSaveModal onSave callback integration ✓

---

## Files Audited

### Pages Data Layer (19-01)
- `app/store/page.store.ts`
- `app/store/root.store.ts` (modified)
- `app/store/types.ts` (modified)
- `src/lib/mock-data.ts` (MOCK_PAGES block)
- `src/lib/hooks/use-pages.ts`
- `src/lib/hooks/use-page-mutations.ts`
- `src/lib/hooks/index.ts` (modified)

### Pages List UI (19-02)
- `app/routes.ts` (PAGE route block)
- `app/workspaces/[workspaceId]/pages/page.tsx`
- `app/components/pages/pages-list-main-content.tsx`
- `app/components/pages/pages-list-view.tsx`
- `app/components/pages/list/tab-navigation.tsx`
- `app/components/pages/list/search-input.tsx`
- `app/components/pages/list/order-by.tsx`
- `app/components/pages/list/root.tsx`
- `app/components/pages/list/block.tsx`
- `app/components/pages/list/block-item-action.tsx`
- `app/components/pages/list/filters/root.tsx`
- `app/components/pages/list/applied-filters/root.tsx`
- `app/components/pages/header/favorite-control.tsx`
- `app/components/pages/header/archived-badge.tsx`
- `app/components/pages/modals/create-page-modal.tsx`
- `app/components/pages/modals/page-form.tsx`
- `app/components/pages/modals/delete-page-modal.tsx`
- `app/components/pages/dropdowns/actions.tsx`
- `app/components/pages/loaders/page-content-loader.tsx`
- `app/components/sidebar/sidebar-tree.tsx` (modified)

### Pages Editor UI (19-03)
- `app/workspaces/[workspaceId]/pages/[pageId]/page.tsx`
- `app/components/pages/editor/page-root.tsx`
- `app/components/pages/editor/editor-body.tsx`
- `app/components/pages/editor/title.tsx`
- `app/components/pages/editor/header/root.tsx`
- `app/components/pages/editor/header/logo-picker.tsx`
- `app/components/pages/editor/toolbar/root.tsx`
- `app/components/pages/editor/toolbar/toolbar.tsx`
- `app/components/pages/editor/toolbar/color-dropdown.tsx`
- `app/components/pages/editor/toolbar/options-dropdown.tsx`
- `app/components/pages/header/root.tsx`
- `app/components/pages/header/actions.tsx`
- `app/components/pages/header/copy-link-control.tsx`
- `app/components/pages/navigation-pane/root.tsx`
- `app/components/pages/navigation-pane/tabs-list.tsx`
- `app/components/pages/navigation-pane/tab-panels/outline.tsx`
- `app/components/pages/navigation-pane/tab-panels/info/root.tsx`
- `app/components/pages/navigation-pane/tab-panels/info/document-info.tsx`
- `app/components/pages/navigation-pane/tab-panels/info/actors-info.tsx`
- `app/components/pages/loaders/page-loader.tsx`

### Views Data Layer + UI (19-04)
- `app/store/view.store.ts`
- `app/src/lib/hooks/use-views.ts`
- `app/src/lib/hooks/use-view-mutations.ts`
- `app/workspaces/[workspaceId]/projects/[projectId]/views/page.tsx`
- `app/components/views/views-list.tsx`
- `app/components/views/view-list-header.tsx`
- `app/components/views/view-list-item.tsx`
- `app/components/views/view-list-item-action.tsx`
- `app/components/views/modal.tsx`
- `app/components/views/form.tsx`
- `app/components/views/delete-view-modal.tsx`
- `app/components/views/quick-actions.tsx`
- `app/components/views/filters/filter-selection.tsx`
- `app/components/views/filters/order-by.tsx`
- `app/components/views/applied-filters/root.tsx`
- `app/components/issues/filters/filter-save-modal.tsx` (modified)
- `src/lib/services/issue-view.service.ts` (modified)
