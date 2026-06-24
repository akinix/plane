# Testing Patterns

**Analysis Date:** 2026-06-16

## Test Frameworks

### Frontend (TypeScript)

**Runner:** Vitest 4.x (`catalog:` reference in `pnpm-workspace.yaml`)
**Assertion:** Vitest built-in (`expect`, `assert`)
**Coverage:** `@vitest/coverage-v8` (v8 provider)
**UI Testing:** Storybook 9.x with `@storybook/test` and `@storybook/addon-interactions`

### Backend (Python / Django)

**Runner:** pytest (via Docker)
**Assertion:** pytest built-in `assert` and Django test utilities
**Test Client:** Django `Client` (app tests), DRF `APIClient` (API tests)
**Database:** Django test runner with `--reuse-db` and `--nomigrations`

### Component Development

**Storybook:** Storybook 9.x with React (webpack5-based)
**Addons:** essentials, interactions, links, onboarding, chromatic, styling-webpack

## Test Configuration

### Frontend - apps/live (`apps/live/vitest.config.ts`)

```typescript
import { defineConfig } from "vitest/config";
export default defineConfig({
  test: {
    environment: "node",
    globals: true,
    include: ["tests/**/*.test.ts", "tests/**/*.spec.ts"],
    coverage: {
      provider: "v8",
      reporter: ["text", "json", "html"],
      include: ["src/**/*.ts"],
      exclude: ["src/**/*.d.ts", "src/**/types.ts"],
    },
  },
  resolve: {
    alias: { "@": path.resolve(__dirname, "./src") },
  },
});
```

### Frontend - packages/codemods (`packages/codemods/vitest.config.ts`)

```typescript
import { defineConfig } from "vitest/config";
export default defineConfig({
  test: {
    environment: "node",
  },
});
```

### Backend - pytest (`apps/api/pytest.ini`)

```ini
[pytest]
DJANGO_SETTINGS_MODULE = plane.settings.test
python_files = test_*.py
python_classes = Test*
python_functions = test_*

markers =
    unit: Unit tests for models, serializers, and utility functions
    contract: Contract tests for API endpoints
    smoke: Smoke tests for critical functionality
    slow: Tests that are slow and might be skipped in some contexts

addopts =
    --strict-markers
    --reuse-db
    --nomigrations
    -vs
```

### Backend - ruff (test-specific overrides in `apps/api/pyproject.toml`)

```toml
[tool.ruff.lint.per-file-ignores]
"tests/*" = ["E402", "F401", "F811"]
"__init__.py" = ["F401"]
```

## Test File Organization

### Frontend

**Location pattern:** Tests in `tests/` directory at app/package root (NOT co-located with source).

```
apps/live/
  src/
    services/
      pdf-export/
        effect-utils.ts
  tests/
    services/
      pdf-export/
        effect-utils.test.ts
    lib/
      pdf/
        pdf-rendering.test.ts
```

**Naming convention:** `*.test.ts` or `*.spec.ts` — matched via vitest include pattern:
`["tests/**/*.test.ts", "tests/**/*.spec.ts"]`

### Backend

**Location pattern:** Tests organized by test marker type in `apps/api/plane/tests/`.

```
apps/api/plane/tests/
  conftest.py                    # Shared fixtures
  unit/
    bg_tasks/
      test_cleanup_task.py
      test_ssrf_advisories.py
    middleware/
      test_api_authentication.py
      test_db_routing.py
      test_logger.py
    models/
      test_workspace_model.py
      test_issue_comment_modal.py
    serializers/
      test_workspace.py
      test_label.py
    utils/
      test_uuid.py
      test_url.py
      test_csv_export_sanitization.py
      test_xlsx_export_sanitization.py
    settings/
      test_retention.py
      test_storage.py
    views/
      test_base_dispatch.py
  contract/
    api/
      test_authentication.py
      test_cycles.py
      test_generic_asset.py
      test_labels.py
      test_projects.py
    app/
      test_api_token.py
      test_authentication.py
      test_project_app.py
      test_workspace_app.py
  smoke/
    test_auth_smoke.py
```

**Naming convention:** `test_*.py` — matched via pytest config `python_files = test_*.py`

### Storybook

**Location:** `*.stories.tsx` files co-located with components.

```
packages/ui/src/
  avatar/
    avatar.tsx
    avatar.stories.tsx
    helper.tsx
  tables/
    table.tsx
    table.stories.tsx
```

## Test Structure

### Frontend (Vitest + Effect-TS testing)

