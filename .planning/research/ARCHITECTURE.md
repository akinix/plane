# API Integration Architecture: Mock Data to Real .NET API

**Project:** YH.Flow (Flow) — v3.0 Milestone
**Researched:** 2026-06-30
**Mode:** Ecosystem (Architecture for Frontend-Backend Integration)
**Overall confidence:** HIGH

---

## 1. Current Architecture State

### 1.1 Layered Architecture (as-built)

```
React Components
  -> TanStack Query hooks (src/lib/hooks/use-*.ts)
    -> queryFn: delay(ms) + MOCK_*.filter()  [ALL hooks currently use mock data]
    -> Returns @plane/types in camelCase
    -> State: TanStack Query server state + MobX UI state

Auth flow only uses real API:
  -> AuthService extends FlowApiService
    -> Axios with JWT Bearer interceptor
    -> Response: humps.camelizeKeys (snake_case -> camelCase)
    -> Error: standardized ErrorResponse
    -> Backend: .NET Minimal API at https://localhost:7030
    -> Proxy: Vite dev server proxies /api -> backend
```

### 1.2 Current Data Flow

```
Component uses useWorkspaces()
  -> useQuery({ queryKey: ["workspaces"], queryFn: async () => {
       await delay(300);          // Simulated latency
       return MOCK_WORKSPACES;    // Hardcoded in-memory array
     }})
  -> Component receives IWorkspace[] instantly (after 300ms simulated delay)
  -> No loading state handling (rarely shows loader)
  -> No error handling (mock never fails)
```

### 1.3 Frontend HTTP Client — `FlowApiService`

| Aspect              | Current State                                                                                            |
| ------------------- | -------------------------------------------------------------------------------------------------------- |
| Base class          | Abstract `FlowApiService` in `src/lib/services/flow-api.service.ts`                                      |
| Auth                | JWT Bearer from localStorage, auto-attached by request interceptor                                       |
| Response conversion | `humps.camelizeKeys` in response interceptor                                                             |
| Request conversion  | **NONE** — no `humps.decamelizeKeys` for outgoing data                                                   |
| 401 handling        | Immediate `removeToken()` + redirect to `/auth/sign-in`                                                  |
| Error format        | Falls back through: `data.error -> data.title -> data.detail -> "Unknown error"`                         |
| Existing services   | `AuthService` (real), `IssueViewService` (mock), `AnalyticsService` (mock), `NotificationService` (mock) |

### 1.4 Backend .NET API Structure

| Aspect         | Implementation                                                                                                                           |
| -------------- | ---------------------------------------------------------------------------------------------------------------------------------------- |
| Framework      | ASP.NET Core Minimal API                                                                                                                 |
| Modules        | Each module implements `IModule` with `ConfigureServices`, `ConfigureMiddleware`, `MapEndpoints`                                         |
| API Versioning | `/api/v{version:apiVersion}/` (currently v1)                                                                                             |
| JSON Naming    | `SnakeCaseLower` + `JsonStringEnumConverter` + `WhenWritingNull`                                                                         |
| Auth           | JWT Bearer (Identity module) + Session Cookie (Plane compat)                                                                             |
| Multi-tenancy  | Finbuckle with `WorkspaceSlugStrategy` (slug as tenant identifier)                                                                       |
| Pagination     | `PlanePagedResult<T>` — Plane-compatible paginated response with `results`, `next`, `previous`, `total_pages`, `total_count`, `per_page` |
| Error format   | Dual-track: Plane format for `/auth/*`, ProblemDetails for everything else                                                               |

**Module route prefixes** (all under `/api/v{version:apiVersion}`):

