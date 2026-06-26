// FLOW: Forked from @plane/types/src/types/auth.ts
/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

export type TEmailCheckTypes = "magic_code" | "password";

export interface IEmailCheckData {
  email: string;
}

export interface IEmailCheckResponse {
  status: "MAGIC_CODE" | "CREDENTIAL";
  existing: boolean;
  // FLOW: snake_case → camelCase post-humps conversion (D-12)
  isPasswordAutoset: boolean;
}

export interface ILoginTokenResponse {
  // FLOW: camelCase post-humps conversion (D-12) — .NET API returns snake_case, humps converts at runtime
  accessToken: string;
  refreshToken: string;
}

export interface IMagicSignInData {
  email: string;
  key: string;
  token: string;
}

export interface IPasswordSignInData {
  email: string;
  password: string;
}

export interface ICsrfTokenData {
  // FLOW: camelCase post-humps conversion (D-12)
  csrfToken: string;
}
