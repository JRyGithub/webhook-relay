import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// Aspire injects the API's address when the AppHost runs this app.
const apiUrl = process.env.services__api__http__0 ?? process.env.API_HTTP

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: apiUrl ? { '/api': { target: apiUrl, changeOrigin: true } } : undefined,
  },
})
