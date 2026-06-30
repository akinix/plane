---
phase: 19-pages-views
reviewed: 2026-06-30T15:00:00Z
depth: standard
files_reviewed: 64
files_reviewed_list:
  - yh-flow/clients/web/app/components/issues/filters/filter-save-modal.tsx
  - yh-flow/clients/web/app/components/pages/dropdowns/actions.tsx
  - yh-flow/clients/web/app/components/pages/editor/editor-body.tsx
  - yh-flow/clients/web/app/components/pages/editor/header/logo-picker.tsx
  - yh-flow/clients/web/app/components/pages/editor/header/root.tsx
  - yh-flow/clients/web/app/components/pages/editor/page-root.tsx
  - yh-flow/clients/web/app/components/pages/editor/title.tsx
  - yh-flow/clients/web/app/components/pages/editor/toolbar/color-dropdown.tsx
  - yh-flow/clients/web/app/components/pages/editor/toolbar/options-dropdown.tsx
  - yh-flow/clients/web/app/components/pages/editor/toolbar/root.tsx
  - yh-flow/clients/web/app/components/pages/editor/toolbar/toolbar.tsx
  - yh-flow/clients/web/app/components/pages/header/actions.tsx
  - yh-flow/clients/web/app/components/pages/header/archived-badge.tsx
  - yh-flow/clients/web/app/components/pages/header/copy-link-control.tsx
  - yh-flow/clients/web/app/components/pages/header/favorite-control.tsx
  - yh-flow/clients/web/app/components/pages/header/root.tsx
  - yh-flow/clients/web/app/components/pages/list/applied-filters/root.tsx
  - yh-flow/clients/web/app/components/pages/list/block-item-action.tsx
  - yh-flow/clients/web/app/components/pages/list/block.tsx
  - yh-flow/clients/web/app/components/pages/list/filters/root.tsx
  - yh-flow/clients/web/app/components/pages/list/order-by.tsx
  - yh-flow/clients/web/app/components/pages/list/root.tsx
  - yh-flow/clients/web/app/components/pages/list/search-input.tsx
  - yh-flow/clients/web/app/components/pages/list/tab-navigation.tsx
  - yh-flow/clients/web/app/components/pages/loaders/page-content-loader.tsx
  - yh-flow/clients/web/app/components/pages/loaders/page-loader.tsx
  - yh-flow/clients/web/app/components/pages/modals/create-page-modal.tsx
  - yh-flow/clients/web/app/components/pages/modals/delete-page-modal.tsx
  - yh-flow/clients/web/app/components/pages/modals/page-form.tsx
  - yh-flow/clients/web/app/components/pages/navigation-pane/root.tsx
  - yh-flow/clients/web/app/components/pages/navigation-pane/tab-panels/info/actors-info.tsx
  - yh-flow/clients/web/app/components/pages/navigation-pane/tab-panels/info/document-info.tsx
  - yh-flow/clients/web/app/components/pages/navigation-pane/tab-panels/info/root.tsx
  - yh-flow/clients/web/app/components/pages/navigation-pane/tab-panels/outline.tsx
  - yh-flow/clients/web/app/components/pages/navigation-pane/tabs-list.tsx
  - yh-flow/clients/web/app/components/pages/pages-list-main-content.tsx
  - yh-flow/clients/web/app/components/pages/pages-list-view.tsx
  - yh-flow/clients/web/app/components/sidebar/sidebar-tree.tsx
  - yh-flow/clients/web/app/components/views/applied-filters/root.tsx
  - yh-flow/clients/web/app/components/views/delete-view-modal.tsx
  - yh-flow/clients/web/app/components/views/filters/filter-selection.tsx
  - yh-flow/clients/web/app/components/views/filters/order-by.tsx
  - yh-flow/clients/web/app/components/views/form.tsx
  - yh-flow/clients/web/app/components/views/helper.tsx
  - yh-flow/clients/web/app/components/views/modal.tsx
  - yh-flow/clients/web/app/components/views/quick-actions.tsx
  - yh-flow/clients/web/app/components/views/view-list-header.tsx
  - yh-flow/clients/web/app/components/views/view-list-item-action.tsx
  - yh-flow/clients/web/app/components/views/view-list-item.tsx
  - yh-flow/clients/web/app/components/views/views-list.tsx
  - yh-flow/clients/web/app/routes.ts
  - yh-flow/clients/web/app/store/page.store.ts
  - yh-flow/clients/web/app/store/root.store.ts
  - yh-flow/clients/web/app/store/types.ts
  - yh-flow/clients/web/app/store/view.store.ts
  - yh-flow/clients/web/app/workspaces/[workspaceId]/pages/[pageId]/page.tsx
  - yh-flow/clients/web/app/workspaces/[workspaceId]/pages/page.tsx
  - yh-flow/clients/web/app/workspaces/[workspaceId]/projects/[projectId]/views/page.tsx
  - yh-flow/clients/web/src/lib/hooks/index.ts
  - yh-flow/clients/web/src/lib/hooks/use-page-mutations.ts
  - yh-flow/clients/web/src/lib/hooks/use-pages.ts
  - yh-flow/clients/web/src/lib/hooks/use-view-mutations.ts
  - yh-flow/clients/web/src/lib/hooks/use-views.ts
  - yh-flow/clients/web/src/lib/mock-data.ts
  - yh-flow/clients/web/src/lib/services/issue-view.service.ts
