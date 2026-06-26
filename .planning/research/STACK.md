# Stack Research: Flow Web Frontend

**Domain:** Project management SPA (React) — progressive migration from Plane Web (React 18 / SWR / MobX) to Flow Web (React 19 / TanStack Query / MobX)
**Researched:** 2026-06-26
**Confidence:** HIGH

## Executive Summary

Flow Web is a **progressive transformation** of Plane Web's frontend. We keep the UI component library (propel), the TipTap-based editor (stripped of Yjs), shared types, and utility code. We replace the data-fetching layer (SWR → TanStack Query v5), the auth layer (Django CSRF/session → JWT Bearer), and remove Plane Web's collaboration stack (Yjs/Hocuspocus). MobX stays for client-side UI state but is **not** used for server data — TanStack Query owns server state. The editor package (`@plane/editor`) is forked and its Yjs collaboration dependencies stripped for v1.

This document details every package decision: what comes from Plane Web unchanged, what gets adapted, what gets replaced, and what gets dropped.

---

## Recommended Stack

### Core Framework

| Technology   | Version | Purpose             | Why                                                                                                                                                                                                          |
| ------------ | ------- | ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| React        | 19.2.7  | UI framework        | Plane Web is on React 18.3.1; Flow Web starts fresh on React 19 for `use()`, better SSR streaming, and the stable concurrent features. React 19 is now mature (19.2.7 latest). Source: npm registry.         |
| TypeScript   | 5.8.3   | Type safety         | Already in Plane Web at 5.8.3 (pnpm catalog). No change needed.                                                                                                                                              |
| Vite         | 7.3.2   | Build tool          | Already in Plane Web at 7.3.2. No change needed.                                                                                                                                                             |
| React Router | 7.15.0  | Client-side routing | Plane Web already migrated from Next.js to React Router 7 (vite.config.ts has `reactRouter()` plugin). No change needed — but we should install as a clean dependency, not inherit the Next.js compat shims. |

### Server State & Data Fetching (CRITICAL CHANGE)

| Technology     | Version | Purpose                 | Why                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           |
| -------------- | ------- | ----------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| TanStack Query | 5.101.0 | Server state management | **Replaces SWR (2.2.4).** Both are HTTP-caching libraries for server data, but TanStack Query has: (a) better DevTools for debugging, (b) mutation APIs (`useMutation`) with optimistic updates — critical for a project management app where drag-and-drop and inline edits must feel instant, (c) query invalidation that is more explicit than SWR's mutate, (d) `queryOptions` factory pattern that pairs perfectly with service classes. SWR was fine for Plane's Django backend but TanStack Query's mutation + optimistic update story is essential for the target UX. |
| Axios          | 1.16.0  | HTTP client             | **KEEP.** Plane Web already uses Axios via `packages/services/src/api.service.ts`. It works well and supports interceptors for JWT token injection and 401 handling. No benefit in switching to fetch-based alternatives — Axios is stable, well-typed, and the entire Plane services layer depends on it.                                                                                                                                                                                                                                                                    |

### Client State (KEEP MobX, DO NOT merge with TanStack Query)

| Technology | Version | Purpose                 | Why                                                                                                                                                                                                                                                                                                                                                                                                                                               |
| ---------- | ------- | ----------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| MobX       | 6.12.0  | Client-side UI state    | **KEEP but narrow scope.** Plane Web uses MobX extensively for: workspace selection, filter state, sidebar collapse, theme preferences, command palette state. This is appropriate — MobX is excellent for complex, interconnected UI state. The **critical rule**: MobX never holds server data fetched via TanStack Query. MobX holds ephemeral UI state only. Filter values in MobX drive TanStack Query parameters, not the other way around. |
| mobx-react | 9.1.1   | React bindings for MobX | Plane Web uses `observer()` HOC extensively. No change needed.                                                                                                                                                                                                                                                                                                                                                                                    |

### Forms & Validation