| Module        | Route Group                                                                                |
| ------------- | ------------------------------------------------------------------------------------------ |
| Identity      | `/auth/*`, `/users/me/*`                                                                   |
| Workspace     | `/workspaces/{slug}/*`, `/workspaces/{slug}/members/*`, `/workspaces/{slug}/invitations/*` |
| Project       | `/workspaces/{slug}/projects/*`, ...`/projects/{projectId}/members/*`                      |
| WorkItems     | `/workspaces/{slug}/projects/{projectId}/work-items/*`                                     |
|               | ...`/projects/{projectId}/states/*`                                                        |
|               | ...`/projects/{projectId}/labels/*`                                                        |
|               | ...`/projects/{projectId}/estimates/*`                                                     |
|               | ...`/projects/{projectId}/cycles/*`                                                        |
|               | ...`/projects/{projectId}/modules/*`                                                       |
| Page          | `/workspaces/{slug}/projects/{projectId}/pages/*`                                          |
| View          | `/workspaces/{slug}/projects/{projectId}/views/*`                                          |
| Analytics     | `/workspaces/{slug}/analytics/*`                                                           |
| Notifications | `/notifications/*`                                                                         |
| Webhooks      | `/webhooks/*`                                                                              |

---

## 2. Target Architecture

### 2.1 After Integration

```
React Components
  -> TanStack Query hooks (src/lib/hooks/use-*.ts)
    -> queryFn calls Service method
       -> Service extends FlowApiService
          -> Axios with JWT Bearer + decamelize request body
          -> .NET API at /api/v1/...
          -> Response: humps.camelizeKeys (snake_case -> camelCase)
       -> Extracts .results[] for paginated responses
       -> Returns @plane/types in camelCase
    -> TanStack Query manages caching, refetch, optimistic updates
    -> MobX stores manage ONLY UI state (no API calls)

Real-time updates via SSE:
  .NET SSE Endpoint -> EventSource -> SSE Dispatcher
    -> queryClient.setQueryData() for targeted cache updates
    -> MobX stores for notification badges, UI indicators
```

### 2.2 Service Layer Pattern

Every domain module gets a dedicated service class. The pattern is:

```typescript
// src/lib/services/workspace.service.ts
import { FlowApiService } from "./flow-api.service";

class WorkspaceService extends FlowApiService {
  private static BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5030/api/v1";

  constructor() {
    super(WorkspaceService.BASE_URL);
  }

  // List returns array (extract from PlanePagedResult)
  async listMyWorkspaces(): Promise<IWorkspace[]> {
    const res = await this.get("/users/me/workspaces/");
    return res?.data?.results ?? [];
  }

  // GET returns single entity
  async getWorkspace(slug: string): Promise<IWorkspace> {
    return this.get(`/workspaces/${slug}/`).then((r) => r?.data);
  }

  // POST returns created entity
  async createWorkspace(data: Partial<IWorkspace>): Promise<IWorkspace> {
    return this.post("/workspaces/", data).then((r) => r?.data);
  }

  // PUT returns updated entity
  async updateWorkspace(slug: string, data: Partial<IWorkspace>): Promise<IWorkspace> {
    return this.put(`/workspaces/${slug}/`, data).then((r) => r?.data);
  }

  // DELETE returns void
  async deleteWorkspace(slug: string): Promise<void> {
    return this.delete(`/workspaces/${slug}/`).then(() => undefined);
  }
}

export const workspaceService = new WorkspaceService();
```

### 2.3 Hook Migration Pattern

```typescript
// BEFORE (mock):
export const useWorkspaces = () => {
  return useQuery<IWorkspace[]>({
    queryKey: ["workspaces"],
    queryFn: async () => {
      await delay(300);
      return MOCK_WORKSPACES;
    },
  });
};

// AFTER (real API):
export const useWorkspaces = () => {
  return useQuery<IWorkspace[]>({
    queryKey: ["workspaces"],
    queryFn: () => workspaceService.listMyWorkspaces(),
    staleTime: 5 * 60 * 1000, // 5 min cache
    retry: 1, // One retry on failure
  });
};
```

For workspace/project-scoped hooks:

```typescript
export const useProjects = () => {
  const { workspace } = useStore();
  const slug = workspace.currentWorkspaceSlug;

  return useQuery<IProject[]>({
    queryKey: ["projects", slug],
    queryFn: () => projectService.listProjects(slug!),
    enabled: !!slug,
    staleTime: 5 * 60 * 1000,
  });
};
```