```typescript
// imports from vitest
import { describe, it, expect, assert } from "vitest";
// Effect-TS imports
import { Effect, Duration, Either } from "effect";
// source under test
import { withTimeoutAndRetry, recoverWithDefault } from "@/services/pdf-export/effect-utils";

// Arrange-Act-Assert pattern within describe/it blocks
describe("effect-utils", () => {
  describe("withTimeoutAndRetry", () => {
    it("should succeed when effect completes within timeout", async () => {
      const effect = Effect.succeed("success");
      const wrapped = withTimeoutAndRetry("test-operation")(effect);
      const result = await Effect.runPromise(wrapped);
      expect(result).toBe("success");
    });

    it("should fail with PdfTimeoutError when effect exceeds timeout", async () => {
      const slowEffect = Effect.gen(function* () {
        yield* Effect.sleep(Duration.millis(500));
        return "success";
      });
      const wrapped = withTimeoutAndRetry("test-operation", {
        timeoutMs: 50,
        maxRetries: 0,
      })(slowEffect);
      const result = await Effect.runPromise(Effect.either(wrapped));
      assert(Either.isLeft(result), "Expected Left but got Right");
      expect(result.left).toBeInstanceOf(PdfTimeoutError);
    });
  });
});
```

**Key patterns:**

- `describe` blocks for module grouping, nested `describe` for function/feature grouping
- `it("should ...")` naming convention
- `async/await` with `Effect.runPromise()` for Effect-TS integration
- Both `expect()` (vitest) and `assert()` (vitest) assertions used
- `Either.isLeft()` pattern for checking Effect-TS error results

### Backend (pytest + Django)

```python
import pytest
from uuid import uuid4
from plane.db.models import Workspace, WorkspaceMember

@pytest.mark.unit
class TestWorkspaceModel:
    """Test the Workspace model"""

    @pytest.mark.django_db
    def test_workspace_creation(self, create_user):
        """Test creating a workspace"""
        workspace = Workspace.objects.create(
            name="Test Workspace", slug="test-workspace", id=uuid4(), owner=create_user
        )
        assert workspace.id is not None
        assert workspace.name == "Test Workspace"
        assert workspace.slug == "test-workspace"
        assert workspace.owner == create_user
```

**Key patterns:**

- `@pytest.mark.unit` / `@pytest.mark.contract` / `@pytest.mark.smoke` on test classes
- `@pytest.mark.django_db` on methods requiring database access
- Fixture injection via function parameters (`create_user`，`api_client`，`api_key_client`)
- Test classes with `Test*` naming
- Methods with `test_*` naming and descriptive docstrings
- Standard `assert` for all assertions (no `self.assertEqual`)

**Contract tests pattern:**

```python
import pytest
from rest_framework import status

@pytest.mark.contract
class TestAPIKeyAuthenticationContract:
    USERS_ME_URL = "/api/v1/users/me/"

    @pytest.mark.django_db
    def test_active_user_can_access_with_api_key(self, api_key_client):
        response = api_key_client.get(self.USERS_ME_URL)
        assert response.status_code == status.HTTP_200_OK

    @pytest.mark.django_db
    def test_deactivated_user_cannot_access_with_api_key(self, api_key_client, create_user):
        create_user.is_active = False
        create_user.save()
        response = api_key_client.get(self.USERS_ME_URL)
        assert response.status_code in (status.HTTP_401_UNAUTHORIZED, status.HTTP_403_FORBIDDEN)
```

### Storybook Stories

```typescript
import type { Meta, StoryObj } from "@storybook/react";
import { Avatar } from "./avatar";

const meta: Meta<typeof Avatar> = {
  title: "Avatar",
  component: Avatar,
};

export default meta;
type Story = StoryObj<typeof Avatar>;

export const Default: Story = {
  args: { name: "John Doe" },
};

export const Large: Story = {
  args: { name: "John Doe" },
};
```

**Pattern:**

- CSF3 (Component Story Format v3) with `Meta` and `StoryObj` types
- Default export: `Meta<typeof Component>` with `title` and `component`
- Named exports for each story variant
- `args` for static prop values

## Fixtures and Factories

### Backend (Django conftest.py - `apps/api/plane/tests/conftest.py`)

