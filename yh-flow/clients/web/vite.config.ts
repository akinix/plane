import path from "node:path";
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tsconfigPaths from "vite-tsconfig-paths";

// FLOW: Vite config for Flow Web frontend (forked from Plane apps/web/vite.config.ts)
export default defineConfig({
  plugins: [react(), tsconfigPaths()],
  resolve: {
    alias: {
      // FLOW: Vite alias @plane/* → src/lib/* (per D-04)
      "@plane/types": path.resolve(__dirname, "src/lib/types"),
      "@plane/utils": path.resolve(__dirname, "src/lib/utils"),
      "@plane/ui": path.resolve(__dirname, "src/lib/ui"),
      "@plane/editor": path.resolve(__dirname, "src/lib/editor"),
    },
  },
  server: {
    port: 5173,
    host: true,
  },
});
