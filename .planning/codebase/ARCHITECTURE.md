# Architecture

**Analysis Date:** 2026-06-16

## System Overview

```text
┌──────────────────────────────────────────────────────────────────────────┐
│                              Client Browser                              │
│                    React Router v7 (CSR, SSR disabled)                   │
└────────────────────────────────────┬─────────────────────────────────────┘
                                     │
                                     ▼
┌──────────────────────────────────────────────────────────────────────────┐
│                        Caddy Reverse Proxy                               │
│                         `apps/proxy/`                                    │
│                    Routes: /api → api:8000                                │
│                            /auth → api:8000                              │
│                            /live → live:3000                             │
│                            /spaces → space:3000                          │
│                            /god-mode → admin:3000                        │
│                            /* → web:3000                                 │
└───────┬──────────────┬──────────────┬──────────────┬─────────────────────┘
        │              │              │              │
        ▼              ▼              ▼              ▼
┌───────────┐  ┌───────────┐  ┌───────────┐  ┌───────────┐
│ Django API │  │ Live Srv  │  │   Web     │  │ Space/    │
│ `apps/api` │  │`apps/live`│  │ `apps/web`│  │ Admin     │
│ :8000      │  │ :3000     │  │ :3000     │  │ :3000/3001│
└─────┬──────┘  └─────┬─────┘  └───────────┘  └───────────┘
      │               │
      ▼               ▼
┌──────────────────────────────────────────────────────────────────────────┐
│                         Infrastructure Layer                             │
├──────────┬──────────┬──────────┬──────────┬─────────────────────────────┤
│PostgreSQL│  Redis   │RabbitMQ  │  MinIO   │  External: OAuth, Sentry    │
│`plane-db`│`plane-   │`plane-mq`│`plane-   │  (Google, GitHub, GitLab)   │
│          │ redis`   │          │ minio`   │                              │
└──────────┴──────────┴──────────┴──────────┴─────────────────────────────┘
```

## Component Responsibilities

| Component             | Responsibility                                           | Key Files                    |
| --------------------- | -------------------------------------------------------- | ---------------------------- |
| Web App               | Full client-side React SPA for project management        | `apps/web/app/root.tsx`      |
| Space App             | Public-facing project views (embeddable)                 | `apps/space/app/`            |
| Admin App             | Instance administration panel                            | `apps/admin/app/`            |
| Django API            | REST API endpoint, business logic, database access       | `apps/api/plane/`            |
| Live Server           | Real-time collaborative editing via Hocuspocus/WebSocket | `apps/live/src/server.ts`    |
| Caddy Proxy           | Reverse proxy routing, TLS termination                   | `apps/proxy/Caddyfile.ce`    |
| Package: Editor       | TipTap/ProseMirror rich text editor                      | `packages/editor/src/`       |
| Package: UI           | Shared React component library (Storybook)               | `packages/ui/src/`           |
| Package: Propel       | New design system component library (Storybook)          | `packages/propel/src/`       |
| Package: Shared-State | Shared MobX stores (rich filters, work item filters)     | `packages/shared-state/src/` |
| Package: Services     | API client service layer (Axios-based)                   | `packages/services/src/`     |
| Package: Types        | Shared TypeScript type definitions                       | `packages/types/src/`        |
| Package: Constants    | Shared constants (API_BASE_URL, auth configs)            | `packages/constants/src/`    |
| Package: Utils        | Shared utility functions                                 | `packages/utils/src/`        |
| Package: i18n         | Translation system based on i18next                      | `packages/i18n/src/`         |
| Package: Hooks        | Shared React hooks                                       | `packages/hooks/src/`        |
| Package: Logger       | Logging abstraction (Winston)                            | `packages/logger/src/`       |
| Package: Decorators   | TypeScript decorators for live server                    | `packages/decorators/src/`   |
| Package: Codemods     | jscodeshift migration transforms                         | `packages/codemods/`         |

## Pattern Overview

**Overall:** Monorepo with CE/EE (Community Edition / Enterprise Edition) code split via file-override pattern.

**Key Characteristics:**

- pnpm workspaces with Turbo orchestration for builds
- React Router v7 for all 3 frontend apps (web, space, admin) running as pure CSR (SSR disabled)
- Django REST Framework for the API backend
- Hocuspocus (Yjs-based) WebSocket server for real-time collaborative text editing
- MobX for state management (shared and app-specific stores)
- CE base code in `core/` directories, EE extensions in `ce/` directories, merged at build time
- Package dependency graph: apps depend on packages; packages are independent or have simple inter-dependency chains
- Centralized dependency catalog in `pnpm-workspace.yaml` with `catalog:` references

## Layers

### Presentation Layer (Frontend Apps)

