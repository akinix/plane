// FLOW: useSorting — sort config hook synced to URL query params (FILT-02)
import { useCallback, useMemo } from "react";
import { useSearchParams } from "react-router-dom";
import type { TSortConfig, TSortConfig as TSort } from "./types";
import type { TIssue } from "@plane/types";

export function useSorting(): {
  sortConfig: TSort;
  setSortBy: (field: string) => void;
  setSortDirection: (dir: "asc" | "desc") => void;
  sortedIssues: (issues: TIssue[]) => TIssue[];
} {
  const [searchParams, setSearchParams] = useSearchParams();

  const sortConfig = useMemo<TSortConfig>(
    () => ({
      sortBy: searchParams.get("sort_by") ?? "updated_at",
      sortDirection: (searchParams.get("sort_direction") as "asc" | "desc") ?? "desc",
    }),
    [searchParams]
  );

  const updateSortParam = useCallback(
    (key: string, value: string) => {
      setSearchParams(
        (prev) => {
          const next = new URLSearchParams(prev);
          next.set(key, value);
          return next;
        },
        { replace: true }
      );
    },
    [setSearchParams]
  );

  const setSortBy = useCallback(
    (field: string) => {
      if (field === sortConfig.sortBy) {
        updateSortParam("sort_direction", sortConfig.sortDirection === "asc" ? "desc" : "asc");
      } else {
        updateSortParam("sort_by", field);
        updateSortParam("sort_direction", "desc");
      }
    },
    [sortConfig, updateSortParam]
  );

  const setSortDirection = useCallback(
    (dir: "asc" | "desc") => {
      updateSortParam("sort_direction", dir);
    },
    [updateSortParam]
  );

  const sortedIssues = useCallback(
    (issues: TIssue[]): TIssue[] => {
      if (!issues || issues.length === 0) return [];
      const field = sortConfig.sortBy as keyof TIssue;
      const dir = sortConfig.sortDirection === "asc" ? 1 : -1;

      return [...issues].toSorted((a, b) => {
        const aVal = a[field];
        const bVal = b[field];
        if (aVal == null && bVal == null) return 0;
        if (aVal == null) return 1;
        if (bVal == null) return -1;
        if (typeof aVal === "string" && typeof bVal === "string") return aVal.localeCompare(bVal) * dir;
        if (typeof aVal === "number" && typeof bVal === "number") return (aVal - bVal) * dir;
        return 0;
      });
    },
    [sortConfig]
  );

  return { sortConfig, setSortBy, setSortDirection, sortedIssues };
}
