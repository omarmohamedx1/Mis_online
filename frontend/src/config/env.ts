const configuredApiOrigin = import.meta.env.VITE_API_URL?.trim();

if (!configuredApiOrigin) {
  throw new Error('VITE_API_URL must be configured. Copy .env.example for local development or configure it in the deployment environment.');
}

export const env = {
  apiUrl: `${configuredApiOrigin.replace(/\/+$/, '')}/api`,
} as const;
