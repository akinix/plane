// FLOW: FlowApiService base class with JWT Bearer auth, humps camelCase conversion,
// request body snake_case transformation (INFRA-01), Token refresh queue (INFRA-02),
// and standardized error handling (INFRA-04)

import axios, { type AxiosInstance, type AxiosRequestConfig, type AxiosResponse } from "axios";
import humps from "humps";
import { standardizeApiError } from "@plane/types";

// Token management (D-11)
const TOKEN_KEY = "flow_access_token";
export const getToken = (): string | null => localStorage.getItem(TOKEN_KEY);
export const setToken = (token: string): void => localStorage.setItem(TOKEN_KEY, token);
export const removeToken = (): void => localStorage.removeItem(TOKEN_KEY);

// Token refresh queue state (INFRA-02)
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (value?: unknown) => void;
  reject: (reason?: unknown) => void;
}> = [];
let refreshPromise: Promise<void> | null = null;

// Refresh handler injection point (avoids circular dependency with AuthService)
let refreshHandler: (() => Promise<void>) | null = null;

/**
 * Register a function that handles token refresh.
 * Called by AuthService in its constructor to avoid circular imports.
 */
export function registerRefreshHandler(handler: () => Promise<void>): void {
  refreshHandler = handler;
}

function processQueue(error: unknown, _token: string | null = null): void {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(_token);
    }
  });
  failedQueue = [];
}

// FLOW: Standardized error response type (D-13) — kept for backward compatibility
export interface ErrorResponse {
  error: string;
}

export abstract class FlowApiService {
  protected baseURL: string;
  protected axiosInstance: AxiosInstance;

  constructor(baseURL: string) {
    this.baseURL = baseURL;
    this.axiosInstance = axios.create({ baseURL }); // eslint-disable-line import/no-named-as-default-member

    // FLOW: Request interceptor — JWT Bearer token (D-11) + camelCase→snake_case body (INFRA-01)
    this.axiosInstance.interceptors.request.use((config) => {
      const token = getToken();
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }

      // INFRA-01: camelCase → snake_case request body for POST/PUT/PATCH
      if (
        config.data &&
        typeof config.data === "object" &&
        !(config.data instanceof FormData) &&
        !(config.data instanceof URLSearchParams)
      ) {
        config.data = humps.decamelizeKeys(config.data, { separator: "_" });
      }
      // Query string parameters also get converted
      if (config.params && typeof config.params === "object") {
        config.params = humps.decamelizeKeys(config.params, { separator: "_" });
      }

      return config;
    });

