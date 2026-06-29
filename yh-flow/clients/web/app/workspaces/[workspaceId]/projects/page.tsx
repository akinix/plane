// FLOW: Project List page — searchable project list with create modal (PROJ-01, PROJ-02)
import { useState } from "react";
import { useParams } from "react-router";
import { Plus } from "lucide-react";
import { ProjectList } from "../../../components/project/project-list";
import { CreateProjectModal } from "../../../components/project/create-project-modal";

export default function ProjectListPage() {
  const { workspaceId } = useParams<{ workspaceId: string }>();
  const [modalOpen, setModalOpen] = useState(false);

  return (
    <div className="mx-auto flex w-full max-w-4xl flex-col gap-6 p-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-custom-text-100">
          项目列表
        </h1>
        <button
          onClick={() => setModalOpen(true)}
          className="flex items-center gap-1.5 rounded-md bg-custom-primary px-3 py-1.5 text-sm font-medium text-white transition-colors hover:bg-custom-primary-200"
        >
          <Plus className="size-4" />
          <span>创建项目</span>
        </button>
      </div>

      {/* List */}
      <ProjectList workspaceId={workspaceId ?? ""} />

      {/* Modal */}
      <CreateProjectModal
        isOpen={modalOpen}
        onClose={() => setModalOpen(false)}
        workspaceId={workspaceId ?? ""}
      />
    </div>
  );
}