| Technology      | Version | Purpose               | Why                                                                                                                                                                                                            |
| --------------- | ------- | --------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| React Hook Form | 7.78.0  | Form state management | Plane Web uses v7.51.5. Upgrade to latest v7 (avoid v8 beta — too early). RHF is the standard for complex forms (project create/edit, issue create/edit) with good TypeScript support.                         |
| Zod             | 4.4.3   | Schema validation     | Plane Web uses v3.25.76. Upgrade to Zod v4 for the improved correctness and soundness. Used with RHF via `@hookform/resolvers`. Critical for validating API request bodies before sending to the .NET backend. |

### Styling & UI Components (KEEP Plane Web's custom system)

| Technology                | Version      | Purpose                | Why                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |
| ------------------------- | ------------ | ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Tailwind CSS              | 4.1.17       | Utility CSS            | Plane Web already on Tailwind v4. Keep as-is.                                                                                                                                                                                                                                                                                                                                                                                                                                                                      |
| @plane/propel             | fork         | UI component library   | **Plane Web's custom shadcn-style library** built on @base-ui-components/react (formerly Radix). Contains: accordion, avatar, badge, button, calendar, cards, charts (recharts wrapper), combobox, command palette, context menu, dialog, emoji picker, icons, input, menu, popover, skeleton, switch, tabs, table (TanStack Table wrapper), toast, tooltip. We fork this into Flow Web and strip packages/plane-web references. Propel is already well-architected — no need to replace with shadcn from scratch. |
| @base-ui-components/react | 1.0.0-beta.3 | Headless UI primitives | Base UI is the successor to Radix Primitives. Propel depends on it. Keep as-is.                                                                                                                                                                                                                                                                                                                                                                                                                                    |
| @headlessui/react         | 1.7.19       | Headless UI            | Some components reference this. V2 exists but Plane Web stuck with v1 — avoid migration cost.                                                                                                                                                                                                                                                                                                                                                                                                                      |
| lucide-react              | 0.469.0      | Icon library           | Already in Plane Web. Good icon coverage for project management. Keep.                                                                                                                                                                                                                                                                                                                                                                                                                                             |
| framer-motion             | 12.23.0      | Animations             | Propel uses framer-motion for transitions. Keep.                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| clsx                      | 2.1.1        | Class merging          | Already in Plane Web via propel. Keep.                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |
| tailwind-merge            | 3.4.0        | Tailwind class dedup   | Used by propel's `cn()` utility. Keep.                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |

### Editor (CRITICAL CHANGE — strip Yjs)

| Technology              | Version        | Purpose             | Why                                                                                                                                                                                                                                                                                                                                                                                                                                                                |
| ----------------------- | -------------- | ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| @plane/editor           | fork           | Rich text editor    | **Plane Web's custom TipTap-based editor.** We fork this and strip Yjs collaboration dependencies for v1. The editor handles: markdown rendering, rich text, code blocks, mentions, emoji, image uploads. Core TipTap extensions stay: `@tiptap/core`, `@tiptap/react`, `@tiptap/starter-kit`, `@tiptap/extension-mention`, `@tiptap/extension-image`, `@tiptap/extension-emoji`, `@tiptap/extension-task-list`, `@tiptap/extension-underline`, `tiptap-markdown`. |
| @tiptap/core            | 2.22.3         | TipTap core         | Keep. Well-maintained, stable.                                                                                                                                                                                                                                                                                                                                                                                                                                     |
| @tiptap/react           | 2.22.3         | React bindings      | Keep.                                                                                                                                                                                                                                                                                                                                                                                                                                                              |
| highlight.js / lowlight | 11.8.0 / 3.0.0 | Syntax highlighting | Keep — used by editor for code blocks.                                                                                                                                                                                                                                                                                                                                                                                                                             |

### Charts & Visualization

