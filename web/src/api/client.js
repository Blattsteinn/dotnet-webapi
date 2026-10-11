import { getStoredToken } from '../auth/tokenStore.js'

// Single source of the API location: VITE_API_URL in .env (empty in development,
// where the Vite dev server proxies /api to the API — see vite.config.js; set it
// to the API's origin for a deployed build). No other file knows the API URL.
const BASE_URL = (import.meta.env.VITE_API_URL ?? '').replace(/\/+$/, '')

// Only used for error messages: with an empty VITE_API_URL the API is reached
// through the page's own origin.
export const API_BASE_URL = BASE_URL || window.location.origin

// Every API failure is RFC 7807 Problem Details (application/problem+json), so
// there is exactly one error shape to parse: status, title, detail and — for
// validation failures — errors keyed by field.
export class ApiError extends Error {
  constructor(problem, status) {
    const message =
      problem?.detail ??
      problem?.title ??
      (status === 0
        ? `Could not reach the API at ${API_BASE_URL}.`
        : `Request failed (${status}).`)

    super(message)
    this.name = 'ApiError'
    this.status = status
    this.title = problem?.title ?? null
    this.detail = problem?.detail ?? null
    this.errors = problem?.errors ?? null
  }

  // Flattens the validation `errors` object into " Field: message" lines.
  get fieldErrors() {
    if (!this.errors) return []
    return Object.entries(this.errors).flatMap(([field, messages]) =>
      (Array.isArray(messages) ? messages : [messages]).map((message) => ({
        field,
        message: String(message),
      })),
    )
  }
}

let unauthorizedHandler = () => {}

// AuthContext registers here: a 401 on a request that carried a token means the
// session is gone (expired after 1h, or a role change rotated the security
// stamp). The app then clears auth state and redirects to /login.
export function setUnauthorizedHandler(handler) {
  unauthorizedHandler = handler ?? (() => {})
}

async function parseBody(response) {
  // 204/205 have no body at all; res.json() would throw on them.
  if (response.status === 204 || response.status === 205) return null

  const text = await response.text()
  if (!text) return null

  try {
    return JSON.parse(text)
  } catch {
    return null
  }
}

export async function request(method, path, body) {
  const headers = { Accept: 'application/json' }
  const token = getStoredToken()
  if (token) headers.Authorization = `Bearer ${token}`

  const init = { method, headers }
  if (body !== undefined) {
    headers['Content-Type'] = 'application/json'
    init.body = JSON.stringify(body)
  }

  let response
  try {
    response = await fetch(`${BASE_URL}${path}`, init)
  } catch {
    // Network/CORS/certificate failure — not a Problem Details response.
    throw new ApiError(null, 0)
  }

  const payload = await parseBody(response)
  if (response.ok) return payload

  // A 401 on the login call itself is a normal "wrong password": the request
  // carried no token, so it must not tear down the session.
  if (response.status === 401 && token) unauthorizedHandler()

  throw new ApiError(payload, response.status)
}

export const api = {
  get: (path) => request('GET', path),
  post: (path, body) => request('POST', path, body),
  put: (path, body) => request('PUT', path, body),
  patch: (path, body) => request('PATCH', path, body),
  del: (path) => request('DELETE', path),
}
