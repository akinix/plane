# External Integrations

**Analysis Date:** 2026-06-16

## APIs & External Services

### Authentication & OAuth Providers

**GitHub OAuth:**

- OAuth 2.0 login for Plane users
- Configuration: `GITHUB_CLIENT_ID`, `GITHUB_CLIENT_SECRET` (env vars)
- Optional organization membership gating: `GITHUB_ORGANIZATION_ID`
- Scopes: `read:user user:email` (plus `read:org` if org gating is configured)
- Sync integration: `ENABLE_GITHUB_SYNC` — bidirectional issue sync with GitHub repositories
- Implementation: `apps/api/plane/authentication/provider/oauth/github.py`
- Models: `apps/api/plane/db/models/integration/github.py` (GithubRepository, GithubRepositorySync, GithubIssueSync, GithubCommentSync)
- Used by: `apps/api/plane/authentication/views/app/github.py`, `apps/api/plane/authentication/views/space/github.py`

**GitLab OAuth:**

- OAuth 2.0 login, supports both hosted and self-managed GitLab instances
- Configuration: `GITLAB_CLIENT_ID`, `GITLAB_CLIENT_SECRET`, `GITLAB_HOST` (env vars, defaults to `https://gitlab.com`)
- Scopes: `read_user`
- Sync integration: `ENABLE_GITLAB_SYNC`
- Implementation: `apps/api/plane/authentication/provider/oauth/gitlab.py`

**Gitea OAuth:**

- OAuth 2.0 login for self-hosted Gitea instances
- Configuration: `GITEA_CLIENT_ID`, `GITEA_CLIENT_SECRET`, `GITEA_HOST` (env vars), `IS_GITEA_ENABLED` flag
- Scopes: `openid email profile`
- Sync integration: `ENABLE_GITEA_SYNC`
- Implementation: `apps/api/plane/authentication/provider/oauth/gitea.py`

**Google OAuth:**

- OAuth 2.0 login with Google accounts
- Configuration: `GOOGLE_CLIENT_ID`, `GOOGLE_CLIENT_SECRET` (env vars)
- Sync integration: `ENABLE_GOOGLE_SYNC`
- Implementation: `apps/api/plane/authentication/provider/oauth/google.py`

**Email/Password Authentication:**

- Traditional email + password login
- Configurable: `ENABLE_EMAIL_PASSWORD` (env var, defaults to `1`/enabled)
- Implementation: `apps/api/plane/authentication/provider/credentials/email.py`

**Magic Link Authentication:**

- Passwordless email login via magic links
- Configurable: `ENABLE_MAGIC_LINK_LOGIN` (env var, defaults to `0`/disabled)
- Implementation: `apps/api/plane/authentication/provider/credentials/magic_code.py`

**API Key Authentication:**

- Per-user API tokens (`plane_api_` prefix) for programmatic access
- Rate limited: `API_KEY_RATE_LIMIT` (default `60/minute`, DRF SimpleRateThrottle format)
- Custom header: `X-API-Key`
- Implementation: `packages/services/src/developer/api-token.service.ts` (frontend), `apps/api/plane/middleware/logger.py` (APITokenLogMiddleware)

**Session Management:**

- Django session-based (server-side sessions stored in database)
- Session engine: custom `plane.db.models.session`
- Cookie name: configurable via `SESSION_COOKIE_NAME` (default `session-id`)
- Session TTL: configurable via `SESSION_COOKIE_AGE` (default 604800 seconds / 7 days)
- CSRF protection: enabled with trusted origins from CORS config
- Admin sessions: separate cookie `admin-session-id` with configurable TTL `ADMIN_SESSION_COOKIE_AGE` (default 3600 seconds)

### AI / LLM Providers

**OpenAI:**

- SDK: `openai` 1.63.2 (Python)
- API endpoint: configured via `OPENAI_API_BASE` (deprecated, use `LLM_PROVIDER`/`LLM_MODEL`/`LLM_API_KEY`)
- Available models: `gpt-3.5-turbo`, `gpt-4o-mini`, `gpt-4o`, `o1-mini`, `o1-preview`
- Default model: `gpt-4o-mini`
- Implementation: `apps/api/plane/app/views/external/base.py` (OpenAIProvider, AnthropicProvider, GeminiProvider)

**Anthropic Claude:**

- SDK: OpenAI-compatible interface (Plane uses a unified LLM abstraction)
- Available models: `claude-3-5-sonnet-20240620`, `claude-3-haiku-20240307`, `claude-3-opus-20240229`, `claude-3-sonnet-20240229`, `claude-2.1`, `claude-2`, `claude-instant-1.2`, `claude-instant-1`

