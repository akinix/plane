// FLOW: ProjectCard — card for workspace dashboard project overview grid (D-P15-09)
import { useNavigate } from "react-router";
import type { IProject } from "@plane/types";

type TProjectCardProps = {
  project: IProject;
  workspaceId: string;
};

export const ProjectCard = ({ project, workspaceId }: TProjectCardProps) => {
  const navigate = useNavigate();
  const emojiCode = project.logo_props?.emoji?.value;
  const emoji = emojiCode ? String.fromCodePoint(parseInt(emojiCode, 10)) : null;

  return (
    <button
      onClick={() =>
        navigate(`/workspaces/${workspaceId}/projects/${project.id}`)
      }
      className="flex flex-col gap-2 rounded-lg border border-custom-border-200 p-4 text-left transition-all hover:shadow-md hover:border-custom-border-300"
    >
      <div className="flex items-center gap-2">
        <span className="flex size-8 items-center justify-center rounded-md bg-custom-background-80 text-lg">
          {emoji ?? project.name.charAt(0)}
        </span>
        <div className="min-w-0 flex-1">
          <p className="truncate text-sm font-medium text-custom-text-100">
            {project.name}
          </p>
          <p className="text-xs text-custom-text-300">{project.identifier}</p>
        </div>
      </div>
      {project.description && (
        <p className="line-clamp-2 text-xs text-custom-text-300">
          {project.description}
        </p>
      )}
    </button>
  );
};
