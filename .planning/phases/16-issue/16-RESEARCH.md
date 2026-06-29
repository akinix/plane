# Phase 16: Issue 列表/详情 & 看板 - Research

**Researched:** 2026-06-29
**Domain:** Issue CRUD, Kanban board, command palette, rich text editing
**Confidence:** HIGH

## Summary

Phase 16 is the largest single-phase implementation in the v2.0 milestone, covering ISSU-01~08, KANB-01~05, and UI-04. It builds on Phase 15's infrastructure (TanStack Query + MobX + React Router v7) and adds three major new capabilities: Issue CRUD with list/detail views, Kanban board with drag-and-drop, and a global command palette.

The implementation target is `yh-flow/clients/web/` (not `apps/web/` which is the Plane reference). The project already has a complete Plane-forked type system (`src/lib/types/`), TipTap editor (`src/lib/editor/`), UI component library (`src/lib/ui/`), and TanStack Query hooks pattern (`src/lib/hooks/`). Phase 16 needs to add `@hello-pangea/dnd` for Kanban drag-and-drop and `cmdk` for command palette, extend the mock data layer, create about 14 new components, and wire up the issue routes.

**Primary recommendation:** Follow the exact patterns established in Phase 15: TanStack Query hooks for server data (mock layer first), MobX for UI-only state, observer() wrapping for components, and flat route configuration. Use Plane's existing UI components directly; only build what doesn't exist.

<user_constraints>

## User Constraints (from CONTEXT.md)

### Locked Decisions

#### Grey Area 1: Page Architecture & State Management

- D-P16-01: Issue view directory structure is `app/issues/` (routes/pages) + `app/components/issues/` (components)
- D-P16-02: Use TanStack Query hooks for server-side Issue data; MobX IssueStore manages UI state only (expand/collapse, selection, kanban scroll position)
- D-P16-03: Extend existing `mock-data.ts` with issue/state/label/priority mock data, don't create separate file
- D-P16-04: Route format: `/workspaces/:workspaceId/projects/:projectId/issues/` (list) + `/workspaces/:workspaceId/projects/:projectId/issues/:issueId` (detail), no React Router nested routing

#### Grey Area 2: Issue List View

- D-P16-05: List rows use medium density (title, Issue ID, priority label, assignee avatar, status badge, estimate)
- D-P16-06: Filter/sort UI uses top-bar quick filter chips + expandable panel for more conditions
- D-P16-07: Use pagination (20 per page) with page controls, not infinite scroll
- D-P16-08: Initial implementation uses fixed columns (ID + title + state + priority + assignee + updated), custom columns deferred to Phase 17

#### Grey Area 3: Issue Detail Page

- D-P16-09: Detail page uses two-column layout — left Issue description, right property panel
- D-P16-10: Property fields use inline editing: click field to expand inline dropdown/popup editor
- D-P16-11: Comments positioned below Issue description, order: comment list -> comment input
- D-P16-12: Issue description editor reuses forked @plane/editor (already stripped Yjs), not textarea/Markdown

#### Grey Area 4: Kanban View & Command Palette

- D-P16-13: Drag-and-drop uses `@hello-pangea/dnd` (react-beautiful-dnd maintained fork)
- D-P16-14: Default group by state column; support switch to group by assignee/priority
- D-P16-15: Sub-group (Swimlane) deferred to Phase 17; this phase only implements single-layer column grouping
- D-P16-16: Command palette as standalone CommandPalette component, Cmd+K global trigger, supports search project/Issue/page navigation

### Claude's Discretion

- Kanban column collapse/expand UI implementation details
- List view row click vs checkbox selection interaction details
- Empty state, loading state, error state presentation details
- Comment editing UI mode (inline edit vs modal)
- Sidebar tree Issue view route placeholder update location
- @hello-pangea/dnd animation and transition configuration
- Command palette search ordering and keyboard navigation details

### Deferred Ideas (OUT OF SCOPE)

- Sub-group/Swimlane (Phase 17)
- Custom list column configuration (IIssueDisplayProperties) — Phase 17 FILT-03
- Infinite scroll/virtual list
- Issue relation/dependency management
- Issue Link (associated links)
  </user_constraints>

<phase_requirements>

## Phase Requirements