**Google Gemini:**

- SDK: OpenAI-compatible interface
- Available models: `gemini-pro`, `gemini-1.5-pro-latest`, `gemini-pro-vision`

**AI Features at Runtime:**

- AI-powered issue creation and description generation
- Frontend service: `packages/services/src/ai/ai.service.ts`
- Backend: `apps/api/plane/app/views/external/base.py`
- Config unified via env vars: `LLM_PROVIDER`, `LLM_MODEL`, `LLM_API_KEY`

### Email Service

**SMTP:**

- Backend: Django SMTP email backend
- Configuration: `ENABLE_SMTP` flag, `EMAIL_HOST`, `EMAIL_HOST_USER`, `EMAIL_HOST_PASSWORD`, `EMAIL_PORT` (default 587), `EMAIL_FROM`, `EMAIL_USE_TLS` (default 1), `EMAIL_USE_SSL` (default 0)
- Uses SMTP for transactional emails: password reset, magic link codes, workspace invitations, project invitations, user activation/deactivation notifications
- Implementation: `apps/api/plane/bgtasks/email_notification_task.py`, `apps/api/plane/bgtasks/forgot_password_task.py`, `apps/api/plane/bgtasks/magic_link_code_task.py`

### Monitoring & Observability

**Scout APM:**

- Production-only APM agent
- Config: `SCOUT_MONITOR` (bool), `SCOUT_KEY` (secret)
- Implementation: `apps/api/plane/settings/production.py`

**OpenTelemetry:**

- Distributed tracing via OTLP
- Django auto-instrumentation
- Implementation: `apps/api/plane/utils/otlp_endpoints.py`, `apps/api/plane/settings/common.py`
- Packages: `opentelemetry-api`, `opentelemetry-sdk`, `opentelemetry-instrumentation-django`, `opentelemetry-exporter-otlp-proto-grpc`

**Logging:**

- Backend: JSON-structured logging via `python-json-logger`, console + file handlers
- Log rotation: `SizedTimedRotatingFileHandler` in `apps/api/plane/utils/logging.py`
- Frontend/Live server: Winston 3.17.0 + express-winston 4.2.0 via `packages/logger`

**Error Tracking:**

- Custom exception logging via `apps/api/plane/utils/exception_logger.py`
- Frontend: `packages/logger` (shared logging package)

### Unsplash Integration

- Used for workspace/project cover image search
- Config: `UNSPLASH_ACCESS_KEY` (env var)
- Backend: `apps/api/plane/app/views/external/base.py`
- Implementation: uses `UNSPLASH_ACCESS_KEY` in `apps/api/plane/settings/common.py`

### Slack Integration

- Project-level Slack sync for notifications
- SDK: `slack-sdk` 3.27.1 (Python)
- Model: `apps/api/plane/db/models/integration/slack.py` (SlackProjectSync)
- Stores: access_token, bot_user_id, webhook_url, team_id, team_name per project
- Implementation: `apps/api/plane/db/models/integration/slack.py`

### PostHog Analytics

- Product analytics tracking
- Config: `POSTHOG_API_KEY`, `POSTHOG_HOST` (env vars)
- SDK: `posthog` 3.5.0 (Python)
- Default: disabled unless configured

## Data Storage

**Databases:**

- PostgreSQL 15.7 (Alpine Docker image)
  - Connection: via `DATABASE_URL` env var or individual `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_HOST`, `POSTGRES_DB`, `POSTGRES_PORT` env vars
  - Client: `psycopg` 3.3.0 (Python), `dj-database-url` 2.1.0 for URL parsing
  - Optional read replica: `ENABLE_READ_REPLICA`, `DATABASE_READ_REPLICA_URL` or individual replica env vars
  - Read replica routing: `apps/api/plane/utils/core/dbrouters.py` (ReadReplicaRouter), `apps/api/plane/middleware/db_routing.py` (ReadReplicaRoutingMiddleware)

**File Storage:**

