# Technology Stack

**Analysis Date:** 2026-06-16

## Languages

**Primary:**
- TypeScript 5.8.3 - All frontend apps (`apps/web`, `apps/admin`, `apps/space`), all packages (`packages/*`), real-time server (`apps/live`)
- Python (Django 4.2.30) - Backend API server (`apps/api`)

**Secondary:**
- PostgreSQL SQL - Database migrations and queries
- CSS (via Tailwind CSS 4.1.17) - Application styling
- Dockerfile / YAML - Infrastructure configuration
- Caddyfile - Reverse proxy configuration (`apps/proxy`)

## Runtime

**Environment:**
- Node.js >= 22.18.0 (required by `engines` field in root `package.json`)
- Python 3.x (Django backend, version not pinned explicitly)

**Package Manager:**
- pnpm 11.3.0 (exact version pinned via `packageManager` field in root `package.json`)
- Lockfile: `pnpm-lock.yaml` (present)
- Python: pip + requirements text files

**Monorepo Orchestration:**
- Turbo 2.9.18 - Task runner and build pipeline
- pnpm Workspaces (with catalog-based dependency versions)

## Frameworks

**Core:**
- **React 18.3.1** - Frontend UI framework (all apps and packages)
- **React Router 7.15.0** - Routing (replaces Next.js — migrated away per react-router.config.ts and Next.js compatibility shims)
- **Django 4.2.30** - Backend API framework (`apps/api`)
- **Django REST Framework 3.15.2** - REST API framework for backend
- **Express 4.22.0** - HTTP server for the real-time collaboration server (`apps/live`)

**State Management:**
- **MobX 6.12.0** - Core state management library across all frontend apps
- **mobx-react 9.1.1** - React bindings for MobX
- **mobx-utils 6.0.8** - Utility functions for MobX
- **SWR 2.2.4** - Data fetching and caching (used alongside MobX)
- `packages/shared-state` - Internal MobX-based shared state store used across apps

**Styling:**
- **Tailwind CSS 4.1.17** - Utility-first CSS framework
- **@tailwindcss/typography 0.5.19** - Prose styling plugin
- **@tailwindcss/postcss 4.1.17** - PostCSS plugin for Tailwind
- **Framer Motion 12.23.0** - Animation library
- **class-variance-authority 0.7.1** - Component variant management
- **tailwind-merge 3.4.0** - Tailwind class conflict resolver
- **clsx 2.1.1** - Conditional className utility

**Real-time Collaboration:**
- **Yjs 13.6.20** - CRDT-based real-time collaborative editing library
- **Hocuspocus 2.15.2** - WebSocket-based backend server for Yjs collaboration
- **y-prosemirror 1.3.7** - Yjs binding for ProseMirror
- **y-protocols 1.0.6** - Yjs awareness protocol
- **y-indexeddb 9.0.12** - Yjs persistence using IndexedDB (browser)

**Rich Text Editing:**
- **TipTap 2.22.3** - Headless rich text editor (based on ProseMirror)
- ProseMirror (via `@tiptap/pm` 2.22.3) - Core editor engine
- **tiptap-markdown 0.8.10** - Markdown serialization/deserialization for TipTap
- **prosemirror-codemark 0.4.2** - Inline code support
- `packages/editor` - Internal wrapper package for the TipTap editor with collaboration, markdown conversion, and plane-specific extensions

**Testing:**
- **Vitest 4.1.8** - Vite-native test runner (frontend)
- **@vitest/coverage-v8 4.1.8** - Code coverage for Vitest
- **pytest 9.0.3** - Backend test framework
- **pytest-django 4.5.2** - Django integration for pytest
- **pytest-cov 4.1.0** - Coverage for pytest
- **pytest-xdist 3.3.1** - Parallel test execution
- **factory-boy 3.3.0** - Test data factories (Python)
- **Storybook 9.1.19** - UI component development and documentation (`packages/ui/.storybook`)

**Build/Dev:**
- **Vite 7.3.2** - Build tool (all frontend apps)
- **tsdown 0.16.0** - TypeScript bundler for packages (used in place of tsc for package outputs)
- **esbuild 0.28.1** - Bundler (explicitly overridden in pnpm-workspace.yaml)
- **tsx 4.20.6** - TypeScript execution for scripts
- **PostCSS 8.5.10** - CSS processing

## Key Dependencies

**Critical:**
- **Axios 1.16.0** - HTTP client for all frontend-to-backend API calls (`packages/services` uses this as its core HTTP transport)
- **Zod 3.25.76** - Schema validation (used extensively in `packages/shared-state` for store models and `apps/live` for env validation)
- **ioredis 5.7.0** - Redis client for Node.js (used by `apps/live` and Hocuspocus Redis extension)
- **date-fns 4.1.0** - Date utility library
- **lodash-es 4.18.1** - Utility functions (ES module version, tree-shakeable)
- **uuid 14.0.0** - Unique ID generation
- **@tanstack/react-table 8.21.3** - Table component
- **@tanstack/react-virtual 3.13.12** - Virtual scrolling
- **react-hook-form 7.51.5** - Form management
- **recharts 2.15.1** - Chart library (used for dashboards, analytics)
- **react-day-picker 9.5.0** - Date picker
- **react-dropzone 14.2.3** - File upload drag-and-drop
- **react-markdown 8.0.7** - Markdown rendering in React
- **lucide-react 0.469.0** - Icon library
- **sanitize-html 2.17.0** - HTML sanitization

**Drag and Drop:**
- **@atlaskit/pragmatic-drag-and-drop 1.7.4** - Atlassian's drag-and-drop library
- **@atlaskit/pragmatic-drag-and-drop-auto-scroll 1.4.0** - Auto-scroll extension
- **@atlaskit/pragmatic-drag-and-drop-hitbox 1.1.0** - Hitbox extension