### 2.4 Optimistic Mutation Pattern (preserved from mock)

```typescript
export const useIssueMutations = () => {
  const queryClient = useQueryClient();
  const { workspace } = useStore();
  const slug = workspace.currentWorkspaceSlug;

  const updateIssue = useMutation({
    mutationFn: async ({ projectId, issueId, data }: { projectId: string; issueId: string; data: Partial<TIssue> }) => {
      return workItemsService.updateIssue(slug!, projectId, issueId, data);
    },

    onMutate: async ({ projectId, issueId, data }) => {
      await queryClient.cancelQueries({
        queryKey: ["issues", slug, projectId],
      });
      const previousData = queryClient.getQueryData<TIssue[]>(["issues", slug, projectId]);

      // Optimistic update
      queryClient.setQueryData<TIssue[]>(["issues", slug, projectId], (old) =>
        old?.map((i) => (i.id === issueId ? { ...i, ...data, updated_at: new Date().toISOString() } : i))
      );

      return { previousData };
    },

    onError: (_err, _vars, context) => {
      if (context?.previousData) {
        queryClient.setQueryData(["issues", slug, context._vars.projectId], context.previousData);
      }
    },

    onSettled: (_data, _error, variables) => {
      queryClient.invalidateQueries({
        queryKey: ["issues", slug, variables.projectId],
      });
    },
  });

  return { updateIssue };
};
```

---

## 3. Critical Architectural Changes

### 3.1 Request Interceptor — Add `humps.decamelizeKeys`

**Current problem**: The `FlowApiService` response interceptor converts snake_case -> camelCase, but the **request interceptor has no reverse conversion**. The .NET backend's `SnakeCaseLower` naming policy only affects serialization (C# -> JSON). Deserialization of incoming JSON uses case-insensitive matching by default, so camelCase request bodies WOULD bind correctly for most POST/PUT/PATCH scenarios.

**EXCEPTION**: Query string parameters in ASP.NET Minimal API bind by exact parameter name. If an endpoint expects `?assignee_ids=user-1&state_id=state-1`, sending `?assigneeIds=user-1&stateId=state-1` will NOT bind.

**Fix** — Add request interceptor to `FlowApiService`:

```typescript
this.axiosInstance.interceptors.request.use((config) => {
  const token = getToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  // Convert camelCase request body to snake_case for .NET backend
  if (config.data && typeof config.data === "object" && !(config.data instanceof FormData)) {
    config.data = humps.decamelizeKeys(config.data);
  }

  // Convert camelCase params to snake_case for query string binding
  if (config.params && typeof config.params === "object") {
    config.params = humps.decamelizeKeys(config.params);
  }

  return config;
});
```

### 3.2 Token Refresh Queue — Fix 401 Race Condition

**Current problem**: On 401, the interceptor immediately clears the token and redirects. When multiple concurrent requests fail with 401 (page load), each triggers the redirect independently, causing race conditions.

**Fix** — Implement request queue + singleton refresh:

```typescript
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (token: string) => void;
  reject: (err: unknown) => void;
}> = [];

const processQueue = (error: unknown, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token!);
    }
  });
  failedQueue = [];
};

// In response error interceptor:
(error) => {
  const originalRequest = error.config;
  if (error?.response?.status === 401 && !originalRequest._retry) {
    if (isRefreshing) {
      // Queue this request until refresh completes
      return new Promise((resolve, reject) => {
        failedQueue.push({ resolve, reject });
      }).then((token) => {
        originalRequest.headers.Authorization = `Bearer ${token}`;
        return axiosInstance(originalRequest);
      });
    }

    originalRequest._retry = true;
    isRefreshing = true;

    return authService
      .refreshToken()
      .then((tokenData) => {
        processQueue(null, tokenData.accessToken);
        originalRequest.headers.Authorization = `Bearer ${tokenData.accessToken}`;
        return axiosInstance(originalRequest);
      })
      .catch((refreshError) => {
        processQueue(refreshError, null);
        removeToken();
        window.location.href = "/auth/sign-in";
        return Promise.reject(refreshError);
      })
      .finally(() => {
        isRefreshing = false;
      });
  }

  // Existing error standardization...
};
```

