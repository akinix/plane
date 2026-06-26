// FLOW: Flow Web Vite configuration with API proxy (SCAFF-05)

import { defineConfig } from "vite";
import path from "path";

export default defineConfig({
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
      "@plane/types": path.resolve(__dirname, "./src/lib/types"),
      "@plane/constants": path.resolve(__dirname, "./src/lib/constants"),
      "@plane/utils": path.resolve(__dirname, "./src/lib/utils"),
      "@plane/ui": path.resolve(__dirname, "./src/lib/ui"),
      "@plane/editor": path.resolve(__dirname, "./src/lib/editor"),
    },
  },
  server: {
    host: true,
    port: 5173,
    proxy: {
      "/api": {
        target: process.env.VITE_API_PROXY_TARGET || "https://localhost:7030",
        changeOrigin: true,
        secure: false, // FLOW: Development self-signed certificate
      },
    },
  },
});