findings:
  critical: 2
  warning: 11
  info: 8
  total: 21
status: issues_found
---

# Phase 19: Code Review Report

**Reviewed:** 2026-06-30T15:00:00Z
**Depth:** standard
**Files Reviewed:** 64
**Status:** issues_found

## Summary

Reviewed 64 files from the Pages and Views feature modules across stores, hooks, services, and UI components. The codebase follows the forked Plane architecture pattern consistently. Two critical issues were found: page emojis fail to render in the card list due to incompatible encoding format between mock data and the parsing logic, and the View edit modal opens empty because the selected view ID is not set before opening the modal. Several warnings around unhandled promise rejections (clipboard, auto-save, archive), unsafe type assertions, and a local state that is never propagated to the parent are also present.

---

## Critical Issues

### CR-01: Page emoji renders as `FileText` fallback for all pages due to data format mismatch

**File:** `yh-flow/clients/web/app/components/pages/list/block.tsx:42-44`

**Issue:** The `renderIcon` function uses `parseInt(logo_props.emoji.value, 10)` to parse the emoji value as a numeric codepoint, then renders it with `String.fromCodePoint(code)`. However, the page mock data (`MOCK_PAGES` in `mock-data.ts`) stores literal emoji characters (e.g. `"📋"`, `"🔌"`, `"🎨"`) in `logo_props.emoji.value`, not decimal codepoint strings. `parseInt("📋", 10)` returns `NaN`, `Number.isSafeInteger(NaN)` is `false`, so every page card falls through to show the generic `FileText` icon instead of the correct emoji.

This is a data-format mismatch: project `logo_props` in mock data use numeric codepoint strings (e.g. `"128640"`) which work with `parseInt`/`fromCodePoint`, but page `logo_props` use literal emoji characters which don't.

**Fix:** Detect whether the emoji value is a numeric codepoint or a literal emoji before parsing. A literal emoji can be rendered directly without `fromCodePoint`:

```typescript
const renderIcon = () => {
  const emojiValue = logo_props?.emoji?.value;
  if (logo_props?.in_use === "emoji" && emojiValue) {
    const code = parseInt(emojiValue, 10);
    if (Number.isSafeInteger(code) && code > 0) {
      return <span className="text-lg">{String.fromCodePoint(code)}</span>;
    }
    // Literal emoji (not a codepoint string) — render directly
    return <span className="text-lg">{emojiValue}</span>;
  }
  return <FileText className="text-custom-text-300 size-5" />;
};
```

---

### CR-02: View edit modal opens with empty form because `selectedViewId` is not set before opening

**File:** `yh-flow/clients/web/app/components/views/view-list-item.tsx:117-119`