### 3.3 Slug Resolution — The UUID vs Slug Problem

**Problem**: Backend routes use `{slug}` (human-readable like "flow-dev"). Frontend hooks currently receive UUID `workspaceId` and `projectId` parameters. The hooks need to resolve slugs before making API calls.

**Solution**: Store slug in MobX and resolve in hooks.

The MobX store already tracks:

- `workspace.currentWorkspaceSlug` — available after workspace selection
- `workspace.currentProjectId` — UUID GUID, used directly in URLs
- `workspace.currentWorkspaceId` — UUID GUID

Hook resolution pattern:

```typescript
// For hooks that always operate in current context:
export const useProjects = () => {
  const { workspace } = useStore();
  const slug = workspace.currentWorkspaceSlug;
  return useQuery({
    queryKey: ["projects", slug],
    queryFn: () => projectService.listProjects(slug!),
    enabled: !!slug,
  });
};

// For hooks that need explicit slug (cross-workspace admin):
export const useProjectsBySlug = (slug: string) => {
  return useQuery({
    queryKey: ["projects", slug],
    queryFn: () => projectService.listProjects(slug),
    enabled: !!slug,
  });
};
```

### 3.4 Paginated Response Handling

**Backend**: All list endpoints return `PlanePagedResult<T>`:

```json
{
  "results": [{...}, {...}],
  "next": "http://...?page=2",
  "previous": null,
  "total_pages": 1,
  "total_count": 3,
  "per_page": 50
}
```

**Frontend handling** — Phase 1 (extract in service):

```typescript
// Service layer strips the wrapper
async listProjects(slug: string): Promise<IProject[]> {
  return this.get(`/workspaces/${slug}/projects/`)
    .then((r) => r?.data?.results ?? []);
}
```

Hooks continue returning `T[]` arrays. Default page size (50) covers typical use cases. Pagination controls added later as a non-breaking enhancement.

### 3.5 Query Key Factory — Centralized Cache Management

**Current**: Inconsistent string keys scattered across files.

**To implement** before migrating hooks:

```typescript
// src/lib/lib/query-keys.ts
export const queryKeys = {
  workspaces: {
    all: () => ["workspaces"] as const,
    detail: (slug: string) => ["workspaces", slug] as const,
  },
  projects: {
    all: (slug: string) => ["projects", slug] as const,
    detail: (slug: string, projectId: string) => ["projects", slug, projectId] as const,
  },
  workItems: {
    all: (slug: string, projectId: string) => ["work-items", slug, projectId] as const,
    list: (slug: string, projectId: string, filters?: object) =>
      ["work-items", slug, projectId, "list", filters] as const,
    detail: (slug: string, projectId: string, issueId: string) => ["work-items", slug, projectId, issueId] as const,
    comments: (issueId: string) => ["work-items", "comments", issueId] as const,
    activities: (issueId: string) => ["work-items", "activities", issueId] as const,
  },
  cycles: {
    all: (slug: string, projectId: string) => ["cycles", slug, projectId] as const,
    detail: (slug: string, projectId: string, cycleId: string) => ["cycles", slug, projectId, cycleId] as const,
    issues: (slug: string, projectId: string, cycleId: string) =>
      ["cycles", slug, projectId, cycleId, "issues"] as const,
    progress: (slug: string, projectId: string, cycleId: string) =>
      ["cycles", slug, projectId, cycleId, "progress"] as const,
  },
  modules: {
    all: (slug: string, projectId: string) => ["modules", slug, projectId] as const,
    detail: (slug: string, projectId: string, moduleId: string) => ["modules", slug, projectId, moduleId] as const,
    issues: (slug: string, projectId: string, moduleId: string) =>
      ["modules", slug, projectId, moduleId, "issues"] as const,
  },
  pages: {
    all: (slug: string, projectId: string) => ["pages", slug, projectId] as const,
    detail: (slug: string, projectId: string, pageId: string) => ["pages", slug, projectId, pageId] as const,
    archived: (slug: string, projectId: string) => ["pages", slug, projectId, "archived"] as const,
  },
  views: {
    all: (slug: string, projectId: string) => ["views", slug, projectId] as const,
    detail: (slug: string, projectId: string, viewId: string) => ["views", slug, projectId, viewId] as const,
  },
  notifications: {
    all: () => ["notifications"] as const,
    unreadCount: () => ["notifications", "unread-count"] as const,
  },
  analytics: {
    overview: (slug: string) => ["analytics", slug, "overview"] as const,
    trend: (slug: string) => ["analytics", slug, "trend"] as const,
    bar: (slug: string) => ["analytics", slug, "bar"] as const,
  },
  members: {
    workspace: (slug: string) => ["members", "workspace", slug] as const,
    project: (slug: string, projectId: string) => ["members", "project", slug, projectId] as const,
  },
  webhooks: {
    subscriptions: (slug: string) => ["webhooks", slug, "subscriptions"] as const,
    deliveries: (slug: string, subId: string) => ["webhooks", slug, "subscriptions", subId, "deliveries"] as const,
  },
};
```