| ID        | Description                                                                   | Research Support                                                                   |
| --------- | ----------------------------------------------------------------------------- | ---------------------------------------------------------------------------------- |
| ISSU-01   | Create Issue (title, description, priority, assignee, labels)                 | IssueCreateModal component + useIssueMutations hook; @plane/editor for description |
| ISSU-02   | Issue list view (state filter, sort, pagination)                              | IssueListView + QuickFilterBar + TanStack Query; pagination via page number state  |
| ISSU-03   | Issue detail page (property panel, comments, activity log)                    | IssueDetailMain + IssueDetailSidebar + PropertyEditor + CommentList                |
| ISSU-04   | Edit Issue properties (state, priority, assignee, labels, estimate, due date) | PropertyEditor inline editing; useMutation with optimistic update                  |
| ISSU-05   | Delete Issue (soft delete)                                                    | DeleteIssueModal; confirmation dialog pattern from existing alert-modal            |
| ISSU-06   | Create/edit/delete comments                                                   | CommentList + CommentInput; uses plane/editor's LiteTextEditor for rich text       |
| ISSU-07   | Bulk operations (status change, assign, delete)                               | BulkActionBar; checkbox selection via MobX IssueStore                              |
| ISSU-08   | TipTap rich text editor for Issue description                                 | @plane/editor RichTextEditorWithRef (already forked at src/lib/editor/)            |
| KANBAN-01 | Kanban board view (columns = states)                                          | IssuesKanbanView + KanbanColumn (horizontal scroll, 280px each)                    |
| KANBAN-02 | Drag card between columns (state change)                                      | @hello-pangea/dnd for drag-and-drop; useMutation to update state                   |
| KANBAN-03 | Group By (assignee/priority) in kanban                                        | Group By selector; MobX state for active group_by                                  |
| KANBAN-04 | Sub-group/Swimlane                                                            | DEFERRED to Phase 17                                                               |
| KANBAN-05 | Filtering and sorting within kanban                                           | Shared filter state with list view (D-P16-16 note)                                 |
| UI-04     | Command palette (Cmd+K)                                                       | CommandPalette component using cmdk; search issues + navigate                      |

</phase_requirements>

## Architectural Responsibility Map

| Capability             | Primary Tier                      | Secondary Tier | Rationale                                                                               |
| ---------------------- | --------------------------------- | -------------- | --------------------------------------------------------------------------------------- |
| Issue data fetching    | Browser (TanStack Query)          | —              | Server data fetched via TanStack Query hooks from mock layer; future real API           |
| Issue state management | Browser (MobX)                    | —              | IssueStore only manages UI state (selection, expanded, scroll position)                 |
| Issue CRUD mutations   | Browser (TanStack Query)          | —              | useMutation hooks with optimistic updates                                               |
| Kanban drag-and-drop   | Browser (React/DOM)               | —              | @hello-pangea/dnd operates entirely in browser; state update triggers TanStack mutation |
| Command palette search | Browser (React)                   | —              | Client-side fuzzy search over available items; future API search                        |
| Rich text editing      | Browser (TipTap/ProseMirror)      | —              | @plane/editor runs entirely in browser; persistence via mutation                        |
| Route configuration    | Frontend server (React Router v7) | —              | Routes defined statically in app/routes.ts                                              |

## Standard Stack

### Core

| Library                      | Version          | Purpose                  | Why Standard                                                                 |
| ---------------------------- | ---------------- | ------------------------ | ---------------------------------------------------------------------------- |
| @tanstack/react-query        | ^5.0.0           | Server state management  | Phase 15 established pattern; useQuery + useMutation                         |
| mobx + mobx-react            | ^6.12.0 + ^9.1.0 | UI-only state management | Phase 15 established pattern; observed components                            |
| react-router                 | ^7.0.0           | Client-side routing      | Existing project dependency; flat route config                               |
| @tiptap/react + @tiptap/core | ^2.27.2          | Rich text editing        | Underlying editor engine; @plane/editor wraps this                           |
| @plane/editor (fork)         | workspace:\*     | Rich text editor wrapper | D-P16-12 decision; forked at src/lib/editor/                                 |
| @plane/ui (fork)             | workspace:\*     | UI component library     | Forked at src/lib/ui/; provides Avatar, Badge, Button, Dropdown, Modal, etc. |

### New Dependencies to Add

| Library           | Version | Purpose                   | Why Standard                                            |
| ----------------- | ------- | ------------------------- | ------------------------------------------------------- |
| @hello-pangea/dnd | ^18.0.0 | Kanban drag-and-drop      | D-P16-13 decision; React 19 compatible since v18.0.0    |
| cmdk              | ^1.1.1  | Command palette primitive | D-P16-16 decision; lightweight, composable, Radix-based |

### Supporting

