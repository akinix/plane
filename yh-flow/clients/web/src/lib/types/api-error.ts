// FLOW: Standardized API error types for interceptors (INFRA-04)
// Supports .NET ProblemDetails (RFC 7807), Plane legacy format, and FluentValidation field errors

/**
 * RFC 7807 ProblemDetails format — .NET native error response
 */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
}

/**
 * Plane legacy error format
 */
export interface PlaneError {
  error?: string;
  status?: number;
}

/**
 * Single field-level validation error
 */
export interface ApiFieldError {
  field: string;
  messages: string[];
}

/**
 * Standardized API error — unified frontend-consumable format
 */
export interface StandardizedApiError {
  message: string;
  statusCode?: number;
  fieldErrors?: ApiFieldError[];
  originalType?: string;
}

/**
 * Normalize any API error response (ProblemDetails, PlaneError, FluentValidation, unknown)
 * into the StandardizedApiError format consumed by frontend components.
 */
export function standardizeApiError(data: unknown): StandardizedApiError {
  if (!data || typeof data !== "object") {
    return {
      message: String(data ?? "未知错误"),
      originalType: "unknown",
    };
  }

  const record = data as Record<string, unknown>;

  // Detect ProblemDetails (RFC 7807): has type/title/detail fields
  if (typeof record.type === "string" || typeof record.title === "string" || typeof record.detail === "string") {
    const fieldErrors: ApiFieldError[] = [];

    // FluentValidation field-level errors: errors[fieldName] = string[]
    if (record.errors && typeof record.errors === "object" && !Array.isArray(record.errors)) {
      for (const [field, messages] of Object.entries(record.errors)) {
        if (Array.isArray(messages)) {
          fieldErrors.push({
            field,
            messages: messages.filter((m): m is string => typeof m === "string"),
          });
        }
      }
    }

    return {
      message: String(record.detail ?? record.title ?? record.error ?? "请求错误"),
      statusCode: typeof record.status === "number" ? record.status : undefined,
      fieldErrors: fieldErrors.length > 0 ? fieldErrors : undefined,
      originalType: "ProblemDetails",
    };
  }

  // Detect PlaneError: has error string field
  if (typeof record.error === "string") {
    return {
      message: record.error,
      statusCode: typeof record.status === "number" ? record.status : undefined,
      originalType: "PlaneError",
    };
  }

  // Fallback: serialize whatever we received
  return {
    message: String(record.detail ?? record.title ?? record.error ?? JSON.stringify(data) ?? "未知错误"),
    statusCode: typeof record.status === "number" ? record.status : undefined,
    originalType: "unknown",
  };
}
