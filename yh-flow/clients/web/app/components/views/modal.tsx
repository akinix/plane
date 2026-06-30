// FLOW: Forked from Plane. Original: apps/web/core/components/views/modal.tsx
// FLOW: ViewModal — 创建/编辑视图弹窗
"use client";
import { useMemo } from "react";
import { observer } from "mobx-react";
import { ModalCore, EModalWidth } from "@plane/ui";
import { useStore } from "@/lib/store-context";
import { useViews } from "@/../src/lib/hooks/use-views";
import { useViewMutations } from "@/../src/lib/hooks/use-view-mutations";
import { EViewAccess } from "@/../src/lib/types/views";
import type { TIssueView } from "@/components/issues/filters/types";
import { ViewForm } from "./form";

type Props = {
  isOpen: boolean;
  onClose: () => void;
  projectId: string;
  workspaceId: string;
};

const ViewModal = observer(function ViewModal({ isOpen, onClose, projectId, workspaceId: _workspaceId }: Props) {
  const { view: viewStore } = useStore();
  const { data: views } = useViews(projectId);
  const { createView, updateView } = useViewMutations();

  const editView = useMemo(() => {
    if (viewStore.viewModalMode !== "edit") return null;
    if (!views) return null;
    return views.find((v) => v.id === viewStore.selectedViewId) ?? null;
  }, [viewStore.viewModalMode, viewStore.selectedViewId, views]);

  const handleSubmit = async (data: { name: string; access: EViewAccess; description: string }) => {
    if (viewStore.viewModalMode === "edit" && editView) {
      await updateView.mutateAsync({
        viewId: editView.id,
        data: { name: data.name, access: data.access, description: data.description } as Partial<TIssueView>,
      });
    } else {
      await createView.mutateAsync({
        name: data.name,
        access: data.access,
        description: data.description,
        projectId,
        filters: { stateIds: [], priorityIds: [], assigneeIds: [], labelIds: [], searchQuery: "", dateRange: null },
        sort: { sortBy: "updated_at", sortDirection: "desc" },
        groupBy: "state",
        subGroupBy: "none",
        displayColumns: ["state", "priority", "assignee", "labels", "created_at"],
        layout: "list",
      } as Omit<TIssueView, "id" | "createdAt" | "updatedAt">);
    }
    viewStore.closeViewModal();
  };

  return (
    <ModalCore isOpen={isOpen} handleClose={onClose} width={EModalWidth.MD}>
      <ViewForm
        title={viewStore.viewModalMode === "edit" ? "编辑视图" : "保存为视图"}
        defaultName={editView?.name ?? ""}
        defaultAccess={(editView as TIssueView & { access?: EViewAccess })?.access ?? EViewAccess.PUBLIC}
        defaultDescription={(editView as TIssueView & { description?: string })?.description ?? ""}
        onSubmit={handleSubmit}
        onCancel={onClose}
        isPending={createView.isPending || updateView.isPending}
      />
    </ModalCore>
  );
});

export { ViewModal };
