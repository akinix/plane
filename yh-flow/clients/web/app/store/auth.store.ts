// FLOW: AuthStore — MobX JWT auth state management (AUTH-01~06)
import { action, computed, makeObservable, observable, runInAction } from "mobx";
import type { IUser } from "@plane/types";
import { AuthService, getToken, removeToken, setToken } from "../../src/lib/services/auth.service";

export interface IAuthStore {
  currentUser: IUser | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  initAuth: () => Promise<void>;
  signIn: (email: string, password: string) => Promise<void>;
  signUp: (email: string, password: string, firstName?: string, lastName?: string) => Promise<void>;
  signOut: () => Promise<void>;
  sendResetPasswordLink: (email: string) => Promise<void>;
  resetPassword: (token: string, password: string) => Promise<void>;
}

export class AuthStore implements IAuthStore {
  currentUser: IUser | null = null;
  isLoading: boolean = false;

  private authService: AuthService;

  constructor() {
    makeObservable(this, {
      currentUser: observable,
      isLoading: observable.ref,
      isAuthenticated: computed,
      initAuth: action,
      signIn: action,
      signUp: action,
      signOut: action,
    });
    this.authService = new AuthService();
  }

  get isAuthenticated(): boolean {
    return this.currentUser !== null && getToken() !== null;
  }

  initAuth = async () => {
    runInAction(() => {
      this.isLoading = true;
    });
    try {
      const token = getToken();
      if (!token) {
        runInAction(() => {
          this.currentUser = null;
        });
        return;
      }
      // FLOW: Token exists — consider authenticated.
      // Future phases can add /api/v1/users/me/ fetch here.
      runInAction(() => {
        this.currentUser = { id: "authenticated" } as IUser;
      });
    } finally {
      runInAction(() => {
        this.isLoading = false;
      });
    }
  };

  signIn = async (email: string, password: string) => {
    try {
      const response = await this.authService.signIn({ email, password });
      if (response?.accessToken) {
        setToken(response.accessToken);
      }
      await this.initAuth();
    } catch (error) {
      throw error;
    }
  };

  signUp = async (email: string, password: string, firstName?: string, lastName?: string) => {
    try {
      const response = await this.authService.signUp({ email, password, firstName, lastName });
      if (response?.accessToken) {
        setToken(response.accessToken);
      }
      await this.initAuth();
    } catch (error) {
      throw error;
    }
  };

  signOut = async () => {
    try {
      await this.authService.signOut();
    } finally {
      removeToken();
      runInAction(() => {
        this.currentUser = null;
      });
    }
  };

  sendResetPasswordLink = async (email: string) => {
    return this.authService.sendResetPasswordLink({ email });
  };

  resetPassword = async (token: string, password: string) => {
    return this.authService.resetPassword(token, { password });
  };
}