| Component      | Path                          | Purpose                                           |
| -------------- | ----------------------------- | ------------------------------------------------- |
| avatar         | src/lib/ui/avatar/            | User avatars in list rows, kanban cards, comments |
| badge          | src/lib/ui/badge/             | Status badges, priority labels                    |
| button         | src/lib/ui/button/            | Primary CTA "创建 Issue", secondary actions       |
| dropdown       | src/lib/ui/dropdown/          | Property editors (single-select, multi-select)    |
| modals         | src/lib/ui/modals/            | Issue create modal, confirmation dialogs          |
| popover        | src/lib/ui/popovers/          | Inline property editor popups                     |
| loader         | src/lib/ui/loader.tsx         | Loading/skeleton states                           |
| drag-handle    | src/lib/ui/drag-handle.tsx    | Kanban card drag handle                           |
| drop-indicator | src/lib/ui/drop-indicator.tsx | Kanban drop zone indicator                        |

### Alternatives Considered

| Instead of        | Could Use                                          | Tradeoff                                                                            |
| ----------------- | -------------------------------------------------- | ----------------------------------------------------------------------------------- |
| @hello-pangea/dnd | @atlaskit/pragmatic-drag-and-drop (Plane original) | @hello-pangea has simpler API; Plane's choice requires more boilerplate             |
| cmdk              | modern-cmdk                                        | modern-cmdk is React 19 native but newer/less proven; cmdk is battle-tested         |
| @hello-pangea/dnd | dnd-kit                                            | dnd-kit is more flexible but less kanban-focused; @hello-pangea has kanban examples |

**Installation:**

```bash
cd yh-flow/clients/web
pnpm add @hello-pangea/dnd@^18.0.0 cmdk@^1.1.1
```

**Version verification:**

```bash
npm view @hello-pangea/dnd version
# Expected: ^18.0.0
npm view cmdk version
# Expected: ^1.1.1
```

## Package Legitimacy Audit

| Package           | Registry | Age    | Downloads | Source Repo                 | slopcheck | Disposition                                                  |
| ----------------- | -------- | ------ | --------- | --------------------------- | --------- | ------------------------------------------------------------ |
| @hello-pangea/dnd | npm      | ~2 yrs | ~500K/wk  | github.com/hello-pangea/dnd | —         | Approved — well-known maintained fork of react-beautiful-dnd |
| cmdk              | npm      | ~3 yrs | ~1.5M/wk  | github.com/pacocoursey/cmdk | —         | Approved — established library, 12.7k stars                  |

**Packages removed due to slopcheck [SLOP] verdict:** none
**Packages flagged as suspicious [SUS]:** none

> Note: slopcheck not run (Python tool not in environment). Both packages are well-known and widely used -- confidence HIGH based on community adoption and prior verification. If strict verification is needed, run `pip install slopcheck && slopcheck install @hello-pangea/dnd cmdk --json`.

## Architecture Patterns

### System Architecture Diagram

```
User Input
  │
  ├── Route: /workspaces/:wsId/projects/:projId/issues/
  │     │
  │     ├── IssueListView
  │     │     ├── QuickFilterBar (state/priority/assignee filter chips)
  │     │     ├── SortDropdown (field + direction)
  │     │     ├── IssueRow[] (checkbox + columns)
  │     │     ├── Pagination (< < 1 2 3 ... N > >)
  │     │     └── BulkActionBar (shown when rows selected)
  │     │
  │     └── IssuesKanbanView
  │           ├── GroupBySelector (state/priority/assignee)
  │           ├── KanbanColumn[] (280px, horizontal scroll)
  │           │     └── KanbanCard[] (with DragHandle)
  │           └── DropZone (column change)
  │
  ├── Route: /workspaces/:wsId/projects/:projId/issues/:issueId
  │     │
  │     ├── IssueDetailMain (left 65%)
  │     │     └── @plane/editor (rich text description)
  │     │
  │     ├── IssueDetailSidebar (right 35%)
  │     │     └── PropertyEditor[] (inline: state, priority, assignee, labels, estimate, due date)
  │     │
  │     ├── CommentList
  │     │     └── CommentInput (LiteTextEditor)
  │     │
  │     └── ActivityLog
  │
  ├── Modal: IssueCreateModal (or slide-over panel)
  │
  ├── Modal: DeleteIssueModal / BulkDeleteConfirmModal
  │
  └── Global: CommandPalette (Cmd+K, search issues + navigate)
        │
        └── cmdk library (Command.Dialog, Command.Input, Command.List, Command.Item)
```

**Data flow for primary use case (create issue):**

1. User clicks "创建 Issue" button -> IssueCreateModal opens
2. User fills title, selects priority/assignee/labels, enters description via @plane/editor
3. Submit -> useMutation -> optimistic add to cache -> invalidate query -> redirect to detail page

**Data flow for kanban drag:**

