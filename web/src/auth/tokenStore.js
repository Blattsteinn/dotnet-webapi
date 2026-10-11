// The storage key and shape of the persisted session live here so the API
// client can read the token without importing React state.
const STORAGE_KEY = 'foodOrderingAuth'

export function readStoredAuth() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return null
    const parsed = JSON.parse(raw)
    if (!parsed?.token || !parsed?.userId) return null
    return {
      token: parsed.token,
      userId: parsed.userId,
      email: parsed.email ?? null,
      roles: Array.isArray(parsed.roles) ? parsed.roles : [],
    }
  } catch {
    return null
  }
}

export function writeStoredAuth(auth) {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(auth))
  } catch {
    // Private-mode localStorage failures must not break signing in.
  }
}

export function clearStoredAuth() {
  try {
    localStorage.removeItem(STORAGE_KEY)
  } catch {
    // ignore
  }
}

export function getStoredToken() {
  return readStoredAuth()?.token ?? null
}