- Purpose: User-facing React applications
- Location: `apps/web/app/`, `apps/space/app/`, `apps/admin/app/`
- Contains: React Router routes, layouts, page components, app-level components
- Depends on: Package layer (`@plane/ui`, `@plane/propel`, `@plane/editor`, `@plane/services`, `@plane/shared-state`, `@plane/types`, etc.)
- Used by: End users via browser

### Component Layer (CE/EE Split)

- Purpose: Reusable component implementations with community/pro extensions
- Location: `apps/web/core/components/` (base), `apps/web/ce/components/` (EE extensions)
- Contains: Feature-specific components organized by domain (issues, cycles, modules, pages, etc.)
- Depends on: Store layer, package layer
- Used by: Page components in routes

### Store Layer (State Management)

- Purpose: MobX observable stores managing application state
- Location: `apps/web/core/store/` (core stores), `apps/web/ce/store/` (EE extensions), `packages/shared-state/src/store/` (cross-app shared stores)
- Contains: Observable classes per domain (IssueStore, CycleStore, ModuleStore, UserStore, etc.)
- Depends on: Service layer, types package
- Used by: Component layer via hooks

### Service Layer (API Communication)

- Purpose: Typed HTTP client services wrapping Axios
- Location: `packages/services/src/` (shared services), `apps/web/core/services/` (app-specific services)
- Contains: Service classes extending APIService base class, organized by domain
- Depends on: Axios, constants (API_BASE_URL), types
- Used by: Store layer

### API Layer (Backend)

- Purpose: Django REST API serving all data operations
- Location: `apps/api/plane/`
- Contains: Django apps (api, app, authentication, db, license, space, web), URL routes, views, serializers, models
- Depends on: PostgreSQL, Redis, MinIO, RabbitMQ
- Used by: All frontend apps via HTTP/HTTPS

### Real-Time Layer

- Purpose: WebSocket-based collaborative editing and PDF export
- Location: `apps/live/src/`
- Contains: Express server with Hocuspocus integration, Redis connection, page/PDF services
- Depends on: Redis (for Yjs document sync), Django API (for auth)
- Used by: Editor package in frontend apps via WebSocket connections

### Infrastructure Layer

- Purpose: Data persistence, message queuing, file storage
- Key services: PostgreSQL 15 (plane-db), Valkey/Redis 7.2 (plane-redis), RabbitMQ 3.13 (plane-mq), MinIO (plane-minio)
- Managed via: `docker-compose-local.yml`, `docker-compose.yml`

## Data Flow

### Primary Request Path (Issue CRUD)

1. User interacts with issue component (`apps/web/core/components/issues/`) and triggers a store action
2. Store action in `apps/web/core/store/issue/` calls service method
3. Service method in `apps/web/core/services/issue/issue.service.ts` or `packages/services/src/issue/` makes HTTP request via Axios to Django API
4. Request passes through Caddy proxy (`apps/proxy/Caddyfile.ce`) routing `/api/*` to `api:8000`
5. Django middleware authenticates request (`apps/api/plane/api/middleware/api_authentication.py` with `X-Api-Key` header, or session auth through `apps/api/plane/app/middleware/api_authentication.py`)
6. URL dispatcher routes to appropriate ViewSet (`apps/api/plane/api/urls/work_item.py` -> `apps/api/plane/api/views/issue.py`)
7. ViewSet validates input, applies permissions (`apps/api/plane/app/permissions/`), queries PostgreSQL, serializes response
8. Response flows back to service layer, which updates MobX store observable
9. MobX automatically re-renders observer components in the React tree

### Real-Time Collaboration Flow (Document Editing)

1. User opens a page document in the editor (`packages/editor/src/core/`)
2. Editor initializes a Yjs document (`yjs`) with IndexedDB persistence (`y-indexeddb`)
3. TipTap editor binds to Yjs via `@tiptap/extension-collaboration` and `y-prosemirror`
4. Client establishes WebSocket connection to live server via `@hocuspocus/provider`
5. Live server (`apps/live/src/server.ts`) authenticates the connection (`apps/live/src/lib/auth.ts`)
6. Hocuspocus server syncs the Yjs document across connected clients, with Redis extension for cross-node persistence
7. On document save/sync, live server calls Django API to persist content through page services (`apps/live/src/services/page/`)
8. Changes propagate to all connected editors in real-time

### Authentication Flow