    // FLOW: Response interceptor — snake_case to camelCase (D-12) +
    //       Token refresh queue (INFRA-02) + error standardization (INFRA-04)
    this.axiosInstance.interceptors.response.use(
      (response) => {
        if (response.data && typeof response.data === "object") {
          response.data = humps.camelizeKeys(response.data);
        }
        return response;
      },
      (error) => {
        const originalRequest = error.config;

        // INFRA-04: Error standardization — transform response data into StandardizedApiError
        if (error?.response?.data) {
          error.response.data = standardizeApiError(error.response.data);
        }

        // INFRA-02: 401 Token refresh queue — concurrent-safe, single refresh
        if (error?.response?.status === 401 && !originalRequest?._retry) {
          // Prevent infinite loop on refresh endpoint itself
          if (originalRequest?.url?.includes("/auth/refresh/")) {
            removeToken();
            window.location.href = "/auth/sign-in";
            return Promise.reject(error);
          }

          if (isRefreshing && refreshPromise) {
            // Another request is already refreshing — queue this one
            return new Promise<unknown>((resolve, reject) => {
              failedQueue.push({
                resolve: () => {
                  originalRequest.headers.Authorization = `Bearer ${getToken()}`;
                  resolve(this.axiosInstance(originalRequest));
                },
                reject,
              });
            });
          }

          // Guard: check refresh handler early before mutating any state
          if (!refreshHandler) {
            // No refresh handler registered — should not happen in normal flow
            removeToken();
            window.location.href = "/auth/sign-in";
            return Promise.reject(error);
          }

          // This request initiates the refresh — set isRefreshing and refreshPromise atomically
          originalRequest._retry = true;
          isRefreshing = true;
          refreshPromise = (async (): Promise<void> => {
            try {
              await refreshHandler!();
              isRefreshing = false;
              processQueue(null, getToken());
            } catch (refreshError: unknown) {
              isRefreshing = false;
              processQueue(refreshError, null);
              removeToken();
              window.location.href = "/auth/sign-in";
              throw refreshError;
            }
          })();

          return new Promise<unknown>((resolve, reject) => {
            failedQueue.push({
              resolve: () => {
                originalRequest.headers.Authorization = `Bearer ${getToken()}`;
                resolve(this.axiosInstance(originalRequest));
              },
              reject,
            });
          });
        }

        return Promise.reject(error);
      }
    );
  }

  // Methods following Plane APIService signature convention (per STRUCTURE.md)
  get(url: string, params = {}, config: AxiosRequestConfig = {}) {
    return this.axiosInstance.get(url, { ...params, ...config });
  }

  post(url: string, data = {}, config: AxiosRequestConfig = {}) {
    return this.axiosInstance.post(url, data, config);
  }

  put(url: string, data = {}, config: AxiosRequestConfig = {}) {
    return this.axiosInstance.put(url, data, config);
  }

  patch(url: string, data = {}, config: AxiosRequestConfig = {}) {
    return this.axiosInstance.patch(url, data, config);
  }

  delete(url: string, data?: any, config: AxiosRequestConfig = {}) {
    return this.axiosInstance.delete(url, { data, ...config });
  }

  request(config = {}) {
    return this.axiosInstance(config);
  }

  // INFRA-05: === Pagination extraction layer ===

  /**
   * Unwrap paginated response, extracting the `results` array.
   * Falls back to returning `data` directly if results is absent or already an array.
   * Returns an empty array if data is neither a paginated response with results nor an array.
   */
  protected unwrapPaginated<T>(response: AxiosResponse): T[] {
    const data = response.data;
    if (data && Array.isArray(data.results)) {
      return data.results;
    }
    if (Array.isArray(data)) return data;
    return [];
  }

  /**
   * GET request returning a paginated list — automatically unwraps results[].
   * Type: `Promise<T[]>` — hooks receive a plain array.
   */
  async getList<T>(
    url: string,
    params: Record<string, unknown> = {},
    config: AxiosRequestConfig = {}
  ): Promise<T[]> {
    // Merge config.params into explicit params — explicit params take precedence
    const mergedConfig: AxiosRequestConfig = {
      ...config,
      params: { ...(config.params as Record<string, unknown> || {}), ...params },
    };
    const response = await this.get(url, mergedConfig);
    return this.unwrapPaginated<T>(response);
  }

  /**
   * GET request returning a single object — returns response.data as T.
   */
  async getOne<T>(
    url: string,
    config: AxiosRequestConfig = {}
  ): Promise<T> {
    const response = await this.get(url, config);
    return response.data as T;
  }

  /**
   * POST request returning a paginated list (for complex filter body queries).
   */
  async postList<T>(
    url: string,
    data: Record<string, unknown> = {},
    config: AxiosRequestConfig = {}
  ): Promise<T[]> {
    const response = await this.post(url, data, config);
    return this.unwrapPaginated<T>(response);
  }

  /**
   * POST request returning a single object (for create operations).
   */
  async postOne<T>(
    url: string,
    data: Record<string, unknown> = {},
    config: AxiosRequestConfig = {}
  ): Promise<T> {
    const response = await this.post(url, data, config);
    return response.data as T;
  }

  /**
   * PATCH request returning a single object.
   */
  async patchOne<T>(
    url: string,
    data: Record<string, unknown> = {},
    config: AxiosRequestConfig = {}
  ): Promise<T> {
    const response = await this.patch(url, data, config);
    return response.data as T;
  }

  /**
   * PUT request returning a single object.
   */
  async putOne<T>(
    url: string,
    data: Record<string, unknown> = {},
    config: AxiosRequestConfig = {}
  ): Promise<T> {
    const response = await this.put(url, data, config);
    return response.data as T;
  }
}
