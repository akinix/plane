# Codebase Structure

**Analysis Date:** 2026-06-16

## Directory Layout

```
plane/
├── apps/                    # Application layer
│   ├── web/                 # Main React web application (port 3000)
│   │   ├── app/             # Routes, pages, layouts, assets (React Router)
│   │   ├── core/            # CE base: components, store, hooks, layouts, lib, services
│   │   ├── ce/              # EE extensions: components, store, hooks (community stubs)
│   │   ├── helpers/         # Authentication, UI helper functions
│   │   ├── styles/          # Global CSS
│   │   ├── public/          # Static assets for build
│   │   └── nginx/           # Nginx config for deployment
│   │
│   ├── space/               # Public space/embed views (port 3002)
│   │   ├── app/             # Routes, pages, assets (React Router)
│   │   ├── components/      # Components: issues, views, editor, account
│   │   ├── hooks/           # OAuth, store hooks
│   │   ├── store/           # MobX stores for space-specific data
│   │   ├── helpers/         # Authentication helpers
│   │   ├── styles/          # Global CSS
│   │   ├── public/          # Static assets
│   │   └── nginx/           # Nginx config
│   │
│   ├── admin/               # Admin panel (port 3001)
│   │   ├── app/             # Routes, pages, assets (React Router)
│   │   ├── components/      # Admin-specific components
│   │   ├── hooks/           # OAuth, store hooks
│   │   ├── store/           # Admin-specific MobX stores
│   │   ├── helpers/         # Helpers
│   │   ├── providers/       # React context providers
│   │   ├── styles/          # Global CSS
│   │   ├── public/          # Static assets
│   │   └── nginx/           # Nginx config
│   │
│   ├── api/                 # Django REST API backend (port 8000)
│   │   ├── plane/           # Main Django project
│   │   │   ├── api/         # v1 REST API (views, serializers, urls, middleware)
│   │   │   ├── app/         # Internal app endpoints (views, permissions, serializers, urls)
│   │   │   ├── authentication/  # Auth: adapters, providers, middleware, views, utils
│   │   │   ├── db/          # Django models (30+ model files), migrations
│   │   │   ├── license/     # Instance license management (API, models, migrations)
│   │   │   ├── space/       # Public space API (views, serializers, urls)
│   │   │   ├── web/         # Web-specific endpoints
│   │   │   ├── bgtasks/     # Background task definitions
│   │   │   ├── middleware/  # Global middleware (db_routing, request_body_size, logger)
│   │   │   ├── settings/    # Django settings
│   │   │   ├── utils/       # Utilities: exporters, filters, paginators, permissions, porters
│   │   │   ├── tests/       # Contract, smoke, unit tests
│   │   │   ├── seeds/       # Seed data
│   │   │   └── static/      # Static files (CSS, JS, logos)
│   │   ├── requirements/    # Python dependencies
│   │   ├── templates/       # Django templates (emails, admin)
│   │   └── tests/           # Top-level test directories
│   │
│   ├── live/                # Real-time collaboration server (port 3000)
│   │   ├── src/             # Source code
│   │   │   ├── server.ts    # Express server setup with Hocuspocus
│   │   │   ├── start.ts     # Entry point with graceful shutdown
│   │   │   ├── hocuspocus.ts # HocusPocus server manager (singleton)
│   │   │   ├── redis.ts     # Redis connection manager
│   │   │   ├── env.ts       # Environment config with Zod validation
│   │   │   ├── controllers/ # Route controllers
│   │   │   ├── extensions/  # Hocuspocus extensions (title-update)
│   │   │   ├── lib/         # Auth, stateless handlers, error handling, PDF base
│   │   │   ├── schema/      # Zod schemas
│   │   │   ├── services/    # Business logic (page services, PDF export)
│   │   │   ├── types/       # TypeScript type definitions
│   │   │   └── utils/       # Utility functions
│   │   └── tests/           # Test directory
│   │
│   └── proxy/               # Caddy reverse proxy/gateway
│       ├── Caddyfile.ce     # Community edition proxy config
│       ├── Caddyfile.aio.ce # All-in-one deployment config
│       └── Dockerfile.ce    # Docker image for Caddy
│
├── packages/                # Shared library layer
│   ├── ui/                  # Shared component library with Storybook
│   │   ├── src/             # Components: avatar, badge, button, card, dropdown, form-fields,
│   │   │                   #   header, link, modals, popovers, progress, row, sortable,
│   │   │                   #   spinners, tables, tabs, tag, tooltip, typography, etc.
│   │   └── .storybook/      # Storybook configuration
│   │
│   ├── propel/              # New design system library with Storybook
│   │   ├── src/             # Components: accordion, avatar, badge, banner, button,
│   │   │                   #   calendar, card, charts, combobox, command, dialog,
│   │   │                   #   emoji-icon-picker, emoji-reaction, icons, input,
│   │   │                   #   menu, pill, popover, portal, scrollarea, skeleton,
│   │   │                   #   spinners, switch, tabs, table, toast, toolbar, tooltip
│   │   └── .storybook/      # Storybook configuration
│   │
│   ├── editor/              # Rich text editor (TipTap/ProseMirror)
│   │   └── src/
│   │       ├── core/        # Base editor: components, extensions, hooks, plugins,
│   │       │                #   constants, contexts, helpers, types
│   │       ├── ce/          # CE extensions (community stubs)
│   │       ├── ee/          # EE extensions
│   │       └── styles/      # Editor CSS
│   │
│   ├── shared-state/        # Shared MobX stores
│   │   └── src/
│   │       ├── store/       # Rich filter store, work-item filter store
│   │       └── utils/       # Store utilities
│   │
│   ├── services/            # API client services (Axios-based)
│   │   └── src/             # Service classes: ai, auth, cycle, dashboard,
│   │                        #   developer, file, instance, intake, issue,
│   │                        #   label, module, project, state, user, workspace
│   │
│   ├── types/               # TypeScript type definitions
│   │   └── src/             # Type files: cycle, editor, favorite, importer,
│   │                        #   instance, intake, issues, layout, module, page,
│   │                        #   project, rich-filters, workspace-draft-issues
│   │
│   ├── constants/           # Shared constants
│   │   └── src/             # analytics, auth, event-tracker, issue,
│   │                        #   rich-filters, settings
│   │
│   ├── utils/               # Utility functions
│   │   └── src/             # editor (markdown parser), permission, theme,
│   │                        #   rich-filters (factories, operations, operators,
│   │                        #   validators, values), work-item, work-item-filters
│   │
│   ├── hooks/               # Shared React hooks
│   │   └── src/
│   │
│   ├── i18n/                # Translation (i18next)
│   │   └── src/
│   │       ├── locales/     # Translation files: cs, de, en, es, fr, id, it, ja,
│   │       │                #   ko, pl, pt-BR, ro, ru, sk, tr-TR, ua, vi-VN, zh-CN, zh-TW
│   │       ├── core/        # Core i18n setup
│   │       ├── hooks/       # Translation hooks
│   │       ├── provider/    # TranslationProvider component
│   │       └── types/       # i18n type definitions
│   │
│   ├── logger/              # Logging abstraction (Winston)
│   │   └── src/
│   │
│   ├── decorators/          # TypeScript decorators for live server
│   │   └── src/
│   │
│   ├── tailwind-config/     # Tailwind CSS v4 config
│   │
│   ├── typescript-config/   # Shared TypeScript configs
│   │
│   └── codemods/            # jscodeshift migration transforms
│       └── tests/
│
├── deployments/             # Deployment configurations
│   ├── aio/                 # All-in-one deployment
│   ├── cli/                 # CLI deployment tooling
│   ├── kubernetes/          # K8s manifests
│   └── swarm/               # Docker Swarm config
│
├── .github/                 # GitHub Actions CI, issue templates
├── docs/                    # Documentation
├── .planning/               # Planning documents (this file)
├── .claude/                 # Claude Code skills
├── .husky/                  # Git hooks (lint-staged)
│
├── package.json             # Root package (scripts, engines, husky config)
├── pnpm-workspace.yaml      # Workspace definitions, catalog deps, overrides
├── pnpm-lock.yaml           # Dependency lockfile
├── turbo.json               # Turborepo configuration
├── .npmrc                   # npm config (note: package manager auth)
├── .oxfmtrc.json            # Oxfmt formatter config
├── .oxlintrc.json            # Oxlint linter config
├── .mise.toml               # Mise runtime version manager config
├── docker-compose.yml       # Production Docker Compose
├── docker-compose-local.yml # Local development Docker Compose
├── docker-compose-test.yml  # Test Docker Compose
├── setup.sh                 # Setup script
├── LICENSE.txt              # AGPL-3.0
├── AGENTS.md                # Agent instructions
└── README.md                # Project README
```