**Authentication:**
- **PyJWT 2.12.0** - JWT handling in the backend
- **cryptography 46.0.7** - Cryptographic operations

**Infrastructure:**
- **boto3 1.34.96** - AWS S3 SDK (Python, used for S3/MinIO storage)
- **redis 5.0.4** - Python Redis client
- **django-redis 5.4.0** - Django Redis cache backend
- **celery 5.4.0** - Distributed task queue
- **django-celery-beat 2.6.0** - Periodic task scheduler for Celery
- **django-celery-results 2.5.1** - Task result storage
- **gunicorn 23.0.0** - WSGI server for production
- **uvicorn 0.29.0** - ASGI server (for WebSocket support via Django Channels)
- **channels 4.1.0** - Django WebSocket framework
- **whitenoise 6.11.0** - Static file serving
- **django-storages 1.14.2** - Storage backends (S3)
- **django-cors-headers 4.3.1** - CORS handling

**Monitoring & Observability:**
- **scout-apm 3.1.0** - APM agent (production only)
- **opentelemetry-api 1.28.1** / **opentelemetry-sdk 1.28.1** - OpenTelemetry instrumentation
- **opentelemetry-instrumentation-django 0.49b1** - Django auto-instrumentation
- **opentelemetry-exporter-otlp 1.28.1** - OTLP exporter
- **winston 3.17.0** - Node.js logging (used in `apps/live`)
- **express-winston 4.2.0** - Express logging middleware
- **python-json-logger 4.0.0** - JSON log formatting for Python

**AI/LLM:**
- **openai 1.63.2** - OpenAI API client
- **Effect 3.20.0** / **@effect/platform 0.94.0** - Effect system used in live server for structured error handling and service modeling
- **posthog 3.5.0** - Product analytics

**Other Libraries:**
- **emoji-picker-react 4.5.16** - Emoji picker component
- **highlight.js 11.8.0** - Syntax highlighting
- **lowlight 3.0.0** - Virtual syntax highlighting (used in editor)
- **linkifyjs 4.3.2** - Link detection
- **sharp 0.34.3** - Image processing (used in `apps/live` for PDF generation)
- **react-pdf-html 2.1.2** - HTML to PDF rendering
- **@react-pdf/renderer 4.3.0** - PDF generation with React components
- **pdf-parse 2.4.5** - PDF text extraction
- **helmet 7.1.0** - Express security headers
- **compression 1.8.1** - HTTP compression middleware
- **cors 2.8.5** - Express CORS middleware
- **next-themes 0.4.6** - Theme management (dark/light mode)
- **chroma-js 3.2.0** - Color manipulation library
- **class-variance-authority 0.7.1** - Component variant definitions

## Configuration

**Environment:**
- Root `.env` file (not committed) — shared infrastructure variables (PostgreSQL, Redis, RabbitMQ, MinIO/AWS, proxy ports)
- `apps/api/.env` — backend-specific variables (debug, CORS, all service URLs, secrets)
- `apps/web/.env` — frontend-specific variables (VITE_-prefixed API base URLs)
- `apps/admin/.env` — admin panel variables
- `apps/space/.env` — public space variables
- `apps/live/.env` — real-time server variables
- `.env.example` files present at root and each app directory

**Build:**
- `turbo.json` — Turbo pipeline configuration at project root
- `pnpm-workspace.yaml` — Workspace definition with shared catalog
- `tsconfig.json` (per app/package) — TypeScript configuration
- `vite.config.ts` (`apps/web`) — Vite build config with React Router plugin and tsconfig-paths
- `react-router.config.ts` (`apps/web`) — React Router configuration (SSR disabled — pure client-side SPA)
- `Dockerfile.*` — Multi-stage Docker builds for each service
- `docker-compose.yml` — Production deployment with Caddy reverse proxy
- `docker-compose-local.yml` — Local development infrastructure
- `docker-compose-test.yml` — Test environment
- `apps/proxy/Caddyfile.ce` — Caddy reverse proxy configuration

**Linting & Formatting:**
- **oxlint 1.51.0** — Linting (all TypeScript/JavaScript — no ESLint found)
- **oxfmt 0.35.0** — Code formatting (all TypeScript/JavaScript — no Prettier found)
- **lint-staged 16.2.7** — Pre-commit hook runner (formats with oxfmt, lints with oxlint)
- **husky 9.1.7** — Git hooks manager

**Formatting Configuration (inline via lint-staged):**
- oxfmt is used for: `*.{js,jsx,ts,tsx,cjs,mjs,cts,mts,json,css,md}`
- oxlint with `--fix --deny-warnings` for: `*.{js,jsx,ts,tsx,cjs,mjs,cts,mts}`

## Platform Requirements

**Development:**
- Node.js >= 22.18.0
- pnpm 11.3.0
- Python 3.x (for backend development)
- PostgreSQL 15.7 (via Docker)
- Redis (Valkey 7.2.11) (via Docker)
- RabbitMQ 3.13.6 (via Docker)
- MinIO (S3-compatible storage, via Docker)
- Docker & Docker Compose (for local infrastructure)

**Production:**
- Docker Compose deployment (main docker-compose.yml)
- Services: web (Node.js), admin (Node.js), space (Node.js), api (Django+Gunicorn), worker (Celery), beat-worker (Celery Beat), migrator (Django migrations), live (Node.js/Hocuspocus), plane-db (PostgreSQL), plane-redis (Valkey), plane-mq (RabbitMQ), plane-minio (MinIO), proxy (Caddy)

---

*Stack analysis: 2026-06-16*
