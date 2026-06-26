import path from "node:path";
import { defineConfig } from "vite";
import { reactRouter } from "@react-router/dev/vite";
import tsconfigPaths from "vite-tsconfig-paths";

// FLOW: Vite config for Flow Web frontend (forked from Plane apps/web/vite.config.ts)
export default defineConfig({
  plugins: [reactRouter(), tsconfigPaths()],
  resolve: {
    alias: {
      // FLOW: @ → clients/web/ (Plane convention for root alias)
      "@": path.resolve(__dirname),
      // FLOW: Vite alias @plane/* → src/lib/* (per D-04)
      "@plane/types": path.resolve(__dirname, "src/lib/types"),
      "@plane/utils": path.resolve(__dirname, "src/lib/utils"),
      "@plane/constants": path.resolve(__dirname, "src/lib/constants"),
      "@plane/ui": path.resolve(__dirname, "src/lib/ui"),
      "@plane/editor": path.resolve(__dirname, "src/lib/editor"),
    },
  },
  server: {
    port: 5173,
    host: true,
  },
});