```python
@pytest.fixture(scope="session")
def django_db_setup(django_db_setup):
    """Set up the Django database for the test session"""
    pass

@pytest.fixture
def api_client():
    """Return an unauthenticated API client"""
    return APIClient()

@pytest.fixture
def user_data():
    """Return standard user data for tests"""
    return {"email": "test@plane.so", "password": "test-password", "first_name": "Test", "last_name": "User"}

@pytest.fixture
def create_user(db, user_data):
    """Create and return a user instance"""
    user = User.objects.create(email=user_data["email"], first_name=user_data["first_name"], ...)
    user.set_password(user_data["password"])
    user.save()
    return user

@pytest.fixture
def api_token(db, create_user):
    """Create and return an API token for testing the external API"""
    token = APIToken.objects.create(user=create_user, label="Test API Token", token="test-api-token-12345")
    return token

@pytest.fixture
def api_key_client(api_client, api_token):
    """Return an API key authenticated client for external API testing"""
    api_client.credentials(HTTP_X_API_KEY=api_token.token)
    return api_client

@pytest.fixture
def session_client(api_client, create_user):
    """Return a session authenticated API client for app API testing"""
    api_client.force_authenticate(user=create_user)
    return api_client
```

**Key patterns:**

- Fixture chaining: `api_key_client` depends on `api_client` + `api_token`; `api_token` depends on `create_user`
- Separate fixtures for data (`user_data`), model creation (`create_user`), and auth (`api_key_client`，`session_client`)
- `scope="session"` for DB setup fixture

### Frontend

**No centralized fixture system detected.** Test data is created inline within test files:

```typescript
const doc: TipTapDocument = { type: "doc", content: [...] };
const metadata: PDFExportMetadata = { userMentions: [{ id: "user-123", display_name: "John Doe" }] };
```

## Mocking

### Python (unittest.mock)

```python
from unittest.mock import patch

@pytest.mark.contract
class TestMagicLinkGenerate:
    @pytest.fixture
    def setup_user(self, db):
        user = User.objects.create(email="user@plane.so")
        user.set_password("user@123")
        user.save()
        return user

    # patching used for external calls and async tasks
    @patch("plane.authentication.provider.credentials.magic_code.MagicCodeProvider.send_magic_code")
    def test_magic_code_generation(self, mock_send, setup_user, django_client):
        mock_send.return_value = True
        # ... test logic
```

**Key patterns:**

- `unittest.mock.patch` decorator on test methods
- Patches placed on the method, not the import location (Django convention)
- Mocked return values set inline or via `return_value`

### Frontend

No external mocking library configured. Effect-TS tests use `Effect.fail()` / `Effect.succeed()` for controlled effects. The test files examined do not use `vi.mock()` or similar Vitest mocking utilities.

## Docker-Based Backend Test Infrastructure

**Config:** `docker-compose-test.yml` at repo root

**Services:**
| Service | Image | Purpose |
|---------|-------|---------|
| `test-db` | `postgres:15.7-alpine` | Application database |
| `test-redis` | `valkey/valkey:7.2.11-alpine` | Cache / Celery broker |
| `test-mq` | `rabbitmq:3.13.6-management-alpine` | Task queue |
| `test-minio` | `minio/minio` | S3-compatible object storage |
| `api-tests` | Built from `apps/api/Dockerfile.dev` | Installs `requirements/test.txt`, runs pytest |

**Key characteristics:**

- All data directories use `tmpfs` — every run starts clean
- Health checks on all services before tests start
- `DJANGO_SETTINGS_MODULE=plane.settings.test`
- Override hostnames via env: `POSTGRES_HOST=test-db`, `REDIS_URL=redis://test-redis:6379/`
- Install test-only dependencies from `apps/api/requirements/test.txt`
- Teardown: `docker compose -f docker-compose-test.yml down -v`

## Run Commands

### Frontend

```bash
# Run all tests (via turbo)
pnpm turbo run test

# Run tests for a specific package
pnpm --filter=@plane/live test

# Run specific test file
pnpm --filter=@plane/live vitest tests/services/pdf-export/effect-utils.test.ts
```

### Backend

```bash
# Full test suite
docker compose -f docker-compose-test.yml up --build --abort-on-container-exit --exit-code-from api-tests

# Unit tests only
docker compose -f docker-compose-test.yml run --rm --build api-tests pytest -m unit

# Specific test file with verbose output
docker compose -f docker-compose-test.yml run --rm api-tests pytest plane/tests/unit/models/test_workspace_model.py -vv

# Filter by test name
docker compose -f docker-compose-test.yml run --rm api-tests pytest plane/tests/unit -k "test_workspace"

# Teardown
docker compose -f docker-compose-test.yml down -v
```

### Storybook

```bash
pnpm --filter=@plane/ui storybook    # Start on port 6006
pnpm build-storybook                  # Build static storybook
```

