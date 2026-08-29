import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig(({ command, mode }) => {
  const environment = loadEnv(mode, process.cwd(), 'VITE_');
  const apiOrigin = environment.VITE_API_URL?.trim();

  if (command === 'build') {
    if (!apiOrigin) {
      throw new Error('VITE_API_URL must be configured when building the frontend.');
    }

    const parsedApiOrigin = new URL(apiOrigin);
    if (!['http:', 'https:'].includes(parsedApiOrigin.protocol) || parsedApiOrigin.username || parsedApiOrigin.password || parsedApiOrigin.pathname !== '/' || parsedApiOrigin.search || parsedApiOrigin.hash) {
      throw new Error('VITE_API_URL must be an HTTP(S) origin without a path, query string, or fragment. Do not append /api.');
    }
  }

  return {
    plugins: [react()],
    server: {
      port: 5173,
    },
  };
});