## Directory Purposes

**apps/web/:** The main React application. Users interact with workspaces, projects, issues, cycles, modules, pages through this app. Uses React Router v7 for routing, MobX for state, and communicates with the Django API. Runs as pure client-side SPA (no SSR).

- Entry point: `apps/web/app/entry.client.tsx` - Hydrates the React Router app
- Root component: `apps/web/app/root.tsx` - HTML shell, meta tags, theme provider, fonts
- Provider chain: `apps/web/app/provider.tsx` - StoreProvider -> TranslationProvider -> SWRConfig -> StoreWrapper -> InstanceWrapper
- Route config: `apps/web/app/routes.ts` - Merges core + extended routes
- Route definitions: `apps/web/app/routes/core.ts`, `apps/web/app/routes/extended.ts`, `apps/web/app/routes/redirects/`
- Auth layout: `apps/web/core/layouts/auth-layout/` - Authentication gate
- Default layout: `apps/web/core/layouts/default-layout/` - Base page wrapper

**apps/space/:** Public-facing project views. Allows unauthenticated users to view project issues, submit feedback. Runs on port 3002.

- Entry point: `apps/space/app/entry.client.tsx`
- Routes: `apps/space/app/[workspaceSlug]/[projectId]/issues/`
- Components: `apps/space/components/issues/` - Issue list, peek overview, filters, reactions