## CI/CD Testing

### Build and lint web apps (`.github/workflows/pull-request-build-lint-web-apps.yml`)

Triggers on PR to `preview` branch. Runs 4 parallel/consecutive jobs:

1. **check:format** — `pnpm turbo run check:format --affected` (no build dependency)
2. **Build packages** — `pnpm turbo run build --affected` (prerequisite for type checking)
3. **check:lint** — `pnpm turbo run check:lint --affected` (no build dependency)
4. **check:types** — `pnpm turbo run check:types --affected` (depends on build)

Turbo `--affected` filter uses `TURBO_SCM_BASE` (PR base SHA) and `TURBO_SCM_HEAD` (PR head SHA).

**Notable: No automated test execution (`pnpm test` or `pytest`) runs in CI for either frontend or backend.**

### Build and lint API (`.github/workflows/pull-request-build-lint-api.yml`)

Triggers on PR to `preview` branch when `apps/api/**` changes. Runs:

1. **Lint API** — Python 3.12, ruff check + fix

### Other CI checks:

- `build-branch.yml` — Full branch build
- `check-version.yml` — Version consistency check
- `codeql.yml` — CodeQL security analysis
- `copyright-check.yml` — Copyright header verification
- `i18n-sync-check.yml` — Translation key sync validation
- `react-doctor.yml` — React best practices check

## Test Coverage

**Frontend (Vitest):** Coverage available via `@vitest/coverage-v8` provider. Output formats: text, json, html. Configured in `apps/live/vitest.config.ts` with `include: ["src/**/*.ts"]` excluding `*.d.ts` and `types.ts`. No minimum threshold enforced in CI.

**Backend:** Coverage configuration not detected in pytest config. No coverage reporting in CI.

## Test Types

### Unit Tests

**Frontend scope:** Testing utility functions, Effect-TS effect composition, PDF rendering logic. Located in `tests/` directories.

**Backend scope:** Models, serializers, utility functions, middleware, settings, views. Marked with `@pytest.mark.unit`. Located in `apps/api/plane/tests/unit/`.

### Integration/Contract Tests

**Backend scope:** API endpoint testing with real request/response cycle against test database. Uses `APIClient` for external API and `Client` for app API. Marked with `@pytest.mark.contract`. Located in `apps/api/plane/tests/contract/{api,app}/`.

### Smoke Tests

**Backend scope:** Critical path testing. Marked with `@pytest.mark.slow`. Located in `apps/api/plane/tests/smoke/`. Current test: `test_auth_smoke.py`.

### Component Tests (Storybook)

**Scope:** UI component development and visual verification in isolation. Uses `@storybook/react-webpack5`. Located in `*.stories.tsx` files co-located with components.

### E2E Tests

**Not detected.** No Playwright, Cypress, or Selenium configurations found in the codebase.

## Test Documentation

**Key documentation files:**

- `AGENTS.md` — Brief testing section with Docker commands
- `apps/api/tests/RUNNING_TESTS.md` — Comprehensive guide: prerequisites, full suite, filtered runs, teardown, architecture, troubleshooting
- `apps/api/tests/TESTING_GUIDE.md` — (referenced but not read) Test conventions and fixtures
- `CONTRIBUTING.md` — States: "All features or bug fixes must be tested by one or more specs (unit-tests)"

## Test Coverage Gaps and Observations

1. **No automated test execution in CI**: Both frontend (`vitest`) and backend (`pytest`) tests are not executed in CI pipelines. Only format, lint, and type checks run automatically.

2. **Limited frontend test scope**: Tests observed only in `apps/live` (pdf rendering, effect utilities). No tests found in `apps/web`, `packages/ui`, `packages/services`, `packages/hooks`, or `packages/shared-state` via the Vitest configuration globs.

3. **No React component unit tests**: Storybook provides visual testing but no `@testing-library/react` or browser-based component tests detected.

4. **No E2E testing**: No Playwright, Cypress, or Selenium setup exists.

5. **No frontend mocking infrastructure**: No MSW, nock, or `vi.mock()` patterns observed in existing tests. Effect-TS tests rely on controlled effects (`Effect.fail()`, `Effect.succeed()`).

6. **No coverage thresholds**: Neither frontend nor backend enforces minimum coverage.

7. **Backend test infrastructure is well-documented** but requires Docker and has a non-trivial dependency stack (Postgres, Valkey, RabbitMQ, MinIO).

---

_Testing analysis: 2026-06-16_