**Issue:** The `onEdit` handler calls `viewStore.openViewModal("edit")` but never sets `viewStore.selectedViewId` to the current view's ID. Inside `ViewModal` (`modal.tsx:25-29`), the edit view lookup depends on `viewStore.selectedViewId`:

```typescript
const editView = useMemo(() => {
  if (viewStore.viewModalMode !== "edit") return null;
  if (!views) return null;
  return views.find((v) => v.id === viewStore.selectedViewId) ?? null;
}, [viewStore.viewModalMode, viewStore.selectedViewId, views]);
```

Since `selectedViewId` is never updated to the current `view.id`, `editView` will always be `null` in edit mode and the form will show an empty name/description.

**Fix:** Set `selectedViewId` before opening the modal:

```typescript
onEdit={() => {
  viewStore.setSelectedViewId(view.id);
  viewStore.openViewModal("edit");
}}
```

---

## Warnings

### WR-01: Clipboard `writeText` promise not handled on failure

**Files:**

- `yh-flow/clients/web/app/components/pages/dropdowns/actions.tsx:34`
- `yh-flow/clients/web/app/components/pages/header/copy-link-control.tsx:19`

**Issue:** `navigator.clipboard.writeText()` returns a Promise that can reject (permissions denied, HTTP context, etc.). Neither call site handles the rejection, resulting in an unhandled promise rejection.

**Fix:** Add `.catch()`:

```typescript
navigator.clipboard.writeText(url).catch(() => {
  // fallback: create temporary input element for older browsers
});
```

---

### WR-02: Logo picker triggers mutation without changing any value

**File:** `yh-flow/clients/web/app/components/pages/editor/header/logo-picker.tsx:21-39`

**Issue:** `handleLogoClick` reads `page.logo_props?.emoji?.value || "📄"` and sets the same value back via `updatePage.mutate`. This is a no-op API call that triggers a network/state update for no benefit. The component also calls `queryClient.invalidateQueries` only on error (line 35-38) — on success the cache goes stale.

**Fix:** Either implement the actual emoji picker (with a different value selection) or guard the mutation:

```typescript
const handleLogoClick = () => {
  // Placeholder: emoji picker not yet implemented
};
```

---

### WR-03: Auto-save failure silently swallowed in editor

**File:** `yh-flow/clients/web/app/components/pages/editor/page-root.tsx:31`

**Issue:** The auto-save debounce handler calls `updatePage.mutate()` with no error callback. If the save fails (network error, permission denied), the user receives no feedback and the content change is silently lost.

**Fix:** Add error handling:

```typescript
debounceRef.current = setTimeout(() => {
  updatePage.mutate(
    { pageId, data: { description_html: html } },
    {
      onError: () => {
        /* show toast */
      },
    }
  );
}, 1500);
```

---

### WR-04: DeletePageModal reads from global MobX store instead of local state

**File:** `yh-flow/clients/web/app/components/pages/editor/header/root.tsx:115-121`

**Issue:** The `DeletePageModal` receives `pageId`, `pageName`, and `isOpen` from the MobX store (`store.page.deletePageId`, `store.page.deletePageName`, `store.page.pageDeleting`) rather than from local state. This couples the modal to global store state: if another component modifies `deletePageId` or `pageDeleting` for any reason, this modal's content and visibility will change unexpectedly.

The `dropdowns/actions.tsx` (line 87-95) already implements the correct pattern using local `useState` — the editor header should follow the same pattern.

**Fix:** Use local state:

```typescript
const [deleteModalOpen, setDeleteModalOpen] = useState(false);
// ...
<DeletePageModal
  pageId={page.id}
  pageName={page.name}
  isOpen={deleteModalOpen}
  onClose={() => setDeleteModalOpen(false)}
  workspaceId={workspaceId}
/>
```

---

### WR-05: Title blur/focus cycle causes visual flicker

**File:** `yh-flow/clients/web/app/components/pages/editor/title.tsx:26-42`

**Issue:** `handleBlur` immediately sets `isEditing = false` (causing component to render the static button), then schedules a save 800ms later. If the user clicks back on the title within 800ms, `handleFocus` cancels the timer but `isEditing` was already `false`, causing a brief render of the static title before the click re-enters edit mode.

