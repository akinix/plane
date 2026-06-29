# Spreadsheet View — 电子表格视图

## Overview

Forked from Plane `apps/web/core/components/issues/issue-layouts/spreadsheet/` (AGPL-3.0).

Provides a spreadsheet-style (table) view for issues, supporting:
- 16 column types (state, priority, assignee, dates, labels, etc.)
- Inline cell editing with click-to-edit (SHEE-02)
- Column width drag resizing
- Row multi-select + bulk editing
- Horizontal scroll with sticky first column
- Loading skeleton, empty state ("暂无 Issue"), error state

## Architecture

```
spreadsheet-view/
├── README.md                  # This file
├── spreadsheet-view.tsx       # Main container — data fetching, state management
├── base-spreadsheet-root.tsx  # Root wrapper (issue layout HOC)
├── spreadsheet-table.tsx      # Table body (virtual scroll, grid layout)
├── spreadsheet-header.tsx     # Sticky header row
├── spreadsheet-header-column.tsx  # Single header column (sort + resize)
├── issue-row.tsx              # Single issue row (horizontal scroll)
├── issue-column.tsx           # Wrapper for individual column cells
└── columns/
    ├── index.ts               # Column registry
    ├── state-column.tsx
    ├── priority-column.tsx
    ├── assignee-column.tsx
    ├── due-date-column.tsx
    ├── start-date-column.tsx
    ├── label-column.tsx
    ├── cycle-column.tsx
    ├── module-column.tsx
    ├── estimate-column.tsx
    ├── created-on-column.tsx   # Read-only
    ├── updated-on-column.tsx   # Read-only
    ├── attachment-column.tsx   # Read-only
    ├── link-column.tsx         # Read-only
    ├── sub-issue-column.tsx    # Read-only
    └── header-column.tsx       # Row header (ID + drag handle + checkbox)
```

## Data Flow

1. `spreadsheet-view.tsx` uses `useIssues(projectId)` (TanStack Query) to fetch issues
2. Uses `useStore().issue.visibleColumnIds` to determine visible columns
3. Renders `<SpreadsheetTable>` with header + rows + columns
4. Inline editing: click cell -> show native control -> onChange -> `updateIssue.mutate` (800ms debounce)
5. Multi-select: `useStore().issue.selectedIssueIds` + `BulkActionBar`

## Differences from Plane

- Simplified architecture (no MultipleSelectGroup, no SelectionHelper)
- Native HTML controls instead of @plane/ui Dropdown components
- Column registry in `columns/index.ts` (no SPREADSHEET_COLUMNS from plane-web)
- Uses yh-flow's MobX store (`useStore().issue.visibleColumnIds`)
- No virtual scrolling (full DOM rendering for mock/large tables)
- All Plane-originated files marked with `// FLOW: Forked from Plane spreadsheet/`
