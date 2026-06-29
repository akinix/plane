// FLOW: ProjectList — searchable and sortable project list (PROJ-02)
import { useMemo, useState } from "react";
import { Search, ArrowUpDown } from "lucide-react";
import { useProjects } from "../../../src/lib/hooks/use-projects";
import { ProjectCard } from "./project-card";

type TProps = { workspaceId: string };

export const ProjectList = ({ workspaceId }: TProps) => {
  const { data: projects, isLoading } = useProjects(workspaceId);
  const [search, setSearch] = useState("");
  const [sort, setSort] = useState<"name-asc" | "name-desc" | "created">("name-asc");

  const filtered = useMemo(() => {
    if (!projects) return [];
    let list = [...projects];
    if (search) {
      const q = search.toLowerCase();
      list = list.filter((p) => p.name.toLowerCase().includes(q));
    }
    if (sort === "name-asc") list.sort((a, b) => a.name.localeCompare(b.name));
    else if (sort === "name-desc") list.sort((a, b) => b.name.localeCompare(a.name));
    else if (sort === "created")
      list.sort((a, b) => {
        const aT = a.created_at ? new Date(a.created_at).getTime() : 0;
        const bT = b.created_at ? new Date(b.created_at).getTime() : 0;
        return bT - aT;
      });
    return list;
  }, [projects, search, sort]);

  if (isLoading) {
    return (
      <div className="flex flex-col gap-2">
        {[1, 2, 3].map((i) => (
          <div key={i} className="h-16 animate-pulse rounded-lg bg-custom-background-80" />
        ))}
      </div>
    );
  }

  if (!projects || projects.length === 0) {
    return (
      <p className="py-8 text-center text-sm text-custom-text-300">暂无项目</p>
    );
  }

  return (
    <div className="flex flex-col gap-4">
      {/* Search + sort */}
      <div className="flex items-center gap-2">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 size-3.5 -translate-y-1/2 text-custom-text-400" />
          <input
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="搜索项目..."
            className="w-full rounded-md border border-custom-border-200 bg-custom-background-100 py-2 pl-8 pr-3 text-sm text-custom-text-100 outline-none focus:border-custom-primary"
          />
        </div>
        <select
          value={sort}
          onChange={(e) => setSort(e.target.value as typeof sort)}
          className="rounded-md border border-custom-border-200 bg-custom-background-100 px-2 py-2 text-xs text-custom-text-200 outline-none"
        >
          <option value="name-asc">名称 A-Z</option>
          <option value="name-desc">名称 Z-A</option>
          <option value="created">最新创建</option>
        </select>
      </div>

      {/* Results */}
      {filtered.length === 0 ? (
        <p className="py-8 text-center text-sm text-custom-text-300">
          未找到匹配的项目
        </p>
      ) : (
        <div className="flex flex-col gap-2">
          {filtered.map((p) => (
            <ProjectCard key={p.id} project={p} workspaceId={workspaceId} />
          ))}
        </div>
      )}
    </div>
  );
};
