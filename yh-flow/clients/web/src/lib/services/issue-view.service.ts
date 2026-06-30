// FLOW: IssueView service — mock API layer for persisted views (per D-P17-14, D-P17-15)
// Mock implementation: in-memory storage, returns IDs for created views
import type { TIssueView } from "@/components/issues/filters/types";
import { FlowApiService } from "./flow-api.service";

// In-memory mock storage for issue views
// Extra fields (access, description, is_favorite, owned_by, created_by) are used by View UI
const MOCK_VIEWS: TIssueView[] = [
  {
    id: "view-1",
    name: "我的任务",
    projectId: "proj-1",
    filters: {
      stateIds: [],
      priorityIds: ["urgent", "high"],
      assigneeIds: ["user-1"],
      labelIds: [],
      searchQuery: "",
      dateRange: null,
    },
    sort: { sortBy: "updated_at", sortDirection: "desc" },
    groupBy: "state",
    subGroupBy: "none",
    displayColumns: ["state", "priority", "assignee", "labels", "created_at"],
    layout: "list",
    createdAt: "2026-06-01T00:00:00Z",
    updatedAt: "2026-06-01T00:00:00Z",
  } as TIssueView & { access: number; description: string; is_favorite: boolean; owned_by: string; created_by: string },
  {
    id: "view-2",
    name: "高优先级 Issues",
    projectId: "proj-1",
    filters: {
      stateIds: [],
      priorityIds: ["urgent", "high"],
      assigneeIds: [],
      labelIds: [],
      searchQuery: "",
      dateRange: null,
    },
    sort: { sortBy: "priority", sortDirection: "desc" },
    groupBy: "priority",
    subGroupBy: "none",
    displayColumns: ["state", "priority", "assignee", "labels", "created_at"],
    layout: "list",
    createdAt: "2026-06-05T00:00:00Z",
    updatedAt: "2026-06-20T00:00:00Z",
  } as TIssueView & { access: number; description: string; is_favorite: boolean; owned_by: string; created_by: string },
  {
    id: "view-3",
    name: "待审阅",
    projectId: "proj-1",
    filters: {
      stateIds: ["state-ff-3"],
      priorityIds: [],
      assigneeIds: ["user-2", "user-3"],
      labelIds: [],
      searchQuery: "",
      dateRange: null,
    },
    sort: { sortBy: "updated_at", sortDirection: "desc" },
    groupBy: "state",
    subGroupBy: "none",
    displayColumns: ["state", "priority", "assignee", "labels", "created_at"],
    layout: "kanban",
    createdAt: "2026-06-10T00:00:00Z",
    updatedAt: "2026-06-25T00:00:00Z",
  } as TIssueView & { access: number; description: string; is_favorite: boolean; owned_by: string; created_by: string },
  {
    id: "view-4",
    name: "本周计划",
    projectId: "proj-2",
    filters: {
      stateIds: [],
      priorityIds: [],
      assigneeIds: [],
      labelIds: [],
      searchQuery: "",
      dateRange: { start: "2026-06-29", end: "2026-07-05" },
    },
    sort: { sortBy: "target_date", sortDirection: "asc" },
    groupBy: "state",
    subGroupBy: "none",
    displayColumns: ["state", "priority", "assignee", "labels", "created_at"],
    layout: "list",
    createdAt: "2026-06-28T00:00:00Z",
    updatedAt: "2026-06-29T00:00:00Z",
  } as TIssueView & { access: number; description: string; is_favorite: boolean; owned_by: string; created_by: string },
  {
    id: "view-5",
    name: "待办事项",
    projectId: "proj-1",
    filters: {
      stateIds: ["state-ff-1"],
      priorityIds: [],
      assigneeIds: [],
      labelIds: [],
      searchQuery: "",
      dateRange: null,
    },
    sort: { sortBy: "created_at", sortDirection: "asc" },
    groupBy: "state",
    subGroupBy: "none",
    displayColumns: ["state", "priority", "assignee", "labels", "created_at"],
    layout: "list",
    createdAt: "2026-06-15T00:00:00Z",
    updatedAt: "2026-06-22T00:00:00Z",
  } as TIssueView & { access: number; description: string; is_favorite: boolean; owned_by: string; created_by: string },
];

let nextId = 100;

export class IssueViewService extends FlowApiService {
  private static BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5030/api/v1";

  constructor() {
    super(IssueViewService.BASE_URL);
  }

  /**
   * Create a new issue view (current: mock in-memory, future: POST /api/v1/views/)
   */
  async createIssueView(data: Omit<TIssueView, "id" | "createdAt" | "updatedAt">): Promise<TIssueView> {
    // FUTURE: replace with real API call:
    // return this.post("/views/", data).then((res) => res?.data);
    const now = new Date().toISOString();
    const view: TIssueView = {
      ...data,
      id: `view-mock-${nextId++}`,
      createdAt: now,
      updatedAt: now,
    };
    MOCK_VIEWS.push(view);
    return view;
  }

  /**
   * Get all views for a project (current: mock in-memory, future: GET /api/v1/views/)
   */
  async getIssueViews(projectId: string): Promise<TIssueView[]> {
    // FUTURE: return this.get("/views/", { params: { project_id: projectId } }).then((res) => res?.data);
    return MOCK_VIEWS.filter((v) => v.projectId === projectId);
  }

  /**
   * Get a single view by ID
   */
  async getIssueView(viewId: string): Promise<TIssueView | undefined> {
    return MOCK_VIEWS.find((v) => v.id === viewId);
  }

  /**
   * Delete a view
   */
  async deleteIssueView(viewId: string): Promise<void> {
    const idx = MOCK_VIEWS.findIndex((v) => v.id === viewId);
    if (idx >= 0) {
      MOCK_VIEWS.splice(idx, 1);
    }
  }

  /**
   * Update a view (current: mock in-memory, future: PATCH /api/v1/views/:id)
   */
  async updateIssueView(viewId: string, data: Partial<TIssueView>): Promise<TIssueView | undefined> {
    const idx = MOCK_VIEWS.findIndex((v) => v.id === viewId);
    if (idx >= 0) {
      MOCK_VIEWS[idx] = { ...MOCK_VIEWS[idx], ...data, updatedAt: new Date().toISOString() };
    }
    return MOCK_VIEWS[idx];
  }

  /**
   * Toggle favorite status for a view
   */
  async favoriteIssueView(viewId: string, isFavorite: boolean): Promise<void> {
    const view = MOCK_VIEWS.find((v) => v.id === viewId);
    if (view) {
      (view as Record<string, unknown>).is_favorite = isFavorite;
    }
  }
}

// Singleton export for convenience
export const issueViewService = new IssueViewService();
