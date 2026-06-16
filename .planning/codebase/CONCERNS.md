# Codebase Concerns

**Analysis Date:** 2026-06-16

---

## 1. Dependency Health

### 1.1 Large Dependency Footprint

The `pnpm-lock.yaml` is **18,131 lines** (648KB), reflecting a deep and wide dependency tree across 6 apps and 15 packages in the pnpm workspace. The TypeScript side alone has 3,468 source files, and the Python API has 628 source files plus 121 Django migration files.

- **Root `package.json`** (`d:\github\akinix-plane\package.json`): Node >=22.18.0, pnpm@11.3.0
- **21 `package.json` files** across the monorepo (root + 6 apps + 14 packages)
- The catalog in `d:\github\akinix-plane\pnpm-workspace.yaml` pins ~195 packages to specific versions

### 1.2 Aggressive Override Management

The `pnpm-workspace.yaml` override section (lines 197-235) contains **37 override entries**, many driven by security concerns:

| Override | Rationale |
|----------|-----------|
| `esbuild: 0.28.1` | Advisory resolution (ref: #9236) |
| `vite: "catalog:"` | Advisory resolution (ref: #9215) |
| `axios: "catalog:"` | CVE resolution (ref: #8930) |
| `follow-redirects: 1.16.0` | Security patch pin |
| `rollup: 4.59.0` | Advisory resolution |
| `qs: 6.15.2` | Security pin |
| `diff: 5.2.2` | Security pin |
| `serialize-javascript: 7.0.5` | Security pin |
| `path-to-regexp: 0.1.13` | Security pin |
| `express: "catalog:"` | Version alignment |
| `@types/express: 4.17.23` | Version alignment |
| `webpack: 5.104.1` | Security pin |
| `lodash/lodash-es: 4.18.1` | Single-version policy |
| `fast-uri@<3.1.2: >=3.1.2` | Minimum version guard |

Concern: **37 overrides are brittle** -- any upstream package that upgrades its transitive dependency past the pinned version can create unresolvable conflicts. The override list needs active maintenance.

### 1.3 Python Dependency Pinning

Python requirements in `d:\github\akinix-plane\apps\api\requirements\base.txt` are reasonably pinned:
- Django 4.2.30 (LTS, security-supported)
- urllib3 >=2.7.0 (explicit CVE pin for CVE-2026-44431/CVE-2026-44432)
- requests 2.33.0 (SSRF IP pinning fix)
- cryptography 46.0.7
- celery 5.4.0

Concern: Django 4.2 LTS reaches end of extended support in **April 2026** (this analysis is June 2026). An upgrade to Django 5.x will be required soon. Additionally, only **2 Ruff lint rules** (E + F) are enabled in `pyproject.toml` -- a minimal set that catches basic syntax/import issues but no style or complexity rules.

### 1.4 Known Suppressed Vulnerability

**File:** `d:\github\akinix-plane\.trivyignore`

One CVE is suppressed:

| CVE | Issue | Status |
|-----|-------|--------|
| CVE-2026-30242 | SSRF in Plane webhook URL serializer | **False positive**: Trivy matches the backend's distribution name "Plane" v0.24.0 against a makeplane/plane CVE. The fix targets the upstream web version (v1.2.3+). The mitigation is confirmed in place for the applicable code path. |

Concern: The Apache Plane v0.24.0 version in `pyproject.toml` diverges from the public Plane release versioning (v1.3.1 in `package.json`). This version mismatch confuses vulnerability scanners and should be aligned.

---

## 2. Architecture Concerns

### 2.1 Monorepo Complexity

**6 apps + 15 packages** across a mixed-language monorepo:

| App | Purpose | Language |
|-----|---------|----------|
| `apps/web` | Main web app | TypeScript (React Router) |
| `apps/admin` | Admin dashboard | TypeScript (React Router) |
| `apps/space` | Public space view | TypeScript (React Router) |
| `apps/api` | Backend API | Python (Django + DRF) |
| `apps/live` | Real-time collaboration | TypeScript (Hocuspocus + Yjs) |
| `apps/proxy` | Reverse proxy | Caddy (2 configs: CE + AIO) |

| Package | Purpose |
|---------|---------|
| `packages/editor` | Rich text editor (ProseMirror/TipTap) - 225 source files |
| `packages/ui` | Shared UI component library |
| `packages/propel` | Design system components |
| `packages/types` | Shared TypeScript types |
| `packages/utils` | Shared utilities |
| `packages/constants` | Shared constants |
| `packages/services` | API service layer |
| `packages/hooks` | Shared React hooks |
| `packages/i18n` | Internationalization |
| `packages/logger` | Logging utilities |
| `packages/shared-state` | State synchronization (Yjs) |
| `packages/codemods` | Code transformation scripts |
| `packages/decorators` | Decorator utilities |
| `packages/tailwind-config` | Tailwind CSS configuration |
| `packages/typescript-config` | Shared TS config base |

Concern: **21 package.json files with interdependent builds.** The `turbo.json` build pipeline chains dependencies via `"dependsOn": ["^build"]`, meaning the full build graph must resolve correctly. The Turbo cache must stay warm or rebase CI times become very long.

### 2.2 Infrastructure Complexity (Docker)

The system requires **7 separate Docker images** at runtime:
- `plane-backend` (Django API)
- `plane-frontend` (web app)
- `plane-space` (public space)
- `plane-admin` (admin dashboard)
- `plane-live` (collaboration server)
- `plane-proxy` (Caddy reverse proxy)
- `plane-aio-community` (all-in-one image)

Runtime services (production `docker-compose.yml`):
- PostgreSQL 15.7-alpine
- Valkey 7.2.11-alpine (Redis fork)
- RabbitMQ 3.13.6-management-alpine
- MinIO (S3-compatible object storage)
- Caddy reverse proxy
- API + worker + beat-worker + migrator (4 separate processes from same image)

**Total: 10 containers in production, 11 in local dev, 6 in test.**

**3 compose files with different configurations:**
- `docker-compose.yml` -- production/composed deployment
- `docker-compose-local.yml` -- local development with volume mounts
- `docker-compose-test.yml` -- isolated test environment with tmpfs volumes

Concern: Self-hosted adopters must run 5 infrastructure services (Postgres, Valkey, RabbitMQ, MinIO, Caddy) plus 4+ application containers. This is a high operational burden for small teams.

### 2.3 Community Edition (CE) Architecture

The codebase uses a CE (Community Edition) pattern with:
- `apps/web/ce/` -- community edition implementations
- `apps/web/ee/` (if exists) -- enterprise edition overrides
- `packages/editor/src/ce/` -- editor CE components
- The proxy has separate `Caddyfile.ce` and `Caddyfile.aio.ce` configs

Import paths use aliases like `@/plane-web/*` mapped to `./ce/*` in `apps/web/tsconfig.json`, making the CE/EE split a build-time concern rather than runtime feature flags. This adds complexity to import resolution and testing.

### 2.4 Real-Time Collaboration Complexity

The `apps/live` package implements real-time collaboration using:
- **Hocuspocus** (WebSocket server for collaborative editing) -- 5 packages pinned at v2.15.2
- **Yjs** (CRDT library) -- v13.6.20 for conflict-free data synchronization
- **TipTap** (ProseMirror-based editor) -- 22 packages for rich text editing with collaboration extensions
- **Y-IndexedDB** (v9.0.12) -- offline persistence

This stack is sophisticated but adds:
- Complex state synchronization across clients
- Operational transformation / CRDT debugging challenges
- Special deployment considerations (WebSocket infrastructure)
- A dedicated server process (`plane-live`) that must be scaled independently

### 2.5 State Management Fragmentation

The frontend uses multiple state management patterns:
- **MobX** (mobx 6.12.0, mobx-react 9.1.1) -- legacy stores in `apps/web/core/store/`
- **SWR** (2.2.4) -- data fetching/caching
- **React Router v7** -- URL-based state
- **Yjs** -- collaborative state
- **@plane/shared-state** -- cross-tab synchronization
- **IndexedDB** (y-indexeddb) -- offline persistence

Concern: The MobX stores contain significant business logic (e.g., `base-issues.store.ts` at **1,965 lines**). There was a recent migration of i18n from MobX to react-i18next (#8898), but the bulk of store logic remains in MobX. The coexistence of MobX, SWR, and React Router loaders creates potential data-fetching conflicts and makes the data flow harder to trace.

---

## 3. Code Quality Signals

### 3.1 Extremely Low Test Coverage

This is the most critical concern. Test infrastructure exists but coverage is minimal:

| Area | Source Files | Test Files | Test Lines |
|------|-------------|------------|------------|
| TypeScript (all) | 3,468 | **4** | **1,755** |
| Python (all) | 628 | ~46 | ~6,162 |
| **Total** | **~4,096** | **~50** | **~7,917** |

TypeScript test files:
- `apps/live/tests/lib/pdf/pdf-rendering.test.ts` -- 732 lines
- `apps/live/tests/services/pdf-export/effect-utils.test.ts` -- 155 lines
- `packages/codemods/tests/function-declaration.spec.ts` -- 572 lines
- `packages/codemods/tests/remove-directives.spec.ts` -- 296 lines

This means:
- **No tests for `apps/web`** (the primary application, ~2,500+ source files)
- **No tests for `packages/ui`, `packages/propel`, `packages/editor`, `packages/services`**
- **No tests for `apps/admin` or `apps/space`**
- Only the live collaboration server and codemods have TypeScript tests

Python tests exist but are concentrated in contract/smoke tests:
- `apps/api/plane/tests/contract/` -- API contract tests (token auth, cycles, projects, labels, etc.)
- `apps/api/plane/tests/smoke/` -- Basic smoke tests
- `apps/api/plane/tests/unit/` -- One empty `__init__.py` (no unit tests)
- Docker-based test runner: `docker-compose-test.yml` with pytest

Concern: **No CI test gate**. Neither `pull-request-build-lint-web-apps.yml` nor `pull-request-build-lint-api.yml` run any test suite. The CI pipeline runs format checks, lint checks, and type checks but **zero functional tests**. The `turbo.json` `test` task exists but is never invoked in CI.

### 3.2 Large Files (Complexity Risk)

| File | Lines | Concern |
|------|-------|---------|
| `apps/web/core/store/issue/helpers/base-issues.store.ts` | 1,965 | Massive store with complex filtering/bulk operations |
| `packages/utils/src/tlds.ts` | 1,447 | Auto-generated TLD list (acceptable) |
| `apps/web/core/constants/plans.tsx` | 1,311 | Plan/feature flag constants |
| `packages/ui/src/constants/icons.ts` | 928 | Icon definitions |
| `apps/web/core/hooks/use-issues-actions.tsx` | 807 | Large hook with many action handlers |
| `apps/web/core/components/core/activity.tsx` | 775 | Complex activity feed component |
| `apps/web/core/components/issues/issue-layouts/utils.tsx` | 771 | Issue layout utilities |
| `apps/web/core/store/cycle.store.ts` | 725 | Large MobX store |
| `apps/web/core/store/module.store.ts` | 641 | Large MobX store |

The `base-issues.store.ts` at nearly 2,000 lines is a particularly high risk for bugs and maintenance difficulty.

### 3.3 TODO/FIXME/HACK Comments

**~60 TODO/FIXME comments** identified across the codebase. Key patterns:

**Unfinished features / placeholder implementations:**
- `apps/web/helpers/authentication.helper.tsx:111` -- "move all error messages to translation files"
- `apps/web/core/components/common/access-field.tsx:27` -- "Remove label once i18n is done"
- `apps/web/core/store/user/base-permissions.store.ts:29` -- temporary type awaiting migration to plane constants package
- `packages/editor/src/core/extensions/table/plugins/drag-handles/color-selector.tsx:17` -- "implement text color selector" (unimplemented)
- `apps/web/core/components/inbox/modals/create-modal/issue-description.tsx:40` -- "have to implement GPT Assistance"

**Logic verification needed:**
- `packages/editor/src/core/extensions/trailing-node.ts:14` -- "check this logic, might be wrong"
- `apps/web/core/store/issue/issue-details/issue.store.ts:136` -- "check if this function is required"
- `packages/editor/src/core/extensions/custom-color.ts:117` -- "check this and update types"
- `apps/web/core/components/issues/workspace-draft/draft-issue-properties.tsx:70` -- "To be checked"

**Known bugs / workarounds:**
- `packages/ui/src/tooltip/tooltip.tsx:53` -- "FIXME: tooltip should always render on hover and not by default, this is a temporary fix"
- `packages/ui/src/dropdowns/helper.tsx:7` -- "FIXME: fix this!!!"
- `apps/web/core/components/integration/slack/select-channel.tsx:38` -- "FIXME:"

### 3.4 Deprecated API Usage

| Location | Deprecated API | Status |
|----------|---------------|--------|
| `packages/utils/src/string.ts:359-361` | `document.execCommand("copy")` | Fallback in use, acknowledged |
| `apps/space/helpers/string.helper.ts:23-25` | `document.execCommand("copy")` | Fallback in use, acknowledged |
| `packages/editor/src/core/helpers/editor-ref.ts:117` | `document.execCommand("copy")` | In use |
| `apps/web/helpers/graph.helper.ts:7` | Entire file marked DEPRECATED | Still referenced |
| `apps/web/helpers/dashboard.helper.ts:15` | Entire file marked DEPRECATED | Still referenced |
| `packages/utils/src/theme-legacy.ts:12` | Legacy theme functions | Marked @deprecated |
| `.env.example` lines 32-34 | GPT/OPENAI settings marked "deprecated" | Still in .env.example |
| `.env.example` line 39, `apps/api/.env.example` line 40 | DOCKERIZED=1 "deprecated" | Still in .env.example |

The `execCommand` API was deprecated by browsers years ago. While the code treats it as a fallback (after `navigator.clipboard.writeText`), it should be removed entirely for modern browsers.

### 3.5 TypeScript `any` Usage

Despite a strict base config (`packages/typescript-config/base.json`), many individual workspace tsconfigs **override strict settings**:
- `apps/web/tsconfig.json` sets `strictNullChecks: true` but **not full `strict: true`**
- `apps/web/tsconfig.json` sets `noUnusedParameters: false`, `noUnusedLocals: false`, `noImplicitReturns: false`, `noImplicitOverride: false`
- `packages/editor/tsconfig.json` has `noUnusedLocals: false`, `noUnusedParameters: false`, `noImplicitReturns: false`
- **Only 2 tsconfig files** use `"strict": true` (codemods and i18n/scripts)

`any` usage counts:
- `apps/web/core/` -- **166 occurrences**
- `packages/` (all) -- **114 occurrences**
- Many in store methods, service functions, and component props (e.g., `props: any`, `data: any`, `error: any`)

### 3.6 Console Logging in Production Code

**186 `console.log/warn/error/debug` calls** in `apps/web/core/` alone. While some are intentional error logging, many appear to be development leftovers that should use the structured logger (`packages/logger`).

---

## 4. Security Concerns

### 4.1 Recent Security History (Positive: Active Remediation)

The project has been **actively fixing security vulnerabilities**. Recent commits show a strong security posture in terms of response:

| Date | Issue | Severity |
|------|-------|----------|
| 2026-06 (recent) | API key auth for deactivated users (#9225) | Auth bypass |
| 2026-06 (recent) | XLSX formula injection (#9224) | Injection |
| 2026-06 (recent) | Magic-code rate limiting (GHSA-9pvm-fcf6-9234) (#9130) | Brute force |
| 2026-06 (recent) | Workspace membership on GenericAssetEndpoint (#9212) | IDOR |
| 2026-05 | Webhook/link/OAuth-avatar SSRF (4 clusters) (#9163) | SSRF |
| 2026-05 | Workspace role for project member updates (GHSA-x63v-p7wc-47x4) (#9014) | Privilege escalation |
| 2026-05 | Cross-workspace resource IDOR (#9008) | IDOR |
| 2026-04 | Asset endpoint IDOR (#8644) | IDOR |
| 2026-04 | ORM field injection in analytics (GHSA-93x3-ghh7-72j3) (#8864) | Injection |
| 2026-04 | SSRF in favicon fetching (#8858) | SSRF |
| 2026-04 | Project member role privilege escalation (GHSA-494h-3rcq-5g3c) (#8833) | Privilege escalation |
| 2026-03 | Filename path traversal (#8879) | Path traversal |

Key observations:
- **IDOR (Insecure Direct Object Reference)** is the most common vulnerability class -- 4 separate fixes
- **SSRF** in multiple code paths (webhooks, favicons, OAuth avatars)
- Most fixes involve adding workspace/project membership checks before resource access
- The pattern suggests the original codebase relied heavily on URL-level authorization without deep object-level checks -- this is being systematically addressed

### 4.2 Authentication & Authorization

- API Key rate limiting: `API_KEY_RATE_LIMIT="60/minute"` (configurable via settings)
- Magic code auth: Recently rate-limited (#9130)
- Live collaboration auth: `LIVE_SERVER_SECRET_KEY` with a TODO to move to HMAC (`apps/live/src/lib/auth-middleware.ts:34`)
- Workspace membership enforcement has been a recurring fix point

Concern: The `apps/live/src/lib/auth-middleware.ts` has a TODO to move from secret-key auth to HMAC, which would be a stronger authentication mechanism for the WebSocket collaboration server.

### 4.3 Data Deletion / Retention

- **Soft deletion** is used throughout (Django `SoftDeletionManager` mixin)
- **HARD_DELETE_AFTER_DAYS=60** in `apps/api/.env.example` -- files are hard-deleted after 60 days
- File assets have `is_deleted` flag
- Unique constraints include `deleted_at IS NULL` conditions for soft-delete support

Concern: The 60-day hard-delete window means accidentally deleted data is recoverable for 60 days, but after that it is permanently lost. No backup integration is visible in the infrastructure.

### 4.4 Environment Variable Management

- **6 `.env.example` files** (181 total lines of configuration)
- `turbo.json` declares **35 global environment variables** visible to all tasks
- Secrets flow: `.env` -> `env_file` in docker-compose -> container environment
- Default credentials in `.env.example` are weak (`plane`/`plane`/`access-key`/`secret-key`) -- this is standard for examples but dangerous if users copy without changing

Concern: No vault/secrets-manager integration. Self-hosted users manage secrets manually through `.env` files. There is no mechanism to detect unchanged default credentials.

---

## 5. Operational Concerns

### 5.1 Development Environment Setup

Setting up a development environment requires:
1. Node.js >= 22.18.0
2. pnpm 11.3.0
3. Python 3.12
4. Docker (for infrastructure services) OR manually run Postgres + Valkey + RabbitMQ + MinIO
5. Multiple `.env` files across apps

The `docker-compose-local.yml` provides a full local stack but requires building 5+ Docker images. The setup script path is not clearly documented in the compose files.

### 5.2 Database Migration Management

- **121 Django migration files** in `apps/api/plane/db/migrations/`
- A dedicated `migrator` container runs migrations on startup
- Migrations are excluded from Ruff linting

Concern: 121 migrations represent significant schema evolution. Squashing older migrations would reduce startup time and complexity, but carries risk.

### 5.3 Build Pipeline Complexity

The `turbo.json` build graph has 11 task definitions with interleaved dependencies:
- `check:types` depends on `^build` (all upstream packages must build first)
- `test` depends on `^build`
- `dev` depends on `^build` with `persistent: true`

The CI creates **7 Docker images** per build, each with its own Dockerfile. The AIO (all-in-one) build adds an additional image that embeds all assets. Build caching via Turbo is critical for reasonable CI times.

### 5.4 Deployment Surface

Deployment artifacts include:
- 7 Docker images (for standard deployment)
- 1 AIO image (combined)
- CLI installer script (`deployments/cli/community/install.sh`)
- Docker Swarm support (`deployments/swarm/community/swarm.sh`)
- Restore scripts for backup/restore

Multi-architecture builds (amd64 + arm64) are supported for releases via Docker Buildx.

---

## 6. Technical Debt Indicators

### 6.1 i18n Incompleteness

Multiple TODO comments reference missing translations:
- `apps/web/helpers/authentication.helper.tsx:111` -- error messages not in translation files
- `apps/web/core/components/common/access-field.tsx:27` -- label awaiting i18n
- `apps/web/core/components/inbox/sidebar/root.tsx:164` -- untranslated text
- `apps/web/core/components/inbox/modals/delete-issue-modal.tsx:77` -- untranslated confirmation
- `apps/web/core/components/inbox/modals/decline-issue-modal.tsx:50` -- untranslated confirmation
- `apps/web/core/components/estimates/create/stage-one.tsx:88` -- untranslated text
- `apps/web/core/components/issues/select/base.tsx:299` -- untranslated text
- `apps/web/core/components/workspace/confirm-workspace-member-remove.tsx:66` -- untranslated text

A recent PR (#8898) migrated i18n from MobX to react-i18next, but the translation coverage is incomplete.

### 6.2 Permission/Role System in Transition

`apps/web/core/store/user/base-permissions.store.ts:29`:
```typescript
type ETempUserRole = TUserPermissions | EUserWorkspaceRoles | EUserProjectRoles;
// TODO: Remove this once we have migrated user permissions to enums to plane constants package
```

The type name `ETempUserRole` signals this is a known temporary solution awaiting a larger migration.

### 6.3 CE/EE Split Maintenance Burden

Files reference a `ce/` (Community Edition) pattern with import aliases mapping `@/plane-web/*` to `./ce/*`. This means:
- Every feature must be implemented in `ce/` with an optional `ee/` override
- The split creates parallel implementations that must stay in sync
- Import resolution is build-config dependent, making IDE navigation potentially confusing

### 6.4 Legacy Code Markers

- `apps/web/helpers/graph.helper.ts` -- entire file deprecated, recommends recharts instead
- `apps/web/helpers/dashboard.helper.ts` -- entire file deprecated
- `packages/utils/src/theme-legacy.ts` -- legacy theme utilities
- `packages/utils/src/string.ts:251` -- `isCommentEmpty` deprecated in favor of Content type variant

### 6.5 Workspace/Project ID Divergence

`apps/web/core/store/workspace/index.ts:177`:
```typescript
getWorkspaceById = (workspaceId: string) => this.workspaces?.[workspaceId] || null;
// TODO: use undefined instead of null
```

Mixing `null` and `undefined` return values across the codebase creates inconsistent null-checking requirements.

---

## 7. Recent Changes and Stability

### 7.1 Active Security Remediation (Positive)

The project is in an active security hardening phase. The most recent 20 commits include:
- 6 security fixes (rate limiting, auth bypass, formula injection, workspace enforcement)
- 2 dependency vulnerability resolutions
- 1 major refactor (types migration to @plane/types)
- 1 observability improvement (OTLP traces to metrics)

### 7.2 Branch Strategy

- **preview** -- main development branch (PRs target this)
- **canary** -- release candidate / early release
- **master** -- stable releases
- CI triggers on push to `preview` and `canary`
- Release workflow is manual (`workflow_dispatch`) with version parameter

### 7.3 Release Cadence

The project uses automated release tooling via GitHub Actions. Releases are triggered manually with SemVer validation. Both standard and pre-release channels are supported. Docker images are tagged with version numbers and published to Docker Hub under `makeplane/*`.

---

## 8. Top Risks Summary

| Priority | Risk | Impact | Recommended Action |
|----------|------|--------|--------------------|
| **CRITICAL** | **Near-zero frontend test coverage** (0 test files in apps/web, packages/ui, packages/editor) | Bugs ship to production undetected; no regression safety net | Add test infrastructure to CI; require tests for new features |
| **CRITICAL** | **No test execution in CI pipeline** | Even existing tests are never run on PRs | Add `turbo run test` to pull-request-build-lint-web-apps.yml |
| **HIGH** | **Django 4.2 LTS end of life** (April 2026) | Missing security patches if not upgraded | Plan migration to Django 5.x |
| **HIGH** | **Live auth using secret-key instead of HMAC** | Replayable authentication for WebSocket connections | Implement HMAC as noted in TODO at `apps/live/src/lib/auth-middleware.ts:34` |
| **HIGH** | **37 pnpm overrides create brittle dependency graph** | Upstream updates can break the build with conflicting constraints | Audit overrides; move security-driven pins to explicit resolutions |
| **MEDIUM** | **`base-issues.store.ts` at 1,965 lines** | Single point of failure for issue operations; hard to test | Split into smaller focused stores with clear boundaries |
| **MEDIUM** | **execCommand still in use** | Will break in future browser versions | Remove fallback; use only `navigator.clipboard` API |
| **MEDIUM** | **Incomplete i18n coverage** | Mixed-language UI for non-English users | Complete translation coverage; add i18n lint rule to CI |
| **LOW** | **Version mismatch (pyproject v0.24.0 vs package.json v1.3.1)** | Confuses vulnerability scanners | Align versioning schemes |
| **LOW** | **`any` type usage (280+ instances)** | Erodes type safety; masks bugs | Gradual migration to proper types; add ESLint rule to cap new `any` usage |

---

*Concerns audit: 2026-06-16*
