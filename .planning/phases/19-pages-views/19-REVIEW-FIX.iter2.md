---
phase: 19-pages-views
fixed_at: 2026-06-30T19:00:00Z
review_path: .planning/phases/19-pages-views/19-REVIEW.md
iteration: 2
findings_in_scope: 11
fixed: 11
skipped: 0
status: all_fixed
---

# Phase 19: Code Review Fix Report

**Fixed at:** 2026-06-30T19:00:00Z
**Source review:** .planning/phases/19-pages-views/19-REVIEW.md
**Iteration:** 2

**Summary:**

- Findings in scope: 11
- Fixed: 11
- Skipped: 0

## Fixed Issues

### CR-01: Type mismatch between ViewModal.handleSubmit and ViewForm.onSubmit causes corrupt data persistence

**Files modified:** `yh-flow/clients/web/app/components/views/modal.tsx`
**Commit:** ad6c68620
**Applied fix:** Changed `handleSubmit` signature from `(name: string)` to `(data: { name: string; access: EViewAccess; description: string })` to match `ViewForm.onSubmit` prop type. Changed `createView.mutateAsync` call to use `data.name` instead of bare `name`. Added `EViewAccess` import.

### CR-02: `String.fromCodePoint(parseInt(...))` can throw RangeError on malformed emoji values

**Files modified:** `yh-flow/clients/web/app/components/pages/list/block.tsx`, `yh-flow/clients/web/app/components/sidebar/sidebar-tree.tsx`
**Commit:** 9242f43c5
**Applied fix:** In `block.tsx` renderIcon, added `Number.isSafeInteger(code) && code > 0` guard before `String.fromCodePoint`. In `sidebar-tree.tsx`, extracted safe parsing into `safeParseEmoji` helper with fallback to project name first character.

### WR-01: `helper.tsx` is empty dead code

**Files modified:** `yh-flow/clients/web/app/components/views/helper.tsx`
**Commit:** 6d887fe92
**Applied fix:** Removed empty file containing only `export {};`. Confirmed no imports reference this file.

### WR-02: Unused state variable in options-dropdown.tsx

**Files modified:** `yh-flow/clients/web/app/components/pages/editor/toolbar/options-dropdown.tsx`
**Commit:** 3592fd0ef
**Applied fix:** Removed unused `const [_, _set] = useState(false);` and the now-unnecessary `useState` import.

### WR-03: ColorDropdown is non-functional -- clicking colors does nothing

**Files modified:** `yh-flow/clients/web/app/components/pages/editor/toolbar/color-dropdown.tsx`
**Commit:** 57838221c
**Applied fix:** Added `onChange?: (color: string | null) => void` callback prop. Wired each preset color button's `onClick` to `onChange?.(color.textColor)` and the clear button to `onChange?.(null)`.

### WR-04: ViewForm does not initialize access/description from edit target

**Files modified:** `yh-flow/clients/web/app/components/views/form.tsx`
**Commit:** 57c2d6562
**Applied fix:** Added `defaultAccess?: EViewAccess` and `defaultDescription?: string` props. Changed `useState` initializations for `access` and `description` to use the new props instead of hardcoded defaults.

### WR-05: handleCopyLink in quick-actions.tsx uses workspaceSlug prop but URL pattern uses workspaceId

**Files modified:** `yh-flow/clients/web/app/components/views/quick-actions.tsx`
**Commit:** 6e0e64dbe
**Applied fix:** Renamed `workspaceSlug` prop to `workspaceId` in Props type, destructuring, and URL template literal.

### WR-06: delete-view-modal.tsx calls setIsDeleting(false) after onClose() may unmount component

**Files modified:** `yh-flow/clients/web/app/components/views/delete-view-modal.tsx`, `yh-flow/clients/web/app/components/pages/modals/delete-page-modal.tsx`
**Commit:** b929fff0e
**Applied fix:** Moved `onClose()` into `finally` block after `setIsDeleting(false)` to ensure state cleanup runs before potential unmount. In delete-page-modal, removed the duplicate `onClose()` call in the catch block.

### WR-07: favorite-control.tsx mutation lacks onSettled/onError handling causing UI flash on failure

**Files modified:** `yh-flow/clients/web/app/components/pages/header/favorite-control.tsx`, `yh-flow/clients/web/app/components/pages/list/block.tsx`
**Commit:** 4486172e5
**Applied fix:** Changed `onError` to `onSettled` in both `favoritePage.mutate` calls to ensure query invalidation runs on both success and failure, preventing stale UI state.

### WR-08: PageOrderByDropdown conditional handlers silently ignore toggle in already-active state

**Files modified:** `yh-flow/clients/web/app/components/pages/list/order-by.tsx`
**Commit:** 62f88cadb
**Applied fix:** Removed `if (isDescending)` and `if (!isDescending)` conditional guards from the ascending/descending menu item `onClick` handlers, allowing `onChange` to always fire.

### WR-09: Redundant and inconsistent type imports in root.store.ts

**Files modified:** `yh-flow/clients/web/app/store/root.store.ts`
**Commit:** e85789622
**Applied fix:** Consolidated five separate `import type` lines (`./types`, `./cycle.store`, `./module.store`, `./page.store`, `./view.store`) into a single import from `./types`, which already re-exports all store type interfaces.

---

_Fixed: 2026-06-30T19:00:00Z_
_Fixer: Claude (gsd-code-fixer)_
_Iteration: 2_
