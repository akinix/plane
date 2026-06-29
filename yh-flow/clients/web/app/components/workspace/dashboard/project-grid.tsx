// FLOW: ProjectGrid — responsive grid of project cards for workspace dashboard (D-P15-09)
import { useProjects } from "../../../../src/lib/hooks/use-projects";
import { ProjectCard } from "./project-card";
import { useNavigate } from "react-router";

type TProjectGridProps = {
  workspaceId: string;
};

export const ProjectGrid = ({ workspaceId }: TProjectGridProps) => {
  const navigate = useNavigate();
  const { data: projects, isLoading } = useProjects(workspaceId);

  if (isLoading) {
    return (
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
        {Array.from({ length: 3 }).map((_, i) => (
          <div
            key={i}
            className="h-24 animate-pulse rounded-lg bg-custom-background-80"
          />
        ))}
      </div>
    );
  }

  if (!projects || projects.length === 0) {
    return (
      <div className="flex flex-col items-center gap-3 py-8 text-center">
        <p className="text-sm text-custom-text-300">暂无项目</p>
        <button
          onClick={() => navigate("?createProject=true")}
          className="rounded-md bg-custom-primary px-3 py-1.5 text-xs font-medium text-white hover:bg-custom-primary-200"
        >
          创建项目
        </button>
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
      {projects.map((project) => (
        <ProjectCard
          key={project.id}
          project={project}
          workspaceId={workspaceId}
        />
      ))}
    </div>
  );
};