1. User drags card -> @hello-pangea/dnd onDragEnd fires
2. Extract source/destination droppableId (column state)
3. useMutation to update issue state -> optimistic update -> invalidate query

### Recommended Project Structure

```
yh-flow/clients/web/
├── app/
│   ├── issues/                                  # Issue routes (D-P16-01)
│   │   ├── page.tsx                             # Issue list + kanban layout (view toggle)
│   │   └── [issueId]/
│   │       └── page.tsx                         # Issue detail page
│   ├── components/
│   │   ├── issues/                              # Issue components (D-P16-01)
│   │   │   ├── issue-create-modal.tsx           # CREATE: modal/slide-over form
│   │   │   ├── issue-list-view.tsx              # LIST: container with header + rows + pagination
│   │   │   ├── issue-row.tsx                    # LIST: single row (checkbox + columns)
│   │   │   ├── quick-filter-bar.tsx             # LIST: filter chips + expandable panel
│   │   │   ├── issue-detail-main.tsx            # DETAIL: left panel (editor)
│   │   │   ├── issue-detail-sidebar.tsx         # DETAIL: right panel (properties)
│   │   │   ├── property-editor.tsx              # DETAIL: inline property field editor
│   │   │   ├── comment-list.tsx                 # DETAIL: comment list display
│   │   │   ├── comment-input.tsx                # DETAIL: comment creation input
│   │   │   ├── issues-kanban-view.tsx           # KANBAN: container + columns
│   │   │   ├── kanban-column.tsx                # KANBAN: single column
│   │   │   ├── kanban-card.tsx                  # KANBAN: single card
│   │   │   ├── bulk-action-bar.tsx              # BULK: floating action bar
│   │   │   └── delete-issue-modal.tsx           # DELETE: confirmation dialog
│   │   └── command-palette.tsx                  # GLOBAL: Cmd+K command palette
│   ├── store/
│   │   ├── types.ts                             # + IssueStore interface (ISSUE UI state)
│   │   ├── issue.store.ts                       # IssueStore (selectedIds, expanded, viewMode, filter state)
│   │   └── root.store.ts                        # + register IssueStore
│   └── routes.ts                                # + issue list/detail routes
└── src/
    └── lib/
        ├── mock-data.ts                         # + issues, states, labels, comments, activities
        ├── hooks/
        │   ├── use-issues.ts                    # TanStack Query: useIssues, useIssue
        │   ├── use-issue-mutations.ts           # TanStack Query: useCreateIssue, useUpdateIssue, useDeleteIssue
        │   ├── use-comments.ts                  # TanStack Query: useComments, useCreateComment, etc.
        │   └── index.ts                         # barrel export
        ├── types/
        │   └── issues/ (already exists)         # TIssue, TIssueComment, IState, IIssueLabel
        └── ui/ (already exists)                 # Avatar, Badge, Button, Dropdown, Modal, etc.
```

### Pattern 1: TanStack Query Hook Pattern

**What:** TanStack Query hooks follow a consistent pattern established in Phase 15: useQuery for fetching, useMutation for mutations, with mock data + artificial delay.

**Example** (following Phase 15 pattern at `src/lib/hooks/use-workspaces.ts`):

```typescript
// src/lib/hooks/use-issues.ts
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import type { TIssue } from "@plane/types";
import { MOCK_ISSUES } from "../mock-data";

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

export const useIssues = (projectId: string, filters?: IssueFilters) => {
  return useQuery<TIssue[]>({
    queryKey: ["issues", projectId, filters],
    queryFn: async () => {
      await delay(300);
      return MOCK_ISSUES.filter((i) => i.project_id === projectId);
    },
    enabled: !!projectId,
  });
};

export const useIssue = (projectId: string, issueId: string) => {
  return useQuery<TIssue | undefined>({
    queryKey: ["issues", projectId, issueId],
    queryFn: async () => {
      await delay(200);
      return MOCK_ISSUES.find((i) => i.id === issueId && i.project_id === projectId);
    },
    enabled: !!projectId && !!issueId,
  });
};
```

### Pattern 2: Optimistic Mutation (Key Pattern for Issue Updates)

**What:** useMutation with onMutate for optimistic cache updates, onError for rollback.

**Example** (TanStack Query v5 official pattern, `VERIFIED: tanstack.com/query/v5/docs`):

