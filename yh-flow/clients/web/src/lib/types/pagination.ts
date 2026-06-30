// FLOW: Forked from @plane/types/src/types/pagination.ts
/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

// Generic paginated response type for API responses
export type TPaginatedResponse<T> = {
  results: T;
  grouped_by?: string | null;
  sub_grouped_by?: string | null;
  total_count?: number;
  next_cursor?: string;
  prev_cursor?: string;
  next_page_results?: boolean;
  prev_page_results?: boolean;
  count?: number;
  total_pages?: number;
  total_results?: number;
  extra_stats?: string | null;
};

// Paginated response where results is always an array
export type TPaginatedListResponse<T> = TPaginatedResponse<T[]>;

// Utility type: extracts T from TPaginatedResponse<T[]>, returns T unchanged otherwise
export type UnwrapPaginatedResult<T> = T extends TPaginatedResponse<infer R> ? R : T;

// Cursor pagination parameter type for paginated requests
export type TPaginationCursor = {
  cursor?: string;
  per_page?: number;
};
