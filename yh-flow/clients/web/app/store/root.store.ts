// FLOW: CoreRootStore — root MobX store (per CONVENTIONS.md)
import { AuthStore, type IAuthStore } from "./auth.store";

export interface ICoreRootStore {
  auth: IAuthStore;
}

export class CoreRootStore implements ICoreRootStore {
  auth: IAuthStore;

  constructor() {
    this.auth = new AuthStore();
  }
}
