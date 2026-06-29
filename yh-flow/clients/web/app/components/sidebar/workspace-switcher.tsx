// FLOW: WorkspaceSwitcher — dropdown at top of sidebar to switch workspaces (D-P15-03, WORK-02)
import { observer } from "mobx-react";
import { useState, useRef, useEffect } from "react";
import { useNavigate } from "react-router";
import { ChevronDown, Plus } from "lucide-react";
import { useStore } from "@/lib/store-context";
import { useWorkspaces } from "../../../src/lib/hooks/use-workspaces";
import { cn } from "@plane/utils";

export const WorkspaceSwitcher = observer(function WorkspaceSwitcher() {
  const navigate = useNavigate();
  const { workspace: workspaceStore } = useStore();
  const { data: workspaces } = useWorkspaces();
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClick = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        setOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClick);
    return () => document.removeEventListener("mousedown", handleClick);
  }, []);

  const currentWorkspace = workspaces?.find(
    (ws) => ws.id === workspaceStore.currentWorkspaceId,
  );

  const handleSelect = (wsId: string) => {
    workspaceStore.setCurrentWorkspace(wsId);
    workspaceStore.setWorkspaceSwitcherOpen(false);
    setOpen(false);
    navigate(`/workspaces/${wsId}`);
  };

  return (
    <div ref={ref} className="relative">
      <button
        onClick={() => setOpen(!open)}
        className="flex w-full items-center justify-between rounded-md px-2 py-2 text-sm font-medium text-custom-sidebar-text-100 transition-colors hover:bg-custom-sidebar-background-80"
      >
        <span className="truncate">
          {currentWorkspace?.name ?? "选择工作区"}
        </span>
        <ChevronDown
          className={cn("size-3.5 flex-shrink-0 transition-transform", {
            "rotate-180": open,
          })}
        />
      </button>

      {open && (
        <div className="absolute left-0 right-0 z-50 mt-1 rounded-md border border-custom-border-200 bg-custom-sidebar-background-100 shadow-lg">
          <div className="py-1">
            {workspaces?.map((ws) => (
              <button
                key={ws.id}
                onClick={() => handleSelect(ws.id)}
                className={cn(
                  "flex w-full items-center gap-2 px-3 py-1.5 text-sm transition-colors",
                  {
                    "bg-custom-sidebar-background-80 text-custom-sidebar-text-100":
                      ws.id === workspaceStore.currentWorkspaceId,
                    "text-custom-sidebar-text-200 hover:bg-custom-sidebar-background-80":
                      ws.id !== workspaceStore.currentWorkspaceId,
                  },
                )}
              >
                <span className="flex size-5 items-center justify-center rounded bg-custom-sidebar-background-80 text-xs font-medium">
                  {ws.name.charAt(0)}
                </span>
                <span className="truncate">{ws.name}</span>
                {ws.id === workspaceStore.currentWorkspaceId && (
                  <span className="ml-auto size-1.5 rounded-full bg-custom-primary" />
                )}
              </button>
            ))}
          </div>
          <div className="border-t border-custom-border-200">
            <button
              onClick={() => {
                setOpen(false);
                navigate("/workspaces/create");
              }}
              className="flex w-full items-center gap-2 px-3 py-2 text-sm text-custom-sidebar-text-200 transition-colors hover:bg-custom-sidebar-background-80"
            >
              <Plus className="size-3.5" />
              <span>创建工作区</span>
            </button>
          </div>
        </div>
      )}
    </div>
  );
});
