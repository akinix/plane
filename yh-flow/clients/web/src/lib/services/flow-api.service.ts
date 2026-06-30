// FLOW: FlowApiService base class with JWT Bearer auth, humps camelCase conversion,
// request body snake_case transformation (INFRA-01), Token refresh queue (INFRA-02),
// and standardized error handling (INFRA-04)

import axios, { type AxiosInstance, type AxiosRequestConfig } from "axios";
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

          // This request initiates the refresh
          originalRequest._retry = true;
          isRefreshing = true;

          if (!refreshHandler) {
            // No refresh handler registered — should not happen in normal flow
            removeToken();
            window.location.href = "/auth/sign-in";
            return Promise.reject(error);
          }

          const doRefresh = async (): Promise<void> => {
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
          };
          refreshPromise = doRefresh();

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
}
