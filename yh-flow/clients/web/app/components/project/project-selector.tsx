// FLOW: ProjectSelector — project dropdown for quick navigation between projects (PROJ-05)
import { useState, useRef, useEffect } from "react";
import { useNavigate } from "react-router";
import { ChevronDown } from "lucide-react";
import { useProjects } from "../../../src/lib/hooks/use-projects";
import { cn } from "@plane/utils";

type TProps = { workspaceId: string; currentProjectId: string };

export const ProjectSelector = ({
  workspaceId,
  currentProjectId,
}: TProps) => {
  const navigate = useNavigate();
  const { data: projects } = useProjects(workspaceId);
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClick = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener("mousedown", handleClick);
    return () => document.removeEventListener("mousedown", handleClick);
  }, []);

  const current = projects?.find((p) => p.id === currentProjectId);

  return (
    <div ref={ref} className="relative">
      <button
        onClick={() => setOpen(!open)}
        className="flex items-center gap-2 rounded-md px-2 py-1 text-sm font-medium text-custom-text-100 hover:bg-custom-background-80 transition-colors"
      >
        <span className="truncate max-w-32">
          {current?.name ?? "选择项目"}
        </span>
        <ChevronDown className="size-3.5" />
      </button>
      {open && (
        <div className="absolute left-0 z-50 mt-1 w-56 rounded-md border border-custom-border-200 bg-custom-background-100 shadow-lg">
          {projects?.map((p) => {
            const emojiCode = p.logo_props?.emoji?.value;
            const emoji = emojiCode
              ? String.fromCodePoint(parseInt(emojiCode, 10))
              : null;
            return (
              <button
                key={p.id}
                onClick={() => {
                  setOpen(false);
                  navigate(
                    `/workspaces/${workspaceId}/projects/${p.id}`,
                  );
                }}
                className={cn(
                  "flex w-full items-center gap-2 px-3 py-1.5 text-sm transition-colors",
                  p.id === currentProjectId
                    ? "bg-custom-background-80 text-custom-text-100"
                    : "text-custom-text-200 hover:bg-custom-background-80",
                )}
              >
                <span className="text-base">{emoji ?? "📁"}</span>
                <span className="truncate">{p.name}</span>
                <span className="ml-auto text-xs text-custom-text-400">
                  {p.identifier}
                </span>
              </button>
            );
          })}
        </div>
      )}
    </div>
  );
};