| Technology            | Version | Purpose     | Why                                                        |
| --------------------- | ------- | ----------- | ---------------------------------------------------------- |
| Recharts              | 2.15.1  | Charts      | Plane Web uses recharts via propel's chart wrappers. Keep. |
| @tanstack/react-table | 8.21.3  | Data tables | Plane Web uses this in propel's table component. Keep.     |

### Date & Utilities

| Technology | Version | Purpose           | Why                                                                    |
| ---------- | ------- | ----------------- | ---------------------------------------------------------------------- |
| date-fns   | 4.1.0   | Date manipulation | Already in Plane Web. Good Tree-shaking, comprehensive. Keep.          |
| lodash-es  | 4.18.1  | General utilities | Used throughout Plane Web (debounce, throttle, cloneDeep, etc.). Keep. |
| uuid       | 14.0.0  | UUID generation   | Used by editor and stores. Keep.                                       |

### Notifications & Real-time

| Technology         | Version        | Purpose                 | Why                                                                                                                                                                                                                                   |
| ------------------ | -------------- | ----------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Server-Sent Events | browser native | Real-time notifications | The .NET backend supports SSE. No library needed — `EventSource` API in browser. Flow Web will have an `SSEService` that reconnects automatically and dispatches to TanStack Query for invalidation or MobX for in-app notifications. |

### Development Tools

| Tool                           | Purpose         | Notes                                                               |
| ------------------------------ | --------------- | ------------------------------------------------------------------- |
| TypeScript 5.8.3               | Language        | Same as Plane Web.                                                  |
| Vite 7.3.2                     | Build, HMR      | Same as Plane Web. `vite-tsconfig-paths` for path aliases.          |
| oxlint                         | Linting         | Plane Web uses oxlint. We'll adopt it for Flow Web too — it's fast. |
| @tanstack/react-query-devtools | Query debugging | Only in development. Priceless for debugging cache issues.          |

---

## What Comes FROM Plane Web (adapted)

These Plane packages are forked and adapted for Flow Web, not rewritten from scratch:

| Package            | Adaptation Needed                                                                                                                                                                         | Why Not Rewrite                                                                                                                                         |
| ------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `@plane/propel`    | Strip references to `@plane/constants`, `@plane/types`, `@plane/utils` — inline or replace with local versions.                                                                           | 30+ UI components (dialog, combobox, toast, table, etc.). Rewriting is months of work.                                                                  |
| `@plane/editor`    | Strip Yjs, Hocuspocus, y-indexeddb, y-prosemirror dependencies. Keep TipTap core + extensions. Replace `@plane/constants`, `@plane/types`, `@plane/hooks` references with local versions. | TipTap with all the custom extensions (mentions, emoji picker, code blocks, task lists) is the core value. Plane Web's editor is already battle-tested. |
| `@plane/types`     | Fork and add .NET-specific types. Keep Plane-compatible types for API response compatibility.                                                                                             | Large type system that maps to the API responses. Starting from scratch risks silent type mismatches during progressive migration.                      |
| `@plane/utils`     | Fork. Remove i18n-related utils if i18n is deferred.                                                                                                                                      | Various utility functions used across propel and editor.                                                                                                |
| `@plane/constants` | Fork. Replace Django endpoint URLs with .NET endpoints. Keep API_BASE_URL pattern. Add new constants for .NET API paths.                                                                  | Well-structured constant patterns. Small file, easy to adapt.                                                                                           |

## What Gets REPLACED

| From                       | To                     | Rationale                                                                                                                                                                                                                                                                            |
| -------------------------- | ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| SWR 2.2.4                  | TanStack Query 5.101.0 | See rationale in Server State section above. Mutation lifecycles, optimistic updates, devtools — all critical for project management UX.                                                                                                                                             |
| Django CSRF + session auth | JWT Bearer token       | Plane Web uses CSRF tokens + session cookies (see `auth.service.ts` — `requestCSRFToken()`, form-based sign-out). Flow Web switches to JWT: store token in memory/httpOnly cookie, inject via Axios interceptor, redirect to login on 401. The auth service is completely rewritten. |
| Next.js compat shims       | Remove entirely        | `apps/web/vite.config.ts` has `next/link`, `next/navigation`, `next/script` aliases. Flow Web starts fresh with React Router — no compat layer needed.                                                                                                                               |

