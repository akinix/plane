// FLOW: Paginated response type compatible with Plane's TPaginatedResponse format (SCAFF-09)
// .NET API returns snake_case, humps.camelizeKeys converts to camelCase automatically
export interface PlanePagedResult<T> {
  results: T;
  count?: number;
  totalResults?: number;
  totalPages?: number;
  nextCursor?: string;
  prevCursor?: string;
  nextPageResults?: boolean;
  prevPageResults?: boolean;
  extraStats?: string | null;
}
