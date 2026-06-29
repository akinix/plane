// FLOW: AuthService — JWT-based auth service (D-11)
// Replaces Plane's CSRF-based AuthService with JWT Bearer pattern
// Endpoint paths match .NET Identity API conventions

import type {
  IEmailCheckData,
  IEmailCheckResponse,
  IPasswordSignInData,
  ILoginTokenResponse,
} from "@plane/types";
import { FlowApiService, setToken, removeToken } from "./flow-api.service";
export { getToken, setToken, removeToken } from "./flow-api.service";

export class AuthService extends FlowApiService {
  private static BASE_URL =
    import.meta.env.VITE_API_BASE_URL || "http://localhost:5030/api/v1";

  constructor() {
    super(AuthService.BASE_URL);
  }

  // Check if email exists in the system
  async emailCheck(data: IEmailCheckData): Promise<IEmailCheckResponse> {
    return this.post("/auth/email-check/", data)
      .then((res) => res?.data)
      .catch((err) => {
        throw err?.response?.data;
      });
  }

  // Sign in with email + password
  async signIn(data: IPasswordSignInData): Promise<ILoginTokenResponse> {
    return this.post("/auth/sign-in/", data)
      .then((res) => {
        const tokenData = res?.data as ILoginTokenResponse;
        if (tokenData?.accessToken) {
          // humps camelizes access_token → accessToken
          setToken(tokenData.accessToken);
        }
        return tokenData;
      })
      .catch((err) => {
        throw err?.response?.data;
      });
  }

  // Sign up (register new account)
  async signUp(data: {
    email: string;
    password: string;
    firstName?: string;
    lastName?: string;
  }): Promise<ILoginTokenResponse> {
    return this.post("/auth/sign-up/", data)
      .then((res) => {
        const tokenData = res?.data as ILoginTokenResponse;
        if (tokenData?.accessToken) {
          setToken(tokenData.accessToken);
        }
        return tokenData;
      })
      .catch((err) => {
        throw err?.response?.data;
      });
  }

  // Send password reset email
  async sendResetPasswordLink(data: { email: string }): Promise<void> {
    return this.post("/auth/forgot-password/", data)
      .then((res) => res?.data)
      .catch((err) => {
        throw err?.response?.data;
      });
  }

  // Reset password with token
  async resetPassword(
    token: string,
    data: { password: string }
  ): Promise<void> {
    return this.post(`/auth/reset-password/${token}/`, data)
      .then((res) => res?.data)
      .catch((err) => {
        throw err?.response?.data;
      });
  }

  // Sign out — notify server then clear local token
  async signOut(): Promise<void> {
    return this.post("/auth/sign-out/", {})
      .then(() => removeToken())
      .catch(() => removeToken());
  }

  // Refresh access token
  async refreshToken(): Promise<ILoginTokenResponse> {
    return this.post("/auth/refresh/")
      .then((res) => {
        const tokenData = res?.data as ILoginTokenResponse;
        if (tokenData?.accessToken) {
          setToken(tokenData.accessToken);
        }
        return tokenData;
      })
      .catch((err) => {
        throw err?.response?.data;
      });
  }
}