## What Gets DROPPED (anti-features / deferred)

| Package                                      | Why                                                                                                                                                                | When to Revisit                                                                 |
| -------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------- |
| Yjs, y-indexeddb, y-prosemirror, y-protocols | Real-time collaboration. Plane Web uses Yjs for collaborative editing via Hocuspocus server. Flow Web v1 does NOT support real-time collaboration on pages/editor. | Future milestone (v2.x) if real-time collaboration is needed.                   |
| @hocuspocus/provider                         | Yjs provider for WebSocket sync. Dropped with Yjs.                                                                                                                 | Same as above.                                                                  |
| @plane/i18n, i18next, react-i18next          | Internationalization. Initial release is Chinese-only.                                                                                                             | Future milestone when multi-language support is needed.                         |
| @react-pdf/renderer, react-pdf-html          | PDF export. Rarely used feature.                                                                                                                                   | Add when explicitly required.                                                   |
| comlink                                      | Web Worker orchestration. Verify actual usage before dropping — Plane Web may use it for heavy computations.                                                       | Check during implementation. If unused, drop permanently.                       |
| emoji-picker-react                           | Emoji picker. Keep if editor mentions/picker uses it; the editor has its own emoji handling.                                                                       | Evaluate during editor fork — the editor may already have inline emoji support. |
| @popperjs/core, react-popper, tippy.js       | Used by older editor tooltips. Propel uses @floating-ui instead (modern replacement).                                                                              | Drop if editor fork doesn't depend on these.                                    |
| isbot                                        | Bot detection for SSR. Not needed if Flow Web is CSR-only (which it is — React Router SPA mode).                                                                   | Drop.                                                                           |
| serve                                        | Static file server. Use Vite's preview instead.                                                                                                                    | Drop.                                                                           |
| Microsoft Clarity                            | Session recording. Privacy concern, unnecessary for v1.                                                                                                            | Drop permanently unless analytics requirement emerges.                          |
| react-fast-compare                           | Deep comparison utility. Not needed if React 19 has better memoization.                                                                                            | Drop.                                                                           |
| react-masonry-component                      | Masonry layout. Verify actual usage — likely unused.                                                                                                               | Drop if unused.                                                                 |

---

## Installation

### Core Dependencies

```bash
# Framework
npm install react@19 react-dom@19 @types/react @types/react-dom
npm install react-router@7

# Server State
npm install @tanstack/react-query@5

# Forms + Validation
npm install react-hook-form zod @hookform/resolvers

# HTTP Client
npm install axios

# State Management
npm install mobx mobx-react

# Styling
npm install tailwindcss @tailwindcss/postcss @tailwindcss/typography
npm install clsx tailwind-merge
npm install lucide-react
npm install @headlessui/react@1

# Icons
npm install lucide-react
npm install @fontsource-variable/inter @fontsource/ibm-plex-mono @fontsource/material-symbols-rounded

# Utilities
npm install date-fns lodash-es uuid
```

### Dev Dependencies

```bash
npm install -D typescript @types/lodash-es @types/node
npm install -D vite@7 vite-tsconfig-paths
npm install -D @tanstack/react-query-devtools
npm install -D tailwindcss postcss
npm install -D oxlint
```

### Propel (Forked from Plane Web)

```bash
# What propel depends on:
npm install @base-ui-components/react@1.0.0-beta.3
npm install @tanstack/react-table@8
npm install recharts@2
npm install framer-motion
npm install class-variance-authority
npm install cmdk
npm install frimousse
npm install react-day-picker
npm install use-font-face-observer
```

### Editor (Forked from Plane Web, Yjs Stripped)