**Fix:** Defer `setIsEditing(false)` until after save:

```typescript
const handleBlur = useCallback(() => {
  if (debounceRef.current) clearTimeout(debounceRef.current);
  debounceRef.current = setTimeout(() => {
    setIsEditing(false);
    if (localTitle !== title) {
      updatePage.mutate({ pageId, data: { name: localTitle } });
    }
  }, 800);
}, [localTitle, title, pageId, updatePage]);
```

---

### WR-06: Whitespace-only search query causes inconsistent Escape behavior

**File:** `yh-flow/clients/web/app/components/pages/list/search-input.tsx:20`

**Issue:** When search input contains only whitespace (e.g. `"   "`), pressing Escape closes the panel instead of clearing the query. The condition `searchQuery && searchQuery.trim() !== ""` evaluates to `true && false` (false) for whitespace-only strings, skipping the clear branch.

**Fix:** Check the trimmed value directly:

```typescript
if (e.key === "Escape") {
  if (searchQuery.trim() !== "") {
    updateSearchQuery("");
  } else {
    setIsSearchOpen(false);
    inputRef.current?.blur();
  }
}
```

---

### WR-07: Archive/delete mutation error handling closes confirmation prematurely

**Files:**

- `yh-flow/clients/web/app/components/pages/header/actions.tsx:20-23`
- `yh-flow/clients/web/app/components/pages/modals/delete-page-modal.tsx:25-36`

**Issue:** `archivePage.mutateAsync(page.id)` is awaited but any error is uncaught. If the archive fails, `setShowArchiveConfirm(false)` still runs on the next line, dismissing the confirmation dialog and giving false success feedback. The `delete-page-modal.tsx` variant has the same issue: `onClose()` runs in the `finally` block even when the delete failed.

**Fix:** Only close on success:

```typescript
const handleArchive = async () => {
  try {
    await archivePage.mutateAsync(page.id);
    setShowArchiveConfirm(false);
  } catch (error) {
    // show error toast
  }
};
```

---

### WR-08: View filter selection local search state never propagated to parent

**File:** `yh-flow/clients/web/app/components/views/filters/filter-selection.tsx:16,40-46`

**Issue:** The component maintains a local `searchQuery` state but never calls `onFiltersUpdate` with the updated query. The search input (lines 40-46) only updates local state, making the search functionally dead.

**Fix:** Integrate the search into `onFiltersUpdate`:

```typescript
const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
  const value = e.target.value;
  setSearchQuery(value);
  onFiltersUpdate({
    ...filters,
    searchQuery: value,
  });
};
```

---

### WR-09: `ViewListItem` unsafe type assertion and incorrect avatar rendering

**File:** `yh-flow/clients/web/app/components/views/view-list-item.tsx:33-38,85`

**Issue (type assertion, line 33):** `const ext = view as TIssueViewExtended;` casts the view to an extended type with fields (`access`, `is_favorite`, `description`, `owned_by`) that don't exist on the base `TIssueView` type. If a view from a real API response lacks these fields, values will be `undefined`, causing incorrect rendering.

**Issue (avatar, line 85):** `ownedBy.slice(-1).toUpperCase()` takes the last character of the `owned_by` value (a user ID like `"user-1"`) and uses it as the avatar letter. This shows `"1"` or `"-"` instead of a user initial.

**Fix:** Extend the type properly at the component boundary and use a sensible avatar fallback:

```typescript
<div className="...">
  {"U"}
</div>
```

---

### WR-10: `ViewsList` uses `as any` to access `created_by`

**File:** `yh-flow/clients/web/app/components/views/views-list.tsx:29`

**Issue:** `(v as any).created_by === "user-1"` bypasses TypeScript type checking to access a field not on `TIssueView`. This masks potential bugs if the field name changes or the API response shape differs.

**Fix:** Extend the view type at the component boundary:

```typescript
return views.filter((v) => (v as TIssueView & { created_by?: string }).created_by === "user-1");
```

---

### WR-11: View modal creates view without `access` and `description` fields

**File:** `yh-flow/clients/web/app/components/views/modal.tsx:38-47`

