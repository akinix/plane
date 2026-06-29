// FLOW: ProjectCard — card for project list page (PROJ-02)
import { useNavigate } from "react-router";
import type { IProject } from "@plane/types";

type TProps = { project: IProject; workspaceId: string };

export const ProjectCard = ({ project, workspaceId }: TProps) => {
  const navigate = useNavigate();
  const emojiCode = project.logo_props?.emoji?.value;
  const emoji = emojiCode ? String.fromCodePoint(parseInt(emojiCode, 10)) : null;

  return (
    <button
      onClick={() =>
        navigate(`/workspaces/${workspaceId}/projects/${project.id}`)
      }
      className="flex items-center gap-3 rounded-lg border border-custom-border-200 p-3 text-left transition-colors hover:bg-custom-background-80 cursor-pointer"
    >
      <span className="flex size-8 items-center justify-center rounded-md bg-custom-background-80 text-lg">
        {emoji ?? project.name.charAt(0)}
      </span>
      <div className="min-w-0 flex-1">
        <p className="truncate text-sm font-medium text-custom-text-100">
          {project.name}
        </p>
        <p className="text-xs text-custom-text-300">{project.identifier}</p>
      </div>
    </button>
  );
};