```typescript
export const useUpdateIssue = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ issueId, data }: { issueId: string; data: Partial<TIssue> }) => {
      await delay(200);
      // Future: API call
      return { issueId, ...data };
    },
    onMutate: async ({ issueId, data }) => {
      await queryClient.cancelQueries({ queryKey: ["issues"] });
      const previous = queryClient.getQueryData(["issues"]);
      queryClient.setQueryData(["issues"], (old: TIssue[] = []) =>
        old.map((i) => (i.id === issueId ? { ...i, ...data } : i))
      );
      return { previous };
    },
    onError: (_err, _vars, context) => {
      if (context?.previous) queryClient.setQueryData(["issues"], context.previous);
    },
    onSettled: () => queryClient.invalidateQueries({ queryKey: ["issues"] }),
  });
};
```

### Pattern 3: @hello-pangea/dnd Kanban Drag-and-Drop

**What:** Use DragDropContext, Droppable, Draggable for kanban column/card drag.

**Example** (per @hello-pangea/dnd docs, `VERIFIED: deepwiki.com/hello-pangea/dnd`):

```typescript
import { DragDropContext, Droppable, Draggable } from "@hello-pangea/dnd";

function KanbanBoard() {
  const updateIssue = useUpdateIssue();

  const onDragEnd = (result: DropResult) => {
    const { draggableId, destination, source } = result;
    if (!destination) return;
    if (destination.droppableId === source.droppableId && destination.index === source.index) return;

    // destination.droppableId = new state_id
    updateIssue.mutate({ issueId: draggableId, data: { state_id: destination.droppableId } });
  };

  return (
    <DragDropContext onDragEnd={onDragEnd}>
      <div className="flex gap-4 overflow-x-auto">
        {columns.map((col) => (
          <Droppable droppableId={col.id} key={col.id}>
            {(provided, snapshot) => (
              <div ref={provided.innerRef} {...provided.droppableProps} className="w-[280px]">
                {col.issues.map((issue, index) => (
                  <Draggable draggableId={issue.id} index={index} key={issue.id}>
                    {(provided, snapshot) => (
                      <div ref={provided.innerRef} {...provided.draggableProps} {...provided.dragHandleProps}>
                        {/* card content */}
                      </div>
                    )}
                  </Draggable>
                ))}
                {provided.placeholder}
              </div>
            )}
          </Droppable>
        ))}
      </div>
    </DragDropContext>
  );
}
```

### Pattern 4: cmdk Command Palette

**What:** Use cmdk's Command.Dialog for accessible Cmd+K command palette.

**Example** (per cmdk docs, `VERIFIED: github.com/pacocoursey/cmdk`):

```typescript
import { Command } from "cmdk";

function CommandPalette({ open, onOpenChange }) {
  return (
    <Command.Dialog open={open} onOpenChange={onOpenChange} label="命令面板">
      <Command.Input placeholder="搜索 Issue 或页面..." />
      <Command.List>
        <Command.Empty>没有匹配结果</Command.Empty>
        <Command.Group heading="Issues">
          {issues.map((issue) => (
            <Command.Item
              key={issue.id}
              onSelect={() => navigate(`/workspaces/${wsId}/projects/${projId}/issues/${issue.id}`)}
            >
              {issue.name}
            </Command.Item>
          ))}
        </Command.Group>
        <Command.Group heading="页面">
          <Command.Item onSelect={() => navigate("/workspaces/...")}>项目设置</Command.Item>
        </Command.Group>
      </Command.List>
    </Command.Dialog>
  );
}
```

### Anti-Patterns to Avoid

- **Mixing server state into MobX stores:** TanStack Query should own all server data; MobX stores only manage UI state like selectedIds, expandedSections, scrollPosition.
- **Building custom drag-and-drop:** Use @hello-pangea/dnd (D-P16-13) -- don't implement native HTML5 DnD or mouse event-based drag.
- **Building custom command palette:** Use cmdk (D-P16-16) -- don't build modal + search + keyboard nav from scratch.
- **Nested React Router config for issue routes:** Per D-P16-04, use flat route definitions, not nested layout nesting.
- **Direct ProseMirror/TipTap manipulation:** Always go through @plane/editor's RichTextEditorWithRef wrapper.

## Don't Hand-Roll

| Problem                        | Don't Build                         | Use Instead                                      | Why                                                                               |
| ------------------------------ | ----------------------------------- | ------------------------------------------------ | --------------------------------------------------------------------------------- |
| Drag-and-drop in kanban        | Custom mouse event handlers         | @hello-pangea/dnd                                | Handles touch, keyboard, accessibility, animations, cross-browser                 |
| Command palette modal + search | Custom modal with input + filter    | cmdk (Command.Dialog)                            | Handles accessibility, keyboard nav, fuzzy search, grouping, Esc close            |
| Rich text editing              | textarea + markdown parser          | @plane/editor (TipTap fork)                      | D-P16-12: forked and ready; supports bold, lists, headings, mentions, file upload |
| Inline property dropdowns      | Custom dropdown for each field type | @plane/ui Dropdown (single-select, multi-select) | Already forked at src/lib/ui/dropdown/; consistent UX with Plane                  |
| Confirmation dialogs           | Modal from scratch                  | @plane/ui AlertModal                             | Already forked at src/lib/ui/modals/alert-modal.tsx                               |
| Loading/skeleton states        | Manual skeleton divs                | @plane/ui Loader                                 | Already forked at src/lib/ui/loader.tsx                                           |