1. User visits web app, `AuthenticationWrapper` (`apps/web/core/lib/wrappers/authentication-wrapper.tsx`) checks session via SWR call to `fetchCurrentUser()`
2. If unauthorized, redirects to sign-in page (`apps/web/app/(home)/page.tsx`)
3. Sign-in form submits credentials to `/auth/sign-in/` which proxies to Django auth views (`apps/api/plane/authentication/views/common.py`)
4. Django authenticates, creates session, returns user data with session cookie
5. `AuthenticationWrapper` detects authenticated state, loads workspace list
6. Routes user to appropriate workspace or onboarding flow
7. OAuth providers (Google, GitHub, GitLab) configured in `apps/api/plane/authentication/` flow through provider adapters
8. API key authentication for programmatic access uses `X-Api-Key` header validated against `plane.db.models.APIToken`

### State Management Data Flow

1. `AppProvider` (`apps/web/app/provider.tsx`) wraps the app with `StoreProvider` (MobX context), `TranslationProvider` (i18n), `SWRConfig`
2. `StoreWrapper` (`apps/web/core/lib/wrappers/store-wrapper.tsx`) initializes user profile, theme, sidebar state
3. `InstanceWrapper` (`apps/web/core/lib/wrappers/instance-wrapper.tsx`) fetches instance config before rendering
4. Root store (`apps/web/core/store/root.store.ts`) creates all sub-stores on initialization
5. Each store extends `CoreRootStore` and adds domain-specific observables, computed values, and actions
6. Components read via custom hooks (`apps/web/core/hooks/store/`) using `useContext(StoreContext)` pattern
7. MobX `observer()` wraps components for automatic re-rendering on observable changes

## Routing Architecture

### React Router v7 Flat Routes

All three frontend apps use React Router v7's flat route convention with `routes.ts` as the configuration file.

**Web app route structure** (`apps/web/app/routes.ts`):

- Uses `layout()` and `route()` from `@react-router/dev/routes`
- Routes split into `coreRoutes` and `extendedRoutes`, merged at build time (`apps/web/app/routes/helper.ts`)
- `extendedRoutes` is empty in CE; EE extensions add routes here
- Route hierarchy: `/(home)` -> sign-in, `/:workspaceSlug/` -> workspace-scoped layouts -> project/cycle/module/page sub-routes, `settings/` -> profile and workspace settings

**URL pattern conventions:**

- Workspace scoping: `/:workspaceSlug/projects/:projectId/issues/:issueId`
- Settings: `/:workspaceSlug/settings/projects/:projectId/...`
- Public spaces: `/:workspaceSlug/:projectId/issues/:anchor` in space app
- Admin panel: `instance/`, `workspaces/`, `/god-mode/` prefixed routes
- Legacy redirects: defined in `apps/web/app/routes/redirects/` for backward compatibility

**SSR configuration:** All frontend apps set `ssr: false` in `react-router.config.ts`, running as pure client-side rendered SPAs. The web app uses `@react-router/serve` for production preview.

## Multi-Tenancy Architecture

### Workspace / Project Hierarchy

```text
Instance (self-hosted or cloud)
├── Workspace A (tenant/slug-based)
│   ├── Project 1
│   │   ├── Issues (work items)
│   │   ├── Cycles (sprints)
│   │   ├── Modules (feature groupings)
│   │   ├── Pages (documents)
│   │   ├── Views (saved filter/display configurations)
│   │   └── Intake (inbox for external submissions)
│   ├── Project 2
│   │   └── ...
│   └── Workspace-level: Active Cycles, Analytics, Stickies, Members, Webhooks
├── Workspace B
│   └── ...
└── Workspace C (public space via Space app)
```

**Data isolation:** All API endpoints scope queries by `workspace_slug` from URL path. The `BaseAPIView` (`apps/api/plane/api/views/base.py`) enforces authentication. Permissions check workspace membership via `WorkspaceMember` model with role levels (Admin=20, Member=15, Guest=5) in `apps/api/plane/app/permissions/workspace.py`.

## CE/EE Architecture Pattern

Plane uses a **file-override pattern** to separate community edition (CE) code from enterprise edition (EE) code:

```text
apps/web/
├── core/              # CE base implementation
│   ├── components/     # Base component implementations
│   ├── store/          # Base store implementations
│   ├── hooks/          # Base hook implementations
│   ├── layouts/        # Base layouts
│   └── lib/            # Base utilities
│
├── ce/                # EE extension layer (Community-accessible stubs)
│   ├── components/     # EE component extensions / stubs
│   ├── store/          # EE store extensions / stubs
│   └── hooks/          # EE hook extensions / stubs
│
└── app/               # Route definitions, entry points
    └── routes/         # core.ts (CE routes) + extended.ts (EE routes) merged
```

**Root store merge pattern:**

- `apps/web/core/store/root.store.ts` defines `CoreRootStore` with all base stores
- `apps/web/ce/store/root.store.ts` defines `RootStore extends CoreRootStore` adding EE-only stores (e.g., `timelineStore`)
- The `StoreContext` (`apps/web/core/lib/store-context.tsx`) imports `RootStore` from the CE path, which is the final merged root