```bash
# What the stripped editor needs:
npm install @tiptap/core @tiptap/react @tiptap/pm
npm install @tiptap/starter-kit @tiptap/extension-mention @tiptap/extension-image
npm install @tiptap/extension-emoji @tiptap/extension-task-list @tiptap/extension-task-item
npm install @tiptap/extension-underline @tiptap/extension-blockquote
npm install @tiptap/extension-heading @tiptap/extension-placeholder
npm install @tiptap/extension-character-count @tiptap/extension-text-align
npm install @tiptap/extension-text-style @tiptap/html @tiptap/suggestion
npm install tiptap-markdown
npm install highlight.js lowlight linkifyjs emoji-regex is-emoji-supported
npm install @floating-ui/dom @floating-ui/react
```

---

## API Client Architecture

### Current (Plane Web)

```
Component → MobX Store → Service (Axios) → Django REST API
                                     ↕
                               SWR (useSWR hook)
```

Services are class-based (`APIService` base class → `WorkspaceService`, `AuthService`, etc.). Components use either:

- **SWR hooks** for data fetching with caching
- **MobX store methods** that call services internally

### Target (Flow Web)

```
Component → TanStack Query (useQuery/useMutation) → Service (Axios) → .NET API
    ↕                                                   ↕
 MobX (UI state only)                         Axios interceptor injects JWT
```

**Key principles:**

1. **Service classes stay** — The `APIService` base class pattern is sound. Each module gets its own service class (e.g., `WorkspaceService`, `IssueService`). These call the .NET API endpoints.
2. **Service classes do NOT call SWR or TanStack Query** — Services return raw promises. TanStack Query hooks wrap them.
3. **Auth is an Axios interceptor** — Not a MobX store. An Axios request interceptor reads the JWT token and adds `Authorization: Bearer <token>`. A response interceptor catches 401 and redirects to login. No SWR/MobX dependency for auth.
4. **MobX stores own UI state only** — Current workspace slug, filter selections, sidebar state, theme preferences, command palette open/close. Never API response data.
5. **Filters pattern**: MobX store holds filter state (e.g., `statusFilter: "backlog"`). A React component reads this and passes it as TanStack Query parameters. When the filter changes, the MobX store updates, trigger React re-render, TanStack Query refetches with new params.

### Service Pattern

```typescript
// services/workspace.service.ts
import { APIService } from "./api.service";
import type { IWorkspace } from "@/types";

export class WorkspaceService extends APIService {
  constructor() {
    super(import.meta.env.VITE_API_BASE_URL);
  }

  async list(): Promise<IWorkspace[]> {
    return this.get("/api/v1/workspaces")
      .then((res) => res.data)
      .catch((error) => {
        throw error?.response?.data;
      });
  }

  async retrieve(slug: string): Promise<IWorkspace> {
    return this.get(`/api/v1/workspaces/${slug}`)
      .then((res) => res.data)
      .catch((error) => {
        throw error?.response;
      });
  }
}

export const workspaceService = new WorkspaceService();
```

### TanStack Query Hook Pattern

```typescript
// hooks/use-workspaces.ts
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { workspaceService } from "@/services/workspace.service";

export const workspaceKeys = {
  all: ["workspaces"] as const,
  detail: (slug: string) => ["workspaces", slug] as const,
};

export function useWorkspaces() {
  return useQuery({
    queryKey: workspaceKeys.all,
    queryFn: () => workspaceService.list(),
  });
}

export function useWorkspace(slug: string) {
  return useQuery({
    queryKey: workspaceKeys.detail(slug),
    queryFn: () => workspaceService.retrieve(slug),
    enabled: !!slug,
  });
}

export function useUpdateWorkspace(slug: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: Partial<IWorkspace>) => workspaceService.update(slug, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: workspaceKeys.detail(slug) });
      queryClient.invalidateQueries({ queryKey: workspaceKeys.all });
    },
  });
}
```

### MobX Filter → TanStack Query Bridge