**Issue:** The `createView.mutateAsync` call passes view data without the `access` or `description` properties. The `ViewForm` component collects `access` and `description` from user input but `handleSubmit` ignores them in both create and edit paths. On create, the view gets default/undefined access; on edit, only `name` is updated while `access` and `description` are silently dropped.

**Fix:** Pass all form data:

```typescript
const handleSubmit = async (data: { name: string; access: EViewAccess; description: string }) => {
  if (viewStore.viewModalMode === "edit" && editView) {
    await updateView.mutateAsync({
      viewId: editView.id,
      data: { name: data.name, access: data.access, description: data.description },
    });
  } else {
    await createView.mutateAsync({
      name: data.name,
      access: data.access,
      projectId,
      filters: { stateIds: [], priorityIds: [], assigneeIds: [], labelIds: [], searchQuery: "", dateRange: null },
      sort: { sortBy: "updated_at", sortDirection: "desc" },
      groupBy: "state",
      subGroupBy: "none",
      displayColumns: ["state", "priority", "assignee", "labels", "created_at"],
      layout: "list",
    });
  }
  viewStore.closeViewModal();
};
```

---

## Info

### IN-01: Unused variables and parameters across components

| File                            | Line | Symbol                                        |
| ------------------------------- | ---- | --------------------------------------------- |
| `filter-save-modal.tsx`         | 34   | `workspaceId: _workspaceId`                   |
| `logo-picker.tsx`               | 16   | `_setIsOpen`                                  |
| `header/root.tsx` (editor)      | 25   | `_onToggleNavigationPane`                     |
| `title.tsx`                     | 10   | `_workspaceId`                                |
| `delete-view-modal.tsx`         | 17   | `_viewName`, `_projectId`                     |
| `quick-actions.tsx`             | 19   | `_onEdit`                                     |
| `list/applied-filters/root.tsx` | 5-6  | `handleClearAllFilters`, `handleRemoveFilter` |

---

### IN-02: Type casts with `as any` bypass type safety

**Files:**

- `pages-list-main-content.tsx:94-95`
- `view-list-header.tsx:75-76`
- `views-list.tsx:29`
- `sidebar-tree.tsx:60-61`

---

### IN-03: Placeholder menu items with no onClick handler

**File:** `options-dropdown.tsx:31`

The "字数统计" menu item renders without an `onClick` handler.

---

### IN-04: Toolbar component has no connection to editor instance

**File:** `toolbar/toolbar.tsx:39-95`

All formatting buttons (bold, italic, heading, lists, link, quote, code) render UI but the `onFormat` callback is never wired from `PageEditorRoot` or any parent. The toolbar is visually present but functionally dead.

---

### IN-05: `console.error` as sole error handling in catch blocks

**Files:** `filter-save-modal.tsx:63`, `create-page-modal.tsx:41`, `delete-page-modal.tsx:31`, `page-form.tsx:29`, `delete-view-modal.tsx:26`

All mutations use `console.error()` without user-facing notifications. Acceptable during mock-data phase but must be replaced with toast/notification system before production.

---

### IN-06: Navigation pane tabs-list declares controlled API but is used uncontrolled

**File:** `tabs-list.tsx:12-15`

Declares `activeTab` and `onTabChange` props but parent (`navigation-pane/root.tsx:40`) renders without any props, always using internal state.

---

### IN-07: `window.location.reload()` as error recovery can cause infinite loop

**File:** `page-root.tsx:52`

Error state uses `window.location.reload()` as retry. If the error persists (invalid page ID, server down), this causes an infinite reload loop. Use `query.refetch()` instead.

---

### IN-08: Sidebar pages route regex matches child paths

**File:** `sidebar-tree.tsx:60`

Regex `/\/workspaces\/([^/]+)\/pages/` matches URLs like `/workspaces/ws1/pages/page123/settings`. If pages gain sub-routes, the active highlighting will activate for child routes. Add a boundary assertion.

---

_Reviewed: 2026-06-30T15:00:00Z_
_Reviewer: Claude (gsd-code-reviewer)_
_Depth: standard_
