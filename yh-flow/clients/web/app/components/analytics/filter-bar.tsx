// FLOW: FilterBar — Project filter selector + date range + refresh button (per UI-SPEC)
"use client";

import React from "react";
import { RefreshCw, X } from "lucide-react";
import { Button } from "@/lib/ui/button";
import { observer } from "mobx-react";
import { useStore } from "@/lib/store-context";
import type { IProject } from "@plane/types";

const DATE_RANGE_PRESETS = [
  { value: "yesterday", label: "昨天" },
  { value: "last-7-days", label: "最近7天" },
  { value: "last-30-days", label: "最近30天" },
  { value: "last-3-months", label: "最近3个月" },
];

interface FilterBarProps {
  projects?: IProject[];
  onRefresh?: () => void;
  isRefreshing?: boolean;
}

export const FilterBar: React.FC<FilterBarProps> = observer(function FilterBar({
  projects = [],
  onRefresh,
  isRefreshing = false,
}) {
  const { analytics } = useStore();
  const { selectedProjectIds, dateRange, setSelectedProjectIds, setDateRange } = analytics;

  const handleProjectToggle = (projectId: string) => {
    const isSelected = selectedProjectIds.includes(projectId);
    if (isSelected) {
      setSelectedProjectIds(selectedProjectIds.filter((id) => id !== projectId));
    } else {
      setSelectedProjectIds([...selectedProjectIds, projectId]);
    }
  };

  const handleRemoveProject = (projectId: string) => {
    setSelectedProjectIds(selectedProjectIds.filter((id) => id !== projectId));
  };

  const selectedProjects = projects.filter((p) => selectedProjectIds.includes(p.id));

  return (
    <div className="flex flex-wrap items-center gap-3">
      {/* Project filter dropdown */}
      <div className="relative">
        <select
          className="border-custom-border-200 bg-custom-background-90 text-custom-text-200 rounded-md border px-3 py-1.5 text-13 focus:outline-none"
          value=""
          onChange={(e) => {
            if (e.target.value) {
              handleProjectToggle(e.target.value);
            }
          }}
        >
          <option value="" disabled>
            选择项目...
          </option>
          {projects.map((project) => (
            <option key={project.id} value={project.id}>
              {project.name}
            </option>
          ))}
        </select>
      </div>

      {/* Selected project tags */}
      {selectedProjects.map((project) => (
        <span
          key={project.id}
          className="border-custom-border-200 bg-custom-background-90 text-custom-text-200 inline-flex items-center gap-1 rounded-full border px-2.5 py-0.5 text-13"
        >
          {project.name}
          <button
            onClick={() => handleRemoveProject(project.id)}
            className="text-custom-text-300 hover:text-custom-text-100"
          >
            <X className="size-3" />
          </button>
        </span>
      ))}

      {/* Date range selector */}
      <select
        className="border-custom-border-200 bg-custom-background-90 text-custom-text-200 rounded-md border px-3 py-1.5 text-13 focus:outline-none"
        value={dateRange}
        onChange={(e) => setDateRange(e.target.value)}
      >
        {DATE_RANGE_PRESETS.map((preset) => (
          <option key={preset.value} value={preset.value}>
            {preset.label}
          </option>
        ))}
      </select>

      {/* Refresh button */}
      <Button variant="neutral-primary" size="sm" onClick={onRefresh} disabled={isRefreshing} title="刷新">
        <RefreshCw className={`size-3.5 ${isRefreshing ? "animate-spin" : ""}`} />
      </Button>
    </div>
  );
});