```typescript
// store/issue-filters.store.ts
import { makeAutoObservable } from "mobx";

export class IssueFiltersStore {
  status: string[] = [];
  priority: string[] = [];
  assigneeId: string[] = [];

  constructor() {
    makeAutoObservable(this);
  }

  setStatus(status: string[]) {
    this.status = status;
  }
  setPriority(priority: string[]) {
    this.priority = priority;
  }
  setAssigneeId(ids: string[]) {
    this.assigneeId = ids;
  }
  clear() {
    this.status = [];
    this.priority = [];
    this.assigneeId = [];
  }
}

// Component
function IssueList() {
  const { issueFilters } = useStore(); // MobX store
  const { data, isLoading } = useIssues({
    status: issueFilters.status,
    priority: issueFilters.priority,
  });
  // ...
}
```

---

## Alternatives Considered

| Category         | Recommended           | Alternative               | Why Not                                                                                                                                                                                                                                                                           |
| ---------------- | --------------------- | ------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Server state     | TanStack Query v5     | SWR v2                    | SWR has weaker mutation support, no DevTools, less explicit invalidation. For a project management app with heavy mutations (drag-and-drop, inline edit), TanStack Query is the better choice.                                                                                    |
| State management | MobX + TanStack Query | Zustand                   | Plane Web's entire store layer is MobX. Migrating to Zustand means rewriting hundreds of store files. The ROI is negative — MobX works fine for UI state. The problem was mixing server data into MobX, which we fix by splitting responsibilities, not by replacing the library. |
| State management | MobX + TanStack Query | Redux Toolkit + RTK Query | RTK Query would replace both MobX and TanStack Query in one package. But Plane Web has 20+ MobX stores with complex logic (filters, command palette, theme). Rewriting those into Redux slices is not worth the effort. MobX is fine for UI state.                                |
| HTTP client      | Axios                 | ky (fetch-based)          | Axios interceptors are critical for JWT injection and 401 handling. ky has interceptors too but Axios is more battle-tested. The entire Plane Web services layer uses Axios — no benefit in switching.                                                                            |
| UI library       | Fork @plane/propel    | Fresh shadcn/ui           | Propel has 30+ components already built and integrated with the editor, types, and Tailwind config. Starting from shadcn means rebuilding every component (dialog, combobox, table, toast, etc.) while matching Plane Web's exact UI. Months of work for zero end-user value.     |
| Forms            | React Hook Form       | Formik                    | RHF is more performant (uncontrolled by default), has better TypeScript inference, and integrates with Zod via `@hookform/resolvers`. Plane Web already uses RHF.                                                                                                                 |
| Build tool       | Vite 7                | Turbopack                 | Turbopack is not stable enough for production. Vite 7 is proven, fast, and Plane Web already migrated to it.                                                                                                                                                                      |
| Package manager  | pnpm                  | npm                       | Plane Web uses pnpm (workspace catalog, fast installs). Flow Web is a standalone app — could use npm. But pnpm's strict dependency resolution catches missing peer deps early. Use pnpm for consistency with developer tooling.                                                   |

---

## Version Compatibility

| Package                   | Version      | Compatible With              | Notes                                                                                                      |
| ------------------------- | ------------ | ---------------------------- | ---------------------------------------------------------------------------------------------------------- |
| react                     | 19.2.7       | @types/react: 19.x           | Need @types/react@19, not 18 (Plane Web uses 18.3.11)                                                      |
| @tanstack/react-query     | 5.101.0      | react 18+, react-dom 18+     | Fully compatible with React 19                                                                             |
| mobx-react                | 9.1.1        | mobx 6.x, react 18+          | Verify react 19 compatibility                                                                              |
| @base-ui-components/react | 1.0.0-beta.3 | react 18+                    | Base UI is the successor to Radix. Verify React 19 support — as of mid-2026 the beta should be compatible. |
| @headlessui/react         | 1.7.19       | react 18+                    | v1 is stable. v2 exists but v1 is sufficient.                                                              |
| @hookform/resolvers       | latest       | react-hook-form 7.x, zod 4.x | Need to verify Zod v4 compatibility with resolvers                                                         |
| react-router              | 7.15.0       | react 18+                    | Compatible with React 19                                                                                   |
| vite                      | 7.3.2        | all listed deps              | Vite 7 plugins are mostly compatible. `vite-tsconfig-paths` works with Vite 7.                             |