---

## 4. Module-by-Module Integration Plan

### 4.1 Auth (Already Real API)

| File                  | Action                                                                   |
| --------------------- | ------------------------------------------------------------------------ |
| `auth.service.ts`     | Verify token refresh endpoint works; add refresh queue to FlowApiService |
| `flow-api.service.ts` | Add decamelize request interceptor + token refresh queue                 |
| `query-keys.ts`       | Create file                                                              |

### 4.2 Workspace + Member

| File                   | Action                                                      |
| ---------------------- | ----------------------------------------------------------- |
| `workspace.service.ts` | NEW: listMyWorkspaces, getWorkspace, create, update, delete |
| `member.service.ts`    | NEW: listWorkspaceMembers, updateRole, remove, invite       |
| `use-workspaces.ts`    | Replace queryFn with workspaceService calls                 |
| `use-members.ts`       | Replace queryFn with memberService calls                    |

**Endpoint map**:

- `GET /api/v1/users/me/workspaces/` -> `workspaceService.listMyWorkspaces()`
- `GET /api/v1/workspaces/{slug}/` -> `workspaceService.getWorkspace(slug)`
- `POST /api/v1/workspaces/` -> `workspaceService.createWorkspace(data)`
- `PUT /api/v1/workspaces/{slug}/` -> `workspaceService.updateWorkspace(slug, data)`
- `DELETE /api/v1/workspaces/{slug}/` -> `workspaceService.deleteWorkspace(slug)`
- `GET /api/v1/workspaces/{slug}/members/` -> `memberService.listMembers(slug)`

### 4.3 Project

| File                 | Action                                                         |
| -------------------- | -------------------------------------------------------------- |
| `project.service.ts` | NEW: listProjects, getProject, create, update, delete          |
| `use-projects.ts`    | Replace queryFn with projectService calls; add slug resolution |

### 4.4 WorkItems (Issues + States + Labels)

| File                    | Action                                                           |
| ----------------------- | ---------------------------------------------------------------- |
| `work-items.service.ts` | NEW: issues CRUD, list with filters, bulk update, states, labels |
| `use-issues.ts`         | Replace queryFn; preserve optimistic update pattern              |
| `use-comments.ts`       | Replace mock with service calls                                  |
| `use-cycle-issues.ts`   | Replace queryFn; already uses proper queryKeys pattern           |

**Endpoint map**:

- `GET .../work-items/?state_id=X&priority=urgent` -> `workItemsService.listIssues(slug, projectId, filters)`
- `POST .../work-items/` -> `workItemsService.createIssue(slug, projectId, data)`
- `PATCH .../work-items/{issueId}` -> `workItemsService.updateIssue(slug, projectId, issueId, data)`
- `DELETE .../work-items/{issueId}` -> `workItemsService.deleteIssue(slug, projectId, issueId)`
- `POST .../work-items/bulk/` -> `workItemsService.bulkUpdate(slug, projectId, payload)`
- `GET .../states/` -> `workItemsService.listStates(slug, projectId)`
- `GET .../labels/` -> `workItemsService.listLabels(slug, projectId)`
- `GET .../work-items/{issueId}/comments/` -> `workItemsService.listComments(slug, projectId, issueId)`

