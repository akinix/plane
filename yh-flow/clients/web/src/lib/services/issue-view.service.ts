// FLOW: IssueView service — mock API layer for persisted views (per D-P17-14, D-P17-15)
// Mock implementation: in-memory storage, returns IDs for created views
import type { TIssueView } from "@/components/issues/filters/types";
import { FlowApiService } from "./flow-api.service";

// In-memory mock storage for issue views
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
  },
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
}

// Singleton export for convenience
export const issueViewService = new IssueViewService();