**Key insight:** The Plane fork already provides nearly all the UI primitives needed. Phase 16's work is primarily composition (wiring components together) and data-flow (TanStack Query hooks), not building infrastructure.

## Common Pitfalls

### Pitfall 1: Drag-and-Drop Overwriting Optimistic Updates

**What goes wrong:** When dragging a kanban card, the user sees the card move briefly, then snap back to the original position, then move again.
**Why it happens:** @hello-pangea/dnd fires onDragEnd, which triggers useMutation. If the mutation's invalidateQueries fires before the optimistic update applies, the server fetch overwrites the optimistic state.
**How to avoid:** Use `onMutate` to cancel in-flight queries before setting optimistic data. Always return a rollback context in `onMutate` and restore on `onError`.
**Warning signs:** Cards visually jumping back after drag completes.

### Pitfall 2: MobX Store Growing Beyond UI State

**What goes wrong:** The IssueStore starts caching issue data "for convenience," duplicating TanStack Query's cache and causing sync bugs.
**Why it happens:** It's tempting to keep issue data alongside UI state when components need both.
**How to avoid:** Strictly enforce the rule: TanStack Query owns all server data. MobX IssueStore only stores: `selectedIssueIds: string[]`, `expandedGroups: string[]`, `activeView: 'list' | 'kanban'`, `currentPage: number`, `scrollPositions: Record<string, number>`.
**Warning signs:** Bug where updating an issue in one view doesn't reflect in another; or stale data shown after mutation.

### Pitfall 3: List/Kanban Filter State Desync

**What goes wrong:** User sets filters in list view, switches to kanban view, and filters are lost.
**Why it happens:** Each view component initializes its own filter state independently.
**How to avoid:** Per the CONTEXT.md specifics, list and kanban views should share the same filter state. Store filter/sort/group state in a TanStack Query hook with a stable query key that persists across view switches. The MobX IssueStore can hold the active view mode toggle.
**Warning signs:** Different results shown in list vs kanban for the same project.

### Pitfall 4: Inline Property Editor Closes on Blur Too Aggressively

**What goes wrong:** User clicks a property to edit, the dropdown opens, but clicking inside the dropdown to choose a value registers as "blur" and closes the editor.
**Why it happens:** The inline editor's blur handler fires before the dropdown click event registers.
**How to avoid:** Use a click-outside detection with a ref-based approach (already available via `use-outside-click-detector` hook in the codebase). Delay execution to allow click events to propagate before dismissing.
**Warning signs:** Property dropdowns closing before user can make a selection.

### Pitfall 5: @hello-pangea/dnd StrictMode Double-Fire

**What goes wrong:** onDragEnd fires twice in development with React.StrictMode.
**Why it happens:** @hello-pangea/dnd is built on Redux; StrictMode double-invocation causes the drag end action to be processed twice.
**How to avoid:** Wrap onDragEnd handler with a useRef guard: `if (dragHandledRef.current) return; dragHandledRef.current = true;`. Only needed in development; React.StrictMode doesn't double-fire in production builds.

## Code Examples

### Issue Create Route Registration (app/routes.ts)

```typescript
// Add inside the workspace layout group, after project routes:
route(
  "workspaces/:workspaceId/projects/:projectId/issues",
  "app/issues/page.tsx",
),
route(
  "workspaces/:workspaceId/projects/:projectId/issues/:issueId",
  "app/issues/[issueId]/page.tsx",
),
```

### IssueStore (UI State Only)