- S3-compatible storage (AWS S3 or MinIO)
  - Client: `boto3` 1.34.96 (Python), with `django-storages` 1.14.2
  - MinIO: `minio/minio` Docker image (local development)
  - Config: `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`, `AWS_S3_BUCKET_NAME` (default `uploads`), `AWS_REGION`, `AWS_S3_ENDPOINT_URL`
  - Presigned URLs: generated with configurable expiration (`SIGNED_URL_EXPIRATION`, default 3600 seconds)
  - Custom storage class: `apps/api/plane/settings/storage.py` (S3Storage) — extends S3Boto3Storage with presigned URL generation, metadata retrieval, copy operations, direct upload, and batch delete
  - Frontend upload: presigned POST URLs generated by backend, consumed by `packages/services/src/file/file-upload.service.ts`
  - File size limit: `FILE_SIZE_LIMIT` (default 5242880 bytes / 5MB), enforced at Django middleware (`RequestBodySizeLimitMiddleware`) and Caddy proxy level

**Caching:**

- Redis (via Valkey 7.2.11 Docker image)
  - Client: `redis` 5.0.4 (Python), `ioredis` 5.7.0 (Node.js)
  - Django cache backend: `django-redis` 5.4.0
  - SSL support: automatic detection via `rediss://` URL scheme
  - Used for: Django cache, Hocuspocus collaboration state synchronization, Celery task queue backend
  - Note: the Docker image uses Valkey (open-source Redis fork), not Redis proper

**Message Queue:**

- RabbitMQ 3.13.6 (Alpine Docker image with management plugin)
  - Used by: Celery for task distribution
  - Config: `RABBITMQ_HOST`, `RABBITMQ_PORT`, `RABBITMQ_USER`, `RABBITMQ_PASSWORD`, `RABBITMQ_VHOST`
  - Alternative: `AMQP_URL` for direct broker URL
  - Connection string format: `amqp://user:password@host:port/vhost`

## Internal Service Topology

```
                         ┌─────────────────────────────────────────────┐
                         │               Caddy Reverse Proxy            │
                         │              (apps/proxy:80/443)              │
                         │  Routes: /*, /api/*, /auth/*, /live/*,       │
                         │  /spaces/*, /god-mode/*, /static/*,          │
                         │  /uploads/*                                  │
                         └───┬─────────┬─────────┬──────────┬───────────┘
                             │         │         │          │
              ┌──────────────┘  ┌──────┘   ┌─────┘   ┌──────┘
              ▼                  ▼          ▼         ▼
    ┌─────────────────┐  ┌──────────┐ ┌─────────┐ ┌──────────┐
    │     web:3000    │  │api:8000  │ │live:3000│ │plane-minio│
    │ (React/TS SPA)  │  │ (Django) │ │(Hocus-  │ │  :9000    │
    │ /*              │  │ /api/*   │ │ pocus)  │ │ /uploads/*│
    └────────┬────────┘  │ /auth/*  │ │ /live/* │ └──────────┘
             │            │ /static/*│ └────┬─────┘
             │            └────┬─────┘      │
             │                 │             │
             ▼                 ▼             ▼
    ┌─────────────────────────────────────────────────────────────┐
    │                     Backend Services                         │
    │  ┌──────────┐  ┌───────────┐  ┌──────────────┐              │
    │  │  worker  │  │beat-worker│  │  migrator    │              │
    │  │ (Celery) │  │(Celery    │  │ (Django      │              │
    │  │          │  │ Beat)     │  │ migrate)     │              │
    │  └─────┬────┘  └─────┬─────┘  └──────┬───────┘              │
    │        │             │                │                      │
    └────────┼─────────────┼────────────────┼──────────────────────┘
             │             │                │
             ▼             ▼                ▼
    ┌─────────────────┐  ┌──────────────────────────┐
    │  PostgreSQL     │  │       Redis (Valkey)      │
    │  (plane-db)    │  │     (plane-redis:6379)     │
    │  :5432          │  └──────────┬───────────────┘
    └─────────────────┘             │
                                    ▼
                         ┌──────────────────────┐
                         │     RabbitMQ         │
                         │   (plane-mq:5672)    │
                         └──────────────────────┘
```

**Proxy Routing (Caddy):**

- `/*` → `web:3000` (SPA fallback)
- `/api/*` → `api:8000` (Django REST API)
- `/auth/*` → `api:8000` (authentication callbacks)
- `/static/*` → `api:8000` (Django static files via Whitenoise)
- `/live/*` → `live:3000` (WebSocket collaboration server)
- `/spaces/*` → `space:3000` (public space views)
- `/god-mode/*` → `admin:3000` (admin panel)
- `/{BUCKET_NAME}/*` → `plane-minio:9000` (uploaded files, direct access)
- Request body size limit: `${FILE_SIZE_LIMIT}` (enforced at proxy level)

**Service Dependencies (docker-compose.yml):**

