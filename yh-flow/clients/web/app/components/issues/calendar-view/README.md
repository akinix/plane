# Calendar View

FLOW: Forked from Plane `apps/web/core/components/issues/issue-layouts/calendar/`

## Origin

This directory contains a fork of Plane's calendar view components, adapted for
the yh-flow (Flow) project. The original Plane components reside at:

- `apps/web/core/components/issues/issue-layouts/calendar/`

## Modifications

1. **Build system adapter**: Replaced Next.js router with React Router / props
2. **DnD adapter**: Replaced `@atlaskit/pragmatic-drag-and-drop` with `@hello-pangea/dnd`
3. **Store adapter**: Replaced Plane's MobX issue stores with yh-flow's `useIssues` (TanStack Query)
4. **UI adapter**: Replaced `@plane/propel` icons/toast/popover with lucide-react / native patterns
5. **i18n**: Removed `@plane/i18n` dependency; Chinese content hardcoded
6. **Simplification**: Removed Plane store types (ICycleIssuesFilter, etc.); simplified to essential props

## Components

| File                          | Plane Source                            | Purpose                     |
| ----------------------------- | --------------------------------------- | --------------------------- |
| `utils.ts`                    | `calendar/utils.ts`                     | Drag-drop helper            |
| `week-header.tsx`             | `calendar/week-header.tsx`              | Day-of-week header row      |
| `week-days.tsx`               | `calendar/week-days.tsx`                | Week grid of day tiles      |
| `day-tile.tsx`                | `calendar/day-tile.tsx`                 | Single day cell             |
| `calendar-header.tsx`         | `calendar/header.tsx`                   | Month nav + today button    |
| `issue-block.tsx`             | `calendar/issue-block.tsx`              | Issue card in calendar      |
| `issue-blocks.tsx`            | `calendar/issue-blocks.tsx`             | Issue list on a day tile    |
| `issue-block-root.tsx`        | `calendar/issue-block-root.tsx`         | Draggable wrapper           |
| `quick-add-issue-actions.tsx` | `calendar/quick-add-issue-actions.tsx`  | Quick create UI             |
| `base-calendar-root.tsx`      | `calendar/roots/base-calendar-root.tsx` | Root data container         |
| `calendar-view.tsx`           | `calendar/calendar.tsx`                 | Main CalendarView container |
