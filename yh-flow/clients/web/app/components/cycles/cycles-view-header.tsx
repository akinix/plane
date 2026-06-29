// FLOW: Forked from Plane cycles/cycles-view-header.tsx
// FLOW: CyclesViewHeader — 页面标题 + 创建周期按钮 + 视图切换 + Tab 切换
import { useRef, useState } from "react";
import { observer } from "mobx-react";
import { Search, X, LayoutList, LayoutGrid } from "lucide-react";
import { useStore } from "@/lib/store-context";

type Props = {
  projectId: string;
};

const TABS: { key: "active" | "completed" | "all"; label: string }[] = [
  { key: "active", label: "活跃周期" },
  { key: "completed", label: "已完成周期" },
  { key: "all", label: "全部周期" },
];

export const CyclesViewHeader = observer(function CyclesViewHeader(props: Props) {
  const { projectId } = props;
  const inputRef = useRef<HTMLInputElement>(null);
  const [isSearchOpen, setIsSearchOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");

  // Access CycleStore via root store
  const { cycle } = useStore();

  const handleInputKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Escape") {
      if (searchQuery && searchQuery.trim() !== "") setSearchQuery("");
      else {
        setIsSearchOpen(false);
        inputRef.current?.blur();
      }
    }
  };

  return (
    <div className="flex flex-col">
      {/* Title row */}
      <div className="flex items-center justify-between gap-2 px-4 py-2.5">
        <div className="flex items-center gap-3">
          <h2 className="text-18 font-semibold text-primary">周期</h2>
          <button
            className="flex items-center gap-1.5 rounded-md bg-accent-primary px-3 py-1.5 text-13 font-medium text-white hover:bg-accent-primary/90"
            onClick={() => cycle.openCycleModal("create")}
          >
            + 创建周期
          </button>
        </div>

        <div className="flex items-center gap-2">
          {/* Search */}
          {!isSearchOpen ? (
            <button
              className="flex items-center justify-center rounded-md p-1.5 text-tertiary hover:bg-surface-2"
              onClick={() => {
                setIsSearchOpen(true);
                setTimeout(() => inputRef.current?.focus(), 50);
              }}
            >
              <Search className="h-3.5 w-3.5" />
            </button>
          ) : (
            <div className="flex w-64 items-center gap-1.5 rounded-md border border-subtle bg-surface-1 px-2.5 py-1.5">
              <Search className="h-3.5 w-3.5 text-placeholder" />
              <input
                ref={inputRef}
                className="w-full border-none bg-transparent text-13 text-primary outline-none placeholder:text-placeholder"
                placeholder="搜索周期..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                onKeyDown={handleInputKeyDown}
              />
              {isSearchOpen && (
                <button
                  type="button"
                  className="grid place-items-center"
                  onClick={() => {
                    setSearchQuery("");
                    setIsSearchOpen(false);
                  }}
                >
                  <X className="h-3 w-3" />
                </button>
              )}
            </div>
          )}

          {/* View toggle */}
          <div className="flex items-center rounded-md border border-subtle">
            <button
              className={`rounded-l-md p-1.5 ${cycle.viewLayout === "list" ? "bg-surface-2 text-primary" : "text-tertiary hover:bg-surface-1"}`}
              onClick={() => cycle.setViewLayout("list")}
            >
              <LayoutList className="h-3.5 w-3.5" />
            </button>
            <button
              className={`rounded-r-md p-1.5 ${cycle.viewLayout === "board" ? "bg-surface-2 text-primary" : "text-tertiary hover:bg-surface-1"}`}
              onClick={() => cycle.setViewLayout("board")}
            >
              <LayoutGrid className="h-3.5 w-3.5" />
            </button>
          </div>
        </div>
      </div>

      {/* Tabs row */}
      <div className="flex items-center gap-0 border-b border-subtle px-4">
        {TABS.map((tab) => (
          <button
            key={tab.key}
            className={`px-4 py-2 text-13 font-medium transition-colors ${
              cycle.activeTab === tab.key
                ? "border-b-2 border-accent-primary text-primary"
                : "text-tertiary hover:text-primary"
            }`}
            onClick={() => cycle.setActiveTab(tab.key)}
          >
            {tab.label}
          </button>
        ))}
      </div>
    </div>
  );
});
