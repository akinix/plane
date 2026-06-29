// FLOW: Forked from Plane modules/modules-list-view.tsx
// FLOW: ModulesListView — Module 列表页主容器（卡片/列表双视图 + 空/加载/错误状态）
import { useModules } from "@/../src/lib/hooks/use-modules";
import { useStore } from "@/lib/store-context";
import { Loader } from "@plane/ui";
import { ModuleCardItem } from "./module-card-item";
import { ModuleListItem } from "./module-list-item";
import { ModuleViewHeader } from "./module-view-header";
import { DeleteModuleModal } from "./delete-module-modal";

type Props = {
  workspaceId: string;
  projectId: string;
};

export const ModulesListView = function ModulesListView({ workspaceId, projectId }: Props) {
  const { data: modules, isLoading, isError, refetch } = useModules(projectId);
  const { module: moduleStore } = useStore();
  const activeView = (moduleStore as any)?.activeView ?? "board";
  const deleteModuleId = (moduleStore as any)?.deleteModuleId;
  const deleteModuleName = (moduleStore as any)?.deleteModuleName;

  // 加载状态
  if (isLoading) {
    return (
      <div className="flex flex-col">
        <ModuleViewHeader projectId={projectId} />
        <div className="grid grid-cols-1 gap-4 px-4 py-4 lg:grid-cols-2 xl:grid-cols-3">
          {[1, 2, 3].map((i) => (
            <div key={i} className="h-40 rounded-lg border border-subtle bg-surface-1 animate-pulse" />
          ))}
        </div>
      </div>
    );
  }

  // 错误状态
  if (isError) {
    return (
      <div className="flex flex-col">
        <ModuleViewHeader projectId={projectId} />
        <div className="flex flex-col items-center justify-center py-20">
          <p className="text-14 text-tertiary">加载模块列表失败</p>
          <button
            onClick={() => refetch()}
            className="mt-2 rounded-md bg-accent-primary px-3 py-1.5 text-13 text-white"
          >
            重试
          </button>
        </div>
      </div>
    );
  }

  // 空状态（无 Module）
  if (!modules || modules.length === 0) {
    return (
      <div className="flex flex-col">
        <ModuleViewHeader projectId={projectId} />
        <div className="flex flex-col items-center justify-center py-20">
          <h3 className="text-16 font-medium text-primary">暂无模块</h3>
          <p className="mt-1 text-13 text-tertiary">
            创建第一个模块来组织你的功能分组。
          </p>
          <button
            className="mt-4 rounded-md bg-accent-primary px-4 py-2 text-13 text-white"
            onClick={() => moduleStore.openModuleModal("create")}
          >
            创建模块
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="flex flex-col">
      <ModuleViewHeader projectId={projectId} />

      {/* 卡片视图 */}
      {activeView === "board" && (
        <div className="grid grid-cols-1 gap-4 px-4 py-4 lg:grid-cols-2 xl:grid-cols-3 3xl:grid-cols-4">
          {modules.map((mod) => (
            <ModuleCardItem
              key={mod.id}
              moduleId={mod.id}
              projectId={projectId}
              workspaceId={workspaceId}
            />
          ))}
        </div>
      )}

      {/* 列表视图 */}
      {activeView === "list" && (
        <div className="px-4 py-4">
          {modules.map((mod) => (
            <ModuleListItem
              key={mod.id}
              moduleId={mod.id}
              projectId={projectId}
              workspaceId={workspaceId}
            />
          ))}
        </div>
      )}

      {/* 删除确认弹窗 */}
      {deleteModuleId && (
        <DeleteModuleModal
          moduleId={deleteModuleId}
          moduleName={deleteModuleName ?? ""}
          isOpen={!!deleteModuleId}
          onClose={() => moduleStore.closeDeleteModal()}
        />
      )}
    </div>
  );
};