- `web`, `space`, `admin`: depend on `api`
- `api`: depends on `plane-db`, `plane-redis`
- `worker`, `beat-worker`: depend on `api`, `plane-db`, `plane-redis`
- `migrator`: depends on `plane-db`, `plane-redis` (runs once, `restart: no`)
- `proxy`: depends on `web`, `api`, `space`, `admin`

## Authentication & Identity

**Auth Flow:**

1. User authenticates via OAuth provider or email/password
2. Django creates/updates `db.User` record (custom user model: `AUTH_USER_MODEL = "db.User"`)
3. Session cookie set with configurable name and domain
4. React frontend includes session cookie automatically via `axios` with `withCredentials: true`
5. API requests authenticated via `SessionAuthentication` on Django REST Framework
6. CSRF protection with tokens for state-changing requests

**API Key Auth:**

1. User generates API key from workspace settings
2. Key prefixed with `plane_api_`
3. Sent in `X-API-Key` header
4. Rate limited: configurable via `API_KEY_RATE_LIMIT` (default `60/minute`)
5. Key validation: checks user account is not deactivated before authenticating
6. CORS header allowlist includes `X-API-Key`

**Signup Control:**

- `ENABLE_SIGNUP` (env var, default `1`) — controls whether new account registration is allowed
- `DISABLE_WORKSPACE_CREATION` (env var, default `0`) — controls whether users can create new workspaces

## Webhook System

**Outgoing Webhooks:**

