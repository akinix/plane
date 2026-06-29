// FLOW: Global Cmd+K command palette using cmdk (per D-P16-16)
import { useEffect, useState, useMemo } from "react";
import { useNavigate } from "react-router";
import { Command } from "cmdk";
import { Search, ListTodo, LayoutGrid } from "lucide-react";
import { MOCK_ISSUES, MOCK_PROJECTS } from "../../src/lib/mock-data";

interface CommandPaletteProps {
  workspaceId?: string | null;
}

export const CommandPalette = ({ workspaceId }: CommandPaletteProps) => {
  const [open, setOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");
  const navigate = useNavigate();

  // Listen for Cmd+K / Ctrl+K to toggle the command palette
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.metaKey || e.ctrlKey) && e.key === "k") {
        e.preventDefault();
        setOpen((prev) => !prev);
      }
    };
    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, []);

  // Resolve project identifier from project_id (e.g. "proj-1" -> "FF")
  const getProjectIdentifier = (projectId: string | null): string => {
    if (!projectId) return "";
    const project = MOCK_PROJECTS.find((p) => p.id === projectId);
    return project?.identifier ?? "";
  };

  // Filter issues by name (case-insensitive), max 10 results
  const filteredIssues = useMemo(() => {
    if (!searchQuery) return [];
    const q = searchQuery.toLowerCase();
    return MOCK_ISSUES.filter((i) => i.name.toLowerCase().includes(q)).slice(0, 10);
  }, [searchQuery]);

  // Filter projects by name (case-insensitive), max 5 results
  const filteredProjects = useMemo(() => {
    if (!searchQuery) return [];
    const q = searchQuery.toLowerCase();
    return MOCK_PROJECTS.filter((p) => p.name.toLowerCase().includes(q)).slice(0, 5);
  }, [searchQuery]);

  const wsId = workspaceId ?? "ws-1";

  return (
    <>
      <style>{`
        [cmdk-overlay] {
          background: rgba(0, 0, 0, 0.5);
          backdrop-filter: blur(4px);
          position: fixed;
          inset: 0;
          z-index: 49;
        }
        [cmdk-dialog] {
          position: fixed;
          top: 15vh;
          left: 50%;
          transform: translateX(-50%);
          z-index: 50;
          width: 100%;
          max-width: 32rem;
          outline: none;
        }
      `}</style>
      <Command.Dialog
        open={open}
        onOpenChange={setOpen}
        label="命令面板"
        className="bg-custom-background-100 shadow-2xl overflow-hidden rounded-lg"
      >
        {/* Search input with icon */}
        <div className="border-custom-border-200 flex items-center border-b px-3">
          <Search className="text-custom-text-400 size-4 flex-shrink-0" />
          <Command.Input
            placeholder="搜索 Issue 或项目..."
            className="text-sm text-custom-text-100 placeholder:text-custom-text-400 flex-1 bg-transparent px-2 py-3 outline-none"
            onValueChange={setSearchQuery}
          />
        </div>

        {/* Results list */}
        <Command.List className="max-h-80 overflow-y-auto overscroll-contain">
          {/* Empty state */}
          <Command.Empty className="text-sm text-custom-text-400 py-6 text-center">没有匹配结果</Command.Empty>

          {/* Issue results */}
          {filteredIssues.length > 0 && (
            <Command.Group heading="Issues">
              {filteredIssues.map((issue) => (
                <Command.Item
                  key={issue.id}
                  onSelect={() => {
                    setOpen(false);
                    navigate(`/workspaces/${wsId}/projects/${issue.project_id}/issues/${issue.id}`);
                  }}
                  className="text-sm text-custom-text-100 data-[selected=true]:bg-custom-background-80 hover:bg-custom-background-80 flex cursor-pointer items-center gap-2 rounded-md px-2 py-2"
                >
                  <ListTodo className="text-custom-text-400 size-4 flex-shrink-0" />
                  <span className="flex-1 truncate">{issue.name}</span>
                  <span className="text-xs text-custom-text-400 flex-shrink-0">
                    {getProjectIdentifier(issue.project_id)}-{issue.sequence_id}
                  </span>
                </Command.Item>
              ))}
            </Command.Group>
          )}

          {/* Project results */}
          {filteredProjects.length > 0 && (
            <Command.Group heading="项目">
              {filteredProjects.map((project) => (
                <Command.Item
                  key={project.id}
                  onSelect={() => {
                    setOpen(false);
                    navigate(`/workspaces/${wsId}/projects/${project.id}`);
                  }}
                  className="text-sm text-custom-text-100 data-[selected=true]:bg-custom-background-80 hover:bg-custom-background-80 flex cursor-pointer items-center gap-2 rounded-md px-2 py-2"
                >
                  <LayoutGrid className="text-custom-text-400 size-4 flex-shrink-0" />
                  <span className="flex-1 truncate">{project.name}</span>
                </Command.Item>
              ))}
            </Command.Group>
          )}
        </Command.List>
      </Command.Dialog>
    </>
  );
};