### 4.5 Cycle

| File                  | Action                                             |
| --------------------- | -------------------------------------------------- |
| `cycle.service.ts`    | NEW: CRUD + add/remove issues + progress           |
| `use-cycles.ts`       | Replace queryFn                                    |
| `use-cycle-issues.ts` | Replace queryFn (keeps existing query key pattern) |

### 4.6 Module

| File                   | Action                                           |
| ---------------------- | ------------------------------------------------ |
| `module.service.ts`    | NEW: CRUD + add/remove issues + links + progress |
| `use-modules.ts`       | Replace queryFn                                  |
| `use-module-issues.ts` | Replace queryFn                                  |

### 4.7 Page

| File                    | Action                                         |
| ----------------------- | ---------------------------------------------- |
| `page.service.ts`       | NEW: CRUD + archive/unarchive + favorite       |
| `use-pages.ts`          | Replace queryFn                                |
| `use-page-mutations.ts` | Replace mutationFn; preserve optimistic update |

### 4.8 View

| File                               | Action                                                  |
| ---------------------------------- | ------------------------------------------------------- |
| `issue-view.service.ts`            | **UPDATE** — replace in-memory CRUD with real API calls |
| `use-views.ts``use-issue-views.ts` | Already uses service; no hook change needed             |
| `use-view-mutations.ts`            | Already uses service; no hook change needed             |

### 4.9 Notification

| File                      | Action                                        |
| ------------------------- | --------------------------------------------- |
| `notification.service.ts` | **UPDATE** — replace mock with real API calls |
| `use-notifications.ts`    | Already uses service; no hook change needed   |

### 4.10 Analytics

| File                   | Action                                        |
| ---------------------- | --------------------------------------------- |
| `analytics.service.ts` | **UPDATE** — replace mock with real API calls |
| `use-analytics.ts`     | Already uses service; no hook change needed   |

### 4.11 Webhook (New Feature)

| File                       | Action                                      |
| -------------------------- | ------------------------------------------- |
| `webhook.service.ts`       | NEW: subscriptions CRUD + deliveries + test |
| `use-webhooks.ts`          | NEW: TanStack Query hooks                   |
| `use-webhook-mutations.ts` | NEW: mutation hooks                         |
| Components                 | NEW: admin UI pages                         |

---

## 5. Build Order (Dependency Graph)

```
Layer 0: Infrastructure
  [0.1] query-keys.ts                   — Centralized key factory
  [0.2] FlowApiService enhancements     — decamelize request interceptor,
                                          token refresh queue

Layer 1: Foundation (slug/project ID required by everything downstream)
  [1.1] workspace.service.ts            — PREREQUISITE: none
  [1.2] use-workspaces.ts rewrite       — PREREQUISITE: 1.1
  [1.3] member.service.ts + hooks       — PREREQUISITE: 1.1 (depends on slug)
  [1.4] project.service.ts + hooks      — PREREQUISITE: 1.1 (depends on slug)

Layer 2: Core business (heaviest)
  [2.1] work-items.service.ts           — PREREQUISITE: 1.4 (needs slug + projectId)
  [2.2] use-issues.ts rewrite           — PREREQUISITE: 2.1
  [2.3] use-comments.ts rewrite         — PREREQUISITE: 2.1

Layer 3: Cycles + Modules
  [3.1] cycle.service.ts + hooks        — PREREQUISITE: 2.1 (shares WorkItems context)
  [3.2] module.service.ts + hooks       — PREREQUISITE: 2.1

Layer 4: Secondary
  [4.1] page.service.ts + hooks         — PREREQUISITE: 1.4
  [4.2] view.service update + hooks     — PREREQUISITE: 1.4 (already partial)

Layer 5: Utilities
  [5.1] notification.service + hooks    — PREREQUISITE: none (user-scoped)
  [5.2] analytics.service + hooks       — PREREQUISITE: 1.1 (needs slug)

Layer 6: Cleanup + new features
  [6.1] Remove mock-data.ts             — PREREQUISITE: all hooks migrated
  [6.2] webhook.service + hooks + UI    — PREREQUISITE: none (new feature)
```

