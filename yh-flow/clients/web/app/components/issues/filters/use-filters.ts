// FLOW: useFilters — TanStack Query hook syncing filter criteria to URL query params (per D-P17-11)
import { useCallback, useMemo } from "react";
import { useSearchParams } from "react-router-dom";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import type { TFilterCriteria } from "./types";

const DEFAULT_FILTERS: TFilterCriteria = {
  stateIds: [],
  priorityIds: [],
  assigneeIds: [],
  labelIds: [],
  searchQuery: "",
  dateRange: null,
};

function parseFiltersFromParams(searchParams: URLSearchParams): TFilterCriteria {
  const stateParam = searchParams.get("state");
  const priorityParam = searchParams.get("priority");
  const assigneeParam = searchParams.get("assignee");
  const labelsParam = searchParams.get("labels");
  const searchParam = searchParams.get("search");

  return {
    stateIds: stateParam ? stateParam.split(",").filter(Boolean) : [],
    priorityIds: priorityParam ? priorityParam.split(",").filter(Boolean) : [],
    assigneeIds: assigneeParam ? assigneeParam.split(",").filter(Boolean) : [],
    labelIds: labelsParam ? labelsParam.split(",").filter(Boolean) : [],
    searchQuery: searchParam ?? "",
    dateRange: null,
  };
}

function serializeFiltersToParams(filters: TFilterCriteria, searchParams: URLSearchParams): URLSearchParams {
  const next = new URLSearchParams(searchParams);

  if (filters.stateIds.length > 0) {
    next.set("state", filters.stateIds.join(","));
  } else {
    next.delete("state");
  }

  if (filters.priorityIds.length > 0) {
    next.set("priority", filters.priorityIds.join(","));
  } else {
    next.delete("priority");
  }

  if (filters.assigneeIds.length > 0) {
    next.set("assignee", filters.assigneeIds.join(","));
  } else {
    next.delete("assignee");
  }

  if (filters.labelIds.length > 0) {
    next.set("labels", filters.labelIds.join(","));
  } else {
    next.delete("labels");
  }

  if (filters.searchQuery) {
    next.set("search", filters.searchQuery);
  } else {
    next.delete("search");
  }

  return next;
}

type UseFiltersReturn = {
  filters: TFilterCriteria;
  setFilters: (filters: Partial<TFilterCriteria>) => void;
  clearFilters: () => void;
  hasActiveFilters: boolean;
  isLoading: boolean;
};

export function useFilters(
  projectId: string,
  onFilterChange?: (filters: TFilterCriteria) => void
): UseFiltersReturn {
  const [searchParams, setSearchParams] = useSearchParams();
  const queryClient = useQueryClient();

  const queryKey = useMemo(() => ["issue-filters", projectId] as const, [projectId]);

  // Read initial filters from URL params
  const initialFilters = useMemo(() => parseFiltersFromParams(searchParams), [searchParams]);

  const { data: filters = initialFilters, isLoading } = useQuery<TFilterCriteria>({
    queryKey,
    queryFn: async () => initialFilters,
    staleTime: Infinity,
    enabled: !!projectId,
  });

  const hasActiveFilters = useMemo(() => {
    return (
      filters.stateIds.length > 0 ||
      filters.priorityIds.length > 0 ||
      filters.assigneeIds.length > 0 ||
      filters.labelIds.length > 0 ||
      filters.searchQuery.length > 0 ||
      filters.dateRange !== null
    );
  }, [filters]);

  const setFilters = useCallback(
    (partial: Partial<TFilterCriteria>) => {
      const merged: TFilterCriteria = { ...filters, ...partial };
      queryClient.setQueryData(queryKey, merged);

      // Sync to URL params
      setSearchParams(
        (prev) => {
          const next = serializeFiltersToParams(merged, prev);
          return next;
        },
        { replace: true }
      );

      // Bridge: sync to parent (e.g. MobX store) when provided
      onFilterChange?.(merged);
    },
    [filters, queryClient, queryKey, setSearchParams, onFilterChange]
  );

  const clearFilters = useCallback(() => {
    queryClient.setQueryData(queryKey, DEFAULT_FILTERS);

    setSearchParams(
      (prev) => {
        const next = new URLSearchParams(prev);
        next.delete("state");
        next.delete("priority");
        next.delete("assignee");
        next.delete("labels");
        next.delete("search");
        return next;
      },
      { replace: true }
    );

    // Bridge: sync cleared state
    onFilterChange?.(DEFAULT_FILTERS);
  }, [queryClient, queryKey, setSearchParams, onFilterChange]);

  return { filters, setFilters, clearFilters, hasActiveFilters, isLoading };
}