## Unknowns / Research Needed

1. **MobX + React 19**: mobx-react@9.1.1 was tested with React 18. Verify React 19 compatibility. If there are issues, options: (a) use mobx-react-lite, (b) stay on React 18 for the MobX parts (impossible — whole app is React), (c) use MobX 6's `observer()` via direct React integration. This needs verification during setup.

2. **@base-ui-components + React 19**: As a beta, Base UI may have React 19 compatibility issues. If problematic, fall back to `@radix-ui/react-*` primitives (they're stable and React 19 compatible).

3. **Zod v4 + @hookform/resolvers**: Zod v4 has API changes. Verify resolvers compatibility before upgrading from v3.

4. **Plane Web's next-themes usage**: `next-themes@0.4.6` is a SSR themes package originally for Next.js. It works in React Router via the `ThemeProvider`. Need to verify it doesn't depend on Next.js internals. If it does, switch to a simple CSS variable approach or use a React Router-compatible theme provider.

---

## Key Decisions

1. **NO monorepo** for Flow Web — It's a standalone `clients/web/` app under `yh-flow/`. Not part of Plane's turbo monorepo. This avoids: turbo config complexity, build pipeline conflicts, and the need to maintain Plane's workspace protocol. We copy/fork Plane packages as local `src/lib/` directories.

2. **Fork, don't symlink** — Plane's packages (`@plane/propel`, `@plane/editor`) are forked into Flow Web's own directory structure. No symlink or git submodule linking to the original Plane code. This means we own the code and can modify it freely, at the cost of manual upstream syncs. Given that Plane's UI changes infrequently and we're progressively diverging, this is the right tradeoff.

3. **TanStack Query + Axios, not TanStack Query + fetch** — Axios interceptors handle JWT injection and 401 redirection at the transport layer. TanStack Query handles caching/refetching at the data layer. They compose via the `queryFn` that calls Axios-based service methods.

4. **SSE for real-time, not WebSocket** — The .NET backend supports Server-Sent Events for notifications. SSE is unidirectional (server → client), simpler than WebSocket, works over HTTP/2, and requires no library — just the browser `EventSource` API. The `EventSource` callback dispatches to TanStack Query for cache invalidation.

---

## Sources

- Plane Web pnpm-workspace.yaml — exact version catalog for all referenced packages. HIGH confidence.
- Plane Web `packages/services/src/api.service.ts` — confirms Axios-based service architecture. HIGH confidence.
- Plane Web `packages/services/src/auth/auth.service.ts` — confirms CSRF/session auth pattern that must be replaced. HIGH confidence.
- Plane Web `apps/web/provider.tsx` — confirms SWRConfig, MobX StoreProvider, next-themes usage. HIGH confidence.
- Plane Web `apps/web/vite.config.ts` — confirms Next.js compat shims that must be removed. HIGH confidence.
- Plane Web `packages/editor/package.json` — confirms Yjs/Hocuspocus deps to strip. HIGH confidence.
- Plane Web `packages/propel/package.json` — confirms Base UI + framer-motion + cmdk + recharts dependency tree. HIGH confidence.
- npm registry — React 19.2.7 latest. HIGH confidence.
- npm registry — @tanstack/react-query 5.101.0. HIGH confidence.
- npm registry — Zod 4.4.3 latest. HIGH confidence.
- npm registry — react-hook-form 7.78.0 latest (and 8.0.0-beta.2). HIGH confidence.
- Context7 would be used to verify specific TanStack Query migration patterns during implementation.

---

_Stack research for: Flow Web Frontend (v2.0 milestone)_
_Researched: 2026-06-26_