**Critical path**: Layer 0 -> Layer 1 -> Layer 2. Everything depends on getting the service base class right first.

---

## 6. File Inventory

### New files to create:

```
src/lib/query-keys.ts
src/lib/services/workspace.service.ts
src/lib/services/project.service.ts
src/lib/services/member.service.ts
src/lib/services/work-items.service.ts
src/lib/services/cycle.service.ts
src/lib/services/module.service.ts
src/lib/services/page.service.ts
src/lib/services/webhook.service.ts
```

### Files to update:

```
src/lib/services/flow-api.service.ts         — Add decamelize interceptor + token refresh queue
src/lib/services/issue-view.service.ts       — Real API calls instead of in-memory
src/lib/services/analytics.service.ts        — Real API calls instead of mock
src/lib/services/notification.service.ts     — Real API calls instead of mock
src/lib/services/auth.service.ts             — Verify refresh endpoint works
src/lib/hooks/use-workspaces.ts              — Real service calls
src/lib/hooks/use-projects.ts                — Real service calls
src/lib/hooks/use-members.ts                 — Real service calls
src/lib/hooks/use-issues.ts                  — Real service calls
src/lib/hooks/use-comments.ts                — Real service calls
src/lib/hooks/use-cycles.ts                  — Real service calls
src/lib/hooks/use-cycle-issues.ts            — Real service calls
src/lib/hooks/use-modules.ts                 — Real service calls
src/lib/hooks/use-module-issues.ts           — Real service calls
src/lib/hooks/use-pages.ts                   — Real service calls
src/lib/hooks/use-page-mutations.ts          — Real service calls
src/lib/hooks/use-views.ts                   — No change (already uses service)
src/lib/hooks/use-issue-views.ts             — No change (already uses service)
src/lib/hooks/use-view-mutations.ts          — No change (already uses service)
src/lib/hooks/use-notifications.ts           — No change (already uses service)
src/lib/hooks/use-analytics.ts               — No change (already uses service)
```

### Files to remove:

```
src/lib/mock-data.ts          — Remove after ALL hooks migrated
```

---

## 7. Key Architectural Decisions

| Decision            | Choice                               | Rationale                                                         |
| ------------------- | ------------------------------------ | ----------------------------------------------------------------- |
| Request body format | decamelize via interceptor           | Query string params bind by exact name; body is secondary benefit |
| Token refresh       | Singleton promise + request queue    | Prevents concurrent refresh calls + race conditions               |
| Pagination handling | Extract `results[]` in service layer | Minimal hook changes; pagination controls added later             |
| Slug resolution     | From MobX store in hooks             | Matches backend route design; keeps mutation surface minimal      |
| Query keys          | Centralized factory (query-keys.ts)  | Eliminates cache invalidation bugs across modules                 |
| Error format        | Unified normalization in interceptor | `data.error` > `data.title` > `data.detail` fallback chain        |
| Singleton services  | One instance per service class       | Stateless services; state in TanStack Query + MobX                |
| Optimistic updates  | Preserved exact pattern from mock    | Provides instant UI feedback; pattern already proven              |

---

## Sources

- Codebase analysis: `yh-flow/clients/web/src/lib/services/flow-api.service.ts`, `src/lib/hooks/*.ts`, `src/lib/mock-data.ts`
- Backend analysis: `src/Host/YH.Flow.Api/Program.cs`, all `*Module.cs` endpoint registrations
- Existing research: `.planning/codebase/ARCHITECTURE.md`, `.planning/research/api-migration-mapping.md`
- TanStack Query Optimistic Updates: https://tanstack.com/query/latest/docs/framework/react/guides/optimistic-updates
- TanStack Query Important Defaults: https://tanstack.com/query/v5/docs/framework/react/guides/important-defaults