```typescript
// app/store/issue.store.ts
import { action, makeObservable, observable } from "mobx";

export interface IIssueStore {
  selectedIssueIds: string[];
  activeView: "list" | "kanban";
  currentPage: number;
  groupBy: "state" | "priority" | "assignees";
  expandedColumnIds: string[];

  toggleIssueSelection: (id: string) => void;
  selectAll: (ids: string[]) => void;
  clearSelection: () => void;
  setActiveView: (view: "list" | "kanban") => void;
  setCurrentPage: (page: number) => void;
  setGroupBy: (groupBy: "state" | "priority" | "assignees") => void;
  toggleColumnExpand: (id: string) => void;
}

export class IssueStore implements IIssueStore {
  selectedIssueIds: string[] = [];
  activeView: "list" | "kanban" = "list";
  currentPage: number = 1;
  groupBy: "state" | "priority" | "assignees" = "state";
  expandedColumnIds: string[] = [];

  constructor() {
    makeObservable(this, {
      selectedIssueIds: observable,
      activeView: observable.ref,
      currentPage: observable.ref,
      groupBy: observable.ref,
      expandedColumnIds: observable,
      toggleIssueSelection: action,
      selectAll: action,
      clearSelection: action,
      setActiveView: action,
      setCurrentPage: action,
      setGroupBy: action,
      toggleColumnExpand: action,
    });
  }

  toggleIssueSelection = (id: string) => {
    const idx = this.selectedIssueIds.indexOf(id);
    if (idx >= 0) this.selectedIssueIds.splice(idx, 1);
    else this.selectedIssueIds.push(id);
  };
  selectAll = (ids: string[]) => {
    this.selectedIssueIds = ids;
  };
  clearSelection = () => {
    this.selectedIssueIds = [];
  };
  setActiveView = (view: "list" | "kanban") => {
    this.activeView = view;
  };
  setCurrentPage = (page: number) => {
    this.currentPage = page;
  };
  setGroupBy = (groupBy: "state" | "priority" | "assignees") => {
    this.groupBy = groupBy;
  };
  toggleColumnExpand = (id: string) => {
    const idx = this.expandedColumnIds.indexOf(id);
    if (idx >= 0) this.expandedColumnIds.splice(idx, 1);
    else this.expandedColumnIds.push(id);
  };
}
```

### @plane/editor - Rich Text Description Field

```typescript
// Using @plane/editor's RichTextEditorWithRef for Issue description (per D-P16-12)
import { RichTextEditorWithRef, type EditorRefApi } from "@plane/editor";

function IssueDescriptionEditor({ initialValue, onSave, workspaceSlug }) {
  const editorRef = useRef<EditorRefApi>(null);

  return (
    <RichTextEditorWithRef
      ref={editorRef}
      editable={true}
      initialValue={initialValue}
      containerClassName="p-4"
      workspaceSlug={workspaceSlug}
      // ... fileHandler, mentionHandler props
    />
  );
}
```

## State of the Art

| Old Approach                                          | Current Approach                                   | When Changed | Impact                                                          |
| ----------------------------------------------------- | -------------------------------------------------- | ------------ | --------------------------------------------------------------- |
| Plane original: MobX for all state + SWR for fetching | TanStack Query for server state + MobX for UI only | Phase 15     | Cleaner separation, better caching, built-in optimistic updates |
| Plane original: @atlaskit/pragmatic-drag-and-drop     | @hello-pangea/dnd                                  | D-P16-13     | Simpler API, maintained fork, React 19 support                  |
| Plane original: custom command palette                | cmdk                                               | D-P16-16     | Accessible, keyboard-navigable, proven library                  |

**Deprecated/outdated:**

- `react-beautiful-dnd` — Atlassian deprecated it. @hello-pangea/dnd is the maintained fork.
- SWR (still in Plane codebase) — Phase 15 replaced with TanStack Query for new code.

## Assumptions Log

| #   | Claim                                                                         | Section        | Risk if Wrong                                                        |
| --- | ----------------------------------------------------------------------------- | -------------- | -------------------------------------------------------------------- |
| A1  | @hello-pangea/dnd v18.0.0 works with React 19                                 | Standard Stack | LOW — confirmed via WebSearch (multiple sources), install and verify |
| A2  | cmdk v1.1.1 works with React 19                                               | Standard Stack | LOW — widely used, community-tested against React 19                 |
| A3  | @plane/editor RichTextEditorWithRef is available at the alias `@plane/editor` | Standard Stack | LOW — verified in tsconfig and editor index.ts                       |
| A4  | The forked @plane/ui Dropdown components work via `@plane/ui` alias           | Standard Stack | LOW — verified in tsconfig and ui directory                          |

## Open Questions (RESOLVED)

1. **Comment editor component**: Should Issue comments use @plane/editor's LiteTextEditor (lightweight) or RichTextEditor? — RESOLVED: LiteTextEditor for comments, RichTextEditor for description (per D-P16-12).
   - What we know: The Plane reference uses LiteTextEditor for comments (plain text with basic formatting).
   - What's unclear: @plane/editor has `LiteTextEditorWithRef` export. Need to verify it's available in the forked copy.
   - Recommendation: Use LiteTextEditor for comments (simpler UX), RichTextEditor for description.

