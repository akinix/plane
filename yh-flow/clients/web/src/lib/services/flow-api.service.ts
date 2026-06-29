// FLOW: FlowApiService base class with JWT Bearer auth, humps camelCase conversion,
// and standardized error handling (D-11, D-12, D-13)

import axios, { type AxiosInstance, type AxiosRequestConfig } from "axios";
import humps from "humps";

// Token management (D-11)
const TOKEN_KEY = "flow_access_token";
export const getToken = (): string | null => localStorage.getItem(TOKEN_KEY);
export const setToken = (token: string): void =>
  localStorage.setItem(TOKEN_KEY, token);
export const removeToken = (): void => localStorage.removeItem(TOKEN_KEY);

// FLOW: Standardized error response type (D-13)
export interface ErrorResponse {
  error: string;
}

export abstract class FlowApiService {
  protected baseURL: string;
  private axiosInstance: AxiosInstance;

  constructor(baseURL: string) {
    this.baseURL = baseURL;
    this.axiosInstance = axios.create({ baseURL });

    // FLOW: Request interceptor — auto-attach JWT Bearer token (D-11)
    this.axiosInstance.interceptors.request.use((config) => {
      const token = getToken();
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    });

    // FLOW: Response interceptor — snake_case to camelCase (D-12) + error standardization (D-13)
    this.axiosInstance.interceptors.response.use(
      (response) => {
        if (response.data && typeof response.data === "object") {
          response.data = humps.camelizeKeys(response.data);
        }
        return response;
      },
      (error) => {
        // 401 handling: token expired, clear and redirect to sign-in
        if (error?.response?.status === 401) {
          removeToken();
          window.location.href = "/auth/sign-in";
        }
        // FLOW: Standardize .NET API error responses (D-13)
        if (error?.response?.data) {
          const data = error.response.data;
          const standardized: ErrorResponse = {
            error:
              data.error ||
              data.title ||
              data.detail ||
              "Unknown error",
          };
          error.response.data = standardized;
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
