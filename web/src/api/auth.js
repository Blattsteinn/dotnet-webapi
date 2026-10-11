import { api } from './client.js'

// POST /api/v1/auth/login → { token, userId, email, roles }
export function login(email, password) {
  return api.post('/api/v1/auth/login', { email, password })
}

// POST /api/v1/auth/register → 200 with an empty body
export function register(email, password) {
  return api.post('/api/v1/auth/register', { email, password })
}