**Route merge pattern:**

- `apps/web/app/routes.ts` imports `coreRoutes` and `extendedRoutes`, merges them via `mergeRoutes()`
- `extendedRoutes` is empty array in CE; filled with EE routes in enterprise builds

The same pattern applies to the `packages/editor/src/core/` and `packages/editor/src/ce/` (and `ee/` for editor-specific EE extensions).

## API Design

### REST API Patterns

**Framework:** Django REST Framework (DRF) with ModelViewSets and GenericAPIViews

**URL namespacing:**

- `/api/` -> Internal app endpoints (legacy) -> `plane.app.urls`
- `/api/v1/` -> Main REST API -> `plane.api.urls`
- `/api/public/` -> Public space endpoints (unauthenticated read) -> `plane.space.urls`
- `/api/instances/` -> Instance/license management -> `plane.license.urls`
- `/auth/` -> Authentication endpoints -> `plane.authentication.urls`

**View base classes:**

- `BaseAPIView` (`apps/api/plane/api/views/base.py`): Combines `TimezoneMixin`, `GenericAPIView`, `ReadReplicaControlMixin`, `BasePaginator`. Enforces `APIKeyAuthentication` and `IsAuthenticated` permission.
- `BaseViewSet` extends ModelViewSet with similar mixins.

**Serializers:** Organized by app in `apps/api/plane/api/serializers/`, `apps/api/plane/app/serializers/`, `apps/api/plane/space/serializer/`

**Authentication classes:**

- `APIKeyAuthentication` (`apps/api/plane/api/middleware/api_authentication.py`): Validates `X-Api-Key` header against `APIToken` model
- Session authentication through Django's built-in session framework for web app

**Endpoint organization:** URL patterns split by domain in `apps/api/plane/api/urls/` (work_item.py, cycle.py, module.py, project.py, etc.) and merged in `__init__.py`

**API documentation:** Optional DRF Spectacular integration generating OpenAPI schema at `/api/schema/` with Swagger UI and Redoc

## Build Architecture

### Turbo Pipeline

Turborepo (`turbo.json`) orchestrates the monorepo build with these task pipelines:

```text
dev:          dependsOn [^build]     -> First build all dependencies, then start dev server
build:        dependsOn [^build]     -> Build dependencies first, then self
check:types:  dependsOn [^build]     -> Type-checking requires built types from dependencies
test:         dependsOn [^build]     -> Tests run after dependencies are built
clean:        no cache                -> Always clean fresh
```

**Build tools per app/package:**

- Web, Space, Admin: React Router build (Vite-based) -> builds to `build/client/` and `build/server/`
- API (Django): Python-based, excluded from pnpm workspace
- Live: `tsdown` (TypeScript bundler) -> builds to `dist/`
- Proxy: Caddy server, no build step
- Packages: `tsdown` -> builds to `dist/`
- Legacy packages: `tsc` and `tsdown`

**Package dependency graph (simplified):**

```text
apps/web depends on:  @plane/{ui, propel, editor, services, shared-state, hooks, i18n, types, constants, utils}
apps/space depends on: @plane/{ui, propel, editor, services, i18n, types, constants, utils}
apps/admin depends on: @plane/{ui, propel, services, hooks, types, constants, utils}
apps/live depends on: @plane/{decorators, editor, logger, types, utils}

@plane/editor depends on: @plane/{ui, propel, hooks, types, constants, utils}
@plane/shared-state depends on: @plane/{types, constants, utils}
@plane/services depends on: @plane/{types, constants}
@plane/ui depends on: @plane/{propel, types, constants, hooks, utils}
@plane/propel depends on: @plane/{types, constants, hooks}
@plane/utils depends on: @plane/{types, constants}
```

## Cross-Cutting Concerns

**Logging:** `@plane/logger` package wraps Winston for structured logging. Django uses Python's standard logging with `plane.utils.exception_logger`.

**Validation:** Frontend uses Zod schemas (`packages/shared-state/`). Backend uses DRF serializers with field-level validation.

**Authentication:** Session-based for web UI users (Django sessions). API key-based for programmatic access (`X-Api-Key` header). OAuth for social login (Google, GitHub, GitLab).

**Theme:** `next-themes` for light/dark/contrast/custom theme modes, with `ThemeProvider` at the root level. Custom theme support via `@plane/utils` theme helpers.

**Error handling:** API errors propagate through Axios interceptors in `APIService` base class. Django uses DRF exception handling with custom exception logging. Frontend components have error boundaries at the route level (`apps/web/app/error/`).

**File uploads:** MinIO (S3-compatible) for file storage. Assets served through the proxy or directly. File type validation with `file-type` package.

---

_Architecture analysis: 2026-06-16_