2. **Activity log implementation**: How to render the activity log in the Issue detail page? — RESOLVED: Simple text list format.
   - What we know: MOCK_ACTIVITIES already in mock-data.ts, but they are project-level activities, not issue-specific.
   - What's unclear: Whether to build a simple activity log (text list of changes) or a more detailed one following Plane's pattern.
   - Recommendation: Build a simple activity log initially with mock data, style following Plane's pattern.

3. **Issue description editor integration**: How to handle the file upload/mention handler dependencies for @plane/editor? — RESOLVED: No-op handlers for mock phase.
   - What we know: The editor requires `fileHandler` and `mentionHandler` props for full rich text features.
   - What's unclear: These handlers are currently tied to real API services (WorkspaceService, etc.) that may not have mock implementations.
   - Recommendation: For the mock phase, provide no-op handlers that return empty results. The editor will still render and accept text input without file uploads/mentions.

## Environment Availability

| Dependency               | Required By          | Available | Version          | Fallback                                   |
| ------------------------ | -------------------- | --------- | ---------------- | ------------------------------------------ |
| React 19                 | All components       | yes       | 19.0             | —                                          |
| @tanstack/react-query v5 | TanStack Query hooks | yes       | ^5.0.0           | —                                          |
| mobx + mobx-react        | MobX stores          | yes       | ^6.12.0 + ^9.1.0 | —                                          |
| @plane/editor (fork)     | Rich text editing    | yes       | workspace:\*     | Use LiteTextEditor for comments            |
| @plane/ui (fork)         | UI components        | yes       | workspace:\*     | —                                          |
| cmdk                     | Command palette      | NO        | —                | Need install: `pnpm add cmdk`              |
| @hello-pangea/dnd        | Kanban drag-and-drop | NO        | —                | Need install: `pnpm add @hello-pangea/dnd` |

**Missing dependencies with no fallback:** none
**Missing dependencies with fallback:** none — both need install but are simple npm adds

## Validation Architecture

### Test Framework

| Property           | Value                                               |
| ------------------ | --------------------------------------------------- |
| Framework          | No test framework configured for yh-flow/web client |
| Config file        | none                                                |
| Quick run command  | `npx tsc --noEmit` (typecheck)                      |
| Full suite command | `npx tsc --noEmit` (typecheck only)                 |

> Note: The project has no test framework for the web client. TypeScript compilation check (`npx tsc --noEmit`) is the primary validation mechanism, consistent with Phase 15's approach.

### Phase Requirements → Test Map

| Req ID | Behavior         | Test Type   | Automated Command  |
| ------ | ---------------- | ----------- | ------------------ |
| All    | Type correctness | compilation | `npx tsc --noEmit` |

### Wave 0 Gaps

- [ ] No test framework configured — Phase 16 does not introduce testing infrastructure
- [ ] Testing scope is limited to TypeScript compilation checks

## Security Domain

> Not applicable. Phase 16 operates entirely with mock data and no API integration. No authentication, authorization, input validation, or data persistence concerns for the mock layer. Security enforcement will be relevant when real API integration begins (likely Phase 17+).

## Sources

### Primary (HIGH confidence)

- [CONTEXT.md] - Phase 16 locked decisions (D-P16-01 through D-P16-16)
- [UI-SPEC.md] - UI design contract, component inventory, interaction contracts, copywriting
- [Phase 15 implementations] - Established TanStack Query + MobX pattern in yh-flow/clients/web/
- [Codebase] - yh-flow/clients/web/ source code examination (types, components, stores, hooks, editor)

### Secondary (MEDIUM confidence)

- [WebSearch: @hello-pangea/dnd React 19 support] - v18.0.0 adds React 19 support (February 2025)
- [WebSearch: cmdk command palette] - Established library, accessible, composable
- [WebSearch: TanStack Query v5 optimistic updates] - Official v5 onMutate + onError rollback pattern

### Tertiary (LOW confidence)

- @hello-pangea/dnd strict mode double-fire guard pattern - observed in community reports
- cmdk v1.1.1 React 19 compatibility - not officially documented but widely used

## Metadata

**Confidence breakdown:**

- Standard stack: HIGH - Phase 15 established patterns are directly applicable
- Architecture: HIGH - CONTEXT.md decisions provide clear direction
- Pitfalls: MEDIUM - Based on known patterns from similar implementations
- Library compatibility: MEDIUM - @hello-pangea/dnd React 19 support confirmed via WebSearch

**Research date:** 2026-06-29
**Valid until:** 2026-07-29 (30 days — stable libraries, well-established patterns)
