---
phase: 19-pages-views
fixed_at: 2026-06-30T11:00:00Z
review_path: .planning/phases/19-pages-views/19-REVIEW.md
iteration: 1
findings_in_scope: 14
fixed: 14
skipped: 0
status: all_fixed
---

# Phase 19: Code Review Fix Report

**Fixed at:** 2026-06-30T11:00:00Z
**Source review:** .planning/phases/19-pages-views/19-REVIEW.md
**Iteration:** 1

**Summary:**

- Findings in scope: 14 (5 Critical + 9 Warning)
- Fixed: 14
- Skipped: 0

## Fixed Issues

### CR-01: Missing `useNavigate` import in `ViewsList`

**Files modified:** `yh-flow/clients/web/app/components/views/views-list.tsx`
**Commit:** e4d5de5f9
**Applied fix:** Added `import { useNavigate } from "react-router";` to the import block in ViewsList component. The component was calling `useNavigate()` on line 21 without importing it, which would cause a `ReferenceError` at runtime.

### CR-02: `PageEditorTitle` local state desync with async-loaded title prop

**Files modified:** `yh-flow/clients/web/app/components/pages/editor/title.tsx`
**Commit:** 99247ab29
**Applied fix:** Added `useEffect` import and a sync effect after `useState`: `useEffect(() => { if (!isEditing && title !== undefined) { setLocalTitle(title); } }, [title, isEditing]);`. This ensures that when `title` loads asynchronously after mount, the local state is updated before the user enters editing mode, preventing accidental overwrite of the page title with an empty string.

### CR-03: `ViewQuickActions` `handleCopyLink` generates malformed URL

**Files modified:** `yh-flow/clients/web/app/components/views/quick-actions.tsx`
**Commit:** 9edaabe1c
**Applied fix:** Changed the copy-link URL from `${workspaceSlug}/projects/${projectId}/views/${view.id}` (missing `/workspaces/` prefix) to `/workspaces/${workspaceSlug}/projects/${projectId}/views/${view.id}`, matching the route definition at `/workspaces/:workspaceId/projects/:projectId/views`.

### CR-04: `ViewForm` silently drops `access` and `description` on submit

**Files modified:** `yh-flow/clients/web/app/components/views/form.tsx`
**Commit:** 589c5e36d
**Applied fix:** (1) Updated `Props.onSubmit` type from `(name: string) => Promise<void>` to `(data: { name: string; access: EViewAccess; description: string }) => Promise<void>`. (2) Added `description` state with `useState`. (3) Updated `handleSubmit` to call `onSubmit({ name: name.trim(), access, description })`. (4) Added `value={description}` and `onChange={(e) => setDescription(e.target.value)}` to the description input to make it controlled.

### CR-05: `deletePage` in `PageHeaderActions` uses store-based modal API inconsistently

**Files modified:** `yh-flow/clients/web/app/components/pages/modals/delete-page-modal.tsx`
**Commit:** 1f1d55735
**Applied fix:** Updated the empty `catch` block in `DeletePageModal.handleDelete` to log the error and always call `onClose()` on failure, preventing the modal from getting stuck in a permanently visible state. (Note: this same fix also addresses WR-07.)

### WR-01: Empty catch blocks swallow errors in multiple locations

**Files modified:**

- `yh-flow/clients/web/app/components/issues/filters/filter-save-modal.tsx`
- `yh-flow/clients/web/app/components/pages/modals/page-form.tsx`
- `yh-flow/clients/web/app/components/views/delete-view-modal.tsx`
  **Commit:** f370516b9
  **Applied fix:** Added `console.error` logging with component-specific prefixes to all four empty catch blocks (the fourth, `delete-page-modal.tsx`, was already fixed as part of CR-05):
- `filter-save-modal.tsx`: `console.error("[FilterSaveModal] Failed to save view:", error)`
- `page-form.tsx`: `console.error("[PageForm] Failed to submit:", error)`
- `delete-view-modal.tsx`: `console.error("[DeleteViewModal] Failed to delete view:", error)`

### WR-02: Fire-and-forget `mutate()` calls without error handling cause UI state inconsistency