- Workspace-level webhooks for event notifications
- Configurable per webhook: URL, secret key, event types (project, issue, module, cycle, issue_comment)
- Webhook version: `v1` (default)
- Secret key format: `plane_wh_` + UUID4 hex
- URL validation: only HTTP/HTTPS allowed, localhost/127.0.0.1 blocked
- Model: `apps/api/plane/db/models/webhook.py` (Webhook, WebhookLog, ProjectWebhook)
- Task: `apps/api/plane/bgtasks/webhook_task.py` (Celery-delayed, async delivery)
- Supported event types for serialization: project, issue, cycle, module, cycle_issue, module_issue, issue_comment, user, intake_issue
- HMAC-SHA256 signature for payload verification (using webhook's secret key)
- Delivery logs stored in `WebhookLog` model with request/response headers, body, and retry count

**SSRF Protection for Webhooks:**

- IP allowlist: `WEBHOOK_ALLOWED_IPS` — comma-separated CIDRs that bypass private network checks
- Hostname allowlist: `WEBHOOK_ALLOWED_HOSTS` — comma-separated hostnames that bypass private IP checks (useful for dynamic DNS in containerized deployments)
- Disallowed domains: `WEBHOOK_DISALLOWED_DOMAINS` — hostnames that are always rejected
- DNS rebinding protection: `apps/api/plane/utils/url_security.py` (pinned_fetch) — resolves hostname once, validates resolved IP, then connects to the validated IP literal (prevents TOCTOU/DNS rebinding attacks)
- Module: `apps/api/plane/utils/ip_address.py` (resolve_and_validate)

**Webhook Log Retention:**

- Configurable: `WEBHOOK_LOG_RETENTION_DAYS` (default 14 days)

## Import/Export

**Export:**

- Supported formats: CSV, XLSX, JSON
- Async via Celery task: `apps/api/plane/bgtasks/export_task.py`
- Backend endpoint: `apps/api/plane/app/views/exporter/base.py` (ExportIssuesEndpoint)
- Data exporter: `apps/api/plane/utils/porters/exporter.py` (DataExporter)
- Issue export serializer: `apps/api/plane/utils/porters/serializers/issue.py`
- Export files packaged in ZIP, uploaded to S3/MinIO with 7-day expiration presigned URLs
- Model: `apps/api/plane/db/models/exporter.py` (ExporterHistory)
- Frontend: uses `export-to-csv` 1.4.0 npm package for CSV exports

**Import:**

- Supported via `Importer` model (`apps/api/plane/db/models/importer.py`)
- Serializer: `apps/api/plane/app/serializers/importer.py`

**XLSX Security:**

- XLSX generation uses `openpyxl` 3.1.2 with formula injection sanitization (cells prefixed to prevent `=` formulas)

## Real-time Collaboration Architecture

**Live Server (`apps/live`):**

- Express 4.22.0 server with WebSocket support (`express-ws`)
- Hocuspocus 2.15.2 server for Yjs-based collaborative editing
- CORS: configurable via `CORS_ALLOWED_ORIGINS` env var
- Authenticated connections: uses `LIVE_SERVER_SECRET_KEY` for HMAC-based JWT verification
- Port: 3100 (default, configurable)
- Middleware: Helmet (security headers), CORS, compression, JSON body parsing, request logging
- Database persistence: `apps/live/src/extensions/database.ts` — fetches and stores document binary data via the Plane API
- Redis synchronization: `apps/live/src/extensions/redis.ts` — enables multi-instance document synchronization via Redis pub/sub
- Admin commands: cross-server force-close, document management via Redis pub/sub channel `hocuspocus:admin`
- PDF export: `apps/live/src/services/pdf-export/` — server-side HTML-to-PDF generation using `sharp` for image processing
- Page services: `apps/live/src/services/page/` — multiple service handlers for different page types (core, extended, project-page)

**Collaboration Documents:**

- Yjs document types serve as collaborative data stores
- Binary format: optimized ProseMirror/Yjs binary encoding for real-time sync
- IndexedDB persistence: `y-indexeddb` 9.0.12 for offline/local caching in browser
- Title sync: automatic title extraction from editor content
- Force-close handler: `apps/live/src/extensions/force-close-handler.ts` — handles content-too-large errors, page locks, archived pages

## Environment Configuration

**Critical env vars (root `.env`):**

- `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB` — Database credentials
- `REDIS_HOST`, `REDIS_PORT` — Redis connection
- `RABBITMQ_HOST`, `RABBITMQ_PORT`, `RABBITMQ_USER`, `RABBITMQ_PASSWORD`, `RABBITMQ_VHOST` — Message queue
- `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`, `AWS_S3_ENDPOINT_URL`, `AWS_S3_BUCKET_NAME` — Storage
- `LISTEN_HTTP_PORT`, `LISTEN_HTTPS_PORT` — Proxy ports
- `FILE_SIZE_LIMIT` — Upload size limit
- `USE_MINIO` — Enable MinIO mode

**Critical env vars (`apps/api/.env`):**

- `DEBUG` — Debug mode (0/1)
- `SECRET_KEY` — Django secret key
- `DATABASE_URL` or individual PostgreSQL vars
- `REDIS_URL` — Redis connection URL
- `WEB_URL` — Instance URL
- `ADMIN_BASE_URL`, `SPACE_BASE_URL`, `APP_BASE_URL`, `LIVE_BASE_URL` — Service base URLs
- `LIVE_SERVER_SECRET_KEY` — Shared secret for live server authentication
- `CORS_ALLOWED_ORIGINS` — CORS origin allowlist
- `HARD_DELETE_AFTER_DAYS` — Soft-deleted data retention (default 60 days)
- `API_KEY_RATE_LIMIT` — API key rate limiting (default `60/minute`)

**Secrets location:**

- All secrets via environment variables (env files in Docker Compose, or directly injected)
- No vault or secret manager detected — standard env-based configuration for self-hosted deployment

## Webhooks & Callbacks

**Incoming:**

- OAuth callback endpoints at `/auth/{provider}/callback/` for GitHub, GitLab, Gitea, Google
- No user-facing incoming webhooks detected

**Outgoing:**

- Workspace webhooks (`apps/api/plane/bgtasks/webhook_task.py`) — dispatched asynchronously via Celery for events: project, issue, cycle, module, issue_comment, etc.

## CI/CD & Deployment

**Hosting:**

- Self-hosted via Docker Compose
- Community edition Docker Compose: `deployments/cli/community/docker-compose.yml`

**CI Pipeline:**

- Not in source (external CI likely configured — no `.github/workflows` standard configs visible)

**Build:**

- Multi-stage Dockerfiles for each service using Docker BuildKit
- Turbo-powered monorepo pruning for Docker builds

## Additional Service Notes

**Background Task Scheduler:**

- Celery Beat for periodic tasks (via `apps/api/plane/bgtasks/`)
- Scheduled tasks include: issue automation, export expiration cleanup, file asset cleanup, email notifications, data cleanup (HARD_DELETE_AFTER_DAYS), telemetry metrics

**Migrations:**

- Django migrations in `apps/api/plane/db/migrations/` (numbered sequentially)
- Managed via dedicated `migrator` Docker service (runs once on startup)

**Static Files:**

- Collected to `apps/api/static-assets/collected-static/`
- Served via Whitenoise (CompressedManifestStaticFilesStorage)
- Routes through Caddy proxy via `/static/*` → `api:8000`

---

_Integration audit: 2026-06-16_
