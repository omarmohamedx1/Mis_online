import type { AuthResponse } from '../features/auth/types/auth';

const AUTH_STORAGE_KEY = 'mis.auth';

function readAuthFromStorage(storage: Storage): AuthResponse | null {
  const storedValue = storage.getItem(AUTH_STORAGE_KEY);

  if (!storedValue) {
    return null;
  }

  try {
    return JSON.parse(storedValue) as AuthResponse;
  } catch {
    storage.removeItem(AUTH_STORAGE_KEY);
    return null;
  }
}

export function getStoredAuth(): AuthResponse | null {
  return readAuthFromStorage(localStorage) ?? readAuthFromStorage(sessionStorage);
}

export function persistAuth(auth: AuthResponse, rememberMe: boolean): void {
  clearStoredAuth();
  const targetStorage = rememberMe ? localStorage : sessionStorage;
  targetStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(auth));
}

export function clearStoredAuth(): void {
  localStorage.removeItem(AUTH_STORAGE_KEY);
  sessionStorage.removeItem(AUTH_STORAGE_KEY);
}