**Files modified:**

- `yh-flow/clients/web/app/components/pages/header/favorite-control.tsx`
- `yh-flow/clients/web/app/components/pages/list/block.tsx`
- `yh-flow/clients/web/app/components/pages/editor/header/logo-picker.tsx`
  **Commit:** 016ad8591
  **Applied fix:** Added `useQueryClient` import and `queryClient` instance in all three components. Added `onError` callback to the `mutate()` calls that invalidates the `["pages"]` query cache on failure, ensuring the UI reverts to the correct server state when an API error occurs.

### WR-03: Numeric access comparison instead of enum in `PagesListMainContent`

**Files modified:** `yh-flow/clients/web/app/components/pages/pages-list-main-content.tsx`
**Commit:** 9d4cdcb7e
**Applied fix:** Changed `p.access === 0` to `p.access === EPageAccess.PUBLIC` and `p.access === 1` to `p.access === EPageAccess.PRIVATE`. Added `EPageAccess` to the import from `@plane/types`.

### WR-04: `ViewListItem` uses `as any` to access fields not in `TIssueView` type

**Files modified:** `yh-flow/clients/web/app/components/views/view-list-item.tsx`
**Commit:** b52ae9d82
**Applied fix:** Created a `TIssueViewExtended` interface extending `TIssueView` with the additional fields (`access`, `is_favorite`, `description`, `owned_by`, `created_by`). Changed the cast from `view as any` to `view as TIssueViewExtended`, preserving type safety.

### WR-05: NaN from date parsing in sort comparator

**Files modified:**

- `yh-flow/clients/web/app/components/pages/pages-list-main-content.tsx`
- `yh-flow/clients/web/app/components/views/views-list.tsx`
  **Commit:** fef7dee65
  **Applied fix:** Changed date sorting from `new Date(a.created_at ?? 0).getTime() - new Date(b.created_at ?? 0).getTime()` to use conditional checks: `const aTime = a.created_at ? new Date(a.created_at).getTime() : 0; const bTime = ...; cmp = aTime - bTime; if (isNaN(cmp)) cmp = 0;`. Applied to both `created_at`/`createdAt` and `updated_at`/`updatedAt` fields in both files.

### WR-06: Inconsistent `EPageAccess` import sources

**Files modified:** `yh-flow/clients/web/app/components/pages/modals/page-form.tsx`
**Commit:** bc60a8e41
**Applied fix:** Changed `import { EPageAccess } from "@plane/constants"` to `import { EPageAccess } from "@plane/types"` to match the canonical import source used by `block.tsx` and other files.

### WR-07: `DeletePageModal.handleDelete` does not close modal on error

**Commit:** 1f1d55735 (same commit as CR-05)
**Applied fix:** This finding was addressed as part of CR-05. The empty `catch` block was replaced with a `catch (error)` that logs the error and calls `onClose()`, ensuring the modal is always closed even on deletion failure.

### WR-08: `ViewListItemAction` uses `observer` wrapper unnecessarily

**Files modified:** `yh-flow/clients/web/app/components/views/view-list-item-action.tsx`
**Commit:** f7fa6d067
**Applied fix:** Removed the `observer` import from mobx-react and changed the component from `const X = observer(function X(...)` to `export function X(...)`. The component does not access any MobX observables, making the `observer` wrapper unnecessary overhead.

### WR-09: `PageEditorBody` double-sets editor value on mount

**Files modified:** `yh-flow/clients/web/app/components/pages/editor/editor-body.tsx`
**Commit:** 8620bea51
**Applied fix:** Removed the mount-only `useEffect` (empty dependency array) that called `editorRef.current.setEditorValue(initialValueRef.current)`. The `DocumentEditorWithRef` component already receives `value={initialValue}` as a prop, making the initial mount-time `setEditorValue` call redundant. The remaining `useEffect` (with `[initialValue]` dependency) continues to update the ref and editor when `initialValue` changes.

## Skipped Issues

None -- all in-scope findings were successfully fixed.

---

_Fixed: 2026-06-30T11:00:00Z_
_Fixer: Claude (gsd-code-fixer)_
_Iteration: 1_
