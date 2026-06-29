// FLOW: StoreProvider — MobX store React context (per CONVENTIONS.md)
import React, { createContext, useContext } from "react";
import { CoreRootStore, type ICoreRootStore } from "@/store/root.store";

const StoreContext = createContext<ICoreRootStore | null>(null);

export const useStore = (): ICoreRootStore => {
  const store = useContext(StoreContext);
  if (!store) throw new Error("StoreProvider not found");
  return store;
};

export const StoreProvider = ({ children }: { children: React.ReactNode }) => {
  const [store] = React.useState(() => new CoreRootStore());
  return <StoreContext.Provider value={store}>{children}</StoreContext.Provider>;
};