**apps/admin/:** Instance administration panel. Manages workspaces, users, instance settings. Runs on port 3001. Accessed via `/god-mode/` path.

- Routes: `apps/admin/app/(all)/(dashboard)/` - Dashboard and workspace management
- Components: `apps/admin/components/instance/` - Instance configuration
- Components: `apps/admin/components/workspace/` - Workspace management

**apps/api/:** Django REST API backend. All business logic, database operations, and authentication.

- Settings: `apps/api/plane/settings/` - Django settings (multiple environment files)
- URL root: `apps/api/plane/urls.py` - Mounts api/, auth/, space/, license/, app/
- Models: `apps/api/plane/db/models/` - 30+ model files covering all domain entities
- Main REST API: `apps/api/plane/api/` - v1 REST endpoints
- Authentication: `apps/api/plane/authentication/` - OAuth providers, session management
- Background tasks: `apps/api/plane/bgtasks/` - Celery/background task definitions

**apps/live/:** Real-time collaboration server. Express + Hocuspocus for WebSocket-based Yjs document synchronization. Handles collaborative page editing and PDF export.

- Entry point: `apps/live/src/start.ts` - Initializes Server class
- Server: `apps/live/src/server.ts` - Express app with WebSocket, Hocuspocus, Redis
- Auth: `apps/live/src/lib/auth.ts` - WebSocket connection authentication
- Page services: `apps/live/src/services/page/` - Document CRUD via API calls
- PDF export: `apps/live/src/services/pdf-export/` - PDF generation service
- Redis: `apps/live/src/redis.ts` - Redis connection manager for Yjs sync

**apps/proxy/:** Caddy reverse proxy. Routes incoming requests to the appropriate backend service based on URL prefix. Provides TLS termination.

