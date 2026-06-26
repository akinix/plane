// FLOW: Auth type definitions matching Plane's @plane/types/auth.ts
// Fields use camelCase to match the post-humps-camelization response (D-12)

export type TEmailCheckTypes = "magic_code" | "password";

export interface IEmailCheckData {
  email: string;
}

export interface IEmailCheckResponse {
  status: "MAGIC_CODE" | "CREDENTIAL";
  existing: boolean;
  isPasswordAutoset: boolean;
}

export interface ILoginTokenResponse {
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
  csrfToken: string;
}
