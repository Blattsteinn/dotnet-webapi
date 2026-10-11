import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const apiTarget = env.VITE_API_PROXY_TARGET

  return {
    plugins: [react(), tailwindcss()],
    server: {
      // Used only while VITE_API_URL is empty: in development the browser calls
      // this dev server, and the request is forwarded to the API from here, so
      // the ASP.NET development certificate is never presented to the browser.
      proxy: apiTarget
        ? {
            '/api': {
              target: apiTarget,
              changeOrigin: true,
              secure: false,
            },
          }
        : undefined,
    },
  }
})