**packages/ui/:** Legacy shared component library. Storybook at `packages/ui/.storybook/`. Contains components used across all frontend apps.

**packages/propel/:** New design system library. Replaces/extends packages/ui with more modern components. Includes chart components, command palette, emoji picker, and advanced UI primitives. Storybook at `packages/propel/.storybook/`.

**packages/editor/:** Core rich text editor. Built on TipTap/ProseMirror. Supports collaborative editing via Yjs integration, Markdown input, code blocks with syntax highlighting, image handling, mentions, slash commands, and work item embeds.

**packages/shared-state/:** Cross-app MobX stores. Primarily the rich filter and work-item filter systems shared between web and space apps.

**packages/services/:** TypeScript API client layer. Each domain has its own service class extending `APIService` base. Handles HTTP communication with the Django API.

**packages/types/:** Central TypeScript type definitions. All domain types, API request/response types, and shared interfaces.

**packages/constants/:** Shared constants (API_BASE_URL, auth configuration, event tracker configs, issue status values, rich filter definitions).

**packages/utils/:** Shared utility functions. Editor utilities (Markdown parser), permission checks, theme helpers, rich filter operations, work item helpers.

**packages/hooks/:** Reusable React hooks shared across apps.

**packages/i18n/:** Internationalization system. 19 supported locales. Uses i18next with ICU message format. Translation files in JSON format with namespace support.

**packages/logger/:** Logging abstraction built on Winston. Provides structured logging with log levels.

**packages/decorators/:** TypeScript decorator utilities. Used by the live server for controller registration (`registerController`).

## Key File Locations

**Entry Points:**
- `apps/web/app/entry.client.tsx`: Web app hydration entry
- `apps/web/app/root.tsx`: Root HTML shell and layout
- `apps/web/app/provider.tsx`: App-level provider composition
- `apps/web/app/routes.ts`: Route configuration merge point
- `apps/space/app/entry.client.tsx`: Space app entry
- `apps/admin/app/entry.client.tsx`: Admin app entry
- `apps/live/src/start.ts`: Live server entry point
- `apps/api/manage.py`: Django management entry point

**Configuration:**
- `turbo.json`: Turborepo task pipeline and env vars
- `pnpm-workspace.yaml`: Workspace definition, catalog deps, overrides
- `package.json`: Root package scripts (dev, build, check, fix)
- `.oxlintrc.json`: Oxlint configuration
- `.oxfmtrc.json`: Oxfmt formatter configuration
- `.mise.toml`: Runtime version management
- `apps/api/plane/settings/`: Django settings (common, development, production, test)
- `apps/web/react-router.config.ts`: Web app React Router config (ssr: false)
- `apps/live/tsconfig.json`: Live server TypeScript config

**Core Logic:**
- `apps/web/core/store/root.store.ts`: Root MobX store composing all sub-stores
- `apps/web/core/lib/store-context.tsx`: MobX StoreProvider with React Context
- `apps/web/core/lib/wrappers/`: Authentication wrapper, store wrapper, instance wrapper
- `apps/api/plane/api/views/base.py`: Base API view class with auth, pagination, timezone
- `apps/api/plane/db/models/`: All database models
- `apps/live/src/server.ts`: Live server Express app setup
- `apps/live/src/hocuspocus.ts`: HocusPocus server singleton manager
- `packages/services/src/api.service.ts`: APIService base class for HTTP requests

**Testing:**
- `apps/api/plane/tests/`: Django tests (contract, smoke, unit)
- `apps/live/tests/`: Live server tests (Vitest)
- `packages/codemods/tests/`: Codemod tests

## Naming Conventions

**Files:**
- TypeScript: kebab-case for files (`issue.store.ts`, `project-page.service.ts`, `authentication-wrapper.tsx`), PascalCase for component files that export a default component
- Python: snake_case (`api_authentication.py`, `issue.py`, `base.py`)
- Index files: `index.ts` or `index.tsx` for barrel exports
- Configuration: lowercase with extensions (`turbo.json`, `tsconfig.json`, `.oxlintrc.json`)
- Docker: `Dockerfile`, `docker-compose*.yml`
- Caddy: `Caddyfile`, `Caddyfile.*`

