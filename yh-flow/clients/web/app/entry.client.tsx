// FLOW: Forked from Plane apps/web/app/entry.client.tsx
import { StrictMode } from "react";
import { hydrateRoot } from "react-dom/client";
import { HydratedRouter } from "react-router/dom";

hydrateRoot(
  document.getElementById("root")!,
  <StrictMode>
    <HydratedRouter />
  </StrictMode>
);