**Directories:**
- Apps: short descriptive names (`web`, `space`, `admin`, `api`, `live`, `proxy`)
- Packages: scope-prefixed with `@plane/` (`@plane/editor`, `@plane/types`)
- Feature modules: kebab-case directories (`rich-filters`, `work-item-filters`, `base-layouts`)
- Django apps: lowercase (`authentication`, `license`)
- React Router routes: Group folders like `(all)`, `(home)`, `(projects)`, `(settings)` for layout grouping; dynamic segments with brackets `[workspaceSlug]`, `[projectId]`

**Package Names:**
- All internal packages: `@plane/<name>` (e.g., `@plane/ui`, `@plane/editor`)
- Apps: plain names (`web`, `space`, `admin`, `live`)

## Where to Add New Code

**New Feature (Web App):**
- Routes: `apps/web/app/routes/core.ts` (or `extended.ts` for EE)
- Pages: `apps/web/app/(all)/[workspaceSlug]/(projects)/<feature>/`
- Components: `apps/web/core/components/<feature>/` (CE base), `apps/web/ce/components/<feature>/` (EE extensions)
- Store: `apps/web/core/store/<feature>.store.ts`
- Hook: `apps/web/core/hooks/store/<feature>/`
- Services: `apps/web/core/services/<feature>/` or `packages/services/src/<feature>/`
- Types: `packages/types/src/<feature>/`

**New Feature (API Backend):**
- Models: `apps/api/plane/db/models/<feature>.py` + migration
- Views: `apps/api/plane/api/views/<feature>.py`
- Serializers: `apps/api/plane/api/serializers/<feature>/`
- URL config: `apps/api/plane/api/urls/<feature>.py` + register in `__init__.py`
- Permissions: `apps/api/plane/app/permissions/<feature>.py`

**New Shared Package:**
- Create directory: `packages/<name>/`
- Add to `pnpm-workspace.yaml` (already covered by `packages/*`)
- Package name: `@plane/<name>`
- Export: `main`, `module`, `types` in package.json pointing to `dist/`
- Build: use `tsdown` (consistent with existing packages)
- TypeScript: extend `@plane/typescript-config/base.json`

**New Component (Design System):**
- Add to `packages/propel/src/<component-name>/` for new design system components
- Add to `packages/ui/src/<component-name>/` for legacy UI components
- Include Storybook stories (follow existing pattern)

**New Translation/String:**
- Add keys in `packages/i18n/src/locales/en/` JSON files
- Import namespace, use `useTranslation()` hook or `t()` function

**Tests:**
- Django tests: `apps/api/plane/tests/` (contract, smoke, or unit subdirectories)
- Live server tests: `apps/live/tests/` (Vitest, co-located pattern)
- No frontend test suite detected for web/space/admin apps

## Special Directories

**node_modules/:** Package dependencies. Not committed. Managed by pnpm with hoisted structure.

**dist/:** Build output for packages and the live server. Committed: No. Pattern: `packages/*/dist/`, `apps/live/dist/`.

**build/:** React Router build output for web/space/admin apps. Committed: No. Pattern: `apps/web/build/client/`, `apps/web/build/server/`.

**.react-router/:** React Router type generation output. Committed: No. Generated by `react-router typegen`.

**.turbo/:** Turborepo cache. Not committed. Generated during build operations.

**.git/:** Git repository data. Not committed directly.

**apps/api/plane/db/migrations/:** Django database migration files. Committed: Yes. Generated by `python manage.py makemigrations`.

**apps/api/plane/static/:** Django static files (CSS, JS, logos). Committed: Yes. Collected via `collectstatic`.

**Storybook static:** Storybook build output in `storybook-static/`. Not committed.

---

*Structure analysis: 2026-06-16*
